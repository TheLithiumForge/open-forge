using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Mutation.Locking;

public sealed class WorkspaceLockContractIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Workspace lock leases require one exact zero-byte external handle"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public void WorkspaceLockLeaseRequiresExactExternalHandle()
    {
        using var workspaceDirectory = TemporaryWorkspace.Create("lock-lease-workspace");
        using var storeDirectory = TemporaryWorkspace.Create("lock-lease-store");
        using var otherStoreDirectory = TemporaryWorkspace.Create("lock-lease-other-store");
        var workspace = new CliWorkspace(
            workspaceDirectory.Path,
            workspaceDirectory.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var request = new WorkspaceLockRequest(workspace, "index", Guid.NewGuid());
        var storeRoot = WorkspaceLockStoreRoot.FromAbsolutePath(storeDirectory.Path);
        var otherStoreRoot = WorkspaceLockStoreRoot.FromAbsolutePath(otherStoreDirectory.Path);
        var lockPath = CreateLockFile(storeDirectory, storeRoot, workspace, []);
        var otherPath = CreateLockFile(otherStoreDirectory, otherStoreRoot, workspace, []);

        using (var handle = Open(lockPath))
        {
            Assert.Throws<ArgumentException>(() => new WorkspaceLockLease(
                request: request,
                storeRoot: otherStoreRoot,
                lockPath: lockPath,
                handle: handle));
        }

        using (var handle = Open(otherPath))
        {
            Assert.Throws<ArgumentException>(() => new WorkspaceLockLease(
                request: request,
                storeRoot: storeRoot,
                lockPath: lockPath,
                handle: handle));
        }

        var otherWorkspace = new CliWorkspace(
            Path.Combine(workspaceDirectory.Path, "other"),
            Path.Combine(workspaceDirectory.Path, "other"),
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var nonzeroPath = CreateLockFile(
            storeDirectory,
            storeRoot,
            otherWorkspace,
            "not-zero"u8.ToArray());
        using (var handle = Open(nonzeroPath))
        {
            Assert.Throws<ArgumentException>(() => new WorkspaceLockLease(
                request: new WorkspaceLockRequest(otherWorkspace, "index", Guid.NewGuid()),
                storeRoot: storeRoot,
                lockPath: nonzeroPath,
                handle: handle));
        }

        using var lease = new WorkspaceLockLease(
            request: request,
            storeRoot: storeRoot,
            lockPath: lockPath,
            handle: Open(lockPath));
        Assert.True(lease.IsHeldFor(workspace));
        Assert.False(lease.IsHeldFor(new CliWorkspace(
            Path.Combine(workspaceDirectory.Path, "foreign"),
            Path.Combine(workspaceDirectory.Path, "foreign"),
            CliWorkspaceSelectionMethod.ExplicitWorkspace)));
        lease.Dispose();
        Assert.False(lease.IsHeldFor(workspace));
    }

    private static string CreateLockFile(
        TemporaryWorkspace temporary,
        WorkspaceLockStoreRoot storeRoot,
        CliWorkspace workspace,
        byte[] contents)
    {
        var lockPath = WorkspaceLockPathIdentity.LockPath(storeRoot, workspace);
        var relativePath = Path.GetRelativePath(temporary.Path, lockPath);
        return temporary.CreateFile(relativePath, contents);
    }

    private static FileStream Open(string path)
        => new(
            path,
            new FileStreamOptions
            {
                Mode = FileMode.Open,
                Access = FileAccess.ReadWrite,
                Share = FileShare.None,
            });
}
