using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal sealed partial class RouteRemoveNavigationPlanner
{
    private static IReadOnlyList<SelectedRegion> SelectRegions(
        RouteRemoveNavigationPlanningRequest request,
        GeneratedNavigationFormation observed,
        GeneratedNavigationFormation intended)
    {
        var node = observed.Topology.FindByPath(
            request.Subject.SelectedSource.Identity.CanonicalBasePath);
        if (node is null)
        {
            return [];
        }

        SourceLogicalSource? parent = null;
        if (node.ParentState == SourceRouteParentState.Resolved)
        {
            parent = observed.FindSource(node.ParentPaths[0]);
        }
        else
        {
            parent = observed.Loader;
        }

        if (parent is null || intended.FindSource(parent.Identity.CanonicalBasePath) is null)
        {
            return [];
        }

        var reason = parent.Base.Form == SourceDocumentForm.Loader
            ? RouteRemoveGeneratedReason.Loader
            : RouteRemoveGeneratedReason.OldParent;
        return [new SelectedRegion(parent, [reason])];
    }

    private sealed record SelectedRegion(
        SourceLogicalSource Source,
        ImmutableArray<RouteRemoveGeneratedReason> Reasons);
}
