using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal sealed partial class RouteRemoveSubjectSelector
{
    private static RouteRemoveResultFormation? ReadRouteBoundary(
        RouteRemoveSubjectDiscovery discovery,
        SourceRouteFacts routeFacts)
    {
        var formation = RouteRemoveBoundary.Start(discovery.Request) with
        {
            Source = discovery.Source,
        };
        if (routeFacts.IsCancelled)
        {
            return Boundary(
                formation,
                RouteRemoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                discovery.SelectedSource.Identity.CanonicalBasePath,
                "Route Remove route inspection was interrupted.");
        }

        var route = routeFacts.RouteFacts.Single(fact => string.Equals(
            fact.Identity.CanonicalBasePath,
            discovery.SelectedSource.Identity.CanonicalBasePath,
            StringComparison.Ordinal));
        if (route.State != SourceRouteState.Routed)
        {
            return FromRouteState(formation, discovery.SelectedSource, route.State);
        }

        if (!route.IsIdentityUnique && !HasExactPhysicalSelection(discovery.Request))
        {
            return Boundary(
                formation,
                RouteRemoveFindingCode.IdentityCollision,
                CliSemanticStatus.Blocked,
                discovery.SelectedSource.Identity.CanonicalBasePath,
                "The selected source does not retain one unique logical and physical identity.");
        }

        return HasPhysicalAlias(discovery.Catalogue, discovery.SelectedSource)
            ? Boundary(
                formation,
                RouteRemoveFindingCode.IdentityCollision,
                CliSemanticStatus.Blocked,
                discovery.SelectedSource.Identity.CanonicalBasePath,
                "The selected source does not retain one unique logical and physical identity.")
            : null;
    }

    private static bool HasExactPhysicalSelection(RouteRemoveRequest request)
        => request.FrozenSourceReference is not null
            || SourceReferenceParser.Parse(request.SourceReference).Kind == SourceReferenceKind.SourcePath;

    private static bool HasPhysicalAlias(SourceCatalogue catalogue, SourceLogicalSource source)
        => catalogue.Issues.Any(issue => issue.Code == SourceCatalogueIssueCode.PhysicalAlias
            && (string.Equals(
                    issue.AttemptedCanonicalPath,
                    source.Identity.CanonicalBasePath,
                    StringComparison.Ordinal)
                || issue.RelatedPaths.Contains(
                    source.Identity.CanonicalBasePath,
                    StringComparer.Ordinal)));

    private static RouteRemoveResultFormation FromRootIssue(
        RouteRemoveResultFormation formation,
        SourceCatalogueIssue issue)
    {
        var unsafeRoot = issue.Code == SourceCatalogueIssueCode.RootUnsafe;
        return Boundary(
            formation,
            unsafeRoot ? RouteRemoveFindingCode.WorkspaceUnsafe : RouteRemoveFindingCode.WorkspaceUnavailable,
            unsafeRoot ? CliSemanticStatus.Blocked : CliSemanticStatus.Incomplete,
            issue.AttemptedCanonicalPath,
            issue.Failure?.DirectCause ?? "The workspace source root is unsafe or unavailable.");
    }

    private static RouteRemoveResultFormation FromUnresolved(
        RouteRemoveResultFormation formation,
        SourceReferenceResolution resolution)
    {
        if (resolution.Form == SourceReferenceKind.SourcePath)
        {
            formation = formation with
            {
                Source = formation.Source with
                {
                    SelectedBy = RouteRemoveSourceSelection.BasePath,
                },
            };
        }

        var (code, status) = resolution.State switch
        {
            SourceReferenceResolutionState.Invalid =>
                (RouteRemoveFindingCode.InvalidSource, CliSemanticStatus.Invalid),
            SourceReferenceResolutionState.Unknown =>
                (RouteRemoveFindingCode.SourceNotFound, CliSemanticStatus.Invalid),
            SourceReferenceResolutionState.Unsupported =>
                (RouteRemoveFindingCode.InvalidSubject, CliSemanticStatus.Invalid),
            SourceReferenceResolutionState.Ambiguous =>
                (RouteRemoveFindingCode.IdentityCollision, CliSemanticStatus.Blocked),
            SourceReferenceResolutionState.Unsafe =>
                (RouteRemoveFindingCode.SourceUnsafe, CliSemanticStatus.Blocked),
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
            resolution.Cause ?? "The Route Remove source could not be resolved.");
    }

    private static RouteRemoveResultFormation FromRouteState(
        RouteRemoveResultFormation formation,
        SourceLogicalSource source,
        SourceRouteState state)
    {
        var code = state == SourceRouteState.Ambiguous
            ? RouteRemoveFindingCode.RouteAmbiguous
            : RouteRemoveFindingCode.InvalidSubject;
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

    private static RouteRemoveResultFormation Boundary(
        RouteRemoveResultFormation formation,
        RouteRemoveFindingCode code,
        CliSemanticStatus status,
        string? target,
        string cause)
        => RouteRemoveBoundary.Stop(formation, code, status, target, cause);
}
