using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Shared.Interop;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.IntegrationTests.Framework.Mutation.Locking.Shared.Processes;

internal static class WorkspaceLockProbe
{
    internal const string ImplicitDisabledMode = "implicit-disabled";
    internal const string ManagedOpened = "managed-opened";
    internal const string CancelFallbackMode = "cancel-fallback";
    internal const string NoHandle = "no-handle";
    internal const string UnexpectedHandle = "unexpected-handle";
    internal const string Acquired = "Acquired";
    internal const string FifoMode = "fifo";
    internal const string LocalReleased = "local-released";
    internal const string CommandIdentity = "probe";
    internal const string ChildArgument = "--workspace-lock-probe";

    internal static async Task RunAsync(string[] arguments)
    {
        var workspace = new CliWorkspace(arguments[1], arguments[1], CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var manager = new WorkspaceLockManager(WorkspaceLockStoreRoot.FromAbsolutePath(arguments[2]));
        if (arguments[3] == ImplicitDisabledMode)
        {
            using var managed = new FileStream(arguments[4], FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            Console.WriteLine(ManagedOpened);
        }

        if (arguments[3] == CancelFallbackMode)
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var failure = UnixWorkspaceLock.TryOpenReadOnly(arguments[4], cancellation.Token, out var handle);
            using (handle)
            {
                Console.WriteLine(failure?.Kind);
                Console.WriteLine(handle is null ? NoHandle : UnexpectedHandle);
            }

            var cancelled = await manager.AcquireAsync(new(workspace, CommandIdentity, Guid.NewGuid()), cancellation.Token);
            await using var cancelledLease = cancelled.Lease;
            Console.WriteLine(cancelled.LockFailure?.Kind);
            return;
        }

        var result = await manager.AcquireAsync(new(workspace, CommandIdentity, Guid.NewGuid()), CancellationToken.None);
        await using var lease = result.Lease;
        Console.WriteLine(result.LockFailure?.Kind.ToString() ?? Acquired);
        Console.WriteLine(result.LockFailure?.Path ?? lease?.LockPath);
        Console.WriteLine(result.LockFailure?.NativeError);
        if (arguments[3] == FifoMode)
        {
            var path = WorkspaceRuntimePaths.LockPath(workspace);
            using var local = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            var failure = UnixWorkspaceLock.TryAcquire(local.SafeFileHandle, path, CancellationToken.None);
            Console.WriteLine(failure?.Kind.ToString() ?? LocalReleased);
        }
    }
}
