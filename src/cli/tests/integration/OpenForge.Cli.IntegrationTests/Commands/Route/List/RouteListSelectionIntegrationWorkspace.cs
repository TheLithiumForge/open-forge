using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Workspace.Models;
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
        Write(".agents/loader.md", GeneratedLoaderDocumentBuilder.Build(entries));
    }

    internal string Absolute(string relativePath)
    {
        return _temporary.Combine(relativePath);
    }

    internal RouteSource Source(
        string id,
        string canonicalPath,
        RouteListSourceKind kind = RouteListSourceKind.RoutedLeaf,
        string? physicalRelativePath = null,
        string? overwritePath = null,
        bool isRouteAmbiguous = false)
    {
        var physicalPath = _temporary.Combine(physicalRelativePath ?? canonicalPath);
        var form = ReadForm(canonicalPath, kind);
        var sourceKind = form switch
        {
            SourceDocumentForm.Loader => RouteSourceKind.Loader,
            SourceDocumentForm.CanonicalEntrypoint
                or SourceDocumentForm.IndexEntrypoint
                or SourceDocumentForm.UnderscoreIndexEntrypoint
                or SourceDocumentForm.ReferencesEntrypoint
                or SourceDocumentForm.UnderscoreReferencesEntrypoint => RouteSourceKind.Entrypoint,
            SourceDocumentForm.Skill => RouteSourceKind.Native,
            SourceDocumentForm.Markdown => RouteSourceKind.Markdown,
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The source form is not defined."),
        };
        var metadata = form == SourceDocumentForm.Loader
            ? RouteSourceMetadata.WithoutValues(
                RouteSourceMetadataState.NotApplicable,
                isCompatibilityEntrypoint: false,
                isOverwritePresent: overwritePath is not null)
            : kind == RouteListSourceKind.Unrouted
            ? RouteSourceMetadata.WithoutValues(
                RouteSourceMetadataState.Missing,
                IsCompatibility(form),
                overwritePath is not null)
            : RouteSourceMetadata.Complete(
                $"Description for {id}",
                form == SourceDocumentForm.Skill ? [] : ["Route"],
                IsCompatibility(form),
                overwritePath is not null);
        var baseDocument = new RouteSourceDocument(
            canonicalPath,
            physicalPath,
            form,
            FileReadState.Complete,
            "body");
        var overwrite = overwritePath is null
            ? null
            : new RouteSourceDocument(
                overwritePath,
                _temporary.Combine(overwritePath),
            SourceDocumentForm.OverwriteCompanion,
                FileReadState.Complete,
                "overwrite");
        var source = new RouteSource(
            baseDocument,
            metadata,
            sourceKind,
            overwrite,
            isRouteAmbiguous);
        if (!string.Equals(id, source.Id, StringComparison.Ordinal))
        {
            throw new ArgumentException("The source ID must match its canonical path identity.", nameof(id));
        }

        return source;
    }

    internal async ValueTask<RouteListSelectionIntegrationBoundary> BoundaryAsync(
        CancellationToken cancellationToken)
    {
        var catalogue = await new SourceCatalogueReader()
            .ReadAsync(Request(".agents"), cancellationToken);
        var selection = catalogue.SelectAll();
        var reader = new SourceDocumentReader(Workspace);
        var projection = await new RouteListSourceProjectionBuilder()
            .ReadAsync(selection, reader, cancellationToken);
        var routeFacts = await new SourceRouteFactsResolver()
            .ResolveAsync(
                new SourceRouteFactsRequest(catalogue, selection),
                reader,
                cancellationToken);
        return new RouteListSelectionIntegrationBoundary(
            catalogue,
            projection.ProjectionSet,
            routeFacts);
    }

    internal SourceCatalogueRequest Request(params string[] logicalRoots)
    {
        return new SourceCatalogueRequest(Workspace, logicalRoots);
    }

    internal RouteListRequest RouteRequest(string? sourceReference = null)
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

    private static SourceDocumentForm ReadForm(string path, RouteListSourceKind kind)
    {
        if (kind == RouteListSourceKind.Loader)
        {
            return SourceDocumentForm.Loader;
        }

        var fileName = path[(path.LastIndexOf('/') + 1)..];
        if (fileName == "SKILL.md")
        {
            return SourceDocumentForm.Skill;
        }

        if (kind != RouteListSourceKind.Entrypoint)
        {
            return SourceDocumentForm.Markdown;
        }

        return fileName switch
        {
            "index.md" => SourceDocumentForm.IndexEntrypoint,
            "_index.md" => SourceDocumentForm.UnderscoreIndexEntrypoint,
            "references.md" => SourceDocumentForm.ReferencesEntrypoint,
            "_references.md" => SourceDocumentForm.UnderscoreReferencesEntrypoint,
            _ => SourceDocumentForm.CanonicalEntrypoint,
        };
    }

    private static bool IsCompatibility(SourceDocumentForm form)
    {
        return form is SourceDocumentForm.IndexEntrypoint
            or SourceDocumentForm.UnderscoreIndexEntrypoint
            or SourceDocumentForm.ReferencesEntrypoint
            or SourceDocumentForm.UnderscoreReferencesEntrypoint;
    }
}

internal sealed record RouteListSelectionIntegrationBoundary(
    SourceCatalogue Catalogue,
    RouteSourceProjectionSet ProjectionSet,
    SourceRouteFacts RouteFacts);
