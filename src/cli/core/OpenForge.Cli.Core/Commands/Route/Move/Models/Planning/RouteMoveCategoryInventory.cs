using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;

internal sealed record RouteMoveInventoryItem
{
    public required RouteMoveItemKind Kind { get; init; }

    public RouteMoveLayerKind? Layer { get; init; }

    public string? SourceId { get; init; }

    public required string SourcePath { get; init; }

    public required string RelativePath { get; init; }

    public required FileStateSnapshot Snapshot { get; init; }
}

internal sealed record RouteMoveCategoryInventory
{
    public required RouteMoveResolvedSubject Subject { get; init; }

    public required LifecycleOwnershipReadResult Ownership { get; init; }

    public ImmutableArray<RouteMoveInventoryItem> Items { get; init; } = [];
}

internal sealed record RouteMoveCategoryInventoryResult
{
    internal RouteMoveCategoryInventoryResult(
        RouteMoveCategoryInventory? inventory,
        RouteMoveResultFormation? boundary)
    {
        if ((inventory is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Move category inventory requires exactly one inventory or boundary.");
        }

        Inventory = inventory;
        Boundary = boundary;
    }

    internal RouteMoveCategoryInventory? Inventory { get; }

    internal RouteMoveResultFormation? Boundary { get; }
}
