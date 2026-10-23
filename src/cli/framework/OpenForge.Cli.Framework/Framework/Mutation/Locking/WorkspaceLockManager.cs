using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Shared.Interop;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Shared.Legacy;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Shared.Runtime;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.Core.Framework.Mutation.Locking;

internal sealed class WorkspaceLockManager
{
    private readonly WorkspaceLockStoreRoot? _configuredStoreRoot;
    private readonly bool _resolveForCurrentUser;

    internal WorkspaceLockManager(WorkspaceLockStoreRoot storeRoot)
    {
        _configuredStoreRoot = storeRoot;
    }

    private WorkspaceLockManager()
    {
        _resolveForCurrentUser = true;
    }

    internal static WorkspaceLockManager CreateForCurrentUser()
        => new();

    internal ValueTask<WorkspaceLockResult> AcquireAsync(
        WorkspaceLockRequest request,
        CancellationToken cancellationToken)
        => ValueTask.FromResult(Acquire(request, cancellationToken));

    private WorkspaceLockResult Acquire(WorkspaceLockRequest request, CancellationToken cancellationToken)
    {
        var lockPath = WorkspaceRuntimePaths.LockPath(request.Workspace);
        if (cancellationToken.IsCancellationRequested)
        {
            return WorkspaceLockResult.Cancelled(lockPath);
        }

        var failure = WorkspaceRuntimeBootstrap.EnsureCreated(request.Workspace, cancellationToken)
            ?? WorkspaceLockTarget.Validate(lockPath, directory: false, allowMissing: true);
        if (failure is not null)
        {
            return WorkspaceLockResult.Failed(failure);
        }

        FileStream? handle = null;
        LegacyWorkspaceLockResult? legacy = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            handle = new FileStream(lockPath, new FileStreamOptions
            {
                Mode = FileMode.OpenOrCreate,
                Access = FileAccess.ReadWrite,
                Share = FileShare.None,
            });
            failure = WorkspaceLockTarget.Validate(lockPath, directory: false, allowMissing: false);
            if (failure is null && !handle.CanSeek)
            {
                failure = WorkspaceLockTarget.Invalid(lockPath);
            }

            failure ??= UnixWorkspaceLock.TryAcquire(handle.SafeFileHandle, lockPath, cancellationToken);
            if (failure is not null)
            {
                return WorkspaceLockResult.Failed(failure);
            }

            cancellationToken.ThrowIfCancellationRequested();
            var storeRoot = ResolveLegacyRoot();
            legacy = LegacyWorkspaceLockBridge.Acquire(request, storeRoot, cancellationToken);
            if (legacy.Failure is not null)
            {
                return WorkspaceLockResult.Failed(legacy.Failure);
            }

            cancellationToken.ThrowIfCancellationRequested();
            var lease = new WorkspaceLockLease(request, handle, legacy);
            handle = null;
            legacy = null;
            return WorkspaceLockResult.Acquired(lease);
        }
        catch (OperationCanceledException)
        {
            return WorkspaceLockResult.Cancelled(lockPath);
        }
        catch (Exception exception) when (WorkspaceLockTarget.IsFilesystemException(exception))
        {
            return WorkspaceLockResult.Failed(WorkspaceLockTarget.FromException(lockPath, exception));
        }
        finally
        {
            try
            {
                legacy?.Handle?.Dispose();
            }
            finally
            {
                handle?.Dispose();
            }
        }
    }

    private WorkspaceLockStoreRoot? ResolveLegacyRoot()
    {
        if (!_resolveForCurrentUser)
        {
            return _configuredStoreRoot;
        }

        try
        {
            return WorkspaceLockStoreRoot.ResolveForCurrentUser(Environment.SpecialFolderOption.DoNotVerify);
        }
        catch (Exception exception) when (WorkspaceLockTarget.IsFilesystemException(exception))
        {
            return null;
        }
    }
}
