using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Interaction;
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
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal sealed partial class RouteRemoveSubjectSelector(
    SourceCatalogueReader catalogueReader,
    SourceReferenceResolver referenceResolver,
    SourceRouteFactsResolver routeFactsResolver,
    RouteNavigationExposureReader exposureReader,
    CliPrompt<RouteRemoveSourceSelectionQuestion, string>? sourceSelectionPrompt = null)
{
    private readonly SourceCatalogueReader _catalogueReader = catalogueReader;
    private readonly SourceReferenceResolver _referenceResolver = referenceResolver;
    private readonly SourceRouteFactsResolver _routeFactsResolver = routeFactsResolver;
    private readonly RouteNavigationExposureReader _exposureReader = exposureReader;
    private readonly CliPrompt<RouteRemoveSourceSelectionQuestion, string>? _sourceSelectionPrompt = sourceSelectionPrompt;

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
        var discovery = await DiscoverAsync(request, catalogue, cancellationToken)
            .ConfigureAwait(false);
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

    private async ValueTask<RouteRemoveSubjectDiscoveryResult> DiscoverAsync(
        RouteRemoveRequest request,
        SourceCatalogue catalogue,
        CancellationToken cancellationToken)
    {
        var formation = RouteRemoveBoundary.Start(request);
        var resolution = _referenceResolver.Resolve(request.ResolutionReference, catalogue);
        if (resolution.State == SourceReferenceResolutionState.Ambiguous
            && request.FrozenSourceReference is null)
        {
            var choice = await SelectAmbiguousSourceAsync(request, resolution, cancellationToken)
                .ConfigureAwait(false);
            if (choice.State == CliPromptState.Cancelled)
            {
                return new RouteRemoveSubjectDiscoveryResult(
                    discovery: null,
                    Boundary(
                        formation,
                        RouteRemoveFindingCode.Interrupted,
                        CliSemanticStatus.Interrupted,
                        request.SourceReference,
                        "Route remove was cancelled. Nothing was changed."));
            }

            if (choice.State == CliPromptState.Unavailable)
            {
                return new RouteRemoveSubjectDiscoveryResult(
                    discovery: null,
                    FromUnresolved(formation, resolution));
            }

            request = request.FreezeSource(choice.Value);
            resolution = _referenceResolver.Resolve(request.ResolutionReference, catalogue);
        }

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

    private async ValueTask<CliPromptReply<string>> SelectAmbiguousSourceAsync(
        RouteRemoveRequest request,
        SourceReferenceResolution resolution,
        CancellationToken cancellationToken)
    {
        if (request.Automatic
            || !request.AllowInteractiveSourceSelection
            || _sourceSelectionPrompt is null)
        {
            return CliPromptReply<string>.Unavailable();
        }

        var paths = resolution.Candidates
            .Select(candidate => candidate.Identity.CanonicalBasePath)
            .Order(StringComparer.Ordinal)
            .ToArray();
        CliPromptReply<string> reply;
        try
        {
            reply = await _sourceSelectionPrompt(
                    new RouteRemoveSourceSelectionQuestion(request.SourceReference, paths),
                    new CliPromptPolicy(true),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return CliPromptReply<string>.Cancelled();
        }
        if (reply.State == CliPromptState.Answered
            && paths.Contains(reply.Value, StringComparer.Ordinal))
        {
            return reply;
        }

        return reply.State == CliPromptState.Cancelled
            ? CliPromptReply<string>.Cancelled()
            : CliPromptReply<string>.Unavailable();
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

        var exposure = await _exposureReader.ReadAsync(
            discovery.Request.Workspace,
            discovery.Catalogue,
            cancellationToken)
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
