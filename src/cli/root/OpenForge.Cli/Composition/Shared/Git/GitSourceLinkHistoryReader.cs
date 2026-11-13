using System.Diagnostics;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;

namespace OpenForge.Cli.Composition.Shared.Git;

internal sealed class GitSourceLinkHistoryReader(Action<ProcessStartInfo>? configureProcess = null)
{
    private readonly GitProcessRunner _git = new("git", configureProcess);

    internal async ValueTask<SourceLinkHistory> ReadAsync(SourceLinkHistoryRequest request, CancellationToken token)
    {
        var entries = new List<SourceLinkHistoryEntry>();
        using var timeout = new CancellationTokenSource(GitRenameEvidenceReader.Deadline);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token, timeout.Token);
        try
        {
            var prefixRead = await Run(["rev-parse", "--show-prefix"]);
            if (prefixRead.ExitCode != 0) return new(entries);
            var prefix = prefixRead.Output.TrimEnd('\r', '\n');
            foreach (var reference in request.References.Where(reference => reference.Facts.Target.Path is not null
                && reference.Facts.Target.Resolution is SourceLinkTargetResolution.Missing or SourceLinkTargetResolution.FragmentMissing))
            {
                deadline.Token.ThrowIfCancellationRequested();
                var line = reference.Location?.Line ?? 1;
                var blame = await Run(["blame", "--porcelain", "-L", $"{line},{line}", "--", reference.SourcePath]);
                if (blame.ExitCode != 0) continue;
                var commit = blame.Output.Split(' ', 2)[0];
                if (commit.Length is not (40 or 64) || commit.All(character => character == '0')
                    || !commit.All(char.IsAsciiHexDigit)) continue;
                var oldPath = reference.Facts.Target.Path ?? throw new InvalidOperationException("History requires a target path.");
                var old = await Run(["show", $"{commit}:{prefix}{oldPath}"]);
                var diff = await Run(["diff", "--relative", "--name-status", "-z", "--find-renames", commit, "HEAD", "--"]);
                var moved = diff.ExitCode == 0 ? ReadMove(diff.Output, oldPath) : null;
                var targetDiff = await Run(["diff", "--no-ext-diff", "--no-textconv", "--unified=0", "--find-renames", commit, "--", oldPath, moved ?? oldPath]);
                entries.Add(new(reference.SourcePath, reference.Destination, commit, moved,
                    old.ExitCode == 0 ? old.Output : null)
                { TargetChanges = targetDiff.ExitCode == 0 ? GitSourceLinkDiffReader.Read(targetDiff.Output) : [] });
            }
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            return new(entries, TimedOut: true);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            return new(entries);
        }
        return new(entries);

        async Task<(int ExitCode, string Output, string Error)> Run(string[] arguments)
            => await _git.RunAsync(request.Workspace.PhysicalRoot, arguments, deadline.Token).ConfigureAwait(false)
                ?? (-1, string.Empty, string.Empty);
    }

    private static string? ReadMove(string output, string oldPath)
    {
        var fields = output.Split('\0', StringSplitOptions.RemoveEmptyEntries);
        for (var index = 0; index < fields.Length;)
        {
            var status = fields[index++];
            if (status.StartsWith('R') || status.StartsWith('C'))
            {
                if (index + 1 >= fields.Length) return null;
                var previous = fields[index++];
                var current = fields[index++];
                if (status.StartsWith('R') && previous == oldPath) return current;
            }
            else index++;
        }
        return null;
    }
}
