using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Shared.Interop;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Shared.Runtime;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.IntegrationTests.Framework.Mutation.Locking.Shared.Processes;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.IntegrationTests.TestSupport.Processes;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Mutation.Locking;

public sealed partial class UnixWorkspaceLockIntegrationTests
{
    [Theory(DisplayName = "A write-denied legacy FIFO returns promptly and releases the local lock")]
    [InlineData(Linux), InlineData(MacOS)]
    public async Task ReadOnlyFifoCannotHoldTheWorkspace(string platform)
    {
        RequirePlatform(platform);
        using var temporary = TemporaryWorkspace.Create("legacy-read-only-fifo");
        using var store = WorkspaceLockTestStore.Create("legacy-read-only-fifo-store");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var path = store.Track(workspace);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await CreateWriteDeniedFifoAsync(path);

        var lines = await IntegrationProbeProcess.RunAsync([WorkspaceLockProbe.ChildArgument, temporary.Path, store.StoreRoot.Path, WorkspaceLockProbe.FifoMode, path]);

        Assert.Equal([nameof(WorkspaceLockFailureKind.InvalidTarget), path, WorkspaceLockProbe.LocalReleased], lines);
        File.Delete(path);
        var acquired = await store.AcquireAsync(new(workspace, IndexDefinitions.CommandIdentity, Guid.NewGuid()), TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
    }

    [Theory(DisplayName = "Cancellation on the read-only FIFO path returns no handle or workspace lease")]
    [InlineData(Linux), InlineData(MacOS)]
    public async Task ReadOnlyFifoHonorsCancellation(string platform)
    {
        RequirePlatform(platform);
        using var temporary = TemporaryWorkspace.Create("legacy-fifo-cancellation");
        using var store = WorkspaceLockTestStore.Create("legacy-fifo-cancellation-store");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var path = store.Track(workspace);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await CreateWriteDeniedFifoAsync(path);

        var lines = await IntegrationProbeProcess.RunAsync([WorkspaceLockProbe.ChildArgument, temporary.Path, store.StoreRoot.Path, WorkspaceLockProbe.CancelFallbackMode, path]);

        Assert.Equal([nameof(WorkspaceLockFailureKind.Cancelled), WorkspaceLockProbe.NoHandle, nameof(WorkspaceLockFailureKind.Cancelled)], lines);
        Assert.False(Directory.Exists(WorkspaceRuntimePaths.DirectoryPath(workspace)));
    }

    [Theory(DisplayName = "Manager contention is detected when implicit runtime file locking is disabled")]
    [InlineData(Linux, false), InlineData(Linux, true)]
    [InlineData(MacOS, false), InlineData(MacOS, true)]
    public async Task ManagerRequiresCheckedFlock(string platform, bool legacy)
    {
        RequirePlatform(platform);
        using var temporary = TemporaryWorkspace.Create("explicit-flock-manager");
        using var store = WorkspaceLockTestStore.Create("explicit-flock-manager-store");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        Assert.Null(WorkspaceRuntimeBootstrap.EnsureCreated(workspace, TestContext.Current.CancellationToken));
        var path = legacy ? store.Track(workspace) : WorkspaceRuntimePaths.LockPath(workspace);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, []);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.CancelAfter(TimeSpan.FromSeconds(30));
        using var holder = UnixLockWitness.Hold(path);
        try
        {
            Assert.Equal(UnixLockWitness.Held, await holder.StandardOutput.ReadLineAsync(cancellation.Token));
            var lines = await IntegrationProbeProcess.RunAsync(
                [WorkspaceLockProbe.ChildArgument, temporary.Path, store.StoreRoot.Path, WorkspaceLockProbe.ImplicitDisabledMode, path], disableImplicitLocking: true);

            Assert.Equal([WorkspaceLockProbe.ManagedOpened, nameof(WorkspaceLockFailureKind.Busy), path, (platform == MacOS ? 35 : 11).ToString(System.Globalization.CultureInfo.InvariantCulture)], lines);
            await holder.StandardInput.WriteLineAsync();
            await holder.WaitForExitAsync(cancellation.Token);
            Assert.Equal(0, holder.ExitCode);
            var acquired = await store.AcquireAsync(new(workspace, IndexDefinitions.CommandIdentity, Guid.NewGuid()), cancellation.Token);
            await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        }
        finally
        {
            Stop(holder);
        }
    }

    [Theory(DisplayName = "Native read-only open distinguishes absence and permission denial")]
    [InlineData(Linux), InlineData(MacOS)]
    public void NativeOpenPreservesAbsenceAndPermission(string platform)
    {
        RequirePlatform(platform);
        using var temporary = TemporaryWorkspace.Create("legacy-native-open-errors");
        Assert.Throws<FileNotFoundException>(() => UnixWorkspaceLock.TryOpenReadOnly(
            temporary.Combine("missing"), TestContext.Current.CancellationToken, out _));
        var path = temporary.CreateFile("denied");
        if (OperatingSystem.IsWindows()) return;
        File.SetUnixFileMode(path, UnixFileMode.None);
        try
        {
            var failure = UnixWorkspaceLock.TryOpenReadOnly(path, TestContext.Current.CancellationToken, out var handle);
            using (handle)
            {
                if (handle is not null) Assert.Fail("This process bypasses Unix read permissions.");
                Assert.Equal(WorkspaceLockFailureKind.AccessDenied, failure?.Kind);
                Assert.Equal(13, failure?.NativeError);
            }
        }
        finally
        {
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }

    private static async Task CreateWriteDeniedFifoAsync(string path)
    {
        using var create = UnixLockWitness.CreateWriteDeniedFifo(path);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.CancelAfter(TimeSpan.FromSeconds(15));
        var permission = await create.StandardOutput.ReadLineAsync(cancellation.Token);
        await create.WaitForExitAsync(cancellation.Token);
        Assert.Equal(0, create.ExitCode);
        if (permission == UnixLockWitness.Writable) Assert.Fail("This process bypasses Unix write permissions, so a read-only fallback cannot be qualified.");
        Assert.Equal(UnixLockWitness.Denied, permission);
    }
}
