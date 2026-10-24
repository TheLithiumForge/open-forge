using System.Collections.Immutable;
using System.Text;
using System.Collections.Concurrent;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Content;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Input;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;

internal sealed class RenameEvidenceReader(PhysicalPathResolver physicalPathResolver, RenameGitReader? gitReader = null,
    TimeProvider? timeProvider = null, Action<string, string>? contentComparisonStarted = null,
    Func<string, CancellationToken, Task<byte[]>>? readCandidate = null)
{
    internal static readonly TimeSpan ContentDeadline = TimeSpan.FromSeconds(5);
    internal const int MaximumConcurrentReads = 16;
    private readonly Func<string, CancellationToken, Task<byte[]>> _readCandidate = readCandidate ?? File.ReadAllBytesAsync;

    internal async ValueTask<RenameEvidenceResult> ReadAsync(RenameEvidenceRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        var git = gitReader is null
            ? RenameGitObservation.Unavailable(RenameGitAvailability.NotRequested)
            : await gitReader(new RenameGitRequest(request.Workspace, request.Policy), cancellationToken).ConfigureAwait(false);
        try
        {
            return await ReadContentAsync(request, git, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new RenameEvidenceResult(new RenameEvidenceFacts
            {
                Layout = request.Policy.Layout,
                Candidates = [],
                MissingPaths = [],
                OldIdentities = [],
                Git = git with { Availability = RenameGitAvailability.TimedOut },
                CandidateUniverseComplete = false,
                WeakCandidates = request.WeakCandidates,
                PolicyExpectations = request.PolicyExpectations,
            }, []);
        }
    }

    private async ValueTask<RenameEvidenceResult> ReadContentAsync(RenameEvidenceRequest request, RenameGitObservation git, CancellationToken cancellationToken)
    {
        var paths = physicalPathResolver.CreateReadSession();
        var policy = request.Policy;
        var blockedPhysicalPaths = RenameCandidateAdmission.ReadBlockedPhysicalPaths(request.Workspace, paths, policy);
        var contentsByHash = new ConcurrentDictionary<string, Lazy<RenameContent>>(StringComparer.Ordinal);
        var candidatePaths = request.CandidatePaths.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        var observed = new RenameCurrentCandidate?[candidatePaths.Length];
        await Parallel.ForEachAsync(Enumerable.Range(0, candidatePaths.Length), new ParallelOptions
        {
            MaxDegreeOfParallelism = MaximumConcurrentReads,
            CancellationToken = cancellationToken,
        }, async (index, token) =>
        {
            var path = candidatePaths[index];
            if (!RenameCandidateAdmission.Allows(path, policy)) return;
            var logical = SourceLogicalPath.ToLexicalPath(request.Workspace.LexicalRoot, path);
            if (NoFollowLeafObserver.Observe(paths, request.Workspace, logical, token).State != NoFollowLeafState.OrdinaryFile) return;
            var resolution = paths.ResolveCandidate(request.Workspace.LexicalRoot, request.Workspace.PhysicalRoot, logical);
            if (resolution.State != PhysicalPathState.Contained
                || RenameCandidateAdmission.IsPhysicallyBlocked(resolution.GetContainedPhysicalPath(), blockedPhysicalPaths)) return;
            var physical = resolution.GetContainedPhysicalPath();
            try
            {
                var bytes = await _readCandidate(physical, token).ConfigureAwait(false);
                request.Documents.TryGetValue(path, out var document);
                var content = ReadContent(bytes, document);
                if (content.Document.ManagedHost.State != MarkdownManagedHostState.Absent) return;
                var snapshot = FileStateSnapshot.File(logicalPath: logical, physicalPath: physical, bytes: bytes);
                observed[index] = new RenameCurrentCandidate(path, snapshot, content, RenameMetadataIdentity.Read(content.Document));
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or DecoderFallbackException or ArgumentException or InvalidOperationException)
            {
                // Only readable ordinary Markdown participates in the admitted universe.
            }
        }).ConfigureAwait(false);
        var candidates = observed.OfType<RenameCurrentCandidate>().ToImmutableArray();

        var records = request.RecordedIdentities.Where(record => RenameCandidateAdmission.Allows(record.Path, policy)
            && !RenameCandidateAdmission.IsPathPhysicallyBlocked(request.Workspace, paths, record.Path, blockedPhysicalPaths)).ToImmutableArray();
        var requestedOldPaths = request.MissingPaths.Concat(records.Select(record => record.Path)).Distinct(StringComparer.Ordinal).ToImmutableArray();
        var old = records.ToBuilder();
        if (git.Availability == RenameGitAvailability.Available)
        {
            var blobs = git.Blobs.ToDictionary(blob => blob.Path, StringComparer.Ordinal);
            var contents = new Dictionary<string, RenameContent>(StringComparer.Ordinal);
            foreach (var tracked in git.TrackedPaths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!RenameCandidateAdmission.Allows(tracked.Path, policy)) continue;
                blobs.TryGetValue(tracked.Path, out var blob);
                try
                {
                    RenameContent? content = null;
                    if (blob is not null && !contents.TryGetValue(blob.Blob, out content))
                    {
                        content = ReadContent(blob.Bytes.AsSpan());
                        contents.Add(blob.Blob, content);
                    }
                    old.Add(new RenameRecordedIdentity
                    {
                        Path = tracked.Path,
                        Content = content,
                        Metadata = content is null ? null : RenameMetadataIdentity.Read(content.Document),
                        Title = content is null ? null : RenameMetadataIdentity.ReadTitle(content.Document),
                        Head = git.Head,
                        Blob = tracked.Blob,
                    });
                }
                catch (Exception exception) when (exception is DecoderFallbackException or ArgumentException or InvalidOperationException)
                {
                    // An unusable optional blob does not suppress stale Entries evidence.
                    old.Add(new RenameRecordedIdentity { Path = tracked.Path, Metadata = null, Head = git.Head, Blob = tracked.Blob });
                }
            }
        }

        var oldPaths = requestedOldPaths.Concat(old.Select(record => record.Path)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        var missingPaths = new RenameMissingPath?[oldPaths.Length];
        await Parallel.ForEachAsync(Enumerable.Range(0, oldPaths.Length), new ParallelOptions
        {
            MaxDegreeOfParallelism = MaximumConcurrentReads,
            CancellationToken = cancellationToken,
        }, (index, token) =>
        {
            var path = oldPaths[index];
            if (!RenameCandidateAdmission.Allows(path, policy)) return ValueTask.CompletedTask;
            var logical = SourceLogicalPath.ToLexicalPath(request.Workspace.LexicalRoot, path);
            if (NoFollowLeafObserver.Observe(paths, request.Workspace, logical, token).State == NoFollowLeafState.Missing)
            {
                missingPaths[index] = new RenameMissingPath(path, FileExpectation.Missing(logical));
            }
            return ValueTask.CompletedTask;
        }).ConfigureAwait(false);

        var facts = new RenameEvidenceFacts
        {
            Layout = request.Policy.Layout,
            Candidates = candidates,
            MissingPaths = missingPaths.OfType<RenameMissingPath>().ToImmutableArray(),
            OldIdentities = old.ToImmutable(),
            Git = git,
            CandidateUniverseComplete = request.CandidateUniverseComplete,
            WeakCandidates = request.WeakCandidates,
            PolicyExpectations = request.PolicyExpectations,
        };
        using var timeout = new CancellationTokenSource(ContentDeadline, timeProvider ?? TimeProvider.System);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        var result = new RenameEvidenceEvaluator(contentComparisonStarted).Evaluate(facts, deadline.Token);
        deadline.Token.ThrowIfCancellationRequested();
        return result;

        RenameContent ReadContent(ReadOnlySpan<byte> bytes, MarkdownDocumentFacts? document = null)
        {
            var hash = FileExpectation.Hash(bytes);
            if (contentsByHash.TryGetValue(hash, out var retained)) return retained.Value;
            var contentBytes = bytes.ToArray();
            return contentsByHash.GetOrAdd(hash, static (_, content) => new(() => RenameContentReader.Read(content.Bytes, content.Document)),
                (Bytes: contentBytes, Document: document)).Value;
        }
    }
}
