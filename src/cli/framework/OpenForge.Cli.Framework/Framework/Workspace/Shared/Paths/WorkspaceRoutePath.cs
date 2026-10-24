using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;

namespace OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;

internal static class WorkspaceRoutePath
{
    internal const string MarkdownExtension = ".md";

    internal static string CanonicalEntrypointFileName(string directoryName)
        => $"_{directoryName}{MarkdownExtension}";

    internal static string CanonicalEntrypointPath(string directoryPath)
    {
        if (!PortableWorkspacePath.TryNormalize(directoryPath, out _))
            throw new ArgumentException("A route directory must be canonical and relative.", nameof(directoryPath));
        var name = directoryPath[(directoryPath.LastIndexOf('/') + 1)..];
        return directoryPath + "/" + CanonicalEntrypointFileName(name);
    }
}
