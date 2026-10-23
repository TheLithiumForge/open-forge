using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

internal static class WorkspaceRuntimePaths
{
    private const string DirectoryName = ".open-forge-state";

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
        var directory = WorkspaceIdentity.NormalizePhysicalPath(DirectoryPath(workspace));
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        return string.Equals(candidate, directory, comparison)
            || candidate.StartsWith(directory + Path.DirectorySeparatorChar, comparison);
    }
}
