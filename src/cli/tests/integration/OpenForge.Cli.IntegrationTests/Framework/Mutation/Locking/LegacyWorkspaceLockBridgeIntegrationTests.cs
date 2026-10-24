using System.Security.AccessControl;
using System.Security.Principal;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Shared.Legacy;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Mutation.Locking;

[Trait("Boundary", "OS"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
public sealed class LegacyWorkspaceLockBridgeIntegrationTests
{
    [Theory(DisplayName = "Legacy read-only and read-write handles exclude each other in both directions")]
    [InlineData(false), InlineData(true)]
    public void LegacyAccessDirectionsExcludeEachOther(bool readOnly)
    {
        using var temporary = TemporaryWorkspace.Create("legacy-access-directions");
        using var store = WorkspaceLockTestStore.Create("legacy-access-directions-store");
        var path = store.Track(Workspace(temporary));
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? throw new InvalidOperationException());
        File.WriteAllBytes(path, []);
        using var holder = new FileStream(path, FileMode.Open, readOnly ? FileAccess.Read : FileAccess.ReadWrite, FileShare.None);
        Assert.Throws<IOException>(() => new FileStream(
            path, FileMode.Open, readOnly ? FileAccess.ReadWrite : FileAccess.Read, FileShare.None));
    }

    [Theory(DisplayName = "Legacy readers and writers exclude composite acquisition and failure releases local ownership")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LegacyHolderBlocksCompositeAndReleasesLocal(bool readOnly)
    {
        using var temporary = TemporaryWorkspace.Create("bridge-holder");
        using var store = WorkspaceLockTestStore.Create("bridge-holder-store");
        var workspace = Workspace(temporary);
        var path = store.Track(workspace);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? throw new InvalidOperationException());
        File.WriteAllBytes(path, []);
        using (var holder = new FileStream(path, FileMode.Open, readOnly ? FileAccess.Read : FileAccess.ReadWrite, FileShare.None))
        {
            var result = await store.AcquireAsync(Request(workspace), TestContext.Current.CancellationToken);
            Assert.Equal(WorkspaceLockFailureKind.Busy, result.LockFailure?.Kind);
            Assert.Equal(path, result.LockFailure?.Path);
            AssertLocalReleased(workspace);
        }

        var acquired = await store.AcquireAsync(Request(workspace), TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        Assert.Throws<IOException>(() => new FileStream(path, FileMode.Open, readOnly ? FileAccess.Read : FileAccess.ReadWrite, FileShare.None));
    }

    [Theory(DisplayName = "Invalid legacy targets preserve evidence and release local ownership")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InvalidLegacyTargetReleasesLocal(bool directory)
    {
        using var temporary = TemporaryWorkspace.Create("bridge-invalid");
        using var store = WorkspaceLockTestStore.Create("bridge-invalid-store");
        var workspace = Workspace(temporary);
        var path = store.Track(workspace);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? throw new InvalidOperationException());
        if (directory)
        {
            Directory.CreateDirectory(path);
        }
        else
        {
            File.WriteAllText(path, "legacy-content");
        }

        var result = await store.AcquireAsync(Request(workspace), TestContext.Current.CancellationToken);
        Assert.Equal(WorkspaceLockFailureKind.InvalidTarget, result.LockFailure?.Kind);
        Assert.Equal(path, result.LockFailure?.Path);
        AssertLocalReleased(workspace);
        if (!directory)
        {
            Assert.Equal("legacy-content", File.ReadAllText(path));
        }
    }

    [Fact(DisplayName = "Write-denied existing legacy files are held read-only and still exclude writers")]
    public async Task DeniedExistingLegacyIsHeldReadOnly()
    {
        using var temporary = TemporaryWorkspace.Create("bridge-readonly");
        using var store = WorkspaceLockTestStore.Create("bridge-readonly-store");
        var workspace = Workspace(temporary);
        var path = store.Track(workspace);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? throw new InvalidOperationException());
        File.WriteAllBytes(path, []);
        var attributes = File.GetAttributes(path);
        UnixFileMode mode = default;
        if (OperatingSystem.IsWindows())
        {
            File.SetAttributes(path, attributes | FileAttributes.ReadOnly);
        }
        else
        {
            mode = File.GetUnixFileMode(path);
            File.SetUnixFileMode(path, UnixFileMode.UserRead);
        }

        try
        {
            if (!OperatingSystem.IsWindows())
            {
                try
                {
                    using var probe = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    Assert.Fail("This host bypasses Unix write permissions.");
                }
                catch (UnauthorizedAccessException)
                {
                }
            }

            var result = await store.AcquireAsync(Request(workspace), TestContext.Current.CancellationToken);
            await using var lease = Assert.IsType<WorkspaceLockLease>(result.Lease);
            Assert.Equal(LegacyWorkspaceLockState.HeldReadOnly, lease.LegacyState);
            Assert.True(lease.HoldsLegacyLock(store.StoreRoot));
            var contended = LegacyWorkspaceLockBridge.Acquire(Request(workspace), store.StoreRoot, TestContext.Current.CancellationToken);
            Assert.Equal(WorkspaceLockFailureKind.Busy, contended.Failure?.Kind);
        }
        finally
        {
            if (OperatingSystem.IsWindows())
            {
                File.SetAttributes(path, attributes);
            }
            else
            {
                File.SetUnixFileMode(path, mode);
            }
        }
    }

    [Fact(DisplayName = "Denied creation with observed legacy absence permits local ownership")]
    public async Task DeniedAbsentLegacyIsSkipped()
    {
        using var temporary = TemporaryWorkspace.Create("bridge-absent");
        using var store = WorkspaceLockTestStore.Create("bridge-absent-store");
        var workspace = Workspace(temporary);
        var path = store.Track(workspace);
        var directory = new DirectoryInfo(Path.GetDirectoryName(path) ?? throw new InvalidOperationException());
        directory.Create();
        DirectorySecurity? original = null;
        UnixFileMode mode = default;
        if (OperatingSystem.IsWindows())
        {
            original = directory.GetAccessControl();
            var denied = directory.GetAccessControl();
            using var identity = WindowsIdentity.GetCurrent();
            denied.AddAccessRule(new FileSystemAccessRule(identity.User ?? throw new InvalidOperationException(), FileSystemRights.CreateFiles, AccessControlType.Deny));
            directory.SetAccessControl(denied);
        }
        else
        {
            mode = File.GetUnixFileMode(directory.FullName);
            File.SetUnixFileMode(directory.FullName, UnixFileMode.UserRead | UnixFileMode.UserExecute);
        }

        try
        {
            var result = await store.AcquireAsync(Request(workspace), TestContext.Current.CancellationToken);
            await using var lease = Assert.IsType<WorkspaceLockLease>(result.Lease);
            if (!OperatingSystem.IsWindows() && lease.LegacyState == LegacyWorkspaceLockState.HeldReadWrite)
            {
                Assert.Fail("This host bypasses Unix directory write permissions.");
            }

            Assert.Equal(LegacyWorkspaceLockState.SkippedAbsent, lease.LegacyState);
            Assert.False(lease.HoldsLegacyLock(store.StoreRoot));
            Assert.False(File.Exists(path));
        }
        finally
        {
            if (OperatingSystem.IsWindows() && original is not null)
            {
                directory.SetAccessControl(original);
            }
            else if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(directory.FullName, mode);
            }
        }
    }

    [Fact(DisplayName = "Cancelled bridge acquisition carries its path and creates no legacy files")]
    public void CancelledBridgeCreatesNothing()
    {
        using var temporary = TemporaryWorkspace.Create("bridge-cancelled");
        using var store = WorkspaceLockTestStore.Create("bridge-cancelled-store");
        var workspace = Workspace(temporary);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var result = LegacyWorkspaceLockBridge.Acquire(Request(workspace), store.StoreRoot, cancellation.Token);
        Assert.Equal(LegacyWorkspaceLockState.Failed, result.State);
        Assert.Equal(WorkspaceLockFailureKind.Cancelled, result.Failure?.Kind);
        Assert.False(store.InfrastructureExists);
        Assert.Null(result.Handle);
    }

    private static void AssertLocalReleased(CliWorkspace workspace)
    {
        using var handle = new FileStream(WorkspaceRuntimePaths.LockPath(workspace), FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Assert.True(handle.CanWrite);
    }

    private static CliWorkspace Workspace(TemporaryWorkspace temporary)
        => new(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);

    private static WorkspaceLockRequest Request(CliWorkspace workspace)
        => new(workspace, IndexDefinitions.CommandIdentity, Guid.NewGuid());
}
