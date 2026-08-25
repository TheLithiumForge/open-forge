using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Routing;

namespace OpenForge.Cli.Core.Commands.Route.List;

internal static class RouteListOperationFactory
{
    internal static RouteListOperation Create()
    {
        var physicalPathResolver = new PhysicalPathResolver();
        var coordinator = new RouteListOperationCoordinator(
            new RouteListOperationComponents
            {
                InventoryReader = new RouteListInventoryReader(),
                SelectionResolver = new RouteListSelectionResolver(physicalPathResolver),
                RouteFactsResolver = new SourceRouteFactsResolver(),
                TopologySelector = new RouteListTopologySelector(),
                CoverageBuilder = new RouteListCoverageBuilder(),
                ResultBuilder = new RouteListResultBuilder(),
            });
        return coordinator.ExecuteAsync;
    }
}

internal sealed class RouteListOperationCoordinator
{
    private readonly RouteListInventoryReader _inventoryReader;
    private readonly RouteListSelectionResolver _selectionResolver;
    private readonly SourceRouteFactsResolver _routeFactsResolver;
    private readonly RouteListTopologySelector _topologySelector;
    private readonly RouteListCoverageBuilder _coverageBuilder;
    private readonly RouteListResultBuilder _resultBuilder;

    internal RouteListOperationCoordinator(RouteListOperationComponents components)
    {
        ArgumentNullException.ThrowIfNull(components);
        ArgumentNullException.ThrowIfNull(components.InventoryReader);
        ArgumentNullException.ThrowIfNull(components.SelectionResolver);
        ArgumentNullException.ThrowIfNull(components.RouteFactsResolver);
        ArgumentNullException.ThrowIfNull(components.TopologySelector);
        ArgumentNullException.ThrowIfNull(components.CoverageBuilder);
        ArgumentNullException.ThrowIfNull(components.ResultBuilder);
        _inventoryReader = components.InventoryReader;
        _selectionResolver = components.SelectionResolver;
        _routeFactsResolver = components.RouteFactsResolver;
        _topologySelector = components.TopologySelector;
        _coverageBuilder = components.CoverageBuilder;
        _resultBuilder = components.ResultBuilder;
    }

    internal async ValueTask<RouteListResult> ExecuteAsync(
        RouteListRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var attemptedSelection = RouteListSelectionFactory.Attempted(request.SourceReference);
        try
        {
            var documentReader = new SourceDocumentReader(request.Workspace);
            var inventory = await _inventoryReader
                .ReadAsync(
                    new RouteListInventoryRequest(request.Workspace, cancellationToken),
                    documentReader)
                .ConfigureAwait(false);
            var sourceCatalogue = inventory.SourceCatalogue;
            var catalogueSelection = sourceCatalogue.SelectAll();
            var projectionSet = inventory.ProjectionBuildResult.ProjectionSet;
            var routeFacts = await _routeFactsResolver
                .ResolveAsync(
                    new SourceRouteFactsRequest(sourceCatalogue, catalogueSelection),
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
            attemptedSelection = selection.Selection;

            RouteListSelectionResolution? loaderRootSelection = null;
            if (request.SourceReference is not null
                && selection.State == RouteListSelectionResolutionState.Resolved)
            {
                loaderRootSelection = await _selectionResolver
                    .ResolveAsync(
                        new RouteListRequest(
                            request.Workspace,
                            null,
                            request.RequestedDepth),
                        sourceCatalogue,
                        projectionSet,
                        routeFacts,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            var input = new RouteListTopologyInput(
                request,
                inventory,
                selection,
                loaderRootSelection);
            var selected = _topologySelector.SelectSources(input, routeFacts, cancellationToken);
            var coverage = _coverageBuilder.Build(input, selected);
            return _resultBuilder.Build(input, selected, coverage);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return _resultBuilder.BuildFailure(request, attemptedSelection);
        }
    }

}
