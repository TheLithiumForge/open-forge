namespace OpenForge.Cli.Core.Commands.Route.Shared.Models.Topology;

internal sealed record RouteTopologyParentRelationship(
    RouteTopologyParentState State,
    IReadOnlyList<string> Paths)
{
    internal static RouteTopologyParentRelationship None { get; } = new(
        RouteTopologyParentState.None,
        []);
}
