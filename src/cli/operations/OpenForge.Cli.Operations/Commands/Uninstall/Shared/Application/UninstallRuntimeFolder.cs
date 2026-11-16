using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.Core.Commands.Uninstall.Shared.Application;

/// <summary>
/// Removes the runtime folder after a verified uninstall has released its lease, but only
/// when it holds nothing except the lock file, its ignore file and empty folders.
/// </summary>
internal static class UninstallRuntimeFolder
{
    internal static bool TryRemove(CliWorkspace workspace)
    {
        var root = new DirectoryInfo(WorkspaceRuntimePaths.DirectoryPath(workspace));
        if (!root.Exists)
        {
            return true;
        }

        if (root.Attributes.HasFlag(FileAttributes.ReparsePoint))
        {
            return false;
        }

        var allowed = new HashSet<string>(
            [Path.GetFullPath(WorkspaceRuntimePaths.LockPath(workspace)), Path.GetFullPath(WorkspaceRuntimePaths.IgnorePath(workspace))],
            OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        var entries = root.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).ToArray();
        if (entries.Any(entry => entry.Attributes.HasFlag(FileAttributes.ReparsePoint)
            || entry is not DirectoryInfo && !allowed.Contains(Path.GetFullPath(entry.FullName))))
        {
            return false;
        }

        try
        {
            foreach (var file in entries.Where(entry => entry is not DirectoryInfo))
            {
                file.Delete();
            }

            foreach (var directory in entries.OfType<DirectoryInfo>().OrderByDescending(directory => directory.FullName.Length))
            {
                directory.Delete();
            }

            root.Delete();
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }
}
