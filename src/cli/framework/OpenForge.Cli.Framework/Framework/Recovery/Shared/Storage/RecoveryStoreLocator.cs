using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Storage;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;

internal static class RecoveryStoreLocator
{
    internal static RecoveryStoreLocation Local(CliWorkspace workspace)
        => new(workspace, RecoveryStoreKind.Workspace, WorkspaceRuntimePaths.RecoveryRoot(workspace), legacyLockStoreRoot: null);

    internal static RecoveryStoreLocation? Legacy(CliWorkspace workspace)
    {
        try
        {
            var lockRoot = WorkspaceLockStoreRoot.ResolveForCurrentUser(Environment.SpecialFolderOption.DoNotVerify);
            return lockRoot is null
                ? null
                : new RecoveryStoreLocation(
                    workspace,
                    RecoveryStoreKind.Legacy,
                    Path.Combine(
                        lockRoot.Path,
                        RecoveryBundleFormatV1.ApplicationDirectoryName,
                        RecoveryBundleFormatV1.RecoveryDirectoryName,
                        RecoveryBundleFormatV1.VersionDirectoryName),
                    lockRoot);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return null;
        }
    }
}
