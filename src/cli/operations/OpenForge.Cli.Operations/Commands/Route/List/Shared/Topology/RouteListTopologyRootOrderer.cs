using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

internal static class RouteListTopologyRootOrderer
{
    internal static IReadOnlyList<SourceRouteNode> Order(
        IReadOnlyList<SourceRouteNode> roots,
        SourceRouteTopology topology)
    {
        var selectedByPath = roots.ToDictionary(
            root => root.Identity.CanonicalBasePath,
            StringComparer.Ordinal);
        var childrenBySelectedParent = new Dictionary<string, List<SourceRouteNode>>(StringComparer.Ordinal);
        var topLevel = new List<SourceRouteNode>();
        foreach (var root in roots)
        {
            var selectedParent = ReadNearestSelectedParent(root, selectedByPath, topology);
            if (selectedParent is null)
            {
                topLevel.Add(root);
                continue;
            }

            if (!childrenBySelectedParent.TryGetValue(selectedParent, out var children))
            {
                children = [];
                childrenBySelectedParent.Add(selectedParent, children);
            }

            children.Add(root);
        }

        var ordered = new List<SourceRouteNode>(roots.Count);
        foreach (var root in topLevel.OrderBy(node => node.Identity.CanonicalBasePath, StringComparer.Ordinal))
        {
            AddRootAndSelectedDescendants(root, childrenBySelectedParent, ordered);
        }

        return ordered;
    }

    private static string? ReadNearestSelectedParent(
        SourceRouteNode root,
        IReadOnlyDictionary<string, SourceRouteNode> selectedByPath,
        SourceRouteTopology topology)
    {
        var current = root;
        while (current.ParentState == SourceRouteParentState.Resolved)
        {
            var parentPath = current.ParentPaths[0];
            if (selectedByPath.ContainsKey(parentPath))
            {
                return parentPath;
            }

            current = topology.FindByPath(parentPath)
                ?? throw new InvalidOperationException("A selected root parent is missing from the immutable source topology.");
        }

        return null;
    }

    private static void AddRootAndSelectedDescendants(
        SourceRouteNode root,
        IReadOnlyDictionary<string, List<SourceRouteNode>> childrenBySelectedParent,
        ICollection<SourceRouteNode> ordered)
    {
        ordered.Add(root);
        var path = root.Identity.CanonicalBasePath;
        if (!childrenBySelectedParent.TryGetValue(path, out var children))
        {
            return;
        }

        foreach (var child in children.OrderBy(node => node.Identity.CanonicalBasePath, StringComparer.Ordinal))
        {
            AddRootAndSelectedDescendants(child, childrenBySelectedParent, ordered);
        }
    }
}
