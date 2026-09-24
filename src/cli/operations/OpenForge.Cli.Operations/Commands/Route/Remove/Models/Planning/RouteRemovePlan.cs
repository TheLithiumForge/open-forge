using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Commands.Route.Shared.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;

internal sealed record RouteRemovePlanProjectionInput
{
    public required RouteRemoveResolvedSubject Subject { get; init; }

    public required WorkspaceOwnershipRead Ownership { get; init; }

    public required WorkspaceSettingsRead Settings { get; init; }

    public required WorkspaceRemovalSelection RemovalSelection { get; init; }

    public ImmutableArray<string> ContentPathsToRelease { get; init; } = [];

    public ImmutableArray<RouteRemoveOwnershipClaim> ClaimsToRelease { get; init; } = [];

    public PlannedFileChange? SettingsChange { get; init; }

    public PlannedFileChange? OwnershipChange { get; init; }

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
        && Projection.DirectoryDeletions.IsEmpty
        && Projection.SettingsChange is null
        && Projection.OwnershipChange is null;
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
