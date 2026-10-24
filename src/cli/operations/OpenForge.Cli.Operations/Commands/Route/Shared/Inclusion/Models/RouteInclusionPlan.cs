using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;

internal sealed record RouteInclusionPlan
{
    public required RouteInclusionRequest Request { get; init; }
    public required WorkspaceLayoutRead LayoutRead { get; init; }
    public required RouteInclusionOutcome Preview { get; init; }
    public required ImmutableArray<PlannedFileChange> Changes { get; init; }
    public required ImmutableArray<RecoveryBundleTarget> RecoveryTargets { get; init; }
    public required ImmutableArray<FileStateSnapshot> Observations { get; init; }
}

internal sealed record RouteInclusionPlanBuild(RouteInclusionOutcome Outcome, RouteInclusionPlan? Plan = null);
