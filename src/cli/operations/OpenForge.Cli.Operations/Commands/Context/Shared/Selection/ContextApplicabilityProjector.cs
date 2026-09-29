using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Operation;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Selection;

internal static class ContextApplicabilityProjector
{
    internal static SourceApplicabilityResult? Evaluate(
        ContextGraph graph,
        ContextGraphSource source,
        IReadOnlyList<string> workingPaths)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(workingPaths);

        var chain = ReadChain(graph, source);
        if (chain is null)
        {
            return null;
        }

        return SourceApplicabilityEvaluator.Evaluate(
            chain.Select(item => new SourceApplyToCondition(item.CanonicalPath, item.Metadata.ApplyTo)).ToArray(),
            workingPaths);
    }

    internal static ContextApplicability? Project(
        SourceApplicabilityResult? result,
        bool workingPathsSupplied)
    {
        if (result is null)
        {
            return null;
        }

        if (!result.Conditions.Any(condition => condition.Metadata.State != ApplyToMetadataState.Absent)
            && !workingPathsSupplied)
        {
            return null;
        }

        return new ContextApplicability(
            result.State switch
            {
                SourceApplicabilityState.Unconditioned => ContextApplicabilityState.Unconditioned,
                SourceApplicabilityState.Matched => ContextApplicabilityState.Matched,
                SourceApplicabilityState.Unmatched => ContextApplicabilityState.Unmatched,
                SourceApplicabilityState.Pending => ContextApplicabilityState.Pending,
                SourceApplicabilityState.Invalid => ContextApplicabilityState.Invalid,
                _ => throw new ArgumentOutOfRangeException(nameof(result), result.State, "The source applicability state is not defined."),
            },
            result.Conditions
                .Where(condition => condition.Metadata.State != ApplyToMetadataState.Absent)
                .Select(condition => new ContextApplicabilityCondition(
                    condition.CanonicalSourcePath,
                    condition.Metadata.Patterns.Select(pattern => pattern.Text))),
            result.MatchingPaths);
    }

    internal static IReadOnlyList<ContextPendingCondition> PendingConditions(SourceApplicabilityResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.Conditions
            .Where(condition => condition.Metadata.State == ApplyToMetadataState.Valid)
            .Select(condition => new ContextPendingCondition(
                condition.CanonicalSourcePath,
                condition.Metadata.Patterns.Select(pattern => pattern.Text)))
            .ToArray();
    }

    internal static IReadOnlyList<ContextGraphSource>? ReadChain(ContextGraph graph, ContextGraphSource source)
    {
        var current = graph.RouteFacts.Topology.FindByPath(source.CanonicalPath);
        if (current is null)
        {
            return source.RouteState == SourceRouteState.Routed ? null : [source];
        }

        var chain = new List<ContextGraphSource>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        while (seen.Add(current.Identity.CanonicalBasePath))
        {
            var item = graph.FindByPath(current.Identity.CanonicalBasePath);
            if (item is null)
            {
                return null;
            }

            chain.Add(item);
            if (current.ParentState == SourceRouteParentState.None)
            {
                chain.Reverse();
                return chain;
            }

            if (current.ParentState != SourceRouteParentState.Resolved)
            {
                return null;
            }

            current = graph.RouteFacts.Topology.FindByPath(current.ParentPaths[0]);
            if (current is null)
            {
                return null;
            }
        }

        return null;
    }
}
