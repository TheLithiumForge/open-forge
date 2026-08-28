using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Framework.Recovery;

internal static class RecoveryBundlePathIdentity
{
    internal static string NormalizeWorkspacePath(string workspacePhysicalPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workspacePhysicalPath);
        if (!Path.IsPathFullyQualified(workspacePhysicalPath))
        {
            throw new ArgumentException(
                "A recovery workspace path must be fully qualified.",
                nameof(workspacePhysicalPath));
        }

        var normalized = Path.TrimEndingDirectorySeparator(Path.GetFullPath(workspacePhysicalPath));
        return string.IsNullOrEmpty(normalized)
            ? Path.GetPathRoot(Path.GetFullPath(workspacePhysicalPath))
                ?? throw new ArgumentException(
                    "A recovery workspace path requires a rooted identity.",
                    nameof(workspacePhysicalPath))
            : normalized;
    }

    internal static string WorkspaceKey(string workspacePhysicalPath)
    {
        var normalized = NormalizeWorkspacePath(workspacePhysicalPath);
        var identity = OperatingSystem.IsWindows()
            ? normalized.ToUpperInvariant()
            : normalized;
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
    }

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
        => Path.Combine(storeRoot, WorkspaceKey(workspacePhysicalPath));

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
