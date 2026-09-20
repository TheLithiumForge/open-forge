using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;

internal sealed class RouteInspectGraph
{
    private readonly RouteSourceProjectionSet _projectionSet;
    private readonly SourceRouteFacts _routeFacts;

    internal RouteInspectGraph(
        RouteSourceProjectionSet projectionSet,
        SourceRouteFacts routeFacts)
    {
        ArgumentNullException.ThrowIfNull(projectionSet);
        ArgumentNullException.ThrowIfNull(routeFacts);

        var projectionsByPath = projectionSet.Projections.ToDictionary(
            projection => projection.LogicalSource.Identity.CanonicalBasePath,
            StringComparer.Ordinal);
        foreach (var projection in projectionSet.Projections)
        {
            if (projection.Source is not null
                && !ReferenceEquals(
                    projection.Source,
                    projectionSet.FindByPath(projection.LogicalSource.Identity.CanonicalBasePath)))
            {
                throw new ArgumentException(
                    "Every projected Route source must remain associated with its neutral logical source.",
                    nameof(projectionSet));
            }
        }

        foreach (var node in routeFacts.Topology.Nodes)
        {
            if (!projectionsByPath.TryGetValue(node.Identity.CanonicalBasePath, out var projection)
                || !ReferenceEquals(projection.LogicalSource.Identity, node.Identity))
            {
                throw new ArgumentException(
                    "Topology identities must be reference-identical members of the Route source projection set.",
                    nameof(routeFacts));
            }
        }

        foreach (var fact in routeFacts.RouteFacts)
        {
            if (!projectionsByPath.TryGetValue(fact.Identity.CanonicalBasePath, out var projection)
                || !ReferenceEquals(projection.LogicalSource.Identity, fact.Identity))
            {
                throw new ArgumentException(
                    "Route fact identities must be reference-identical members of the Route source projection set.",
                    nameof(routeFacts));
            }
        }

        _projectionSet = projectionSet;
        _routeFacts = routeFacts;
    }

    internal RouteSourceProjectionSet ProjectionSet => _projectionSet;

    internal SourceRouteFacts RouteFacts => _routeFacts;
}
