using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.TestSupport;

internal sealed class WorkspaceLockTestStore : IDisposable
{
    private static readonly StringComparer PathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;
    private readonly TemporaryWorkspace _temporary;
    private readonly HashSet<string> _lockPaths = new(PathComparer);
    private bool _disposed;

    private WorkspaceLockTestStore(TemporaryWorkspace temporary)
    {
        _temporary = temporary;
        StoreRoot = WorkspaceLockStoreRoot.FromAbsolutePath(temporary.Path);
    }

    internal WorkspaceLockStoreRoot StoreRoot { get; }

    internal WorkspaceLockManager Manager => new(StoreRoot);

    internal bool InfrastructureExists
        => Directory.Exists(WorkspaceLockPathIdentity.StoreDirectory(StoreRoot));

    internal static WorkspaceLockTestStore Create(string purpose)
        => new(TemporaryWorkspace.Create(purpose));

    internal string Track(CliWorkspace workspace)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var path = WorkspaceLockPathIdentity.LockPath(StoreRoot, workspace);
        _lockPaths.Add(path);
        return path;
    }

    internal ValueTask<WorkspaceLockResult> AcquireAsync(
        WorkspaceLockRequest request,
        CancellationToken cancellationToken)
    {
        _ = Track(request.Workspace);
        return Manager.AcquireAsync(request, cancellationToken);
    }

    internal FileStream OpenExclusive(CliWorkspace workspace)
    {
        var path = Track(workspace);
        Directory.CreateDirectory(Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException("The external lock path requires a directory."));
        return new FileStream(
            path,
            new FileStreamOptions
            {
                Mode = FileMode.OpenOrCreate,
                Access = FileAccess.ReadWrite,
                Share = FileShare.None,
                Options = FileOptions.Asynchronous,
            });
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var lockPath in _lockPaths.OrderByDescending(path => path, PathComparer))
        {
            DeleteExactLockTarget(lockPath);
        }

        DeleteEmptyDirectory(WorkspaceLockPathIdentity.StoreDirectory(StoreRoot));
        DeleteEmptyDirectory(Path.Combine(
            StoreRoot.Path,
            WorkspaceLockPathIdentity.ApplicationDirectoryName,
            WorkspaceLockPathIdentity.LocksDirectoryName));
        DeleteEmptyDirectory(Path.Combine(
            StoreRoot.Path,
            WorkspaceLockPathIdentity.ApplicationDirectoryName));
        _temporary.Dispose();
        _disposed = true;
    }

    private static void DeleteExactLockTarget(string path)
    {
        try
        {
            var attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.Directory) != 0)
            {
                Directory.Delete(path, recursive: false);
            }
            else
            {
                File.Delete(path);
            }
        }
        catch (FileNotFoundException)
        {
        }
        catch (DirectoryNotFoundException)
        {
        }
    }

    private static void DeleteEmptyDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: false);
        }
    }
}
