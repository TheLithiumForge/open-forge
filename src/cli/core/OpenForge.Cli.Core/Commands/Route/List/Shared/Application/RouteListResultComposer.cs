using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Application;

internal sealed class RouteListResultComposer(
    RouteListTopologySelector topologySelector,
    RouteListCoverageBuilder coverageBuilder,
    RouteListResultBuilder resultBuilder)
{
    private readonly RouteListTopologySelector _topologySelector = topologySelector;
    private readonly RouteListCoverageBuilder _coverageBuilder = coverageBuilder;
    private readonly RouteListResultBuilder _resultBuilder = resultBuilder;

    internal RouteListResult Build(
        RouteListRequest request,
        RouteListSourceResolution source,
        RouteListSelectionResolution? loaderRootSelection,
        CancellationToken cancellationToken)
    {
        var input = new RouteListTopologyInput(
            request,
            source.Inventory,
            source.Selection,
            loaderRootSelection);
        var selected = _topologySelector.SelectSources(input, source.RouteFacts, cancellationToken);
        var coverage = _coverageBuilder.Build(input, selected);
        return _resultBuilder.Build(input, selected, coverage);
    }

    internal RouteListResult BuildFailure(
        RouteListRequest request,
        RouteListSelection attemptedSelection)
        => _resultBuilder.BuildFailure(request, attemptedSelection);
}
