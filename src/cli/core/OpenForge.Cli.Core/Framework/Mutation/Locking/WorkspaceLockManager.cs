using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

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
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Completed(WorkspaceLockResult.Cancelled());
        }

        WorkspaceLockStoreRoot? storeRoot;
        try
        {
            storeRoot = _resolveForCurrentUser
                ? WorkspaceLockStoreRoot.ResolveForCurrentUser(Environment.SpecialFolderOption.DoNotVerify)
                : _configuredStoreRoot;
        }
        catch (UnauthorizedAccessException exception)
        {
            return Completed(Failed(FilesystemFailureKind.AccessDenied, exception));
        }
        catch (Exception exception) when (exception is ArgumentException or PathTooLongException)
        {
            return Completed(Failed(FilesystemFailureKind.InvalidPath, exception));
        }
        catch (NotSupportedException exception)
        {
            return Completed(Failed(FilesystemFailureKind.Unsupported, exception));
        }
        catch (IOException exception)
        {
            return Completed(Failed(FilesystemFailureKind.InputOutput, exception));
        }

        if (storeRoot is null)
        {
            return Completed(WorkspaceLockResult.Failed(
                new FilesystemFailure(
                    FilesystemFailureKind.Unsupported,
                    "The local application-data directory is unavailable for workspace locking.")));
        }

        var storeDirectory = WorkspaceLockPathIdentity.StoreDirectory(storeRoot);
        var lockPath = WorkspaceLockPathIdentity.LockPath(storeRoot, request.Workspace);
        try
        {
            _ = Directory.CreateDirectory(storeDirectory);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Completed(Failed(FilesystemFailureKind.AccessDenied, exception));
        }
        catch (Exception exception) when (exception is ArgumentException or PathTooLongException)
        {
            return Completed(Failed(FilesystemFailureKind.InvalidPath, exception));
        }
        catch (NotSupportedException exception)
        {
            return Completed(Failed(FilesystemFailureKind.Unsupported, exception));
        }
        catch (IOException exception)
        {
            return Completed(Failed(FilesystemFailureKind.InputOutput, exception));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Completed(WorkspaceLockResult.Cancelled());
        }

        var targetFailure = ValidateTargetBeforeOpen(lockPath);
        if (targetFailure is not null)
        {
            return Completed(targetFailure);
        }

        FileStream? handle = null;
        try
        {
            handle = new FileStream(
                lockPath,
                new FileStreamOptions
                {
                    Mode = File.Exists(lockPath) ? FileMode.Open : FileMode.CreateNew,
                    Access = FileAccess.ReadWrite,
                    Share = FileShare.None,
                    Options = FileOptions.Asynchronous,
                });

            var openedFailure = ValidateOpenedTarget(lockPath, handle);
            if (openedFailure is not null)
            {
                handle.Dispose();
                return Completed(openedFailure);
            }
        }
        catch (UnauthorizedAccessException exception)
        {
            handle?.Dispose();
            return Completed(Failed(FilesystemFailureKind.AccessDenied, exception));
        }
        catch (Exception exception) when (exception is ArgumentException or PathTooLongException)
        {
            handle?.Dispose();
            return Completed(Failed(FilesystemFailureKind.InvalidPath, exception));
        }
        catch (NotSupportedException exception)
        {
            handle?.Dispose();
            return Completed(Failed(FilesystemFailureKind.Unsupported, exception));
        }
        catch (IOException exception)
        {
            handle?.Dispose();
            return Completed(Failed(FilesystemFailureKind.InputOutput, exception));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            handle.Dispose();
            return Completed(WorkspaceLockResult.Cancelled());
        }

        return Completed(WorkspaceLockResult.Acquired(
            new WorkspaceLockLease(
                request: request,
                storeRoot: storeRoot,
                lockPath: lockPath,
                handle: handle)));
    }

    private static WorkspaceLockResult? ValidateTargetBeforeOpen(string lockPath)
    {
        try
        {
            if (!File.Exists(lockPath))
            {
                return Directory.Exists(lockPath)
                    ? InvalidTarget("The workspace lock path is not an ordinary file.")
                    : null;
            }

            var attributes = File.GetAttributes(lockPath);
            return (attributes & (FileAttributes.Directory | FileAttributes.Device | FileAttributes.ReparsePoint)) == 0
                ? null
                : InvalidTarget("The workspace lock path is not an ordinary file.");
        }
        catch (UnauthorizedAccessException exception)
        {
            return Failed(FilesystemFailureKind.AccessDenied, exception);
        }
        catch (Exception exception) when (exception is ArgumentException or PathTooLongException)
        {
            return Failed(FilesystemFailureKind.InvalidPath, exception);
        }
        catch (NotSupportedException exception)
        {
            return Failed(FilesystemFailureKind.Unsupported, exception);
        }
        catch (IOException exception)
        {
            return Failed(FilesystemFailureKind.InputOutput, exception);
        }
    }

    private static WorkspaceLockResult? ValidateOpenedTarget(
        string lockPath,
        FileStream handle)
    {
        var targetFailure = ValidateTargetBeforeOpen(lockPath);
        if (targetFailure is not null)
        {
            return targetFailure;
        }

        return handle.Length == 0
            ? null
            : InvalidTarget("The persistent workspace lock file must remain zero bytes.");
    }

    private static WorkspaceLockResult InvalidTarget(string cause)
        => WorkspaceLockResult.Failed(
            new FilesystemFailure(FilesystemFailureKind.InvalidPath, cause));

    private static WorkspaceLockResult Failed(
        FilesystemFailureKind kind,
        Exception exception)
        => WorkspaceLockResult.Failed(FilesystemFailure.FromException(kind, exception));

    private static ValueTask<WorkspaceLockResult> Completed(WorkspaceLockResult result)
        => ValueTask.FromResult(result);
}
