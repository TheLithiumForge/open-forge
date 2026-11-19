using OpenForge.Cli.Core.Commands.Stripping.Models.Application;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.Core.Commands.Stripping.Shared.Application;

/// <summary>
/// The uninstall plan's last step. The runtime folder holds the lease's own lock file, so it goes
/// once the lease is released, and only when it holds nothing but that file, its ignore file and empty folders.
/// </summary>
internal static class StripRuntimeFolder
{
    internal static string Path => WorkspaceRuntimePaths.DirectoryName;

    internal static StripKeptReason KeptReason(StripRuntimeFolderRemoval removal)
        => removal.State switch
        {
            StripRuntimeFolderState.HoldsRecoveryData => StripKeptReason.RecoveryData,
            StripRuntimeFolderState.Link => StripKeptReason.Link,
            StripRuntimeFolderState.Failed => StripKeptReason.Unreadable,
            _ => throw new ArgumentOutOfRangeException(nameof(removal), removal.State, "Only a retained runtime folder has a kept reason."),
        };

    // The runtime-retained message names the real reason the folder stays.
    internal static string Retained(StripRuntimeFolderRemoval removal)
        => removal.State switch
        {
            StripRuntimeFolderState.HoldsRecoveryData => $"{Path} stays because it holds recovery data.",
            StripRuntimeFolderState.Link => $"{Path} stays because it is a link or holds one.",
            StripRuntimeFolderState.Failed => $"{Path} stays because it could not be read or removed: {removal.Cause}",
            _ => throw new ArgumentOutOfRangeException(nameof(removal), removal.State, "Only a retained runtime folder has a reason to stay."),
        };

    internal static StripRuntimeFolderRemoval Classify(CliWorkspace workspace)
    {
        try
        {
            return Read(workspace).Removal;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return new(StripRuntimeFolderState.Failed, exception.Message);
        }
    }

    internal static StripRuntimeFolderRemoval TryRemove(CliWorkspace workspace)
    {
        try
        {
            var (removal, root, entries) = Read(workspace);
            if (removal.State != StripRuntimeFolderState.Removable || root is null)
            {
                return removal;
            }

            foreach (var file in entries.Where(entry => entry is not DirectoryInfo))
            {
                file.Delete();
            }

            foreach (var directory in entries.OfType<DirectoryInfo>().OrderByDescending(directory => directory.FullName.Length))
            {
                directory.Delete();
            }

            root.Delete();
            return Directory.Exists(root.FullName)
                ? new(StripRuntimeFolderState.Failed, "It still exists after removal.")
                : new(StripRuntimeFolderState.Removable);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return new(StripRuntimeFolderState.Failed, exception.Message);
        }
    }

    private static (StripRuntimeFolderRemoval Removal, DirectoryInfo? Root, FileSystemInfo[] Entries) Read(CliWorkspace workspace)
    {
        var root = new DirectoryInfo(WorkspaceRuntimePaths.DirectoryPath(workspace));
        if (!root.Exists)
        {
            return (new(StripRuntimeFolderState.Absent), null, []);
        }

        if (root.Attributes.HasFlag(FileAttributes.ReparsePoint))
        {
            return (new(StripRuntimeFolderState.Link), root, []);
        }

        var allowed = new HashSet<string>(
            [System.IO.Path.GetFullPath(WorkspaceRuntimePaths.LockPath(workspace)), System.IO.Path.GetFullPath(WorkspaceRuntimePaths.IgnorePath(workspace))],
            OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        var entries = root.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).ToArray();
        if (entries.Any(entry => entry.Attributes.HasFlag(FileAttributes.ReparsePoint)))
        {
            return (new(StripRuntimeFolderState.Link), root, entries);
        }

        return entries.Any(entry => entry is not DirectoryInfo && !allowed.Contains(System.IO.Path.GetFullPath(entry.FullName)))
            ? (new(StripRuntimeFolderState.HoldsRecoveryData), root, entries)
            : (new(StripRuntimeFolderState.Removable), root, entries);
    }
}
