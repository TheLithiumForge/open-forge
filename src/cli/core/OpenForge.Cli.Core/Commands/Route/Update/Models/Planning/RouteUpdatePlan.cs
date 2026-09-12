using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Shared.Templates.Models;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;

internal sealed record RouteUpdatePlanProjectionInput
{
    public required RouteUpdateDestinationPlan Destination { get; init; }

    public required RouteUpdateNavigationPlan Navigation { get; init; }
}

internal sealed record RouteUpdatePlan
{
    public required RouteUpdateRequest Request { get; init; }

    public required RouteUpdateResultFormation Preview { get; init; }

    public required RouteUpdateObservation Observation { get; init; }

    public RouteTemplateResolution? Template { get; init; }

    public required RouteUpdateDestinationPlan Destination { get; init; }

    public required RouteUpdateNavigationPlan Navigation { get; init; }

    public required ImmutableArray<PlannedFileChange> FileChanges { get; init; }

    public required ImmutableArray<RecoveryBundleTarget> RecoveryTargets { get; init; }

    internal bool IsNoOp => FileChanges.IsEmpty;
}

internal sealed record RouteUpdatePlanBuild
{
    public required RouteUpdatePlan? Plan { get; init; }

    public required RouteUpdateResultFormation Formation { get; init; }
}
