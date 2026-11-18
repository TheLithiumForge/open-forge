using OpenForge.Cli.Composition.Shared.Git;
using OpenForge.Cli.Composition.Shared.RouteInspectMatchingFiles;

namespace OpenForge.Cli.Composition.Shared.WorkspaceFiles;

/// <summary>
/// The one host helper that lists a workspace's files. Git's own list holds tracked files plus untracked files Git
/// does not ignore, and outside Git the workspace walk applies the .gitignore rules. Route inspect reads it through
/// its enumerator, and comment discovery and export read that enumerator's result. The shared GitOutput rules read
/// Git's answer.
/// </summary>
internal static class WorkspaceFileList
{
    // Comment discovery and export: Git's list, or the ignore-aware workspace walk outside Git, through route inspect's enumerator.
    internal static async ValueTask<IReadOnlyList<string>?> ReadAsync(string workspaceRoot, CancellationToken cancellationToken)
    {
        var files = await new RouteInspectGitFileEnumerator().EnumerateAsync(workspaceRoot, cancellationToken).ConfigureAwait(false);
        return files.Failure is null ? files.Paths : null;
    }

    // Null unless Git listed cleanly. checkpoint runs once per listed path so a caller can enforce its deadline.
    internal static async ValueTask<IReadOnlyList<string>?> ListAsync(
        GitProcessRunner git, string root, Action checkpoint, CancellationToken cancellationToken)
    {
        var inventory = await git.RunAsync(root,
            ["ls-files", "--cached", "--others", "--exclude-standard", "-z", "--", "."], cancellationToken).ConfigureAwait(false);
        if (inventory is not { ExitCode: 0 } files || GitOutput.IsUntrusted(files) || GitOutput.ReadNulEntries(files.Output) is not { } entries)
        {
            return null;
        }

        var paths = new List<string>();
        foreach (var entry in entries)
        {
            checkpoint();
            paths.Add(entry);
        }

        return paths;
    }
}
