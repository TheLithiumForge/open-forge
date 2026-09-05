using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal sealed partial class RouteRemoveSubjectSelector(
    SourceCatalogueReader catalogueReader,
    SourceReferenceResolver referenceResolver,
    SourceRouteFactsResolver routeFactsResolver,
    RouteRemoveNavigationExposureReader exposureReader)
{
    private readonly SourceCatalogueReader _catalogueReader = catalogueReader;
    private readonly SourceReferenceResolver _referenceResolver = referenceResolver;
    private readonly SourceRouteFactsResolver _routeFactsResolver = routeFactsResolver;
    private readonly RouteRemoveNavigationExposureReader _exposureReader = exposureReader;

    internal async ValueTask<RouteRemoveSubjectSelection> SelectAsync(
        RouteRemoveRequest request,
        CancellationToken cancellationToken)
    {
        var catalogueSelection = await ReadCatalogueAsync(request, cancellationToken)
            .ConfigureAwait(false);
        if (catalogueSelection.Boundary is { } catalogueBoundary)
        {
            return new RouteRemoveSubjectSelection(subject: null, catalogueBoundary);
        }

        var catalogue = catalogueSelection.Catalogue
            ?? throw new InvalidOperationException(
                "A successful Route Remove catalogue selection requires its catalogue.");
        var discovery = Discover(request, catalogue);
        if (discovery.Boundary is { } discoveryBoundary)
        {
            return new RouteRemoveSubjectSelection(subject: null, discoveryBoundary);
        }

        return await InspectRouteAsync(
            discovery.Discovery
                ?? throw new InvalidOperationException(
                    "A successful Route Remove source discovery requires its source."),
            cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<RouteRemoveSubjectCatalogueSelection> ReadCatalogueAsync(
        RouteRemoveRequest request,
        CancellationToken cancellationToken)
    {
        var formation = RouteRemoveBoundary.Start(request);
        if (cancellationToken.IsCancellationRequested)
        {
            return new RouteRemoveSubjectCatalogueSelection(
                catalogue: null,
                Boundary(
                    formation,
                    RouteRemoveFindingCode.Interrupted,
                    CliSemanticStatus.Interrupted,
                    request.SourceReference,
                    "Route Remove subject resolution was interrupted."));
        }

        var catalogue = await _catalogueReader.ReadAsync(
            new SourceCatalogueRequest(request.Workspace, [SourceLogicalPath.AgentsRoot]),
            cancellationToken).ConfigureAwait(false);
        if (catalogue.IsCancelled)
        {
            return new RouteRemoveSubjectCatalogueSelection(
                catalogue: null,
                Boundary(
                    formation,
                    RouteRemoveFindingCode.Interrupted,
                    CliSemanticStatus.Interrupted,
                    request.SourceReference,
                    "Route Remove source discovery was interrupted."));
        }

        if (catalogue.Issues.FirstOrDefault(issue =>
                issue.Stage == SourceCatalogueIssueStage.Root) is { } rootIssue)
        {
            return new RouteRemoveSubjectCatalogueSelection(
                catalogue: null,
                FromRootIssue(formation, rootIssue));
        }

        return new RouteRemoveSubjectCatalogueSelection(catalogue, boundary: null);
    }

    private RouteRemoveSubjectDiscoveryResult Discover(
        RouteRemoveRequest request,
        SourceCatalogue catalogue)
    {
        var formation = RouteRemoveBoundary.Start(request);
        var resolution = _referenceResolver.Resolve(request.SourceReference, catalogue);
        if (resolution.State != SourceReferenceResolutionState.Resolved
            || resolution.Source is not { } source)
        {
            return new RouteRemoveSubjectDiscoveryResult(
                discovery: null,
                FromUnresolved(formation, resolution));
        }

        var sourceFacts = ProjectSource(request.SourceReference, resolution, source);
        formation = formation with { Source = sourceFacts };
        if (!TryReadForm(source.Base.Form, out var form, out var kind))
        {
            return new RouteRemoveSubjectDiscoveryResult(
                discovery: null,
                Boundary(
                    formation,
                    RouteRemoveFindingCode.InvalidSubject,
                    CliSemanticStatus.Invalid,
                    source.Identity.CanonicalBasePath,
                    "Route Remove accepts routed ordinary Markdown leaves and recognized entrypoint categories only."));
        }

        return new RouteRemoveSubjectDiscoveryResult(
            new RouteRemoveSubjectDiscovery
            {
                Request = request,
                Catalogue = catalogue,
                SelectedSource = source,
                Source = sourceFacts with { Form = form },
                Kind = kind,
            },
            boundary: null);
    }

    private async ValueTask<RouteRemoveSubjectSelection> InspectRouteAsync(
        RouteRemoveSubjectDiscovery discovery,
        CancellationToken cancellationToken)
    {
        var routeFacts = await _routeFactsResolver.ResolveAsync(
            new SourceRouteFactsRequest(discovery.Catalogue, discovery.Catalogue.SelectAll()),
            new SourceDocumentReader(discovery.Request.Workspace),
            cancellationToken).ConfigureAwait(false);
        var boundary = ReadRouteBoundary(discovery, routeFacts);
        if (boundary is not null)
        {
            return new RouteRemoveSubjectSelection(subject: null, boundary);
        }

        var exposure = await _exposureReader.ReadAsync(discovery, cancellationToken)
            .ConfigureAwait(false);
        var exposureBoundary = ReadExposureBoundary(discovery, routeFacts, exposure);
        return exposureBoundary is not null
            ? new RouteRemoveSubjectSelection(subject: null, exposureBoundary)
            : new RouteRemoveSubjectSelection(
                new RouteRemoveResolvedSubject
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

    private static RouteRemoveSource ProjectSource(
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

    private static RouteRemoveSourceSelection ReadSelection(
        SourceReferenceResolution resolution,
        SourceLogicalSource source)
    {
        if (resolution.Form == SourceReferenceKind.SourceId)
        {
            return RouteRemoveSourceSelection.SourceId;
        }

        return string.Equals(
            resolution.CanonicalPath,
            source.Overwrite?.CanonicalPath,
            StringComparison.Ordinal)
            ? RouteRemoveSourceSelection.OverwritePath
            : RouteRemoveSourceSelection.BasePath;
    }

    private static bool TryReadForm(
        SourceDocumentForm sourceForm,
        out RouteRemoveSourceForm form,
        out RouteRemoveSubjectKind kind)
    {
        kind = SourceFormClassifier.IsEntrypoint(sourceForm)
            ? RouteRemoveSubjectKind.Category
            : RouteRemoveSubjectKind.Leaf;
        form = sourceForm switch
        {
            SourceDocumentForm.Markdown => RouteRemoveSourceForm.OrdinaryMarkdown,
            SourceDocumentForm.CanonicalEntrypoint => RouteRemoveSourceForm.CanonicalEntrypoint,
            SourceDocumentForm.IndexEntrypoint
                or SourceDocumentForm.UnderscoreIndexEntrypoint
                or SourceDocumentForm.ReferencesEntrypoint
                or SourceDocumentForm.UnderscoreReferencesEntrypoint =>
                RouteRemoveSourceForm.CompatibilityEntrypoint,
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
