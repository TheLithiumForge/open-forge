using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Input;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;

internal sealed class RenameEvidenceReader(PhysicalPathResolver physicalPathResolver, RenameGitReader? gitReader = null)
{
    internal async ValueTask<RenameEvidenceResult> ReadAsync(RenameEvidenceRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        var policy = request.Policy with
        {
            PackageRoots = request.Policy.PackageRoots.AddRange(request.CandidatePaths.Where(path => Path.GetFileName(path) == "SKILL.md")
                .Select(SourceLogicalPath.ReadParent)).Distinct(StringComparer.Ordinal).ToImmutableArray(),
        };
        var blockedPhysicalPaths = RenameCandidateAdmission.ReadBlockedPhysicalPaths(request.Workspace, physicalPathResolver, policy);
        var candidates = ImmutableArray.CreateBuilder<RenameCurrentCandidate>();
        foreach (var path in request.CandidatePaths.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!RenameCandidateAdmission.Allows(path, policy)) continue;
            var logical = SourceLogicalPath.ToLexicalPath(request.Workspace.LexicalRoot, path);
            if (NoFollowLeafObserver.Observe(physicalPathResolver, request.Workspace, logical, cancellationToken).State != NoFollowLeafState.OrdinaryFile) continue;
            var physical = physicalPathResolver.ResolveCandidate(request.Workspace.LexicalRoot, request.Workspace.PhysicalRoot, logical);
            if (physical.State != PhysicalPathState.Contained
                || RenameCandidateAdmission.IsPhysicallyBlocked(physical.GetContainedPhysicalPath(), blockedPhysicalPaths)) continue;
            try
            {
                var bytes = await File.ReadAllBytesAsync(physical.GetContainedPhysicalPath(), cancellationToken).ConfigureAwait(false);
                var content = RenameContentReader.Read(bytes);
                if (content.Document.ManagedHost.State != MarkdownManagedHostState.Absent) continue;
                var snapshot = FileStateSnapshot.File(logicalPath: logical, physicalPath: physical.GetContainedPhysicalPath(), bytes: bytes);
                candidates.Add(new RenameCurrentCandidate(path, snapshot, content, RenameMetadataIdentity.Read(content.Document)));
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or DecoderFallbackException or ArgumentException or InvalidOperationException)
            {
                // Only readable ordinary Markdown participates in the admitted universe.
            }
        }

        var records = request.RecordedIdentities.Where(record => RenameCandidateAdmission.Allows(record.Path, policy)
            && !RenameCandidateAdmission.IsPathPhysicallyBlocked(request.Workspace, physicalPathResolver, record.Path, blockedPhysicalPaths)).ToImmutableArray();
        var requestedOldPaths = request.MissingPaths.Concat(records.Select(record => record.Path)).Distinct(StringComparer.Ordinal).ToImmutableArray();
        var git = gitReader is null
            ? RenameGitObservation.Unavailable(RenameGitAvailability.NotRequested)
            : await gitReader(new RenameGitRequest(request.Workspace, policy), cancellationToken).ConfigureAwait(false);
        var old = records.ToBuilder();
        if (git.Availability == RenameGitAvailability.Available)
        {
            foreach (var tracked in git.TrackedPaths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!RenameCandidateAdmission.Allows(tracked.Path, policy)) continue;
                var blob = git.Blobs.FirstOrDefault(blob => blob.Path == tracked.Path);
                try
                {
                    var content = blob is null ? null : RenameContentReader.Read(blob.Bytes.AsSpan());
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

        var missingPaths = ImmutableArray.CreateBuilder<RenameMissingPath>();
        foreach (var path in requestedOldPaths.Concat(old.Select(record => record.Path)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!RenameCandidateAdmission.Allows(path, policy)) continue;
            var logical = SourceLogicalPath.ToLexicalPath(request.Workspace.LexicalRoot, path);
            if (NoFollowLeafObserver.Observe(physicalPathResolver, request.Workspace, logical, cancellationToken).State == NoFollowLeafState.Missing)
            {
                missingPaths.Add(new RenameMissingPath(path, FileExpectation.Missing(logical)));
            }
        }

        var facts = new RenameEvidenceFacts
        {
            Candidates = candidates.ToImmutable(),
            MissingPaths = missingPaths.ToImmutable(),
            OldIdentities = old.ToImmutable(),
            Git = git,
            CandidateUniverseComplete = request.CandidateUniverseComplete,
            WeakCandidates = request.WeakCandidates,
            PolicyExpectations = request.PolicyExpectations,
        };
        return new RenameEvidenceEvaluator().Evaluate(facts, cancellationToken);
    }
}
