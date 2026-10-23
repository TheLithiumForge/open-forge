using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Locking.Shared.Interop;

internal static partial class UnixWorkspaceLock
{
    private const int LockExclusive = 2;
    private const int LockNonBlocking = 4;
    private const int Interrupted = 4;

    internal static WorkspaceLockFailure? TryAcquire(
        SafeFileHandle handle,
        string path,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return new(WorkspaceLockFailureKind.Cancelled, path);
        }

        if (OperatingSystem.IsWindows())
        {
            return null;
        }

        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
        {
            return new(WorkspaceLockFailureKind.Unsupported, path);
        }

        var addedReference = false;
        try
        {
            handle.DangerousAddRef(ref addedReference);
            var descriptor = checked((int)handle.DangerousGetHandle());
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return new(WorkspaceLockFailureKind.Cancelled, path);
                }

                var result = OperatingSystem.IsLinux()
                    ? LinuxFlock(descriptor, LockExclusive | LockNonBlocking)
                    : MacOSFlock(descriptor, LockExclusive | LockNonBlocking);
                var error = Marshal.GetLastPInvokeError();
                if (result == 0)
                {
                    return null;
                }

                if (error != Interrupted)
                {
                    return new(Classify(error), path, NativeError: error);
                }
            }
        }
        catch (Exception exception) when (exception is DllNotFoundException or EntryPointNotFoundException)
        {
            return new(WorkspaceLockFailureKind.Unsupported, path);
        }
        finally
        {
            if (addedReference)
            {
                handle.DangerousRelease();
            }
        }
    }

    private static WorkspaceLockFailureKind Classify(int error)
    {
        if (error == (OperatingSystem.IsMacOS() ? 35 : 11))
        {
            return WorkspaceLockFailureKind.Busy;
        }

        if (error is 1 or 13)
        {
            return WorkspaceLockFailureKind.AccessDenied;
        }

        var unsupported = OperatingSystem.IsMacOS()
            ? error is 45 or 78 or 77 or 102
            : error is 38 or 95 or 37;
        return unsupported ? WorkspaceLockFailureKind.Unsupported : WorkspaceLockFailureKind.IoError;
    }

    [LibraryImport("libc.so.6", EntryPoint = "flock", SetLastError = true)]
    private static partial int LinuxFlock(int fd, int operation);

    [LibraryImport("/usr/lib/libSystem.B.dylib", EntryPoint = "flock", SetLastError = true)]
    private static partial int MacOSFlock(int fd, int operation);
}
