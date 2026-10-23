using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Shared.Interop;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.IntegrationTests.Framework.Mutation.Locking.Shared.Processes;

internal static class WorkspaceLockProbe
{
    internal const string ChildArgument = "--workspace-lock-probe";

    internal static async Task RunAsync(string[] arguments)
    {
        var workspace = new CliWorkspace(arguments[1], arguments[1], CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var manager = new WorkspaceLockManager(WorkspaceLockStoreRoot.FromAbsolutePath(arguments[2]));
        if (arguments[3] == "implicit-disabled")
        {
            using var managed = new FileStream(arguments[4], FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            Console.WriteLine("managed-opened");
        }

        if (arguments[3] == "cancel-fallback")
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var failure = UnixWorkspaceLock.TryOpenReadOnly(arguments[4], cancellation.Token, out var handle);
            using (handle)
            {
                Console.WriteLine(failure?.Kind);
                Console.WriteLine(handle is null ? "no-handle" : "unexpected-handle");
            }

            var cancelled = await manager.AcquireAsync(new(workspace, "probe", Guid.NewGuid()), cancellation.Token);
            await using var cancelledLease = cancelled.Lease;
            Console.WriteLine(cancelled.LockFailure?.Kind);
            return;
        }

        var result = await manager.AcquireAsync(new(workspace, "probe", Guid.NewGuid()), CancellationToken.None);
        await using var lease = result.Lease;
        Console.WriteLine(result.LockFailure?.Kind.ToString() ?? "Acquired");
        Console.WriteLine(result.LockFailure?.Path ?? lease?.LockPath);
        Console.WriteLine(result.LockFailure?.NativeError);
        if (arguments[3] == "fifo")
        {
            var path = WorkspaceRuntimePaths.LockPath(workspace);
            using var local = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            var failure = UnixWorkspaceLock.TryAcquire(local.SafeFileHandle, path, CancellationToken.None);
            Console.WriteLine(failure?.Kind.ToString() ?? "local-released");
        }
    }
}
