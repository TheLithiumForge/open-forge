using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;

internal sealed record RouteMoveDestinationResolutionRequest
{
    public required RouteMoveCategoryInventory Inventory { get; init; }
}

internal sealed record RouteMoveResolvedDestination
{
    public required RouteMoveCategoryInventory Inventory { get; init; }

    public required RouteMoveDestination Destination { get; init; }

    public required RouteMoveSubject Subject { get; init; }
}

internal sealed record RouteMoveDestinationItem
{
    public required RouteMoveInventoryItem Item { get; init; }

    public required string DestinationPath { get; init; }
}

internal sealed record RouteMoveDestinationProjection
{
    public required RouteMoveCategoryInventory Inventory { get; init; }

    public required string DestinationPath { get; init; }

    public required string DestinationId { get; init; }

    public required SourceLogicalSource Parent { get; init; }

    public ImmutableArray<RouteMoveDestinationItem> Items { get; init; } = [];
}

internal sealed record RouteMoveDestinationProjectionResult
{
    internal RouteMoveDestinationProjectionResult(
        RouteMoveDestinationProjection? projection,
        RouteMoveResultFormation? boundary)
    {
        if ((projection is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Move destination projection requires exactly one projection or boundary.");
        }

        Projection = projection;
        Boundary = boundary;
    }

    internal RouteMoveDestinationProjection? Projection { get; }

    internal RouteMoveResultFormation? Boundary { get; }
}

internal sealed record RouteMoveDestinationResolution
{
    internal RouteMoveDestinationResolution(
        RouteMoveResolvedDestination? destination,
        RouteMoveResultFormation? boundary)
    {
        if ((destination is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Move destination resolution requires exactly one destination or boundary.");
        }

        Destination = destination;
        Boundary = boundary;
    }

    internal RouteMoveResolvedDestination? Destination { get; }

    internal RouteMoveResultFormation? Boundary { get; }
}
