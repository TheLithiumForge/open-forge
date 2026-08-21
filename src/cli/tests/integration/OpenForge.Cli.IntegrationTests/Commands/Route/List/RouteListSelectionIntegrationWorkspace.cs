using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List;

internal sealed class RouteListSelectionIntegrationWorkspace : IDisposable
{
    private readonly TemporaryWorkspace _temporary;

    private RouteListSelectionIntegrationWorkspace(TemporaryWorkspace temporary)
    {
        _temporary = temporary;
        _temporary.CreateDirectory(".agents");
    }

    internal string Path => _temporary.Path;

    internal CliWorkspace Workspace => new(
        Path,
        Path,
        CliWorkspaceSelectionMethod.ExplicitWorkspace);

    internal static RouteListSelectionIntegrationWorkspace Create()
    {
        return new RouteListSelectionIntegrationWorkspace(
            TemporaryWorkspace.Create("route-list-selection-integration"));
    }

    internal void Write(string relativePath, string contents)
    {
        _temporary.WriteText(relativePath, contents);
    }

    internal void WriteLoader(string entries)
    {
        Write(".agents/loader.md", $"""
            # Open Forge Loader

            ## Entries

            <!-- open-forge:generated-index:start -->
            {entries}
            <!-- open-forge:generated-index:end -->
            """);
    }

    internal string Absolute(string relativePath)
    {
        return _temporary.Combine(relativePath);
    }

    internal RouteListSource Source(
        string id,
        string canonicalPath,
        RouteListSourceKind kind = RouteListSourceKind.RoutedLeaf,
        string? physicalRelativePath = null,
        string? overwritePath = null,
        bool isRouteAmbiguous = false)
    {
        var physicalPath = _temporary.Combine(physicalRelativePath ?? canonicalPath);
        return new RouteListSource(
            id,
            canonicalPath,
            physicalPath,
            kind,
            overwritePath,
            isRouteAmbiguous);
    }

    internal RouteListSourceCatalogue Catalogue(params RouteListSource[] sources)
    {
        return new RouteListSourceCatalogue(sources);
    }

    internal RouteListRequest Request(string? sourceReference = null)
    {
        return new RouteListRequest(
            Workspace,
            sourceReference,
            RouteListDepth.Default);
    }

    internal IReadOnlyDictionary<string, string> SnapshotHashes()
    {
        return _temporary.SnapshotHashes();
    }

    internal bool TryCreateDirectorySymbolicLink(
        string relativeLinkPath,
        string targetPath,
        out string? linkPath)
    {
        return _temporary.TryCreateDirectorySymbolicLink(relativeLinkPath, targetPath, out linkPath);
    }

    internal bool TryCreateFileSymbolicLink(
        string relativeLinkPath,
        string targetPath,
        out string? linkPath)
    {
        return _temporary.TryCreateFileSymbolicLink(relativeLinkPath, targetPath, out linkPath);
    }

    public void Dispose()
    {
        _temporary.Dispose();
    }
}
