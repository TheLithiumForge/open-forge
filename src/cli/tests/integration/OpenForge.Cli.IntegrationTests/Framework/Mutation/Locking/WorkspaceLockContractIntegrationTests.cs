using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Mutation.Locking;

public sealed class WorkspaceLockContractIntegrationTests
{
    private const string PreservedContent = "preserved";
    private const string OtherLockFile = "other.lock";

    [Fact(DisplayName = "Workspace lock leases require the exact local handle and accept nonempty sentinels")]
    [Trait("Boundary", "OS"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public void WorkspaceLockLeaseRequiresExactLocalHandle()
    {
        using var temporary = TemporaryWorkspace.Create("lock-lease-workspace");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var request = new WorkspaceLockRequest(workspace, IndexDefinitions.CommandIdentity, Guid.NewGuid());
        var path = temporary.CreateFile(Path.GetRelativePath(temporary.Path, WorkspaceRuntimePaths.LockPath(workspace)), PreservedContent);
        var otherPath = temporary.CreateFile(OtherLockFile);
        var legacy = new LegacyWorkspaceLockResult(LegacyWorkspaceLockState.Unavailable);
        using (var handle = Open(otherPath))
        {
            Assert.Throws<ArgumentException>(() => new WorkspaceLockLease(request, handle, legacy));
        }

        using (var handle = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            Assert.Throws<ArgumentException>(() => new WorkspaceLockLease(request, handle, legacy));
        }

        using var lease = new WorkspaceLockLease(request, Open(path), legacy);
        Assert.True(lease.IsHeldFor(workspace));
        Assert.Equal(WorkspaceRuntimePaths.LockPath(workspace), lease.LockPath);
        Assert.False(lease.IsHeldFor(new CliWorkspace(otherPath, otherPath, CliWorkspaceSelectionMethod.ExplicitWorkspace)));
        lease.Dispose();
        Assert.False(lease.IsHeldFor(workspace));
        Assert.Equal(PreservedContent, File.ReadAllText(path));
    }

    [Fact(DisplayName = "A composite lease loses authority when either owned handle closes")]
    [Trait("Boundary", "OS"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task ClosedLegacyHandleInvalidatesCompositeAuthority()
    {
        using var temporary = TemporaryWorkspace.Create("lock-composite-authority");
        using var store = WorkspaceLockTestStore.Create("lock-composite-store");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var request = new WorkspaceLockRequest(workspace, IndexDefinitions.CommandIdentity, Guid.NewGuid());
        var legacyPath = store.Track(workspace);
        Directory.CreateDirectory(Path.GetDirectoryName(legacyPath) ?? throw new InvalidOperationException());
        using var legacyHandle = new FileStream(legacyPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        var legacy = new LegacyWorkspaceLockResult(LegacyWorkspaceLockState.HeldReadWrite, store.StoreRoot, legacyPath, legacyHandle);
        var localPath = temporary.CreateFile(Path.GetRelativePath(temporary.Path, WorkspaceRuntimePaths.LockPath(workspace)));
        await using var lease = new WorkspaceLockLease(request, Open(localPath), legacy);
        Assert.True(lease.HoldsLegacyLock(store.StoreRoot));
        legacyHandle.Dispose();
        Assert.False(lease.IsHeld);
        Assert.False(lease.HoldsLegacyLock(store.StoreRoot));
        await lease.DisposeAsync();
        using var reopened = Open(localPath);
    }

    private static FileStream Open(string path)
        => new(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
}
