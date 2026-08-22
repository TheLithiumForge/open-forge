using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Models.Topology;

internal sealed class RouteTopologyFacts
{
    private readonly IReadOnlyDictionary<string, RouteTopologyNode> _nodesByPath;

    internal RouteTopologyFacts(
        IEnumerable<RouteTopologyNode> nodes,
        IEnumerable<string> loaderRootPaths)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(loaderRootPaths);

        var materializedNodes = nodes
            .Select(node => node ?? throw new ArgumentException("Topology nodes cannot contain null.", nameof(nodes)))
            .OrderBy(node => node.Source.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        if (materializedNodes.Select(node => node.Source.CanonicalPath)
            .Distinct(StringComparer.Ordinal).Count() != materializedNodes.Length)
        {
            throw new ArgumentException("Topology nodes require unique canonical source paths.", nameof(nodes));
        }

        var byPath = materializedNodes.ToDictionary(node => node.Source.CanonicalPath, StringComparer.Ordinal);
        var roots = loaderRootPaths.OrderBy(path => path, StringComparer.Ordinal).ToArray();
        if (roots.Distinct(StringComparer.Ordinal).Count() != roots.Length
            || roots.Any(path => !byPath.TryGetValue(path, out var node)
                || node.Source.Kind != RouteSourceKind.Entrypoint))
        {
            throw new ArgumentException("Every Loader root must identify one entrypoint node.", nameof(loaderRootPaths));
        }

        ValidateRelationships(byPath);
        ValidateAcyclic(byPath);

        _nodesByPath = new ReadOnlyDictionary<string, RouteTopologyNode>(byPath);
        Nodes = new ReadOnlyCollection<RouteTopologyNode>(materializedNodes);
        LoaderRootPaths = new ReadOnlyCollection<string>(roots);
    }

    internal IReadOnlyList<RouteTopologyNode> Nodes { get; }

    internal IReadOnlyList<string> LoaderRootPaths { get; }

    internal RouteTopologyNode? FindByPath(string canonicalPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalPath);
        return _nodesByPath.TryGetValue(canonicalPath, out var node) ? node : null;
    }

    internal int? ReadAbsoluteDepth(string canonicalPath)
    {
        var node = FindByPath(canonicalPath)
            ?? throw new ArgumentException("The source path is not present in the topology.", nameof(canonicalPath));
        var depth = 0;
        var hasLoaderExposure = false;
        while (node.ParentState == RouteTopologyParentState.Resolved)
        {
            hasLoaderExposure |= LoaderRootPaths.Contains(
                node.Source.CanonicalPath,
                StringComparer.Ordinal);
            node = _nodesByPath[node.ParentPath!];
            depth = checked(depth + 1);
        }

        hasLoaderExposure |= LoaderRootPaths.Contains(
            node.Source.CanonicalPath,
            StringComparer.Ordinal);
        return node.ParentState == RouteTopologyParentState.None && hasLoaderExposure
            ? depth
            : null;
    }

    internal bool TryReadRelativeDepth(
        string rootPath,
        string descendantPath,
        out int relativeDepth)
    {
        if (!_nodesByPath.TryGetValue(descendantPath, out var current))
        {
            relativeDepth = default;
            return false;
        }

        relativeDepth = 0;
        var visited = new HashSet<string>(StringComparer.Ordinal);
        while (visited.Add(current.Source.CanonicalPath))
        {
            if (string.Equals(current.Source.CanonicalPath, rootPath, StringComparison.Ordinal))
            {
                return true;
            }

            if (current.ParentState != RouteTopologyParentState.Resolved)
            {
                break;
            }

            current = _nodesByPath[current.ParentPath!];
            relativeDepth = checked(relativeDepth + 1);
        }

        relativeDepth = default;
        return false;
    }

    internal static string ReadRouteDirectory(RouteTopologyNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return RouteLogicalPath.ReadParent(node.Source.CanonicalPath);
    }

    private static void ValidateRelationships(IReadOnlyDictionary<string, RouteTopologyNode> byPath)
    {
        foreach (var node in byPath.Values)
        {
            foreach (var parentPath in node.ParentPaths)
            {
                if (!byPath.TryGetValue(parentPath, out var parent)
                    || parent.Source.Kind != RouteSourceKind.Entrypoint)
                {
                    throw new ArgumentException("Every topology parent candidate must identify an entrypoint node.", nameof(byPath));
                }
            }

            foreach (var childPath in node.ChildPaths)
            {
                if (!byPath.TryGetValue(childPath, out var child)
                    || child.ParentState != RouteTopologyParentState.Resolved
                    || child.ParentPath != node.Source.CanonicalPath)
                {
                    throw new ArgumentException("Topology child relationships must be reciprocal.", nameof(byPath));
                }
            }

            if (node.ParentState == RouteTopologyParentState.Resolved
                && !byPath[node.ParentPath!].ChildPaths.Contains(node.Source.CanonicalPath, StringComparer.Ordinal))
            {
                throw new ArgumentException("Topology parent relationships must be reciprocal.", nameof(byPath));
            }
        }
    }

    private static void ValidateAcyclic(IReadOnlyDictionary<string, RouteTopologyNode> byPath)
    {
        foreach (var node in byPath.Values)
        {
            var current = node;
            var visited = new HashSet<string>(StringComparer.Ordinal);
            while (current.ParentState == RouteTopologyParentState.Resolved)
            {
                if (!visited.Add(current.Source.CanonicalPath))
                {
                    throw new ArgumentException("Resolved route topology cannot contain a parent cycle.", nameof(byPath));
                }

                current = byPath[current.ParentPath!];
            }
        }
    }
}
