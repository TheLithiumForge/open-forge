using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;

internal sealed record RouteCreatePlanningBoundary
{
    public required RouteCreateTarget Target { get; init; }

    public RouteCreateParent? Parent { get; init; }

    public RouteCreateTemplate? Template { get; init; }

    public required RouteCreateFinding Finding { get; init; }

    public required bool IsIncomplete { get; init; }
}

internal sealed record RouteCreateTargetInspection
{
    public required RouteCreateRequest Request { get; init; }

    public required RouteCreateTarget Target { get; init; }

    public required SourceLogicalIdentity Identity { get; init; }

    public required SourceCatalogue Catalogue { get; init; }

    public required FileStateSnapshot Snapshot { get; init; }
}

internal sealed class RouteCreateTargetInspectionBuild
{
    private RouteCreateTargetInspectionBuild(
        RouteCreateTargetInspection? inspection,
        RouteCreatePlanningBoundary? boundary)
    {
        Inspection = inspection;
        Boundary = boundary;
    }

    internal RouteCreateTargetInspection? Inspection { get; }

    internal RouteCreatePlanningBoundary? Boundary { get; }

    internal static RouteCreateTargetInspectionBuild Complete(
        RouteCreateTargetInspection inspection)
        => new(inspection: inspection, boundary: null);

    internal static RouteCreateTargetInspectionBuild Stop(
        RouteCreatePlanningBoundary boundary)
        => new(inspection: null, boundary: boundary);
}

internal sealed record RouteCreateDestinationPlan
{
    public required RouteCreateTargetInspection Inspection { get; init; }

    public required RouteCreateTemplateResolution Template { get; init; }

    public required SourceLogicalSource TargetSource { get; init; }

    public required ImmutableArray<byte> IntendedBytes { get; init; }
}

internal sealed record RouteCreateNavigationPlan
{
    public required GeneratedNavigationFormation Formation { get; init; }

    public required SourceLogicalSource ParentSource { get; init; }

    public required FileStateSnapshot ParentSnapshot { get; init; }

    public required GeneratedNavigationBoundedChange Change { get; init; }
}

internal sealed class RouteCreateNavigationPlanBuild
{
    private RouteCreateNavigationPlanBuild(
        RouteCreateNavigationPlan? plan,
        RouteCreatePlanningBoundary? boundary)
    {
        Plan = plan;
        Boundary = boundary;
    }

    internal RouteCreateNavigationPlan? Plan { get; }

    internal RouteCreatePlanningBoundary? Boundary { get; }

    internal static RouteCreateNavigationPlanBuild Complete(
        RouteCreateNavigationPlan plan)
        => new(plan: plan, boundary: null);

    internal static RouteCreateNavigationPlanBuild Stop(
        RouteCreatePlanningBoundary boundary)
        => new(plan: null, boundary: boundary);
}

internal sealed record RouteCreateCompletePlanStage
{
    public required RouteCreateDestinationPlan Destination { get; init; }

    public required RouteCreateNavigationPlan Navigation { get; init; }

    public required ImmutableArray<PlannedFileChange> FileChanges { get; init; }

    public required ImmutableArray<RecoveryBundleTarget> RecoveryTargets { get; init; }
}
