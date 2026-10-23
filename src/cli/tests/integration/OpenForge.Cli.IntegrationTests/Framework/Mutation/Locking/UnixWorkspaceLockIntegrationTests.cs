using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using Microsoft.Win32.SafeHandles;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Shared.Interop;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Shared.Runtime;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Mutation.Locking;

[Trait("Boundary", "OS"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
public sealed partial class UnixWorkspaceLockIntegrationTests
{
    [Theory(DisplayName = "Unix constructor contention is Busy for local and legacy lock files")]
    [InlineData("linux", false), InlineData("linux", true)]
    [InlineData("macos", false), InlineData("macos", true)]
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
        using var holder = Python(
            "import fcntl,sys\nf=open(sys.argv[1],'r+b')\nfcntl.flock(f,fcntl.LOCK_EX|fcntl.LOCK_NB)\nprint('held',flush=True)\nsys.stdin.readline()", path);
        try
        {
            Assert.Equal("held", await holder.StandardOutput.ReadLineAsync(cancellation.Token));
            var exception = Assert.Throws<IOException>(() =>
            {
                using var handle = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            });
            Assert.Equal(platform == "macos" ? 35 : 11, exception.HResult);
            var result = await store.AcquireAsync(new(workspace, "index", Guid.NewGuid()), cancellation.Token);
            Assert.Null(result.Lease);
            Assert.Equal(WorkspaceLockFailureKind.Busy, result.LockFailure?.Kind);
            Assert.Equal(path, result.LockFailure?.Path);
            Assert.Equal(exception.HResult, result.LockFailure?.NativeError);

            await holder.StandardInput.WriteLineAsync();
            await holder.WaitForExitAsync(cancellation.Token);
            Assert.Equal(0, holder.ExitCode);
            var acquired = await store.AcquireAsync(new(workspace, "index", Guid.NewGuid()), cancellation.Token);
            await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        }
        finally
        {
            Stop(holder);
        }
    }

    [Theory(DisplayName = "Platform flock imports acquire a valid handle without assembly resolver initialization")]
    [InlineData("linux"), InlineData("macos")]
    public void PlatformImportAcquiresWithoutTypeInitialization(string platform)
    {
        RequirePlatform(platform);
        using var temporary = TemporaryWorkspace.Create("direct-flock-import");
        var path = temporary.CreateFile("lock");
        using var handle = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

        Assert.Null(typeof(UnixWorkspaceLock).TypeInitializer);
        Assert.Null(UnixWorkspaceLock.TryAcquire(handle.SafeFileHandle, path, TestContext.Current.CancellationToken));
    }

    [Theory(DisplayName = "Checked flock excludes another process in both acquisition directions")]
    [InlineData("linux", false)]
    [InlineData("linux", true)]
    [InlineData("macos", false)]
    [InlineData("macos", true)]
    public async Task CheckedFlockExcludesOtherProcess(string platform, bool readOnly)
    {
        RequirePlatform(platform);
        using var temporary = TemporaryWorkspace.Create("flock");
        var path = temporary.CreateFile("lock");
        // A FileStream would take an implicit shared flock, which would stop the holder below from locking.
        // The native descriptor carries no lock until the checked call takes one.
        Assert.Null(UnixWorkspaceLock.TryOpenReadOnly(path, TestContext.Current.CancellationToken, out var native));
        using var handle = Assert.IsType<SafeFileHandle>(native);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.CancelAfter(TimeSpan.FromSeconds(15));
        using var holder = Python(
            "import fcntl,sys\nf=open(sys.argv[1],sys.argv[2])\nfcntl.flock(f,fcntl.LOCK_EX|fcntl.LOCK_NB)\nprint('held',flush=True)\nsys.stdin.readline()",
            path, readOnly ? "rb" : "r+b");
        try
        {
            Assert.Equal("held", await holder.StandardOutput.ReadLineAsync(cancellation.Token));
            var busy = UnixWorkspaceLock.TryAcquire(handle, path, cancellation.Token);
            Assert.Equal(WorkspaceLockFailureKind.Busy, busy?.Kind);
            Assert.Equal(platform == "macos" ? 35 : 11, busy?.NativeError);
            await holder.StandardInput.WriteLineAsync();
            await holder.WaitForExitAsync(cancellation.Token);
            Assert.Equal(0, holder.ExitCode);
            Assert.Null(UnixWorkspaceLock.TryAcquire(handle, path, cancellation.Token));
            using var contender = Python(
                "import fcntl,sys\nf=open(sys.argv[1],sys.argv[2])\ntry:\n fcntl.flock(f,fcntl.LOCK_EX|fcntl.LOCK_NB)\n print('acquired')\nexcept BlockingIOError:\n print('busy')",
                path, readOnly ? "rb" : "r+b");
            try
            {
                Assert.Equal("busy", await contender.StandardOutput.ReadLineAsync(cancellation.Token));
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
    [InlineData("linux")]
    [InlineData("macos")]
    public void CheckedFlockRejectsBadDescriptorAndCancellation(string platform)
    {
        RequirePlatform(platform);
        using var handle = new SafeFileHandle((nint)int.MaxValue, ownsHandle: false);
        var failure = UnixWorkspaceLock.TryAcquire(handle, "invalid-descriptor", TestContext.Current.CancellationToken);
        Assert.Equal(WorkspaceLockFailureKind.IoError, failure?.Kind);
        Assert.Equal(9, failure?.NativeError);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var cancelled = UnixWorkspaceLock.TryAcquire(handle, "cancelled-descriptor", cancellation.Token);
        Assert.Equal(WorkspaceLockFailureKind.Cancelled, cancelled?.Kind);
        Assert.Equal("cancelled-descriptor", cancelled?.Path);
    }

    [Theory(DisplayName = "Unix lock and ignore special-file targets are refused and preserved")]
    [InlineData("linux", "fifo")]
    [InlineData("linux", "ignore-fifo")]
    [InlineData("linux", "socket")]
    [InlineData("linux", "link")]
    [InlineData("linux", "device")]
    [InlineData("macos", "fifo")]
    [InlineData("macos", "ignore-fifo")]
    [InlineData("macos", "socket")]
    [InlineData("macos", "link")]
    [InlineData("macos", "device")]
    public async Task SpecialSentinelTargetsAreRefused(string platform, string kind)
    {
        RequirePlatform(platform);
        using var temporary = TemporaryWorkspace.Create("special");
        using var store = WorkspaceLockTestStore.Create("special-store");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var path = kind == "ignore-fifo" ? WorkspaceRuntimePaths.IgnorePath(workspace) : WorkspaceRuntimePaths.LockPath(workspace);
        Directory.CreateDirectory(WorkspaceRuntimePaths.DirectoryPath(workspace));
        Socket? socket = null;
        try
        {
            switch (kind)
            {
                case "fifo":
                case "ignore-fifo":
                    using (var create = Python("import os,sys\nos.mkfifo(sys.argv[1])", path))
                    {
                        await create.WaitForExitAsync(TestContext.Current.CancellationToken);
                        Assert.Equal(0, create.ExitCode);
                    }
                    break;
                case "socket":
                    socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                    socket.Bind(new UnixDomainSocketEndPoint(path));
                    break;
                case "link":
                    File.CreateSymbolicLink(path, temporary.CreateFile("target", "preserved"));
                    break;
                case "device":
                    File.CreateSymbolicLink(path, "/dev/null");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }

            var result = await store.AcquireAsync(new(workspace, "index", Guid.NewGuid()), TestContext.Current.CancellationToken);
            Assert.Null(result.Lease);
            Assert.Equal(WorkspaceLockFailureKind.InvalidTarget, result.LockFailure?.Kind);
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
        if (!(platform == "linux" && OperatingSystem.IsLinux()) && !(platform == "macos" && OperatingSystem.IsMacOS()))
        {
            Assert.Skip($"This evidence requires {platform}.");
        }
    }

    private static Process Python(string script, params string[] arguments)
    {
        var start = new ProcessStartInfo("python3")
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        start.ArgumentList.Add("-c");
        start.ArgumentList.Add(script);
        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        try
        {
            return Process.Start(start) ?? throw new InvalidOperationException("The flock witness process did not start.");
        }
        catch (Win32Exception)
        {
            Assert.Skip("Cross-process Unix lock evidence requires python3 with its standard fcntl module.");
            throw;
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
