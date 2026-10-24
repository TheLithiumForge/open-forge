using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Recovery.Models.Storage;

internal sealed record RecoveryStoreLocation
{
    internal RecoveryStoreLocation(
        CliWorkspace workspace,
        RecoveryStoreKind kind,
        string rootPath,
        WorkspaceLockStoreRoot? legacyLockStoreRoot)
    {
        Workspace = workspace;
        Kind = kind;
        RootPath = WorkspaceIdentity.NormalizePhysicalPath(rootPath);
        WorkspaceDirectory = RecoveryBundlePathIdentity.WorkspaceDirectory(RootPath, workspace.PhysicalRoot);
        LegacyLockStoreRoot = legacyLockStoreRoot;
    }

    internal CliWorkspace Workspace { get; }
    internal RecoveryStoreKind Kind { get; }
    internal string RootPath { get; }
    internal string WorkspaceDirectory { get; }
    internal WorkspaceLockStoreRoot? LegacyLockStoreRoot { get; }
}
