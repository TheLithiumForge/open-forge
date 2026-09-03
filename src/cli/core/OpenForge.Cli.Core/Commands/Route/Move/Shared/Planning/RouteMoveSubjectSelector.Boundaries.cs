using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;

internal sealed partial class RouteMoveSubjectSelector
{
    private static RouteMoveResultFormation? ReadRouteBoundary(
        RouteMoveSubjectDiscovery discovery,
        SourceRouteFacts routeFacts)
    {
        var formation = RouteMoveBoundary.Start(discovery.Request) with
        {
            Source = discovery.Source,
        };
        if (routeFacts.IsCancelled)
        {
            return Boundary(
                formation,
                RouteMoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                discovery.SelectedSource.Identity.CanonicalBasePath,
                "Route Move route inspection was interrupted.");
        }

        var route = routeFacts.RouteFacts.Single(fact => string.Equals(
            fact.Identity.CanonicalBasePath,
            discovery.SelectedSource.Identity.CanonicalBasePath,
            StringComparison.Ordinal));
        if (route.State != SourceRouteState.Routed)
        {
            return FromRouteState(formation, discovery.SelectedSource, route.State);
        }

        return !route.IsIdentityUnique || HasPhysicalAlias(discovery.Catalogue, discovery.SelectedSource)
            ? Boundary(
                formation,
                RouteMoveFindingCode.IdentityCollision,
                CliSemanticStatus.Blocked,
                discovery.SelectedSource.Identity.CanonicalBasePath,
                "The selected source does not retain one unique logical and physical identity.")
            : null;
    }

    private static bool HasPhysicalAlias(SourceCatalogue catalogue, SourceLogicalSource source)
        => catalogue.Issues.Any(issue => issue.Code == SourceCatalogueIssueCode.PhysicalAlias
            && (string.Equals(
                    issue.AttemptedCanonicalPath,
                    source.Identity.CanonicalBasePath,
                    StringComparison.Ordinal)
                || issue.RelatedPaths.Contains(
                    source.Identity.CanonicalBasePath,
                    StringComparer.Ordinal)));

    private static RouteMoveResultFormation FromRootIssue(
        RouteMoveResultFormation formation,
        SourceCatalogueIssue issue)
    {
        var unsafeRoot = issue.Code == SourceCatalogueIssueCode.RootUnsafe;
        return Boundary(
            formation,
            unsafeRoot ? RouteMoveFindingCode.WorkspaceUnsafe : RouteMoveFindingCode.WorkspaceUnavailable,
            unsafeRoot ? CliSemanticStatus.Blocked : CliSemanticStatus.Incomplete,
            issue.AttemptedCanonicalPath,
            issue.Failure?.DirectCause ?? "The workspace source root is unsafe or unavailable.");
    }

    private static RouteMoveResultFormation FromUnresolved(
        RouteMoveResultFormation formation,
        SourceReferenceResolution resolution)
    {
        if (resolution.Form == SourceReferenceKind.SourcePath)
        {
            formation = formation with
            {
                Source = formation.Source with
                {
                    SelectedBy = RouteMoveSourceSelection.BasePath,
                },
            };
        }

        var (code, status) = resolution.State switch
        {
            SourceReferenceResolutionState.Invalid =>
                (RouteMoveFindingCode.InvalidSource, CliSemanticStatus.Invalid),
            SourceReferenceResolutionState.Unknown =>
                (RouteMoveFindingCode.SourceNotFound, CliSemanticStatus.Invalid),
            SourceReferenceResolutionState.Unsupported =>
                (RouteMoveFindingCode.InvalidSubject, CliSemanticStatus.Invalid),
            SourceReferenceResolutionState.Ambiguous =>
                (RouteMoveFindingCode.IdentityCollision, CliSemanticStatus.Blocked),
            SourceReferenceResolutionState.Unsafe =>
                (RouteMoveFindingCode.SourceUnsafe, CliSemanticStatus.Blocked),
            SourceReferenceResolutionState.Resolved => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                resolution.State,
                "A resolved source reference cannot form an unresolved boundary."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                resolution.State,
                "The source-reference state is not defined."),
        };
        return Boundary(
            formation,
            code,
            status,
            resolution.CanonicalPath ?? resolution.Value,
            resolution.Cause ?? "The Route Move source could not be resolved.");
    }

    private static RouteMoveResultFormation FromRouteState(
        RouteMoveResultFormation formation,
        SourceLogicalSource source,
        SourceRouteState state)
    {
        var code = state == SourceRouteState.Ambiguous
            ? RouteMoveFindingCode.RouteAmbiguous
            : RouteMoveFindingCode.InvalidSubject;
        var status = state switch
        {
            SourceRouteState.Ambiguous => CliSemanticStatus.Blocked,
            SourceRouteState.Unavailable => CliSemanticStatus.Incomplete,
            SourceRouteState.Unrouted => CliSemanticStatus.Invalid,
            SourceRouteState.Routed => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "A routed subject cannot form a route-state boundary."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The source route state is not defined."),
        };
        return Boundary(
            formation,
            code,
            status,
            source.Identity.CanonicalBasePath,
            "The selected source does not have one complete routed meaning.");
    }

    private static RouteMoveResultFormation Boundary(
        RouteMoveResultFormation formation,
        RouteMoveFindingCode code,
        CliSemanticStatus status,
        string? target,
        string cause)
        => RouteMoveBoundary.Stop(formation, code, status, target, cause);
}
