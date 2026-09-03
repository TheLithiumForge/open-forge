using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;

internal sealed record RouteMoveNavigationPlanningRequest
{
    public required RouteMoveResolvedDestination Destination { get; init; }

    public ImmutableArray<SourceLogicalSource> IntendedSources { get; init; } = [];
}

internal sealed record RouteMoveNavigationPlan
{
    public required RouteMoveNavigationPlanningRequest Request { get; init; }

    public required RouteMoveGeneratedNavigation GeneratedNavigation { get; init; }

    public ImmutableArray<PlannedFileChange> FileChanges { get; init; } = [];

    public ImmutableArray<RouteMoveNavigationDocumentEdit> DocumentEdits { get; init; } = [];
}

internal sealed record RouteMoveNavigationDocumentEdit
{
    public required FileStateSnapshot Snapshot { get; init; }

    public required string DestinationLogicalPath { get; init; }

    public required SourceLocation Location { get; init; }

    public ImmutableArray<byte> BeforeBytes { get; init; } = [];

    public ImmutableArray<byte> ExpectedBytes { get; init; } = [];
}

internal sealed record RouteMoveNavigationPlanningResult
{
    internal RouteMoveNavigationPlanningResult(
        RouteMoveNavigationPlan? plan,
        RouteMoveResultFormation? boundary)
    {
        if ((plan is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Move navigation planning requires exactly one plan or boundary.");
        }

        Plan = plan;
        Boundary = boundary;
    }

    internal RouteMoveNavigationPlan? Plan { get; }

    internal RouteMoveResultFormation? Boundary { get; }
}

internal enum RouteMoveNavigationPostMoveState
{
    Verified,
    Failed,
    Interrupted,
}

internal sealed record RouteMoveNavigationPostMoveResult(
    RouteMoveNavigationPostMoveState State,
    string? Cause);
