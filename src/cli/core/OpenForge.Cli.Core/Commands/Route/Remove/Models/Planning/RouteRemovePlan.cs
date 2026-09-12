using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;

internal sealed record RouteRemovePlanProjectionInput
{
    public required RouteRemoveResolvedSubject Subject { get; init; }

    public required LifecycleOwnershipReadResult Ownership { get; init; }

    public required RouteRemoveReferencePlan References { get; init; }

    public required RouteRemoveNavigationPlan Navigation { get; init; }

    public ImmutableArray<PlannedFileChange> FileChanges { get; init; } = [];

    public ImmutableArray<PlannedDirectoryDeletion> DirectoryDeletions { get; init; } = [];

    public ImmutableArray<RecoveryBundleTarget> RecoveryTargets { get; init; } = [];
}

internal sealed record RouteRemovePlan
{
    public required RouteRemoveRequest Request { get; init; }

    public required RouteRemoveResultFormation Preview { get; init; }

    public required RouteRemovePlanProjectionInput Projection { get; init; }

    internal bool IsNoOp => Projection.FileChanges.IsEmpty
        && Projection.DirectoryDeletions.IsEmpty;
}

internal sealed record RouteRemovePlanBuild
{
    internal RouteRemovePlanBuild(
        RouteRemovePlan? plan,
        RouteRemoveResultFormation formation)
    {
        ArgumentNullException.ThrowIfNull(formation);
        Plan = plan;
        Formation = formation;
    }

    internal RouteRemovePlan? Plan { get; }

    internal RouteRemoveResultFormation Formation { get; }
}
