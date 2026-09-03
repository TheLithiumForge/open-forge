using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;

internal sealed record RouteMovePlanProjectionInput
{
    public required RouteMoveResolvedDestination Destination { get; init; }

    public required LifecycleOwnershipReadResult Ownership { get; init; }

    public required RouteMoveReferencePlan References { get; init; }

    public required RouteMoveNavigationPlan Navigation { get; init; }

    public ImmutableArray<PlannedDirectoryCreation> DirectoryCreations { get; init; } = [];

    public ImmutableArray<PlannedFileChange> FileChanges { get; init; } = [];

    public ImmutableArray<PlannedDirectoryDeletion> DirectoryDeletions { get; init; } = [];

    public ImmutableArray<RecoveryBundleTarget> RecoveryTargets { get; init; } = [];
}

internal sealed record RouteMovePlan
{
    public required RouteMoveRequest Request { get; init; }

    public required RouteMoveResultFormation Preview { get; init; }

    public required RouteMovePlanProjectionInput Projection { get; init; }

    internal bool IsNoOp => Projection.DirectoryCreations.IsEmpty
        && Projection.FileChanges.IsEmpty
        && Projection.DirectoryDeletions.IsEmpty;
}

internal sealed record RouteMovePlanBuild
{
    internal RouteMovePlanBuild(
        RouteMovePlan? plan,
        RouteMoveResultFormation formation)
    {
        ArgumentNullException.ThrowIfNull(formation);
        Plan = plan;
        Formation = formation;
    }

    internal RouteMovePlan? Plan { get; }

    internal RouteMoveResultFormation Formation { get; }
}
