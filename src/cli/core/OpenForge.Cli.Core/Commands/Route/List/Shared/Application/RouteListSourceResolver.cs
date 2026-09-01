using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Routing;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Application;

internal sealed class RouteListSourceResolver(
    RouteListInventoryReader inventoryReader,
    RouteListSelectionResolver selectionResolver,
    SourceRouteFactsResolver routeFactsResolver)
{
    private readonly RouteListInventoryReader _inventoryReader = inventoryReader;
    private readonly RouteListSelectionResolver _selectionResolver = selectionResolver;
    private readonly SourceRouteFactsResolver _routeFactsResolver = routeFactsResolver;

    internal async ValueTask<RouteListSourceResolution> ResolveAsync(
        RouteListRequest request,
        CancellationToken cancellationToken)
    {
        var documentReader = new SourceDocumentReader(request.Workspace);
        var inventory = await _inventoryReader
            .ReadAsync(
                new RouteListInventoryRequest(request.Workspace, cancellationToken),
                documentReader)
            .ConfigureAwait(false);
        var sourceCatalogue = inventory.SourceCatalogue;
        var projectionSet = inventory.ProjectionBuildResult.ProjectionSet;
        var routeFacts = await _routeFactsResolver
            .ResolveAsync(
                new SourceRouteFactsRequest(sourceCatalogue, sourceCatalogue.SelectAll()),
                documentReader,
                cancellationToken)
            .ConfigureAwait(false);
        var selection = await _selectionResolver
            .ResolveAsync(
                request,
                sourceCatalogue,
                projectionSet,
                routeFacts,
                cancellationToken)
            .ConfigureAwait(false);
        return new RouteListSourceResolution(inventory, routeFacts, selection);
    }

    internal async ValueTask<RouteListSelectionResolution?> ResolveLoaderRootAsync(
        RouteListRequest request,
        RouteListSourceResolution source,
        CancellationToken cancellationToken)
    {
        if (request.SourceReference is null
            || source.Selection.State != RouteListSelectionResolutionState.Resolved)
        {
            return null;
        }

        return await _selectionResolver
            .ResolveAsync(
                new RouteListRequest(
                    request.Workspace,
                    null,
                    request.RequestedDepth),
                source.Inventory.SourceCatalogue,
                source.Inventory.ProjectionBuildResult.ProjectionSet,
                source.RouteFacts,
                cancellationToken)
            .ConfigureAwait(false);
    }
}

internal sealed record RouteListSourceResolution(
    RouteListInventoryFacts Inventory,
    SourceRouteFacts RouteFacts,
    RouteListSelectionResolution Selection);
