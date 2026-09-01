using OpenForge.Cli.Core.Commands.Route.List.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Routing;

namespace OpenForge.Cli.Core.Commands.Route.List;

internal static class RouteListOperationFactory
{
    internal static RouteListOperation Create()
    {
        var physicalPathResolver = new PhysicalPathResolver();
        var inventoryReader = new RouteListInventoryReader();
        var selectionResolver = new RouteListSelectionResolver(physicalPathResolver);
        var routeFactsResolver = new SourceRouteFactsResolver();
        var sourceResolver = new RouteListSourceResolver(
            inventoryReader: inventoryReader,
            selectionResolver: selectionResolver,
            routeFactsResolver: routeFactsResolver);
        var topologySelector = new RouteListTopologySelector();
        var coverageBuilder = new RouteListCoverageBuilder();
        var resultBuilder = new RouteListResultBuilder();
        var resultComposer = new RouteListResultComposer(
            topologySelector: topologySelector,
            coverageBuilder: coverageBuilder,
            resultBuilder: resultBuilder);
        var coordinator = new RouteListOperationCoordinator(
            sourceResolver: sourceResolver,
            resultComposer: resultComposer);
        return coordinator.ExecuteAsync;
    }
}

internal sealed class RouteListOperationCoordinator(
    RouteListSourceResolver sourceResolver,
    RouteListResultComposer resultComposer)
{
    private readonly RouteListSourceResolver _sourceResolver = sourceResolver;
    private readonly RouteListResultComposer _resultComposer = resultComposer;

    internal async ValueTask<RouteListResult> ExecuteAsync(
        RouteListRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var attemptedSelection = RouteListSelectionFactory.Attempted(request.SourceReference);
        try
        {
            var source = await _sourceResolver
                .ResolveAsync(request, cancellationToken)
                .ConfigureAwait(false);
            attemptedSelection = source.Selection.Selection;
            var loaderRootSelection = await _sourceResolver
                .ResolveLoaderRootAsync(request, source, cancellationToken)
                .ConfigureAwait(false);
            return _resultComposer.Build(
                request,
                source,
                loaderRootSelection,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return _resultComposer.BuildFailure(request, attemptedSelection);
        }
    }
}
