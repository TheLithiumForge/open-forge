using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;
using OpenForge.Cli.Core.Framework.Sources.Routing;

namespace OpenForge.Cli.Core.Commands.Route.List.Models.Operation;

internal sealed class RouteListOperationComponents
{
    internal required RouteListInventoryReader InventoryReader { get; init; }

    internal required RouteListSelectionResolver SelectionResolver { get; init; }

    internal required SourceRouteFactsResolver RouteFactsResolver { get; init; }

    internal required RouteListTopologySelector TopologySelector { get; init; }

    internal required RouteListCoverageBuilder CoverageBuilder { get; init; }

    internal required RouteListResultBuilder ResultBuilder { get; init; }
}
