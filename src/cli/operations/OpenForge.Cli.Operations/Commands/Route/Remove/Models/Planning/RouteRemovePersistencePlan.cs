using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;

internal sealed record RouteRemovePersistencePlan
{
    public required WorkspaceSettingsRead Settings { get; init; }

    public required WorkspaceRemovalSelection Selection { get; init; }

    public required WorkspaceOwnershipRead Ownership { get; init; }

    public ImmutableArray<string> ContentPathsToRelease { get; init; } = [];

    public ImmutableArray<RouteRemoveOwnershipClaim> ClaimsToRelease { get; init; } = [];

    public PlannedFileChange? SettingsChange { get; init; }

    public PlannedFileChange? OwnershipChange { get; init; }

    public RecoveryBundleTarget? SettingsRecoveryTarget { get; init; }

    public RecoveryBundleTarget? OwnershipRecoveryTarget { get; init; }
}

internal sealed record RouteRemovePersistencePlanningResult
{
    internal RouteRemovePersistencePlanningResult(
        RouteRemovePersistencePlan? plan,
        RouteRemoveFinding? finding)
    {
        if ((plan is null) == (finding is null))
        {
            throw new ArgumentException(
                "Route Remove persistence planning requires exactly one plan or finding.");
        }

        Plan = plan;
        Finding = finding;
    }

    internal RouteRemovePersistencePlan? Plan { get; }

    internal RouteRemoveFinding? Finding { get; }
}
