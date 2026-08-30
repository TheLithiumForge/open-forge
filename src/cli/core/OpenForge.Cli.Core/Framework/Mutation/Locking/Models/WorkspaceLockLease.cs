using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

internal sealed class WorkspaceLockLease : IDisposable, IAsyncDisposable
{
    private readonly FileStream _handle;
    private int _disposed;

    internal WorkspaceLockLease(
        WorkspaceLockRequest request,
        WorkspaceLockStoreRoot storeRoot,
        string lockPath,
        FileStream handle)
    {
        if (!handle.CanWrite
            || handle.SafeFileHandle.IsInvalid
            || handle.SafeFileHandle.IsClosed
            || handle.Length != 0)
        {
            throw new ArgumentException(
                "A workspace lock lease requires one open writable zero-byte OS handle.",
                nameof(handle));
        }

        var normalizedLockPath = FileExpectation.NormalizeAbsolutePath(
            lockPath,
            nameof(lockPath));
        var expectedLockPath = WorkspaceLockPathIdentity.LockPath(storeRoot, request.Workspace);
        if (!string.Equals(normalizedLockPath, expectedLockPath, PathComparison()))
        {
            throw new ArgumentException(
                "A workspace lock lease path must match its request and store root.",
                nameof(lockPath));
        }

        var openedLockPath = FileExpectation.NormalizeAbsolutePath(handle.Name, nameof(handle));
        if (!string.Equals(normalizedLockPath, openedLockPath, PathComparison()))
        {
            throw new ArgumentException(
                "A workspace lock lease path must match its open handle.",
                nameof(lockPath));
        }

        Request = request;
        StoreRoot = storeRoot;
        LockPath = normalizedLockPath;
        _handle = handle;
    }

    internal WorkspaceLockRequest Request { get; }

    internal WorkspaceLockStoreRoot StoreRoot { get; }

    internal string LockPath { get; }

    internal bool IsHeld => Volatile.Read(ref _disposed) == 0
        && !_handle.SafeFileHandle.IsClosed
        && !_handle.SafeFileHandle.IsInvalid;

    internal bool IsHeldFor(CliWorkspace workspace)
    {
        return IsHeld
            && string.Equals(
                WorkspaceIdentity.Key(Request.Workspace.PhysicalRoot),
                WorkspaceIdentity.Key(workspace.PhysicalRoot),
                StringComparison.Ordinal)
            && string.Equals(
                LockPath,
                WorkspaceLockPathIdentity.LockPath(StoreRoot, workspace),
                PathComparison());
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            _handle.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            await _handle.DisposeAsync().ConfigureAwait(false);
        }
    }

    private static StringComparison PathComparison()
        => OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
}
