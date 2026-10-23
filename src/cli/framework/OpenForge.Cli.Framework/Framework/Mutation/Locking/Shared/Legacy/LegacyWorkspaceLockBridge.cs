using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Shared.Interop;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Shared.Runtime;

namespace OpenForge.Cli.Core.Framework.Mutation.Locking.Shared.Legacy;

internal static class LegacyWorkspaceLockBridge
{
    internal static LegacyWorkspaceLockResult Acquire(
        WorkspaceLockRequest request,
        WorkspaceLockStoreRoot? storeRoot,
        CancellationToken cancellationToken)
    {
        if (storeRoot is null)
        {
            return new(LegacyWorkspaceLockState.Unavailable);
        }

        var lockPath = WorkspaceLockPathIdentity.LockPath(storeRoot, request.Workspace);
        var path = WorkspaceLockPathIdentity.StoreDirectory(storeRoot);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var failure = WorkspaceLockTarget.Validate(path, directory: true, allowMissing: true);
            if (failure is not null)
            {
                return failure.Kind == WorkspaceLockFailureKind.AccessDenied
                    ? ReadOnlyOrAbsent(storeRoot, lockPath, cancellationToken)
                    : Failed(storeRoot, lockPath, failure);
            }

            Directory.CreateDirectory(path);
            path = lockPath;
            failure = WorkspaceLockTarget.Validate(path, directory: false, allowMissing: true);
            if (failure is not null)
            {
                return failure.Kind == WorkspaceLockFailureKind.AccessDenied
                    ? ReadOnlyOrAbsent(storeRoot, lockPath, cancellationToken)
                    : Failed(storeRoot, lockPath, failure);
            }

            return Open(storeRoot, lockPath, FileAccess.ReadWrite, cancellationToken);
        }
        catch (UnauthorizedAccessException)
        {
            return ReadOnlyOrAbsent(storeRoot, lockPath, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return Failed(storeRoot, lockPath, new(WorkspaceLockFailureKind.Cancelled, path));
        }
        catch (Exception exception) when (WorkspaceLockTarget.IsFilesystemException(exception))
        {
            return Failed(storeRoot, lockPath, WorkspaceLockTarget.FromException(path, exception));
        }
    }

    private static LegacyWorkspaceLockResult ReadOnlyOrAbsent(
        WorkspaceLockStoreRoot storeRoot,
        string lockPath,
        CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = File.GetAttributes(lockPath);
            var failure = WorkspaceLockTarget.Validate(lockPath, directory: false, allowMissing: false);
            return failure is null
                ? Open(storeRoot, lockPath, FileAccess.Read, cancellationToken)
                : Failed(storeRoot, lockPath, failure);
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            return new(LegacyWorkspaceLockState.SkippedAbsent, storeRoot, lockPath);
        }
        catch (OperationCanceledException)
        {
            return Failed(storeRoot, lockPath, new(WorkspaceLockFailureKind.Cancelled, lockPath));
        }
        catch (Exception exception) when (WorkspaceLockTarget.IsFilesystemException(exception))
        {
            return Failed(storeRoot, lockPath, WorkspaceLockTarget.FromException(lockPath, exception));
        }
    }

    private static LegacyWorkspaceLockResult Open(
        WorkspaceLockStoreRoot storeRoot,
        string lockPath,
        FileAccess access,
        CancellationToken cancellationToken)
    {
        FileStream? handle = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            handle = new FileStream(lockPath, new FileStreamOptions
            {
                Mode = access == FileAccess.ReadWrite ? FileMode.OpenOrCreate : FileMode.Open,
                Access = access,
                Share = FileShare.None,
            });
            var failure = WorkspaceLockTarget.Validate(lockPath, directory: false, allowMissing: false);
            if (failure is null && (!handle.CanSeek || handle.Length != 0))
            {
                failure = WorkspaceLockTarget.Invalid(lockPath);
            }

            failure ??= UnixWorkspaceLock.TryAcquire(handle.SafeFileHandle, lockPath, cancellationToken);
            if (failure is not null)
            {
                return Failed(storeRoot, lockPath, failure);
            }

            cancellationToken.ThrowIfCancellationRequested();
            var result = new LegacyWorkspaceLockResult(
                access == FileAccess.ReadWrite ? LegacyWorkspaceLockState.HeldReadWrite : LegacyWorkspaceLockState.HeldReadOnly,
                storeRoot,
                lockPath,
                handle);
            handle = null;
            return result;
        }
        finally
        {
            handle?.Dispose();
        }
    }

    private static LegacyWorkspaceLockResult Failed(
        WorkspaceLockStoreRoot storeRoot,
        string lockPath,
        WorkspaceLockFailure failure)
        => new(LegacyWorkspaceLockState.Failed, storeRoot, lockPath, failure: failure);
}
