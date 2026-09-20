using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Interaction;
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
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;

internal sealed partial class RouteMoveSubjectSelector(
    SourceCatalogueReader catalogueReader,
    SourceReferenceResolver referenceResolver,
    SourceRouteFactsResolver routeFactsResolver,
    RouteNavigationExposureReader exposureReader,
    CliPrompt<RouteMoveSourceSelectionQuestion, string>? sourceSelectionPrompt = null)
{
    private readonly SourceCatalogueReader _catalogueReader = catalogueReader;
    private readonly SourceReferenceResolver _referenceResolver = referenceResolver;
    private readonly SourceRouteFactsResolver _routeFactsResolver = routeFactsResolver;
    private readonly RouteNavigationExposureReader _exposureReader = exposureReader;
    private readonly CliPrompt<RouteMoveSourceSelectionQuestion, string>? _sourceSelectionPrompt = sourceSelectionPrompt;

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
        var discovery = await DiscoverAsync(request, catalogue, cancellationToken)
            .ConfigureAwait(false);
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

    private async ValueTask<RouteMoveSubjectDiscoveryResult> DiscoverAsync(
        RouteMoveRequest request,
        SourceCatalogue catalogue,
        CancellationToken cancellationToken)
    {
        var formation = RouteMoveBoundary.Start(request);
        var resolution = _referenceResolver.Resolve(request.ResolutionReference, catalogue);
        if (resolution.State == SourceReferenceResolutionState.Ambiguous
            && request.FrozenSourceReference is null)
        {
            var choice = await SelectAmbiguousSourceAsync(request, resolution, cancellationToken)
                .ConfigureAwait(false);
            if (choice.State == CliPromptState.Cancelled)
            {
                return new RouteMoveSubjectDiscoveryResult(
                    discovery: null,
                    Boundary(
                        formation,
                        RouteMoveFindingCode.Interrupted,
                        CliSemanticStatus.Interrupted,
                        request.SourceReference,
                        "Route move was cancelled. Nothing was changed."));
            }

            if (choice.State == CliPromptState.Unavailable)
            {
                return new RouteMoveSubjectDiscoveryResult(
                    discovery: null,
                    FromUnresolved(formation, resolution));
            }

            request = request.FreezeSource(choice.Value);
            resolution = _referenceResolver.Resolve(request.ResolutionReference, catalogue);
        }

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

    private async ValueTask<CliPromptReply<string>> SelectAmbiguousSourceAsync(
        RouteMoveRequest request,
        SourceReferenceResolution resolution,
        CancellationToken cancellationToken)
    {
        if (!request.AllowInteractiveSourceSelection || _sourceSelectionPrompt is null)
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
                    new RouteMoveSourceSelectionQuestion(request.SourceReference, paths),
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
