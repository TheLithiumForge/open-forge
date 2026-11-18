using OpenForge.Cli.Composition.Shared.Git.Models;

namespace OpenForge.Cli.Composition.Shared.Git;

/// <summary>Interprets <c>git rev-parse --is-inside-work-tree</c>, alone or followed by further rev-parse arguments.</summary>
internal static class GitWorkTreeCheck
{
    internal static readonly string[] Arguments = ["rev-parse", "--is-inside-work-tree"];

    internal static GitWorkTreeProbe Read((int ExitCode, string Output, string Error)? result)
    {
        if (result is not { } read || GitOutput.IsUntrusted(read)) return new(GitWorkTreeState.Unavailable, result?.ExitCode ?? -1, []);
        var lines = read.Output.Split('\n').Select(line => line.TrimEnd('\r')).ToArray();
        return new(lines[0].Trim() == "true" ? GitWorkTreeState.Inside : GitWorkTreeState.Outside, read.ExitCode, lines);
    }
}
