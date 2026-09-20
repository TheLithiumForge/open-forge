using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;

internal static class RecoveryBundlePathIdentity
{
    internal static string? ResolveStoreRoot(Environment.SpecialFolderOption option)
    {
        var dataHome = CurrentUserDataHome.Resolve(option);
        if (dataHome is null)
        {
            return null;
        }

        return Path.Combine(
            dataHome,
            RecoveryBundleFormatV1.ApplicationDirectoryName,
            RecoveryBundleFormatV1.RecoveryDirectoryName,
            RecoveryBundleFormatV1.VersionDirectoryName);
    }

    internal static string WorkspaceDirectory(
        string storeRoot,
        string workspacePhysicalPath)
        => Path.Combine(storeRoot, WorkspaceIdentity.Key(workspacePhysicalPath));

    internal static string FinalPath(
        string storeRoot,
        string workspacePhysicalPath,
        Guid operationId)
        => Path.Combine(
            WorkspaceDirectory(storeRoot, workspacePhysicalPath),
            RecoveryBundleFormatV1.FinalFileName(operationId));

    internal static string DraftPath(
        string storeRoot,
        string workspacePhysicalPath,
        Guid operationId)
        => Path.Combine(
            WorkspaceDirectory(storeRoot, workspacePhysicalPath),
            RecoveryBundleFormatV1.DraftFileName(operationId));
}
