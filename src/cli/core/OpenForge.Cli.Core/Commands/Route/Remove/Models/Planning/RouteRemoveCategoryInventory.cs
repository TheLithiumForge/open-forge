using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;

internal sealed record RouteRemoveInventoryItem
{
    public required RouteRemoveItemKind Kind { get; init; }

    public RouteRemoveLayerKind? Layer { get; init; }

    public string? SourceId { get; init; }

    public required string SourcePath { get; init; }

    public required string RelativePath { get; init; }

    public required FileStateSnapshot Snapshot { get; init; }
}

internal sealed record RouteRemoveCategoryInventory
{
    public required RouteRemoveResolvedSubject Subject { get; init; }

    public required LifecycleOwnershipReadResult Ownership { get; init; }

    public ImmutableArray<RouteRemoveInventoryItem> Items { get; init; } = [];
}

internal sealed record RouteRemoveCategoryInventoryResult
{
    internal RouteRemoveCategoryInventoryResult(
        RouteRemoveCategoryInventory? inventory,
        RouteRemoveResultFormation? boundary)
    {
        if ((inventory is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Remove category inventory requires exactly one inventory or boundary.");
        }

        Inventory = inventory;
        Boundary = boundary;
    }

    internal RouteRemoveCategoryInventory? Inventory { get; }

    internal RouteRemoveResultFormation? Boundary { get; }
}
