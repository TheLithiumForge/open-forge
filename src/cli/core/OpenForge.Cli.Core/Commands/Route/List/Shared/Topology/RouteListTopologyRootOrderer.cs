using OpenForge.Cli.Core.Commands.Route.Shared.Models.Topology;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

internal static class RouteListTopologyRootOrderer
{
    internal static IReadOnlyList<RouteTopologyNode> Order(
        IReadOnlyList<RouteTopologyNode> roots,
        RouteTopologyFacts topology)
    {
        ArgumentNullException.ThrowIfNull(roots);
        ArgumentNullException.ThrowIfNull(topology);
        var selectedByPath = roots.ToDictionary(
            root => root.Source.CanonicalPath,
            StringComparer.Ordinal);
        var childrenBySelectedParent = new Dictionary<string, List<RouteTopologyNode>>(StringComparer.Ordinal);
        var topLevel = new List<RouteTopologyNode>();
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

        var ordered = new List<RouteTopologyNode>(roots.Count);
        foreach (var root in topLevel.OrderBy(node => node.Source.CanonicalPath, StringComparer.Ordinal))
        {
            AddRootAndSelectedDescendants(root, childrenBySelectedParent, ordered);
        }

        return ordered;
    }

    private static string? ReadNearestSelectedParent(
        RouteTopologyNode root,
        IReadOnlyDictionary<string, RouteTopologyNode> selectedByPath,
        RouteTopologyFacts topology)
    {
        var current = root;
        while (current.ParentState == RouteTopologyParentState.Resolved)
        {
            var parentPath = current.ParentPath!;
            if (selectedByPath.ContainsKey(parentPath))
            {
                return parentPath;
            }

            current = topology.FindByPath(parentPath)!;
        }

        return null;
    }

    private static void AddRootAndSelectedDescendants(
        RouteTopologyNode root,
        IReadOnlyDictionary<string, List<RouteTopologyNode>> childrenBySelectedParent,
        ICollection<RouteTopologyNode> ordered)
    {
        ordered.Add(root);
        var path = root.Source.CanonicalPath;
        if (!childrenBySelectedParent.TryGetValue(path, out var children))
        {
            return;
        }

        foreach (var child in children.OrderBy(node => node.Source.CanonicalPath, StringComparer.Ordinal))
        {
            AddRootAndSelectedDescendants(child, childrenBySelectedParent, ordered);
        }
    }
}
