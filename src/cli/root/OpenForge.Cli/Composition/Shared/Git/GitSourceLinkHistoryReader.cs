using System.Diagnostics;
using System.Globalization;
using OpenForge.Cli.Composition.Shared.Git.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;

namespace OpenForge.Cli.Composition.Shared.Git;

internal sealed class GitSourceLinkHistoryReader(Action<ProcessStartInfo>? configureProcess = null,
    TimeProvider? timeProvider = null, Action<Process>? processStarted = null)
{
    private readonly GitProcessRunner _git = new("git", configureProcess, processStarted);
    private readonly GitInvocationDeadline _deadline = new(timeProvider);

    internal async ValueTask<SourceLinkHistory> ReadAsync(SourceLinkHistoryRequest request, CancellationToken token)
    {
        var entries = new List<SourceLinkHistoryEntry>();
        try
        {
            // One rev-parse call answers the work-tree check and the prefix.
            var probe = GitWorkTreeCheck.Read(await _deadline.RunAsync(deadline =>
                _git.RunAsync(request.Workspace.PhysicalRoot, [.. GitWorkTreeCheck.Arguments, "--show-prefix"], deadline), token).ConfigureAwait(false));
            if (probe is not { State: GitWorkTreeState.Inside, ExitCode: 0 } || probe.Lines.Count < 2) return new(entries);
            var prefix = probe.Lines[1];
            var broken = request.References.Where(reference => reference.Facts.Target.Path is not null
                && reference.Facts.Target.Resolution is SourceLinkTargetResolution.Missing or SourceLinkTargetResolution.FragmentMissing).ToArray();
            // Each Git fact is read once and reused: blame per source file, the name-status diff per commit,
            // and the old target bytes and target diff per commit and target path.
            var blames = new Dictionary<string, IReadOnlyDictionary<int, string>?>(StringComparer.Ordinal);
            var moves = new Dictionary<string, (int ExitCode, string Output)>(StringComparer.Ordinal);
            var targets = new Dictionary<(string Commit, string Path), (string? Moved, string? PreviousText, IReadOnlyList<SourceLinkDiffHunk> Changes)>();
            foreach (var reference in broken)
            {
                token.ThrowIfCancellationRequested();
                if (!blames.TryGetValue(reference.SourcePath, out var lineCommits))
                {
                    var lines = broken.Where(item => item.SourcePath == reference.SourcePath).Select(Line).Distinct().Order()
                        .SelectMany(line => new[] { "-L", $"{line},{line}" });
                    var blame = await Run(["blame", "--porcelain", .. lines, "--", reference.SourcePath]);
                    lineCommits = blame.ExitCode == 0 ? ReadBlame(blame.Output) : null;
                    blames.Add(reference.SourcePath, lineCommits);
                }
                if (lineCommits?.GetValueOrDefault(Line(reference)) is not { } commit
                    || commit.Length is not (40 or 64) || commit.All(character => character == '0')
                    || !commit.All(char.IsAsciiHexDigit)) continue;
                var oldPath = reference.Facts.Target.Path ?? throw new InvalidOperationException("History requires a target path.");
                if (!targets.TryGetValue((commit, oldPath), out var target))
                {
                    var old = await Run(["show", $"{commit}:{prefix}{oldPath}"]);
                    if (!moves.TryGetValue(commit, out var diff))
                    {
                        var read = await Run(["diff", "--relative", "--name-status", "-z", "--find-renames", commit, "HEAD", "--"]);
                        diff = (read.ExitCode, read.Output);
                        moves.Add(commit, diff);
                    }
                    var moved = diff.ExitCode == 0 ? ReadMove(diff.Output, oldPath) : null;
                    var targetDiff = await Run(["diff", "--no-ext-diff", "--no-textconv", "--unified=0", "--find-renames", commit, "--", oldPath, moved ?? oldPath]);
                    target = (moved, old.ExitCode == 0 ? old.Output : null,
                        targetDiff.ExitCode == 0 ? GitSourceLinkDiffReader.Read(targetDiff.Output) : []);
                    targets.Add((commit, oldPath), target);
                }
                entries.Add(new(reference.SourcePath, reference.Destination, commit, target.Moved, target.PreviousText)
                { TargetChanges = target.Changes });
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
            => await _deadline.RunAsync(deadline => _git.RunAsync(request.Workspace.PhysicalRoot, arguments, deadline), token)
                .ConfigureAwait(false) ?? (-1, string.Empty, string.Empty);

        static int Line(LocalReferenceObservation reference) => reference.Location?.Line ?? 1;
    }

    // Porcelain output starts each blamed line with "<commit> <original line> <final line>[ <group size>]".
    // Header and content lines never start with an object id, so only those lines are read.
    private static Dictionary<int, string> ReadBlame(string output)
    {
        var commits = new Dictionary<int, string>();
        foreach (var line in output.Split('\n'))
        {
            if (line.StartsWith('\t')) continue;
            var fields = line.TrimEnd('\r').Split(' ');
            if (fields.Length is < 3 or > 4 || fields[0].Length is not (40 or 64) || !fields[0].All(char.IsAsciiHexDigit)
                || !int.TryParse(fields[2], NumberStyles.None, CultureInfo.InvariantCulture, out var finalLine)) continue;
            commits[finalLine] = fields[0];
        }
        return commits;
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
