using System.Text;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Locking;

internal static class WorkspaceLockPathIdentity
{
    internal const string ApplicationDirectoryName = "OpenForge";
    internal const string LocksDirectoryName = "locks";
    internal const string VersionDirectoryName = "v1";
    internal const string FileExtension = ".lock";
    internal const string FallbackFriendlyName = "workspace";
    internal const int MaximumFriendlyNameLength = 48;

    internal static string StoreDirectory(WorkspaceLockStoreRoot storeRoot)
    {
        return Path.Combine(
            storeRoot.Path,
            ApplicationDirectoryName,
            LocksDirectoryName,
            VersionDirectoryName);
    }

    internal static string LockPath(
        WorkspaceLockStoreRoot storeRoot,
        CliWorkspace workspace)
    {
        return Path.Combine(
            StoreDirectory(storeRoot),
            $"{FriendlyName(workspace.PhysicalRoot)}-{WorkspaceIdentity.Key(workspace.PhysicalRoot)}{FileExtension}");
    }

    internal static string FriendlyName(string workspacePhysicalPath)
    {
        var normalized = WorkspaceIdentity.NormalizePhysicalPath(workspacePhysicalPath);
        var directoryName = Path.GetFileName(normalized);
        var friendly = new StringBuilder(Math.Min(
            directoryName.Length,
            MaximumFriendlyNameLength));
        var separatorPending = false;
        foreach (var character in directoryName)
        {
            var lowered = char.ToLowerInvariant(character);
            if (lowered is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                var requiredLength = separatorPending && friendly.Length > 0 ? 2 : 1;
                if (friendly.Length + requiredLength > MaximumFriendlyNameLength)
                {
                    break;
                }

                if (separatorPending && friendly.Length > 0)
                {
                    friendly.Append('-');
                }

                friendly.Append(lowered);
                separatorPending = false;
            }
            else
            {
                separatorPending = friendly.Length > 0;
            }
        }

        while (friendly.Length > 0 && friendly[^1] == '-')
        {
            friendly.Length--;
        }

        return friendly.Length == 0 ? FallbackFriendlyName : friendly.ToString();
    }
}
