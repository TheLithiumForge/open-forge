using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Navigation;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;

internal sealed partial class RouteMoveSubjectSelector
{
    private static RouteMoveResultFormation? ReadExposureBoundary(
        RouteMoveSubjectDiscovery discovery,
        SourceRouteFacts routeFacts,
        RouteNavigationExposure exposure,
        SourceSharing sharing)
    {
        var formation = RouteMoveBoundary.Start(discovery.Request) with { Source = discovery.Source };
        if (exposure.IsCancelled)
        {
            return Boundary(
                formation,
                RouteMoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                discovery.SelectedSource.Identity.CanonicalBasePath,
                "Route Move navigation exposure inspection was interrupted.");
        }

        return IsUnsupportedLeaf(discovery, routeFacts, exposure, sharing)
            ? Boundary(
                formation,
                RouteMoveFindingCode.InvalidSubject,
                CliSemanticStatus.Invalid,
                discovery.SelectedSource.Identity.CanonicalBasePath,
                "The selected ordinary Markdown source is not exposed by its complete routed parent.")
            : null;
    }

    private static bool IsUnsupportedLeaf(
        RouteMoveSubjectDiscovery discovery,
        SourceRouteFacts routeFacts,
        RouteNavigationExposure exposure,
        SourceSharing sharing)
    {
        if (discovery.Kind != RouteMoveSubjectKind.Leaf || !sharing.Includes(discovery.SelectedSource.Identity.CanonicalBasePath))
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
