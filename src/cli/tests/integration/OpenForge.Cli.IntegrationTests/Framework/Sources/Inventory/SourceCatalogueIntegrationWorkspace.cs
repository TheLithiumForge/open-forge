using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.Framework.Sources.Shared;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Inventory;

internal sealed class SourceCatalogueIntegrationWorkspace : IDisposable
{
    private readonly SourceIntegrationWorkspace _workspace;

    private SourceCatalogueIntegrationWorkspace(SourceIntegrationWorkspace workspace)
    {
        _workspace = workspace;
    }

    internal string Path => _workspace.Path;

    internal CliWorkspace Workspace => _workspace.Workspace;

    internal static SourceCatalogueIntegrationWorkspace Create()
    {
        return new SourceCatalogueIntegrationWorkspace(
            SourceIntegrationWorkspace.Create("source-catalogue-inventory-integration"));
    }

    internal SourceCatalogueRequest Request(params string[] logicalRoots)
    {
        return new SourceCatalogueRequest(
            Workspace,
            logicalRoots.Length == 0 ? [".agents"] : logicalRoots);
    }

    internal void Write(string relativePath, string contents)
    {
        _workspace.Write(relativePath, contents);
    }

    internal void Write(string relativePath, byte[] contents)
    {
        _workspace.Write(relativePath, contents);
    }

    internal string CreateDirectory(string relativePath)
    {
        return _workspace.CreateDirectory(relativePath);
    }

    internal string Absolute(string relativePath)
    {
        return _workspace.Absolute(relativePath);
    }

    internal bool TryCreateDirectorySymbolicLink(
        string relativeLinkPath,
        string targetPath,
        out string? linkPath)
    {
        return _workspace.TryCreateDirectorySymbolicLink(relativeLinkPath, targetPath, out linkPath);
    }

    internal bool TryCreateFileSymbolicLink(
        string relativeLinkPath,
        string targetPath,
        out string? linkPath)
    {
        return _workspace.TryCreateFileSymbolicLink(relativeLinkPath, targetPath, out linkPath);
    }

    internal IReadOnlyDictionary<string, string> SnapshotHashes()
    {
        return _workspace.SnapshotHashes();
    }

    public void Dispose()
    {
        _workspace.Dispose();
    }
}
