using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal sealed partial class RouteInspectSourceFactsResolver
{
    internal RouteInspectResolution Resolve(
        RouteInspectSourceResolutionInput input,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return RouteInspectResolutionSupport.Interrupted(input.Selection, input.RequestedPath);
        }

        var logicalSource = input.LogicalSource;
        if (input.Projection.Source is not { } projectedSource)
        {
            throw new InvalidOperationException("A selected Route source projection must be available.");
        }

        if (logicalSource.Base.Form == SourceDocumentForm.Loader)
        {
            return RouteInspectResolution.Create(
                RouteInspectResolutionState.Invalid,
                RouteInspectResolutionSupport.UnresolvedSelection(input.Selection),
                null,
                null,
                [RouteInspectResolutionSupport.CreateIssue(
                    RouteInspectResolutionIssueCode.LoaderSubject,
                    input.RequestedPath,
                    "The Loader is the workspace routing root, not an inspectable route source.")]);
        }

        var routeFacts = input.Resolution.RouteFacts;
        var routeFact = routeFacts.RouteFacts.Single(fact =>
            ReferenceEquals(fact.Identity, logicalSource.Identity));
        var routeState = ReadRouteState(logicalSource, routeFact, input.Resolution.Projections.ProjectionSet);
        var graph = new RouteInspectGraph(
            input.Resolution.Projections.ProjectionSet,
            routeFacts);
        var identity = new RouteInspectIdentity(
            logicalSource.Identity.AutomaticId,
            logicalSource.Identity.CanonicalBasePath,
            RouteInspectSourcePolicy.ReadInspectKind(logicalSource.Base.Form),
            RouteInspectSourcePolicy.ReadInspectForm(logicalSource.Base.Form),
            routeState,
            ReadPhysicalLayers(projectedSource),
            projectedSource.Metadata.Tags);

        var issues = new List<RouteInspectResolutionIssue>();
        AddRouteIssues(input, issues);
        AddReadIssues(input, issues);
        if (routeState == RouteInspectRouteState.Ambiguous
            && issues.All(issue => issue.Code != RouteInspectResolutionIssueCode.AmbiguousRoute))
        {
            issues.Add(RouteInspectResolutionSupport.CreateIssue(
                RouteInspectResolutionIssueCode.AmbiguousRoute,
                input.RequestedPath,
                "The selected source has ambiguous authored route meaning."));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return RouteInspectResolutionSupport.Interrupted(input.Selection, input.RequestedPath);
        }

        var state = RouteInspectResolutionSupport.ReadResolutionState(issues);
        return RouteInspectResolution.Create(state, input.Selection, identity, graph, issues);
    }

    private static RouteInspectRouteState ReadRouteState(
        SourceLogicalSource logicalSource,
        SourceRouteFact fact,
        RouteSourceProjectionSet projectionSet)
    {
        if (HasDuplicateEntrypoint(logicalSource, projectionSet))
        {
            return RouteInspectRouteState.Ambiguous;
        }

        return fact.State switch
        {
            SourceRouteState.Routed => RouteInspectRouteState.Routed,
            SourceRouteState.Unrouted when SourceFormClassifier.IsEntrypoint(logicalSource.Base.Form) =>
                RouteInspectRouteState.Detached,
            SourceRouteState.Unrouted => RouteInspectRouteState.NotRouted,
            SourceRouteState.Ambiguous => RouteInspectRouteState.Ambiguous,
            SourceRouteState.Unavailable => RouteInspectRouteState.Unresolved,
            _ => throw new ArgumentOutOfRangeException(nameof(fact), fact.State, "The neutral route state is not defined."),
        };
    }

    private static bool HasDuplicateEntrypoint(
        SourceLogicalSource logicalSource,
        RouteSourceProjectionSet projectionSet)
    {
        if (!SourceFormClassifier.IsEntrypoint(logicalSource.Base.Form))
        {
            return false;
        }

        var parent = SourceLogicalPath.ReadParent(logicalSource.Identity.CanonicalBasePath);
        return projectionSet.Projections.Count(projection =>
            SourceFormClassifier.IsEntrypoint(projection.LogicalSource.Base.Form)
            && string.Equals(
                SourceLogicalPath.ReadParent(projection.LogicalSource.Identity.CanonicalBasePath),
                parent,
                StringComparison.Ordinal)) > 1;
    }

}
