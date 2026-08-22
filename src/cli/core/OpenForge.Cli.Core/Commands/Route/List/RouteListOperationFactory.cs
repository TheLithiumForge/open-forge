using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Topology;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Commands.Route.List;

internal static class RouteListOperationFactory
{
    internal static RouteListOperation Create()
    {
        var physicalPathResolver = new PhysicalPathResolver();
        var coordinator = new RouteListOperationCoordinator(
            new RouteListInventoryReader(),
            new RouteListSelectionResolver(physicalPathResolver),
            new RouteTopologyBuilder(),
            new RouteListTopologySelector(),
            new RouteListCoverageBuilder(),
            new RouteListResultBuilder());
        return coordinator.ExecuteAsync;
    }
}

internal sealed class RouteListOperationCoordinator
{
    private readonly RouteListInventoryReader _inventoryReader;
    private readonly RouteListSelectionResolver _selectionResolver;
    private readonly RouteTopologyBuilder _graphBuilder;
    private readonly RouteListTopologySelector _topologySelector;
    private readonly RouteListCoverageBuilder _coverageBuilder;
    private readonly RouteListResultBuilder _resultBuilder;

    internal RouteListOperationCoordinator(
        RouteListInventoryReader inventoryReader,
        RouteListSelectionResolver selectionResolver,
        RouteTopologyBuilder graphBuilder,
        RouteListTopologySelector topologySelector,
        RouteListCoverageBuilder coverageBuilder,
        RouteListResultBuilder resultBuilder)
    {
        ArgumentNullException.ThrowIfNull(inventoryReader);
        ArgumentNullException.ThrowIfNull(selectionResolver);
        ArgumentNullException.ThrowIfNull(graphBuilder);
        ArgumentNullException.ThrowIfNull(topologySelector);
        ArgumentNullException.ThrowIfNull(coverageBuilder);
        ArgumentNullException.ThrowIfNull(resultBuilder);
        _inventoryReader = inventoryReader;
        _selectionResolver = selectionResolver;
        _graphBuilder = graphBuilder;
        _topologySelector = topologySelector;
        _coverageBuilder = coverageBuilder;
        _resultBuilder = resultBuilder;
    }

    internal async ValueTask<RouteListResult> ExecuteAsync(
        RouteListRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var attemptedSelection = RouteListSelectionFactory.Attempted(request.SourceReference);
        try
        {
            var inventory = await _inventoryReader
                .ReadAsync(new RouteListInventoryRequest(request.Workspace, cancellationToken))
                .ConfigureAwait(false);
            var selection = await _selectionResolver
                .ResolveAsync(request, inventory.Catalogue, cancellationToken)
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
                        inventory.Catalogue,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            var input = new RouteListTopologyInput(
                request,
                inventory,
                selection,
                loaderRootSelection);
            var topology = _graphBuilder.Build(
                inventory.Sources
                    .Where(source => source.Kind is
                        RouteListSourceKind.Entrypoint
                        or RouteListSourceKind.RoutedLeaf
                        or RouteListSourceKind.RoutedNative)
                    .Select(source => source.Source),
                input.LoaderRootPaths);
            var selected = _topologySelector.Select(input, topology, cancellationToken);
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
