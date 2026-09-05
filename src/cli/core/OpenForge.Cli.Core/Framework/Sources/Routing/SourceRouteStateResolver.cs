using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Framework.Sources.Routing;

internal static class SourceRouteStateResolver
{
    internal static SourceRouteState Read(
        SourceLogicalSource source,
        SourceRouteTopology topology,
        bool areLoaderRootFactsComplete)
    {
        var node = topology.FindByPath(source.Identity.CanonicalBasePath);
        if (node is null)
        {
            return areLoaderRootFactsComplete
                ? SourceRouteState.Unrouted
                : SourceRouteState.Unavailable;
        }

        if (HasAmbiguousParent(node, topology))
        {
            return SourceRouteState.Ambiguous;
        }

        if (topology.ReadAbsoluteDepth(source.Identity.CanonicalBasePath) is not null)
        {
            return SourceRouteState.Routed;
        }

        return areLoaderRootFactsComplete
            ? SourceRouteState.Unrouted
            : SourceRouteState.Unavailable;
    }

    private static bool HasAmbiguousParent(
        SourceRouteNode node,
        SourceRouteTopology topology)
    {
        var current = node;
        var visited = new HashSet<string>(StringComparer.Ordinal);
        while (visited.Add(current.Identity.CanonicalBasePath))
        {
            if (current.ParentState == SourceRouteParentState.Ambiguous)
            {
                return true;
            }

            if (current.ParentState != SourceRouteParentState.Resolved)
            {
                return false;
            }

            current = topology.FindByPath(current.ParentPaths[0])
                ?? throw new InvalidOperationException(
                    "A source route parent is missing from the topology.");
        }

        return true;
    }
}
