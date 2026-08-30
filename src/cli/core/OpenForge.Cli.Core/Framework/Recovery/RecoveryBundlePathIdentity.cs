using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Recovery;

internal static class RecoveryBundlePathIdentity
{
    internal static string? ResolveStoreRoot(Environment.SpecialFolderOption option)
    {
        var localApplicationData = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData,
            option);
        if (string.IsNullOrWhiteSpace(localApplicationData)
            || !Path.IsPathFullyQualified(localApplicationData))
        {
            return null;
        }

        return Path.Combine(
            Path.GetFullPath(localApplicationData),
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
