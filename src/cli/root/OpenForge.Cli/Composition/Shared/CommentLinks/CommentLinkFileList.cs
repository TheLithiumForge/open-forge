using OpenForge.Cli.Composition.Shared.RouteInspectMatchingFiles;

namespace OpenForge.Cli.Composition.Shared.CommentLinks;

/// <summary>
/// Comment discovery reuses the Route Inspect workspace enumeration. It lists Git's tracked and untracked files without
/// ignored ones, and walks the workspace with its .gitignore rules outside Git.
/// </summary>
internal static class CommentLinkFileList
{
    internal static async ValueTask<IReadOnlyList<string>?> ReadAsync(string workspaceRoot, CancellationToken cancellationToken)
    {
        var files = await new RouteInspectGitFileEnumerator().EnumerateAsync(workspaceRoot, cancellationToken).ConfigureAwait(false);
        return files.Failure is null ? files.Paths : null;
    }
}
