using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Navigation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;

internal sealed partial class RouteMoveSubjectSelector(
    SourceCatalogueReader catalogueReader,
    SourceReferenceResolver referenceResolver,
    SourceRouteFactsResolver routeFactsResolver,
    RouteNavigationExposureReader exposureReader)
{
    private readonly SourceCatalogueReader _catalogueReader = catalogueReader;
    private readonly SourceReferenceResolver _referenceResolver = referenceResolver;
    private readonly SourceRouteFactsResolver _routeFactsResolver = routeFactsResolver;
    private readonly RouteNavigationExposureReader _exposureReader = exposureReader;

    internal async ValueTask<RouteMoveSubjectSelection> SelectAsync(
        RouteMoveRequest request,
        CancellationToken cancellationToken)
    {
        var catalogueSelection = await ReadCatalogueAsync(request, cancellationToken)
            .ConfigureAwait(false);
        if (catalogueSelection.Boundary is { } catalogueBoundary)
        {
            return new RouteMoveSubjectSelection(subject: null, catalogueBoundary);
        }

        var catalogue = catalogueSelection.Catalogue
            ?? throw new InvalidOperationException(
                "A successful Route Move catalogue selection requires its catalogue.");
        var discovery = Discover(request, catalogue);
        if (discovery.Boundary is { } discoveryBoundary)
        {
            return new RouteMoveSubjectSelection(subject: null, discoveryBoundary);
        }

        return await InspectRouteAsync(
            discovery.Discovery
                ?? throw new InvalidOperationException(
                    "A successful Route Move source discovery requires its source."),
            cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<RouteMoveSubjectCatalogueSelection> ReadCatalogueAsync(
        RouteMoveRequest request,
        CancellationToken cancellationToken)
    {
        var formation = RouteMoveBoundary.Start(request);
        if (cancellationToken.IsCancellationRequested)
        {
            return new RouteMoveSubjectCatalogueSelection(
                catalogue: null,
                Boundary(
                    formation,
                    RouteMoveFindingCode.Interrupted,
                    CliSemanticStatus.Interrupted,
                    request.SourceReference,
                    "Route Move subject resolution was interrupted."));
        }

        var catalogue = await _catalogueReader.ReadAsync(
            new SourceCatalogueRequest(request.Workspace, [SourceLogicalPath.AgentsRoot]),
            cancellationToken).ConfigureAwait(false);
        if (catalogue.IsCancelled)
        {
            return new RouteMoveSubjectCatalogueSelection(
                catalogue: null,
                Boundary(
                    formation,
                    RouteMoveFindingCode.Interrupted,
                    CliSemanticStatus.Interrupted,
                    request.SourceReference,
                    "Route Move source discovery was interrupted."));
        }

        if (catalogue.Issues.FirstOrDefault(issue =>
                issue.Stage == SourceCatalogueIssueStage.Root) is { } rootIssue)
        {
            return new RouteMoveSubjectCatalogueSelection(
                catalogue: null,
                FromRootIssue(formation, rootIssue));
        }

        return new RouteMoveSubjectCatalogueSelection(catalogue, boundary: null);
    }

    private RouteMoveSubjectDiscoveryResult Discover(
        RouteMoveRequest request,
        SourceCatalogue catalogue)
    {
        var formation = RouteMoveBoundary.Start(request);
        var resolution = _referenceResolver.Resolve(request.SourceReference, catalogue);
        if (resolution.State != SourceReferenceResolutionState.Resolved
            || resolution.Source is not { } source)
        {
            return new RouteMoveSubjectDiscoveryResult(
                discovery: null,
                FromUnresolved(formation, resolution));
        }

        var sourceFacts = ProjectSource(request.SourceReference, resolution, source);
        formation = formation with { Source = sourceFacts };
        if (!TryReadForm(source.Base.Form, out var form, out var kind))
        {
            return new RouteMoveSubjectDiscoveryResult(
                discovery: null,
                Boundary(
                    formation,
                    RouteMoveFindingCode.InvalidSubject,
                    CliSemanticStatus.Invalid,
                    source.Identity.CanonicalBasePath,
                    "Route Move accepts routed ordinary Markdown leaves and recognized entrypoint categories only."));
        }

        return new RouteMoveSubjectDiscoveryResult(
            new RouteMoveSubjectDiscovery
            {
                Request = request,
                Catalogue = catalogue,
                SelectedSource = source,
                Source = sourceFacts with { Form = form },
                Kind = kind,
            },
            boundary: null);
    }

    private async ValueTask<RouteMoveSubjectSelection> InspectRouteAsync(
        RouteMoveSubjectDiscovery discovery,
        CancellationToken cancellationToken)
    {
        var routeFacts = await _routeFactsResolver.ResolveAsync(
            new SourceRouteFactsRequest(discovery.Catalogue, discovery.Catalogue.SelectAll()),
            new SourceDocumentReader(discovery.Request.Workspace),
            cancellationToken).ConfigureAwait(false);
        var boundary = ReadRouteBoundary(discovery, routeFacts);
        if (boundary is not null)
        {
            return new RouteMoveSubjectSelection(subject: null, boundary);
        }

        var exposure = await _exposureReader.ReadAsync(
            discovery.Request.Workspace,
            discovery.Catalogue,
            cancellationToken)
            .ConfigureAwait(false);
        var exposureBoundary = ReadExposureBoundary(discovery, routeFacts, exposure);
        return exposureBoundary is not null
            ? new RouteMoveSubjectSelection(subject: null, exposureBoundary)
            : new RouteMoveSubjectSelection(
                new RouteMoveResolvedSubject
                {
                    Request = discovery.Request,
                    Source = discovery.Source,
                    Kind = discovery.Kind,
                    Catalogue = discovery.Catalogue,
                    SelectedSource = discovery.SelectedSource,
                    RouteFacts = routeFacts,
                    NavigationExposure = exposure,
                },
                boundary: null);
    }

    private static RouteMoveSource ProjectSource(
        string requested,
        SourceReferenceResolution resolution,
        SourceLogicalSource source)
        => new()
        {
            Requested = requested,
            SelectedBy = ReadSelection(resolution, source),
            Id = source.Identity.AutomaticId,
            Path = source.Identity.CanonicalBasePath,
        };

    private static RouteMoveSourceSelection ReadSelection(
        SourceReferenceResolution resolution,
        SourceLogicalSource source)
    {
        if (resolution.Form == SourceReferenceKind.SourceId)
        {
            return RouteMoveSourceSelection.SourceId;
        }

        return string.Equals(
            resolution.CanonicalPath,
            source.Overwrite?.CanonicalPath,
            StringComparison.Ordinal)
            ? RouteMoveSourceSelection.OverwritePath
            : RouteMoveSourceSelection.BasePath;
    }

    private static bool TryReadForm(
        SourceDocumentForm sourceForm,
        out RouteMoveSourceForm form,
        out RouteMoveSubjectKind kind)
    {
        kind = SourceFormClassifier.IsEntrypoint(sourceForm)
            ? RouteMoveSubjectKind.Category
            : RouteMoveSubjectKind.Leaf;
        form = sourceForm switch
        {
            SourceDocumentForm.Markdown => RouteMoveSourceForm.OrdinaryMarkdown,
            SourceDocumentForm.CanonicalEntrypoint => RouteMoveSourceForm.CanonicalEntrypoint,
            SourceDocumentForm.IndexEntrypoint
                or SourceDocumentForm.UnderscoreIndexEntrypoint
                or SourceDocumentForm.ReferencesEntrypoint
                or SourceDocumentForm.UnderscoreReferencesEntrypoint =>
                RouteMoveSourceForm.CompatibilityEntrypoint,
            _ => default,
        };
        return sourceForm is SourceDocumentForm.Markdown
            or SourceDocumentForm.CanonicalEntrypoint
            or SourceDocumentForm.IndexEntrypoint
            or SourceDocumentForm.UnderscoreIndexEntrypoint
            or SourceDocumentForm.ReferencesEntrypoint
            or SourceDocumentForm.UnderscoreReferencesEntrypoint;
    }
}
