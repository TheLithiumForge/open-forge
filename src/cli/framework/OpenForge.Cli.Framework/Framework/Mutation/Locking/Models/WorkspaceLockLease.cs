using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

internal sealed class WorkspaceLockLease : IDisposable, IAsyncDisposable
{
    private readonly FileStream _handle;
    private readonly LegacyWorkspaceLockResult _legacy;
    private readonly string _workspaceKey;
    private int _disposed;

    internal WorkspaceLockLease(
        WorkspaceLockRequest request,
        FileStream handle,
        LegacyWorkspaceLockResult legacy)
    {
        var lockPath = WorkspaceRuntimePaths.LockPath(request.Workspace);
        if (!handle.CanWrite || !handle.CanSeek || !IsLive(handle)
            || !string.Equals(Path.GetFullPath(handle.Name), lockPath, PathComparison()))
        {
            throw new ArgumentException("A workspace lock lease requires its exact open local handle.", nameof(handle));
        }

        if (legacy.State == LegacyWorkspaceLockState.Failed
            || legacy.Handle is not null && (!IsLive(legacy.Handle)
                || legacy.StoreRoot is null
                || !string.Equals(legacy.LockPath, WorkspaceLockPathIdentity.LockPath(legacy.StoreRoot, request.Workspace), PathComparison())
                || !string.Equals(legacy.Handle.Name, legacy.LockPath, PathComparison())))
        {
            throw new ArgumentException("A workspace lock lease requires a completed matching legacy bridge.", nameof(legacy));
        }

        Request = request;
        LockPath = lockPath;
        _handle = handle;
        _legacy = legacy;
        _workspaceKey = WorkspaceIdentity.Key(request.Workspace.PhysicalRoot);
    }

    internal WorkspaceLockRequest Request { get; }
    internal string LockPath { get; }
    internal WorkspaceLockStoreRoot? LegacyStoreRoot => _legacy.StoreRoot;
    internal string? LegacyLockPath => _legacy.LockPath;
    internal LegacyWorkspaceLockState LegacyState => _legacy.State;

    internal bool IsHeld => Volatile.Read(ref _disposed) == 0
        && IsLive(_handle)
        && (_legacy.Handle is null || IsLive(_legacy.Handle));

    internal bool IsHeldFor(CliWorkspace workspace)
        => IsHeld
            && string.Equals(_workspaceKey, WorkspaceIdentity.Key(workspace.PhysicalRoot), StringComparison.Ordinal)
            && string.Equals(LockPath, WorkspaceRuntimePaths.LockPath(workspace), PathComparison());

    internal bool HoldsLegacyLock(WorkspaceLockStoreRoot storeRoot)
        => IsHeld && _legacy.Handle is not null
            && string.Equals(LegacyStoreRoot?.Path, storeRoot.Path, PathComparison());

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        try
        {
            _legacy.Handle?.Dispose();
        }
        finally
        {
            _handle.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        try
        {
            if (_legacy.Handle is not null)
            {
                await _legacy.Handle.DisposeAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            await _handle.DisposeAsync().ConfigureAwait(false);
        }
    }

    private static bool IsLive(FileStream handle)
        => handle.CanRead && !handle.SafeFileHandle.IsClosed && !handle.SafeFileHandle.IsInvalid;

    private static StringComparison PathComparison()
        => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
}
