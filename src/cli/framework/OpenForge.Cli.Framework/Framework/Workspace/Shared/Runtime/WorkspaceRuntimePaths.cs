using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

internal static class WorkspaceRuntimePaths
{
    internal const string DirectoryName = ".open-forge-state";

    internal static string DirectoryPath(CliWorkspace workspace)
        => Path.Combine(workspace.PhysicalRoot, DirectoryName);

    internal static string LockPath(CliWorkspace workspace)
        => Path.Combine(DirectoryPath(workspace), "workspace.lock");

    internal static string IgnorePath(CliWorkspace workspace)
        => Path.Combine(DirectoryPath(workspace), ".gitignore");

    internal static string RecoveryRoot(CliWorkspace workspace)
        => Path.Combine(DirectoryPath(workspace), "recovery", "v1");

    internal static bool Contains(CliWorkspace workspace, string absolutePath)
    {
        var candidate = WorkspaceIdentity.NormalizePhysicalPath(absolutePath);
        var root = WorkspaceIdentity.NormalizePhysicalPath(workspace.PhysicalRoot);
        var prefix = Path.EndsInDirectorySeparator(root) ? root : root + Path.DirectorySeparatorChar;
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        if (!candidate.StartsWith(prefix, comparison)) return false;
        var relative = candidate[prefix.Length..];
        return string.Equals(relative, DirectoryName, StringComparison.OrdinalIgnoreCase)
            || relative.StartsWith(DirectoryName + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }
}
