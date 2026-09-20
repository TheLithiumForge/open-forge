using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;

internal sealed class LibraryOutputArtifacts : IDisposable
{
    private readonly LibraryMutationWorkspace _workspace;
    private readonly string _lockPath;
    private readonly string _recoveryDirectory;
    private readonly HashSet<string> _links = new(StringComparer.Ordinal);
    private readonly HashSet<string> _recovery = new(StringComparer.Ordinal);
    private readonly HashSet<string> _ordinaryFiles = new(StringComparer.Ordinal);
    private bool _disposed;

    internal LibraryOutputArtifacts(LibraryMutationWorkspace workspace)
    {
        _workspace = workspace;
        var locks = WorkspaceLockStoreRoot.ResolveForCurrentUser(Environment.SpecialFolderOption.None)
            ?? throw new InvalidOperationException("Library output fixtures require a lock root.");
        _lockPath = WorkspaceLockPathIdentity.LockPath(locks, workspace.Workspace);
        var recovery = RecoveryBundlePathIdentity.ResolveStoreRoot(Environment.SpecialFolderOption.None)
            ?? throw new InvalidOperationException("Library output fixtures require a recovery root.");
        _recoveryDirectory = RecoveryBundlePathIdentity.WorkspaceDirectory(recovery, workspace.Path);
        Assert.False(File.Exists(_lockPath));
        Assert.False(Directory.Exists(_recoveryDirectory));
    }

    internal void OwnLink(string relativePath) => _links.Add(relativePath);

    internal void OwnRecovery(LibraryRecoveryView recovery)
    {
        var path = recovery.Path;
        if (path is null) return;
        Assert.Equal(_recoveryDirectory, Path.GetDirectoryName(path));
        _recovery.Add(path);
        Assert.True(recovery.State is LibraryRecoveryState.Removed or LibraryRecoveryState.Retained or LibraryRecoveryState.Prepared);
        Assert.Equal(recovery.State != LibraryRecoveryState.Removed, File.Exists(path));
    }

    internal void OwnSettings()
    {
        const string path = ".agents/open-forge.json";
        Assert.False(File.Exists(_workspace.Absolute(path)));
        _ordinaryFiles.Add(path);
    }

    internal FileStream HoldLock()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_lockPath)!);
        return new FileStream(_lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var relativePath in _links)
        {
            var path = _workspace.Absolute(relativePath);
            TryDeleteLink(path);
        }
        foreach (var path in _recovery)
        {
            TryDeleteFile(path);
        }
        foreach (var relativePath in _ordinaryFiles)
        {
            var path = _workspace.Absolute(relativePath);
            if (!File.Exists(path)) continue;
            TryDeleteFile(path);
        }
        TryDeleteDirectory(_recoveryDirectory);
        TryDeleteFile(_lockPath);
    }

    private static void TryDeleteLink(string path)
    {
        try
        {
            if (new FileInfo(path).LinkTarget is not null) File.Delete(path);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
