using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Locking.Shared.Runtime;

internal static class WorkspaceLockTarget
{
    internal static WorkspaceLockFailure? Validate(string path, bool directory, bool allowMissing)
    {
        try
        {
            FileSystemInfo info = directory ? new DirectoryInfo(path) : new FileInfo(path);
            if (info.LinkTarget is not null)
            {
                return Invalid(path);
            }

            var attributes = File.GetAttributes(path);
            return (attributes & (FileAttributes.ReparsePoint | FileAttributes.Device)) != 0
                || ((attributes & FileAttributes.Directory) != 0) != directory
                ? Invalid(path)
                : null;
        }
        catch (Exception exception) when (allowMissing && exception is FileNotFoundException or DirectoryNotFoundException)
        {
            return null;
        }
        catch (Exception exception) when (IsFilesystemException(exception))
        {
            return FromException(path, exception);
        }
    }

    internal static WorkspaceLockFailure Invalid(string path)
        => new(WorkspaceLockFailureKind.InvalidTarget, path);

    internal static bool IsFilesystemException(Exception exception)
        => exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException;

    internal static WorkspaceLockFailure FromException(string path, Exception exception)
    {
        var detail = FilesystemFailure.FromException(FilesystemFailure.ClassifyException(exception), exception);
        var error = exception.HResult & 0xffff;
        var kind = detail.Kind switch
        {
            FilesystemFailureKind.AccessDenied => WorkspaceLockFailureKind.AccessDenied,
            FilesystemFailureKind.Unsupported => WorkspaceLockFailureKind.Unsupported,
            FilesystemFailureKind.InvalidPath => WorkspaceLockFailureKind.InvalidTarget,
            FilesystemFailureKind.InputOutput => WorkspaceLockFailureKind.IoError,
            _ => throw new ArgumentOutOfRangeException(nameof(exception)),
        };
        var wouldBlock = (OperatingSystem.IsWindows() && error is 32 or 33)
            || (OperatingSystem.IsLinux() && error == 11)
            || (OperatingSystem.IsMacOS() && error == 35);
        if (exception is IOException && wouldBlock)
        {
            kind = WorkspaceLockFailureKind.Busy;
        }
        else if (!OperatingSystem.IsWindows() && exception is IOException && error is 6 or 19)
        {
            kind = WorkspaceLockFailureKind.InvalidTarget;
        }

        return new(kind, path, detail, error);
    }
}
