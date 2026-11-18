using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using OpenForge.Cli.Composition.Shared.Git.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git;

namespace OpenForge.Cli.Composition.Shared.Git;

internal sealed class GitRenameEvidenceReader
{
    private readonly GitProcessRunner _git;
    private readonly GitInvocationDeadline _deadline;
    private readonly PhysicalPathResolver _physicalPaths;

    internal GitRenameEvidenceReader(string gitExecutable = "git", Action<ProcessStartInfo>? configureProcess = null,
        TimeProvider? timeProvider = null, Action<Process>? processStarted = null, PhysicalPathResolver? physicalPaths = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gitExecutable);
        _git = new GitProcessRunner(gitExecutable, configureProcess, processStarted);
        _deadline = new GitInvocationDeadline(timeProvider);
        _physicalPaths = physicalPaths ?? new();
    }

    internal async ValueTask<RenameGitObservation> ReadAsync(RenameGitRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        var root = request.Workspace.PhysicalRoot;
        try
        {
            // One rev-parse call answers the work-tree check, the prefix and HEAD.
            var probe = GitWorkTreeCheck.Read(await _deadline.RunAsync(token => _git.RunAsync(root,
                [.. GitWorkTreeCheck.Arguments, "--show-prefix", "--verify", "--quiet", "HEAD^{commit}"], token), cancellationToken));
            if (probe.State == GitWorkTreeState.Unavailable) return Unavailable(RenameGitAvailability.Unavailable);
            var metadata = probe.Lines;
            if (metadata.Count < 2 || probe.State != GitWorkTreeState.Inside) return Unavailable(RenameGitAvailability.NoRepository);
            if (metadata.Count < 3) return Unavailable(RenameGitAvailability.Unavailable);
            var prefix = metadata[1];
            if (prefix.Length > 0) _ = SourceWorkspaceRelativePath.Validate(prefix.TrimEnd('/'), nameof(request), false);
            if (probe.ExitCode != 0) return Unavailable(RenameGitAvailability.Unborn);
            var head = string.Join('\n', metadata.Skip(2)).Trim();
            if (!IsObjectId(head)) return Unavailable(RenameGitAvailability.Unavailable);

            var tree = await Run(["ls-tree", "-r", "-z", "--full-tree", head]);
            if (tree is not { ExitCode: 0 } treeRead || GitOutput.ReadNulEntries(treeRead.Output) is not { } treeEntries) return Unavailable(RenameGitAvailability.Unavailable);
            var inventory = ReadInventory(treeEntries, prefix);
            var policy = request.Policy;
            var resolver = _physicalPaths.CreateReadSession();
            var blockedPhysicalPaths = RenameCandidateAdmission.ReadBlockedPhysicalPaths(request.Workspace, resolver, policy);
            var admitted = inventory.Where(path => RenameCandidateAdmission.Allows(path.Path, policy)
                && !RenameCandidateAdmission.IsPathPhysicallyBlocked(request.Workspace, resolver, path.Path, blockedPhysicalPaths)).ToImmutableArray();
            var blobs = ImmutableArray<RenameGitBlob>.Empty;
            if (!admitted.IsEmpty)
            {
                var input = Encoding.ASCII.GetBytes(string.Concat(admitted.Select(path => path.Blob).Distinct(StringComparer.Ordinal).Select(blob => $"{blob}\n")));
                // Batch every admitted old identity so metadata uniqueness includes live HEAD records.
                var blob = await _deadline.RunAsync(token => _git.RunBytesWithInputAsync(root, ["cat-file", "--batch"], input, token), cancellationToken);
                if (blob is not { ExitCode: 0 } read || read.Error.Length != 0) return Unavailable(RenameGitAvailability.Unavailable);
                blobs = GitRenameBlobBatchReader.Read(read.Output, admitted);
            }

            var finalHead = await Run(["rev-parse", "--verify", "--quiet", "HEAD^{commit}"]);
            if (finalHead is not { ExitCode: 0 } final || final.Output.Trim() != head) return Unavailable(RenameGitAvailability.Unavailable);
            return new RenameGitObservation
            {
                Availability = RenameGitAvailability.Available,
                Head = head,
                RepositoryPrefix = prefix,
                TrackedPaths = admitted,
                Blobs = blobs,
            };
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Unavailable(RenameGitAvailability.TimedOut);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or DecoderFallbackException or ArgumentException or InvalidOperationException)
        {
            return Unavailable(RenameGitAvailability.Unavailable);
        }

        async Task<(int ExitCode, string Output, string Error)?> Run(string[] arguments)
        {
            var result = await _deadline.RunAsync(token => _git.RunAsync(root, arguments, token), cancellationToken);
            return GitOutput.IsUntrusted(result) ? null : result;
        }
    }

    private static ImmutableArray<RenameGitTrackedPath> ReadInventory(ImmutableArray<string> entries, string prefix)
    {
        var paths = ImmutableArray.CreateBuilder<RenameGitTrackedPath>();
        foreach (var entry in entries)
        {
            var tab = entry.IndexOf('\t');
            if (tab < 0) throw new ArgumentException("Git tree output requires a path separator.", nameof(entries));
            var fields = entry[..tab].Split(' ');
            if (fields.Length != 3 || !IsObjectId(fields[2])) throw new ArgumentException("Git tree output requires an object identity.", nameof(entries));
            var path = entry[(tab + 1)..];
            if (fields[0] != "100644" && fields[0] != "100755" || fields[1] != "blob" || !path.StartsWith(prefix, StringComparison.Ordinal)) continue;
            path = path[prefix.Length..];
            if (!SourceLogicalPath.IsCanonicalSource(path)) continue;
            paths.Add(new RenameGitTrackedPath(path, fields[2]));
        }

        return paths.ToImmutable();
    }

    private static bool IsObjectId(string value)
        => value.Length is 40 or 64 && value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static RenameGitObservation Unavailable(RenameGitAvailability availability) => RenameGitObservation.Unavailable(availability);
}
