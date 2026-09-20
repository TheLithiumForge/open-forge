using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;

internal sealed record RouteUpdateGeneratedRegionPlan
{
    public required SourceLogicalSource Source { get; init; }

    public required FileStateSnapshot Snapshot { get; init; }

    public required GeneratedNavigationBoundedChange Change { get; init; }

    public required bool IsTarget { get; init; }
}

internal sealed record RouteUpdateNavigationPlan
{
    public required GeneratedNavigationFormation Formation { get; init; }

    public required ImmutableArray<RouteUpdateGeneratedRegionPlan> Regions { get; init; }
}

internal sealed class RouteUpdateNavigationBuild
{
    private RouteUpdateNavigationBuild(
        RouteUpdateNavigationPlan? navigation,
        RouteUpdatePlanningBoundary? boundary)
    {
        if ((navigation is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Update navigation planning must contain exactly one plan or boundary.");
        }

        Navigation = navigation;
        Boundary = boundary;
    }

    internal RouteUpdateNavigationPlan? Navigation { get; }

    internal RouteUpdatePlanningBoundary? Boundary { get; }

    internal static RouteUpdateNavigationBuild Complete(
        RouteUpdateNavigationPlan navigation)
        => new(navigation: navigation, boundary: null);

    internal static RouteUpdateNavigationBuild Stop(
        RouteUpdatePlanningBoundary boundary)
        => new(navigation: null, boundary: boundary);
}
