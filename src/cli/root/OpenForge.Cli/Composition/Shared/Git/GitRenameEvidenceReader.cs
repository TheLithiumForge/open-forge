using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git;

namespace OpenForge.Cli.Composition.Shared.Git;

internal sealed class GitRenameEvidenceReader
{
    internal static readonly TimeSpan Deadline = TimeSpan.FromSeconds(5);
    private readonly GitProcessRunner _git;
    private readonly TimeProvider _timeProvider;

    internal GitRenameEvidenceReader(string gitExecutable = "git", Action<ProcessStartInfo>? configureProcess = null,
        TimeProvider? timeProvider = null, Action<Process>? processStarted = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gitExecutable);
        _git = new GitProcessRunner(gitExecutable, configureProcess, processStarted);
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    internal async ValueTask<RenameGitObservation> ReadAsync(RenameGitRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        var root = request.Workspace.PhysicalRoot;
        try
        {
            var probe = await Run(["rev-parse", "--is-inside-work-tree"]);
            if (probe is null) return Unavailable(RenameGitAvailability.Unavailable);
            if (probe is not { ExitCode: 0 } inside || inside.Output.Trim() != "true") return Unavailable(RenameGitAvailability.NoRepository);
            var prefixResult = await Run(["rev-parse", "--show-prefix"]);
            if (prefixResult is not { ExitCode: 0 } prefixRead) return Unavailable(RenameGitAvailability.Unavailable);
            var prefix = prefixRead.Output.TrimEnd('\r', '\n');
            if (prefix.Length > 0) _ = SourceWorkspaceRelativePath.Validate(prefix.TrimEnd('/'), nameof(request), false);
            var headResult = await Run(["rev-parse", "--verify", "--quiet", "HEAD^{commit}"]);
            if (headResult is not { ExitCode: 0 } headRead) return Unavailable(RenameGitAvailability.Unborn);
            var head = headRead.Output.Trim();
            if (!IsObjectId(head)) return Unavailable(RenameGitAvailability.Unavailable);

            var tree = await Run(["ls-tree", "-r", "-z", "--full-tree", head]);
            if (tree is not { ExitCode: 0 } treeRead || !ValidNulOutput(treeRead.Output)) return Unavailable(RenameGitAvailability.Unavailable);
            var inventory = ReadInventory(treeRead.Output, prefix);
            var policy = request.Policy with
            {
                PackageRoots = request.Policy.PackageRoots.AddRange(inventory.Where(path => Path.GetFileName(path.Path) == "SKILL.md")
                    .Select(path => SourceLogicalPath.ReadParent(path.Path))).Distinct(StringComparer.Ordinal).ToImmutableArray(),
            };
            var resolver = new PhysicalPathResolver();
            var blockedPhysicalPaths = RenameCandidateAdmission.ReadBlockedPhysicalPaths(request.Workspace, resolver, policy);
            var admitted = inventory.Where(path => RenameCandidateAdmission.Allows(path.Path, policy)
                && !RenameCandidateAdmission.IsPathPhysicallyBlocked(request.Workspace, resolver, path.Path, blockedPhysicalPaths)).ToImmutableArray();
            var blobs = ImmutableArray<RenameGitBlob>.Empty;
            if (!admitted.IsEmpty)
            {
                var input = Encoding.ASCII.GetBytes(string.Concat(admitted.Select(path => path.Blob).Distinct(StringComparer.Ordinal).Select(blob => $"{blob}\n")));
                // Batch every admitted old identity so metadata uniqueness includes live HEAD records.
                var blob = await Invoke(token => _git.RunBytesWithInputAsync(root, ["cat-file", "--batch"], input, token));
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
            var result = await Invoke(token => _git.RunAsync(root, arguments, token));
            return result is { ExitCode: 0 } read && !string.IsNullOrWhiteSpace(read.Error) ? null : result;
        }

        async Task<T> Invoke<T>(Func<CancellationToken, Task<T>> invocation)
        {
            using var timeout = new CancellationTokenSource(Deadline, _timeProvider);
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
            var started = _timeProvider.GetTimestamp();
            deadline.Token.ThrowIfCancellationRequested();
            var result = await invocation(deadline.Token).ConfigureAwait(false);
            if (_timeProvider.GetElapsedTime(started) >= Deadline) timeout.Cancel();
            deadline.Token.ThrowIfCancellationRequested();
            return result;
        }
    }

    private static ImmutableArray<RenameGitTrackedPath> ReadInventory(string output, string prefix)
    {
        var paths = ImmutableArray.CreateBuilder<RenameGitTrackedPath>();
        foreach (var entry in output.Split('\0', StringSplitOptions.RemoveEmptyEntries))
        {
            var tab = entry.IndexOf('\t');
            if (tab < 0) throw new ArgumentException("Git tree output requires a path separator.", nameof(output));
            var fields = entry[..tab].Split(' ');
            if (fields.Length != 3 || !IsObjectId(fields[2])) throw new ArgumentException("Git tree output requires an object identity.", nameof(output));
            var path = entry[(tab + 1)..];
            if (fields[0] != "100644" && fields[0] != "100755" || fields[1] != "blob" || !path.StartsWith(prefix, StringComparison.Ordinal)) continue;
            path = path[prefix.Length..];
            if (!SourceLogicalPath.IsCanonicalSource(path)) continue;
            paths.Add(new RenameGitTrackedPath(path, fields[2]));
        }

        return paths.ToImmutable();
    }

    private static bool ValidNulOutput(string output)
        => output.Length == 0 || output[^1] == '\0' && output[0] != '\0' && !output.Contains("\0\0", StringComparison.Ordinal);

    private static bool IsObjectId(string value)
        => value.Length is 40 or 64 && value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static RenameGitObservation Unavailable(RenameGitAvailability availability) => RenameGitObservation.Unavailable(availability);
}
