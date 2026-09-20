using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;

internal enum RouteInspectResolutionState
{
    Resolved,
    Incomplete,
    Invalid,
    Blocked,
    Failed,
    Interrupted,
}

internal sealed class RouteInspectResolution
{
    private RouteInspectResolution(
        RouteInspectResolutionState state,
        RouteInspectSelection selection,
        RouteInspectIdentity? identity,
        RouteInspectGraph? graph,
        IReadOnlyList<RouteInspectResolutionIssue> issues)
    {
        State = state;
        Selection = selection;
        Identity = identity;
        Graph = graph;
        Issues = issues;
    }

    internal RouteInspectResolutionState State { get; }

    internal RouteInspectSelection Selection { get; }

    internal RouteInspectIdentity? Identity { get; }

    internal RouteInspectGraph? Graph { get; }

    internal IReadOnlyList<RouteInspectResolutionIssue> Issues { get; }

    internal RouteInspectIdentity ReadIdentity()
    {
        return Identity
            ?? throw new InvalidOperationException("A route-inspect resolution requires an identity before it can expose one.");
    }

    internal RouteInspectGraph ReadGraph()
    {
        return Graph
            ?? throw new InvalidOperationException("A route-inspect resolution requires a graph before it can expose one.");
    }

    internal static RouteInspectResolution Create(
        RouteInspectResolutionState state,
        RouteInspectSelection selection,
        RouteInspectIdentity? identity,
        RouteInspectGraph? graph,
        IEnumerable<RouteInspectResolutionIssue> issues)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The route-inspect resolution state is not defined.");
        }

        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(issues);
        var materializedIssues = issues.ToArray();
        if (materializedIssues.Any(issue => issue is null))
        {
            throw new ArgumentException("Route-inspect resolution issues cannot contain null.", nameof(issues));
        }

        ValidateState(state, selection, identity, graph, materializedIssues);
        return new RouteInspectResolution(
            state,
            selection,
            identity,
            graph,
            new ReadOnlyCollection<RouteInspectResolutionIssue>(materializedIssues));
    }

    private static void ValidateState(
        RouteInspectResolutionState state,
        RouteInspectSelection selection,
        RouteInspectIdentity? identity,
        RouteInspectGraph? graph,
        IReadOnlyList<RouteInspectResolutionIssue> issues)
    {
        switch (state)
        {
            case RouteInspectResolutionState.Resolved:
                RequireResolvedFacts(selection, identity, graph);
                if (issues.Count != 0)
                {
                    throw new ArgumentException("A resolved route-inspect resolution cannot contain issues.", nameof(issues));
                }

                return;
            case RouteInspectResolutionState.Incomplete:
                RequireResolvedFacts(selection, identity, graph);
                RequireIssues(issues);
                return;
            case RouteInspectResolutionState.Invalid:
                if (selection.IsResolved || identity is not null || graph is not null)
                {
                    throw new ArgumentException(
                        "An invalid route-inspect resolution requires unresolved selection and no resolved facts.",
                        nameof(selection));
                }

                RequireIssues(issues);
                return;
            case RouteInspectResolutionState.Blocked:
            case RouteInspectResolutionState.Failed:
            case RouteInspectResolutionState.Interrupted:
                RequireIssues(issues);
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, "The route-inspect resolution state is not defined.");
        }
    }

    private static void RequireResolvedFacts(
        RouteInspectSelection selection,
        RouteInspectIdentity? identity,
        RouteInspectGraph? graph)
    {
        if (!selection.IsResolved || identity is null || graph is null)
        {
            throw new ArgumentException("This route-inspect resolution requires resolved selection, identity, and graph facts.");
        }
    }

    private static void RequireIssues(IReadOnlyList<RouteInspectResolutionIssue> issues)
    {
        if (issues.Count == 0)
        {
            throw new ArgumentException("This route-inspect resolution state requires at least one issue.", nameof(issues));
        }
    }
}
