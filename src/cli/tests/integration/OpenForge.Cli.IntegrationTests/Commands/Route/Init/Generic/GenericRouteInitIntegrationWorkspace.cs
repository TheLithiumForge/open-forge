using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init.Generic;

internal sealed class GenericRouteInitIntegrationWorkspace : IDisposable
{
    private const string OwnershipMarker = ".open-forge-test-workspace-owner";

    private readonly TemporaryWorkspace _temporary;
    private readonly WorkspaceLockTestStore _lockStore;
    private bool _disposed;

    private GenericRouteInitIntegrationWorkspace(
        TemporaryWorkspace temporary,
        WorkspaceLockTestStore lockStore)
    {
        _temporary = temporary;
        _lockStore = lockStore;
        Workspace = new CliWorkspace(
            temporary.Path,
            temporary.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        _ = _lockStore.Track(Workspace);
    }

    internal string Path => _temporary.Path;

    internal CliWorkspace Workspace { get; }

    internal WorkspaceLockStoreRoot LockStoreRoot => _lockStore.StoreRoot;

    internal static GenericRouteInitIntegrationWorkspace Create(string purpose)
    {
        var temporary = TemporaryWorkspace.Create(purpose);
        var lockStore = WorkspaceLockTestStore.Create($"{purpose}-lock-store");
        try
        {
            return new GenericRouteInitIntegrationWorkspace(temporary, lockStore);
        }
        catch
        {
            lockStore.Dispose();
            temporary.Dispose();
            throw;
        }
    }

    internal RouteInitRequest Request(
        string routeTarget,
        RouteInitMode mode = RouteInitMode.Apply,
        RouteInitMetadataInput? metadata = null)
        => new(
            Workspace,
            routeTarget,
            RouteInitScaffold.Generic,
            mode,
            metadata ?? RouteInitMetadataInput.None);

    internal void WriteText(string relativePath, string contents)
        => _temporary.WriteText(relativePath, contents);

    internal void WriteBytes(string relativePath, byte[] contents)
        => _temporary.WriteBytes(relativePath, contents);

    internal void CreateDirectory(string relativePath)
        => _temporary.CreateDirectory(relativePath);

    internal bool TryCreateDirectorySymbolicLink(
        string relativeLinkPath,
        string targetPath,
        out string? linkPath)
        => _temporary.TryCreateDirectorySymbolicLink(relativeLinkPath, targetPath, out linkPath);

    internal bool TryCreateFileSymbolicLink(
        string relativeLinkPath,
        string targetPath,
        out string? linkPath)
        => _temporary.TryCreateFileSymbolicLink(relativeLinkPath, targetPath, out linkPath);

    internal string ReadText(string relativePath)
        => File.ReadAllText(_temporary.Combine(relativePath));

    internal bool Exists(string relativePath)
        => File.Exists(_temporary.Combine(relativePath))
            || Directory.Exists(_temporary.Combine(relativePath));

    internal string Absolute(string relativePath)
        => _temporary.Combine(relativePath);

    internal IReadOnlyDictionary<string, string> SnapshotHashes()
        => _temporary.SnapshotHashes();

    internal FileStream HoldLock()
        => _lockStore.OpenExclusive(Workspace);

    internal async ValueTask<int> RecoveryCandidateCountAsync(
        CancellationToken cancellationToken)
    {
        var result = await new RecoveryBundleCatalogue(new RecoveryBundleReader())
            .ReadAsync(Workspace, cancellationToken);
        return result.State switch
        {
            RecoveryBundleCatalogueState.Available => result.Candidates.Length,
            RecoveryBundleCatalogueState.Unavailable => throw new InvalidOperationException(
                "The Route Init recovery catalogue was unavailable."),
            RecoveryBundleCatalogueState.Cancelled => throw new InvalidOperationException(
                "The Route Init recovery catalogue was cancelled."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(result),
                result.State,
                "The Route Init recovery catalogue state is not defined."),
        };
    }

    internal string? RecoveryDirectoryPath()
    {
        var storeRoot = RecoveryBundlePathIdentity.ResolveStoreRoot(
            Environment.SpecialFolderOption.None);
        return storeRoot is null
            ? null
            : RecoveryBundlePathIdentity.WorkspaceDirectory(
                storeRoot,
                Workspace.PhysicalRoot);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        DeleteRecoveryArtifacts();
        DeleteWorkspaceEntries();
        _lockStore.Dispose();
        _temporary.Dispose();
        _disposed = true;
    }

    private void DeleteWorkspaceEntries()
    {
        foreach (var path in Directory.EnumerateFileSystemEntries(Path)
                     .Where(path => !string.Equals(
                         System.IO.Path.GetFileName(path),
                         OwnershipMarker,
                         StringComparison.Ordinal))
                     .ToArray())
        {
            DeleteEntryWithoutFollowingLinks(path);
        }
    }

    private void DeleteRecoveryArtifacts()
    {
        var storeRoot = RecoveryBundlePathIdentity.ResolveStoreRoot(
            Environment.SpecialFolderOption.None);
        if (storeRoot is null)
        {
            return;
        }

        var directory = RecoveryBundlePathIdentity.WorkspaceDirectory(
            storeRoot,
            Workspace.PhysicalRoot);
        if (!Directory.Exists(directory))
        {
            return;
        }

        var attributes = File.GetAttributes(directory);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device))
            != FileAttributes.Directory)
        {
            throw new InvalidOperationException(
                "The Route Init recovery cleanup target is not an ordinary directory.");
        }

        foreach (var path in Directory.EnumerateFileSystemEntries(directory).ToArray())
        {
            if (!RecoveryBundleFormatV1.TryParseCandidateFileName(
                    System.IO.Path.GetFileName(path),
                    out _,
                    out _))
            {
                throw new InvalidOperationException(
                    "The Route Init recovery directory contains an unrecognized artifact.");
            }

            DeleteEntryWithoutFollowingLinks(path);
        }

        Directory.Delete(directory);
    }

    private static void DeleteEntryWithoutFollowingLinks(string path)
    {
        var attributes = File.GetAttributes(path);
        if ((attributes & FileAttributes.ReparsePoint) != 0)
        {
            File.Delete(path);
            return;
        }

        if ((attributes & FileAttributes.Directory) != 0)
        {
            foreach (var child in Directory.EnumerateFileSystemEntries(path).ToArray())
            {
                DeleteEntryWithoutFollowingLinks(child);
            }

            Directory.Delete(path);
            return;
        }

        File.Delete(path);
    }
}
