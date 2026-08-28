using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Locking;

internal sealed partial class WorkspaceLockManager(PhysicalPathResolver physicalPathResolver)
{
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;

    internal ValueTask<WorkspaceLockResult> AcquireAsync(
        WorkspaceLockRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (cancellationToken.IsCancellationRequested)
        {
            return Completed(WorkspaceLockResult.Cancelled());
        }

        var directoryPath = Path.GetDirectoryName(request.LogicalPath)
            ?? throw new InvalidOperationException("The workspace lock path requires a directory.");
        var directory = Resolve(request, directoryPath);
        if (directory.State == PhysicalPathState.Missing)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Completed(WorkspaceLockResult.Cancelled());
            }

            try
            {
                _ = Directory.CreateDirectory(directoryPath);
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

            directory = Resolve(request, directoryPath);
        }

        var directoryFailure = ValidateDirectory(directoryPath, directory);
        if (directoryFailure is not null)
        {
            return Completed(directoryFailure);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Completed(WorkspaceLockResult.Cancelled());
        }

        var lockResolution = Resolve(request, request.LogicalPath);
        if (lockResolution.State is not (PhysicalPathState.Missing or PhysicalPathState.Contained))
        {
            return Completed(FromResolution(lockResolution, "The workspace lock path is unsafe or unavailable."));
        }

        if (lockResolution.State == PhysicalPathState.Contained)
        {
            var targetFailure = ValidateLockTarget(
                request.LogicalPath,
                lockResolution.GetContainedPhysicalPath());
            if (targetFailure is not null)
            {
                return Completed(targetFailure);
            }
        }

        var physicalDirectory = directory.GetContainedPhysicalPath();
        var physicalPath = lockResolution.State == PhysicalPathState.Contained
            ? lockResolution.GetContainedPhysicalPath()
            : Path.Combine(physicalDirectory, Path.GetFileName(request.LogicalPath));
        var mode = lockResolution.State == PhysicalPathState.Contained
            ? FileMode.Open
            : FileMode.CreateNew;
        FileStream? handle = null;
        try
        {
            handle = new FileStream(
                physicalPath,
                new FileStreamOptions
                {
                    Mode = mode,
                    Access = FileAccess.ReadWrite,
                    Share = FileShare.None,
                    Options = FileOptions.Asynchronous,
                });
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
        var confirmedLock = Resolve(request, request.LogicalPath);
        if (confirmedLock.State != PhysicalPathState.Contained
            || !string.Equals(
                physicalPath,
                confirmedLock.GetContainedPhysicalPath(),
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal))
        {
            handle.Dispose();
            return Completed(confirmedLock.State == PhysicalPathState.Contained
                ? WorkspaceLockResult.Failed(
                    new FilesystemFailure(
                        FilesystemFailureKind.InvalidPath,
                        "The workspace lock changed its resolved physical path during acquisition."))
                : FromResolution(
                    confirmedLock,
                    "The workspace lock path became unsafe during acquisition."));
        }

        var confirmedTargetFailure = ValidateLockTarget(
            request.LogicalPath,
            confirmedLock.GetContainedPhysicalPath());
        if (confirmedTargetFailure is not null)
        {
            handle.Dispose();
            return Completed(confirmedTargetFailure);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            handle.Dispose();
            return Completed(WorkspaceLockResult.Cancelled());
        }

        return Completed(WorkspaceLockResult.Acquired(
            new WorkspaceLockLease(
                request,
                logicalPath: request.LogicalPath,
                physicalPath: physicalPath,
                handle: handle)));
    }

    private static ValueTask<WorkspaceLockResult> Completed(WorkspaceLockResult result)
        => ValueTask.FromResult(result);
}
