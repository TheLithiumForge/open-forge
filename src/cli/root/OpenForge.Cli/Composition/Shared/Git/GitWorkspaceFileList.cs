namespace OpenForge.Cli.Composition.Shared.Git;

/// <summary>
/// Git's own list of a work tree's files: tracked files plus untracked files Git does not ignore.
/// Route inspect and export both read it. Neither falls back here, so a caller decides what a
/// missing list means.
/// </summary>
internal static class GitWorkspaceFileList
{
    // Null when the root is not a Git work tree. False when it is one but Git reported a problem, so no list is read.
    internal static async ValueTask<bool?> ProbeAsync(GitProcessRunner git, string root, CancellationToken cancellationToken)
    {
        var probe = await git.RunAsync(root, ["rev-parse", "--is-inside-work-tree"], cancellationToken).ConfigureAwait(false);
        if (probe is not { ExitCode: 0 } confirmed || confirmed.Output.Trim() != "true")
        {
            return null;
        }

        return string.IsNullOrWhiteSpace(confirmed.Error);
    }

    // Null unless Git listed cleanly. checkpoint runs once per listed path so a caller can enforce its deadline.
    internal static async ValueTask<IReadOnlyList<string>?> ListAsync(
        GitProcessRunner git, string root, Action checkpoint, CancellationToken cancellationToken)
    {
        var inventory = await git.RunAsync(root,
            ["ls-files", "--cached", "--others", "--exclude-standard", "-z", "--", "."], cancellationToken).ConfigureAwait(false);
        if (inventory is not { ExitCode: 0 } files || !string.IsNullOrWhiteSpace(files.Error)
            || files.Output.Length != 0 && (files.Output[^1] != '\0' || files.Output[0] == '\0' || files.Output.Contains("\0\0", StringComparison.Ordinal)))
        {
            return null;
        }

        var paths = new List<string>();
        foreach (var range in files.Output.AsSpan().Split('\0'))
        {
            checkpoint();
            if (!files.Output.AsSpan(range).IsEmpty)
            {
                paths.Add(files.Output[range]);
            }
        }

        return paths;
    }
}
