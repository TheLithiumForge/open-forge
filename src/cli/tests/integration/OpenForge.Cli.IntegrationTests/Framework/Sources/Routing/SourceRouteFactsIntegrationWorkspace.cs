using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.Framework.Sources.Shared;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Routing;

internal sealed class SourceRouteFactsIntegrationWorkspace : IDisposable
{
    private readonly SourceIntegrationWorkspace _workspace;

    private SourceRouteFactsIntegrationWorkspace(SourceIntegrationWorkspace workspace)
    {
        _workspace = workspace;
    }

    internal string Path => _workspace.Path;

    internal CliWorkspace Workspace => _workspace.Workspace;

    internal static SourceRouteFactsIntegrationWorkspace Create()
    {
        return new SourceRouteFactsIntegrationWorkspace(
            SourceIntegrationWorkspace.Create("source-route-facts-integration"));
    }

    internal void Write(string relativePath, string contents)
    {
        _workspace.Write(relativePath, contents);
    }

    internal void Write(string relativePath, byte[] contents)
    {
        _workspace.Write(relativePath, contents);
    }

    internal void WriteLoader(string entries)
    {
        Write(".agents/loader.md", GeneratedLoaderDocumentBuilder.Build(entries));
    }

    internal string Absolute(string relativePath)
    {
        return _workspace.Absolute(relativePath);
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

    internal SourceLogicalSource Source(
        string canonicalPath,
        string automaticId,
        SourceDocumentForm form = SourceDocumentForm.Markdown,
        bool withOverwrite = false)
    {
        var baseLayer = new SourceLayer(
            canonicalPath,
            Absolute(canonicalPath),
            form,
            SourceLayerKind.Base);
        var overwritePath = canonicalPath[..^".md".Length] + ".overwrite.md";
        var overwrite = withOverwrite
            ? new SourceLayer(
                overwritePath,
                Absolute(overwritePath),
                SourceDocumentForm.OverwriteCompanion,
                SourceLayerKind.Overwrite)
            : null;
        return new SourceLogicalSource(
            new SourceLogicalIdentity(automaticId, canonicalPath),
            baseLayer,
            overwrite);
    }

    public void Dispose()
    {
        _workspace.Dispose();
    }
}
