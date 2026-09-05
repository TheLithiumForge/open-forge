using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal sealed partial class RouteRemoveSubjectSelector
{
    private static RouteRemoveResultFormation? ReadExposureBoundary(
        RouteRemoveSubjectDiscovery discovery,
        SourceRouteFacts routeFacts,
        RouteRemoveNavigationExposure exposure)
    {
        var formation = RouteRemoveBoundary.Start(discovery.Request) with { Source = discovery.Source };
        if (exposure.IsCancelled)
        {
            return Boundary(
                formation,
                RouteRemoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                discovery.SelectedSource.Identity.CanonicalBasePath,
                "Route Remove navigation exposure inspection was interrupted.");
        }

        return IsUnsupportedLeaf(discovery, routeFacts, exposure)
            ? Boundary(
                formation,
                RouteRemoveFindingCode.InvalidSubject,
                CliSemanticStatus.Invalid,
                discovery.SelectedSource.Identity.CanonicalBasePath,
                "The selected ordinary Markdown source is not exposed by its complete routed parent.")
            : null;
    }

    private static bool IsUnsupportedLeaf(
        RouteRemoveSubjectDiscovery discovery,
        SourceRouteFacts routeFacts,
        RouteRemoveNavigationExposure exposure)
    {
        if (discovery.Kind != RouteRemoveSubjectKind.Leaf)
        {
            return false;
        }

        var node = routeFacts.Topology.FindByPath(
            discovery.SelectedSource.Identity.CanonicalBasePath);
        return node?.ParentState == SourceRouteParentState.Resolved
            && !exposure.UnavailableParents.Contains(node.ParentPaths[0], StringComparer.Ordinal)
            && !exposure.ExposedPaths.Contains(
                discovery.SelectedSource.Identity.CanonicalBasePath,
                StringComparer.Ordinal);
    }
}
