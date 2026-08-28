using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

internal sealed class WorkspaceLockLease : IDisposable, IAsyncDisposable
{
    private readonly FileStream _handle;
    private int _disposed;

    internal WorkspaceLockLease(
        WorkspaceLockRequest request,
        string logicalPath,
        string physicalPath,
        FileStream handle)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(handle);
        if (!handle.CanWrite || handle.SafeFileHandle.IsInvalid || handle.SafeFileHandle.IsClosed)
        {
            throw new ArgumentException(
                "A workspace lock lease requires one open writable OS handle.",
                nameof(handle));
        }

        var normalizedLogicalPath = FileExpectation.NormalizeAbsolutePath(
            logicalPath,
            nameof(logicalPath));
        if (!string.Equals(
                normalizedLogicalPath,
                request.LogicalPath,
                PathComparison()))
        {
            throw new ArgumentException(
                "A workspace lock lease logical path must match its request.",
                nameof(logicalPath));
        }

        var normalizedPhysicalPath = FileExpectation.NormalizeAbsolutePath(
            physicalPath,
            nameof(physicalPath));
        if (!PhysicalContainment.Contains(
                request.Workspace.PhysicalRoot,
                normalizedPhysicalPath))
        {
            throw new ArgumentException(
                "A workspace lock lease physical path must be contained by its workspace.",
                nameof(physicalPath));
        }

        var openedPhysicalPath = FileExpectation.NormalizeAbsolutePath(
            handle.Name,
            nameof(handle));
        if (!string.Equals(
                normalizedPhysicalPath,
                openedPhysicalPath,
                PathComparison()))
        {
            throw new ArgumentException(
                "A workspace lock lease physical path must match its open handle.",
                nameof(physicalPath));
        }

        Request = request;
        LogicalPath = normalizedLogicalPath;
        PhysicalPath = normalizedPhysicalPath;
        _handle = handle;
    }

    internal WorkspaceLockRequest Request { get; }

    internal string LogicalPath { get; }

    internal string PhysicalPath { get; }

    internal bool IsHeld => Volatile.Read(ref _disposed) == 0
        && !_handle.SafeFileHandle.IsClosed
        && !_handle.SafeFileHandle.IsInvalid;

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
        => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
}
