using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Repair;

[Trait("Boundary", "OS"), Trait("Feature", "repair"), Trait("Evidence", "Integration")]
public sealed class RepairFixtureIsolationIntegrationTests
{
    [Fact(DisplayName = "Repair fixture cleanup preserves another workspace's pending lock acquisition")]
    public async Task CleanupPreservesPendingLockAcquisition()
    {
        using var locks = WorkspaceLockTestStore.Create("repair-shared-lock-cleanup");
        using var repair = RepairIntegrationWorkspace.Create("repair-lock-cleanup", lockStoreRoot: locks.StoreRoot);
        using var other = TemporaryWorkspace.Create("repair-concurrent-workspace");
        var otherWorkspace = new CliWorkspace(other.Path, other.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var request = new WorkspaceLockRequest(repair.Workspace, "repair", Guid.NewGuid());
        var acquired = await locks.AcquireAsync(request, TestContext.Current.CancellationToken);
        var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        var repairLockPath = lease.LockPath;
        await lease.DisposeAsync();

        // Pause the other acquisition between creating its shared parent and opening its file.
        var otherLockPath = locks.Track(otherWorkspace);
        Directory.CreateDirectory(WorkspaceLockPathIdentity.StoreDirectory(locks.StoreRoot));
        repair.Dispose();

        using (var pending = new FileStream(otherLockPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
        {
            Assert.Equal(0, pending.Length);
        }
        Assert.False(File.Exists(repairLockPath));
        Assert.True(File.Exists(otherLockPath));
    }
}
