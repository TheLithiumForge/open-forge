using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Routing;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal sealed partial class RouteInspectResolver
{
    private readonly PhysicalPathResolver _physicalPathResolver = new();
    private readonly SourceCatalogueReader _catalogueReader = new();
    private readonly RouteInspectSourceProjectionBuilder _projectionBuilder = new();
    private readonly SourceRouteFactsResolver _routeFactsResolver = new();
    private readonly RouteInspectSourceSelectionResolver _sourceSelectionResolver =
        new(new RouteInspectSourceFactsResolver());

    internal async ValueTask<RouteInspectResolution> ResolveAsync(
        RouteInspectRequest request,
        CancellationToken cancellationToken)
    {
        var parsed = SourceReferenceParser.Parse(request.SourceReference);
        var unresolvedSelection = RouteInspectResolutionSupport.UnresolvedSelection(parsed);
        if (cancellationToken.IsCancellationRequested)
        {
            return RouteInspectResolutionSupport.Interrupted(
                unresolvedSelection,
                request.SourceReference);
        }

        if (parsed.State == SourceReferenceParseState.Invalid)
        {
            var attemptedReference = ReadAttemptedReference(parsed);
            var cause = parsed.Cause
                ?? throw new InvalidOperationException("An invalid source reference requires its cause.");
            return RouteInspectResolution.Create(
                RouteInspectResolutionState.Invalid,
                unresolvedSelection,
                null,
                null,
                [RouteInspectResolutionSupport.CreateIssue(
                    RouteInspectResolutionIssueCode.InvalidReference,
                    attemptedReference,
                    cause)]);
        }

        try
        {
            var exactPathPhysical = parsed.Kind == SourceReferenceKind.SourcePath
                ? ResolveExactPath(request.Workspace, ReadAttemptedReference(parsed))
                : null;
            if (exactPathPhysical is not null)
            {
                var earlyResult = ReadEarlyPathResult(
                    ReadAttemptedReference(parsed),
                    unresolvedSelection,
                    exactPathPhysical,
                    cancellationToken);
                if (earlyResult is not null)
                {
                    return earlyResult;
                }
            }

            var reader = new SourceDocumentReader(request.Workspace);
            var catalogue = await _catalogueReader
                .ReadAsync(
                    new SourceCatalogueRequest(request.Workspace, [SourceLogicalPath.AgentsRoot]),
                    cancellationToken)
                .ConfigureAwait(false);
            if (catalogue.IsCancelled || cancellationToken.IsCancellationRequested)
            {
                return RouteInspectResolutionSupport.Interrupted(
                    unresolvedSelection,
                    ReadAttemptedReference(parsed));
            }

            var boundaryIssues = RouteInspectResolutionSupport.ReadCatalogueBoundaryIssues(catalogue);
            if (boundaryIssues.Count != 0)
            {
                return RouteInspectResolutionSupport.CreateBoundaryResolution(
                    unresolvedSelection,
                    boundaryIssues);
            }

            var catalogueSelection = catalogue.SelectAll();
            var projections = await _projectionBuilder
                .ReadAsync(catalogueSelection, reader, cancellationToken)
                .ConfigureAwait(false);
            if (projections.IsCancelled || cancellationToken.IsCancellationRequested)
            {
                return RouteInspectResolutionSupport.Interrupted(
                    unresolvedSelection,
                    ReadAttemptedReference(parsed));
            }

            var routeFacts = await _routeFactsResolver
                .ResolveAsync(
                    new SourceRouteFactsRequest(catalogue, catalogueSelection),
                    reader,
                    cancellationToken)
                .ConfigureAwait(false);
            if (routeFacts.IsCancelled || cancellationToken.IsCancellationRequested)
            {
                return RouteInspectResolutionSupport.Interrupted(
                    unresolvedSelection,
                    ReadAttemptedReference(parsed));
            }

            var input = new RouteInspectResolutionInput(
                request,
                parsed,
                unresolvedSelection,
                catalogue,
                catalogueSelection,
                projections,
                routeFacts,
                exactPathPhysical);
            if (cancellationToken.IsCancellationRequested)
            {
                return RouteInspectResolutionSupport.Interrupted(
                    unresolvedSelection,
                    ReadAttemptedReference(parsed));
            }

            return _sourceSelectionResolver.Resolve(input, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RouteInspectResolutionSupport.Interrupted(
                unresolvedSelection,
                ReadAttemptedReference(parsed));
        }
        catch (Exception)
        {
            return RouteInspectResolution.Create(
                RouteInspectResolutionState.Failed,
                unresolvedSelection,
                null,
                null,
                [RouteInspectResolutionSupport.CreateIssue(
                    RouteInspectResolutionIssueCode.OperationFailure,
                    ReadAttemptedReference(parsed),
                    "Route inspection failed while resolving the source.")]);
        }
    }

    private static string ReadAttemptedReference(SourceReferenceParseResult parsed)
    {
        return parsed.AttemptedId ?? parsed.AttemptedPath
            ?? throw new InvalidOperationException("A source-reference result requires an attempted identity.");
    }

}
