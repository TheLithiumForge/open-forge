using System.Diagnostics;
using System.Net.Sockets;
using Microsoft.Win32.SafeHandles;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Shared.Interop;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Shared.Runtime;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.IntegrationTests.Framework.Mutation.Locking.Shared.Processes;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Mutation.Locking;

[Trait("Boundary", "OS"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
public sealed partial class UnixWorkspaceLockIntegrationTests
{
    private const string Linux = "linux";
    private const string MacOS = "macos";
    private const string FifoTarget = "fifo";
    private const string IgnoreFifoTarget = "ignore-fifo";
    private const string SocketTarget = "socket";
    private const string LinkTarget = "link";
    private const string DeviceTarget = "device";
    private const string LockFile = "lock";
    private const string CancelledDescriptor = "cancelled-descriptor";

    [Theory(DisplayName = "Unix constructor contention is Busy for local and legacy lock files")]
    [InlineData(Linux, false), InlineData(Linux, true)]
    [InlineData(MacOS, false), InlineData(MacOS, true)]
    public async Task ConstructorContentionIsBusy(string platform, bool legacy)
    {
        RequirePlatform(platform);
        using var temporary = TemporaryWorkspace.Create("constructor-contention");
        using var store = WorkspaceLockTestStore.Create("constructor-contention-store");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        Assert.Null(WorkspaceRuntimeBootstrap.EnsureCreated(workspace, TestContext.Current.CancellationToken));
        var path = legacy ? store.Track(workspace) : WorkspaceRuntimePaths.LockPath(workspace);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, []);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.CancelAfter(TimeSpan.FromSeconds(15));
        using var holder = UnixLockWitness.Hold(path);
        try
        {
            Assert.Equal(UnixLockWitness.Held, await holder.StandardOutput.ReadLineAsync(cancellation.Token));
            var exception = Assert.Throws<IOException>(() =>
            {
                using var handle = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            });
            Assert.Equal(platform == MacOS ? 35 : 11, exception.HResult);
            var result = await store.AcquireAsync(new(workspace, IndexDefinitions.CommandIdentity, Guid.NewGuid()), cancellation.Token);
            Assert.Null(result.Lease);
            Assert.Equal(WorkspaceLockFailureKind.Busy, result.LockFailure?.Kind);
            Assert.Equal(path, result.LockFailure?.Path);
            Assert.Equal(exception.HResult, result.LockFailure?.NativeError);

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

    [Theory(DisplayName = "Platform flock imports acquire a valid handle without assembly resolver initialization")]
    [InlineData(Linux), InlineData(MacOS)]
    public void PlatformImportAcquiresWithoutTypeInitialization(string platform)
    {
        RequirePlatform(platform);
        using var temporary = TemporaryWorkspace.Create("direct-flock-import");
        var path = temporary.CreateFile(LockFile);
        using var handle = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

        Assert.Null(typeof(UnixWorkspaceLock).TypeInitializer);
        Assert.Null(UnixWorkspaceLock.TryAcquire(handle.SafeFileHandle, path, TestContext.Current.CancellationToken));
    }

    [Theory(DisplayName = "Checked flock excludes another process in both acquisition directions")]
    [InlineData(Linux, false)]
    [InlineData(Linux, true)]
    [InlineData(MacOS, false)]
    [InlineData(MacOS, true)]
    public async Task CheckedFlockExcludesOtherProcess(string platform, bool readOnly)
    {
        RequirePlatform(platform);
        using var temporary = TemporaryWorkspace.Create("flock");
        var path = temporary.CreateFile(LockFile);
        // A FileStream would take an implicit shared flock, which would stop the holder below from locking.
        // The native descriptor carries no lock until the checked call takes one.
        Assert.Null(UnixWorkspaceLock.TryOpenReadOnly(path, TestContext.Current.CancellationToken, out var native));
        using var handle = Assert.IsType<SafeFileHandle>(native);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.CancelAfter(TimeSpan.FromSeconds(15));
        using var holder = UnixLockWitness.Hold(path, readOnly);
        try
        {
            Assert.Equal(UnixLockWitness.Held, await holder.StandardOutput.ReadLineAsync(cancellation.Token));
            var busy = UnixWorkspaceLock.TryAcquire(handle, path, cancellation.Token);
            Assert.Equal(WorkspaceLockFailureKind.Busy, busy?.Kind);
            Assert.Equal(platform == MacOS ? 35 : 11, busy?.NativeError);
            await holder.StandardInput.WriteLineAsync();
            await holder.WaitForExitAsync(cancellation.Token);
            Assert.Equal(0, holder.ExitCode);
            Assert.Null(UnixWorkspaceLock.TryAcquire(handle, path, cancellation.Token));
            using var contender = UnixLockWitness.Probe(path, readOnly);
            try
            {
                Assert.Equal(UnixLockWitness.Busy, await contender.StandardOutput.ReadLineAsync(cancellation.Token));
                await contender.WaitForExitAsync(cancellation.Token);
                Assert.Equal(0, contender.ExitCode);
            }
            finally
            {
                Stop(contender);
            }
        }
        finally
        {
            Stop(holder);
        }
    }

    [Theory(DisplayName = "Checked flock preserves failed errno and observes cancellation without granting ownership")]
    [InlineData(Linux)]
    [InlineData(MacOS)]
    public void CheckedFlockRejectsBadDescriptorAndCancellation(string platform)
    {
        RequirePlatform(platform);
        using var handle = new SafeFileHandle((nint)int.MaxValue, ownsHandle: false);
        var failure = UnixWorkspaceLock.TryAcquire(handle, "invalid-descriptor", TestContext.Current.CancellationToken);
        Assert.Equal(WorkspaceLockFailureKind.IoError, failure?.Kind);
        Assert.Equal(9, failure?.NativeError);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var cancelled = UnixWorkspaceLock.TryAcquire(handle, CancelledDescriptor, cancellation.Token);
        Assert.Equal(WorkspaceLockFailureKind.Cancelled, cancelled?.Kind);
        Assert.Equal(CancelledDescriptor, cancelled?.Path);
    }

    [Theory(DisplayName = "Unix lock and ignore special-file targets are refused and preserved")]
    [InlineData(Linux, FifoTarget)]
    [InlineData(Linux, IgnoreFifoTarget)]
    [InlineData(Linux, SocketTarget)]
    [InlineData(Linux, LinkTarget)]
    [InlineData(Linux, DeviceTarget)]
    [InlineData(MacOS, FifoTarget)]
    [InlineData(MacOS, IgnoreFifoTarget)]
    [InlineData(MacOS, SocketTarget)]
    [InlineData(MacOS, LinkTarget)]
    [InlineData(MacOS, DeviceTarget)]
    public async Task SpecialSentinelTargetsAreRefused(string platform, string kind)
    {
        RequirePlatform(platform);
        using var temporary = TemporaryWorkspace.Create("special");
        using var store = WorkspaceLockTestStore.Create("special-store");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var path = kind == IgnoreFifoTarget ? WorkspaceRuntimePaths.IgnorePath(workspace) : WorkspaceRuntimePaths.LockPath(workspace);
        Directory.CreateDirectory(WorkspaceRuntimePaths.DirectoryPath(workspace));
        Socket? socket = null;
        try
        {
            switch (kind)
            {
                case FifoTarget:
                case IgnoreFifoTarget:
                    using (var create = UnixLockWitness.CreateFifo(path))
                    {
                        await create.WaitForExitAsync(TestContext.Current.CancellationToken);
                        Assert.Equal(0, create.ExitCode);
                    }
                    break;
                case SocketTarget:
                    socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                    socket.Bind(new UnixDomainSocketEndPoint(path));
                    break;
                case LinkTarget:
                    File.CreateSymbolicLink(path, temporary.CreateFile("target", "preserved"));
                    break;
                case DeviceTarget:
                    File.CreateSymbolicLink(path, "/dev/null");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }

            var result = await store.AcquireAsync(new(workspace, IndexDefinitions.CommandIdentity, Guid.NewGuid()), TestContext.Current.CancellationToken);
            Assert.Null(result.Lease);
            Assert.True(result.LockFailure?.Kind == WorkspaceLockFailureKind.InvalidTarget, $"Expected InvalidTarget for a {kind}, got {result.LockFailure}.");
            Assert.Equal(path, result.LockFailure?.Path);
            Assert.NotEqual((FileAttributes)(-1), File.GetAttributes(path));
        }
        finally
        {
            socket?.Dispose();
            File.Delete(path);
            _ = store.Track(workspace);
        }
    }

    private static void RequirePlatform(string platform)
    {
        if (!(platform == Linux && OperatingSystem.IsLinux()) && !(platform == MacOS && OperatingSystem.IsMacOS()))
        {
            Assert.Skip($"This evidence requires {platform}.");
        }
    }

    private static void Stop(Process process)
    {
        if (!process.HasExited)
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit();
        }
    }
}
