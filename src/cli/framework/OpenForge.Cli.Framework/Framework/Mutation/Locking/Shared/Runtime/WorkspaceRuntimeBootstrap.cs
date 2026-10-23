using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Shared.Interop;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.Core.Framework.Mutation.Locking.Shared.Runtime;

internal static class WorkspaceRuntimeBootstrap
{
    internal static WorkspaceLockFailure? EnsureCreated(CliWorkspace workspace, CancellationToken cancellationToken)
    {
        var path = WorkspaceRuntimePaths.DirectoryPath(workspace);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var failure = WorkspaceLockTarget.Validate(path, directory: true, allowMissing: true);
            if (failure is not null)
            {
                return InitializationFailure(failure);
            }

            Directory.CreateDirectory(path);
            failure = WorkspaceLockTarget.Validate(path, directory: true, allowMissing: false);
            if (failure is not null)
            {
                return InitializationFailure(failure);
            }

            path = WorkspaceRuntimePaths.IgnorePath(workspace);
            failure = WorkspaceLockTarget.Validate(path, directory: false, allowMissing: true);
            if (failure is not null)
            {
                return InitializationFailure(failure);
            }

            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (new FileInfo(path).Length == 2)
                {
                    using var existing = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    if (!existing.CanSeek)
                    {
                        return WorkspaceLockTarget.Invalid(path);
                    }

                    failure = WorkspaceLockTarget.Validate(path, directory: false, allowMissing: false);
                    if (failure is not null)
                    {
                        return InitializationFailure(failure);
                    }

                    if (ContainsIgnoreRule(existing))
                    {
                        return null;
                    }
                }
            }
            catch (FileNotFoundException)
            {
            }

            cancellationToken.ThrowIfCancellationRequested();
            using var handle = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            if (!handle.CanSeek)
            {
                return WorkspaceLockTarget.Invalid(path);
            }

            failure = WorkspaceLockTarget.Validate(path, directory: false, allowMissing: false)
                ?? UnixWorkspaceLock.TryAcquire(handle.SafeFileHandle, path, cancellationToken);
            if (failure is not null)
            {
                return InitializationFailure(failure);
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (ContainsIgnoreRule(handle))
            {
                return null;
            }

            handle.Position = 0;
            handle.Write("*\n"u8);
            handle.SetLength(2);
            handle.Flush(flushToDisk: true);
            return null;
        }
        catch (OperationCanceledException)
        {
            return new(WorkspaceLockFailureKind.Cancelled, path);
        }
        catch (Exception exception) when (WorkspaceLockTarget.IsFilesystemException(exception))
        {
            return InitializationFailure(WorkspaceLockTarget.FromException(path, exception));
        }
    }

    private static bool ContainsIgnoreRule(FileStream handle)
        => handle.Length == 2 && handle.ReadByte() == '*' && handle.ReadByte() == '\n';

    private static WorkspaceLockFailure InitializationFailure(WorkspaceLockFailure failure)
        => failure.Kind == WorkspaceLockFailureKind.Busy
            ? failure with { Kind = WorkspaceLockFailureKind.IoError }
            : failure;
}
