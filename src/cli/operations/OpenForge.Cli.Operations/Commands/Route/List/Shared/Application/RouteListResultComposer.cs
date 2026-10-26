using OpenForge.Cli.Core.Commands.Route.List.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Commands.Route.List.Models.Result;
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
        if (cancellationToken.IsCancellationRequested && source.IncludedSources.Count != 0)
        {
            return _resultBuilder.BuildInterrupted(request, source.Selection.Selection);
        }

        var input = new RouteListTopologyInput(
            request,
            source.Inventory,
            source.Selection,
            loaderRootSelection);
        var selected = _topologySelector.SelectSources(input, source.RouteFacts, cancellationToken);
        var coverage = _coverageBuilder.Build(input, selected);
        var result = _resultBuilder.Build(input, selected, coverage);
        return RouteListIncludedResultBuilder.Build(request, source, result, cancellationToken);
    }

    internal RouteListResult BuildLayoutUnavailable(
        RouteListRequest request,
        RouteListSelection selection,
        WorkspaceLayoutRead layoutRead)
        => _resultBuilder.BuildLayoutUnavailable(request, selection, layoutRead);

    internal RouteListResult BuildFailure(
        RouteListRequest request,
        RouteListSelection attemptedSelection)
        => _resultBuilder.BuildFailure(request, attemptedSelection);

    internal RouteListResult BuildInterrupted(RouteListRequest request, RouteListSelection attemptedSelection)
        => _resultBuilder.BuildInterrupted(request, attemptedSelection);
}
