using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Routing;

internal sealed class SourceRouteTopology
{
    private readonly IReadOnlyDictionary<string, SourceRouteNode> _nodesByPath;

    internal SourceRouteTopology(
        IEnumerable<SourceRouteNode> nodes,
        IEnumerable<string> loaderRootPaths)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(loaderRootPaths);

        var orderedNodes = nodes
            .Select(node => node ?? throw new ArgumentException("Source route topology nodes cannot contain null.", nameof(nodes)))
            .OrderBy(node => node.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
        if (orderedNodes.Select(node => node.Identity.CanonicalBasePath)
            .Distinct(StringComparer.Ordinal)
            .Count() != orderedNodes.Length)
        {
            throw new ArgumentException("Source route topology nodes require unique canonical paths.", nameof(nodes));
        }

        var byPath = orderedNodes.ToDictionary(node => node.Identity.CanonicalBasePath, StringComparer.Ordinal);
        var orderedRoots = loaderRootPaths.OrderBy(path => path, StringComparer.Ordinal).ToArray();
        if (orderedRoots.Any(path => !byPath.ContainsKey(path))
            || orderedRoots.Distinct(StringComparer.Ordinal).Count() != orderedRoots.Length)
        {
            throw new ArgumentException("Every Loader root must identify one topology node.", nameof(loaderRootPaths));
        }

        ValidateRelationships(byPath);
        ValidateAcyclic(byPath);

        _nodesByPath = new ReadOnlyDictionary<string, SourceRouteNode>(byPath);
        Nodes = new ReadOnlyCollection<SourceRouteNode>(orderedNodes);
        LoaderRootPaths = new ReadOnlyCollection<string>(orderedRoots);
    }

    internal IReadOnlyList<SourceRouteNode> Nodes { get; }

    internal IReadOnlyList<string> LoaderRootPaths { get; }

    internal SourceRouteNode? FindByPath(string canonicalPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalPath);
        return _nodesByPath.TryGetValue(canonicalPath, out var node)
            ? node
            : null;
    }

    internal int? ReadAbsoluteDepth(string canonicalPath)
    {
        if (!_nodesByPath.TryGetValue(canonicalPath, out var node))
        {
            return null;
        }

        var depth = 0;
        var hasLoaderExposure = false;
        var visited = new HashSet<string>(StringComparer.Ordinal);
        while (node.ParentState == SourceRouteParentState.Resolved)
        {
            if (!visited.Add(node.Identity.CanonicalBasePath))
            {
                return null;
            }

            hasLoaderExposure |= LoaderRootPaths.Contains(
                node.Identity.CanonicalBasePath,
                StringComparer.Ordinal);
            node = _nodesByPath[node.ParentPaths[0]];
            depth = checked(depth + 1);
        }

        hasLoaderExposure |= LoaderRootPaths.Contains(
            node.Identity.CanonicalBasePath,
            StringComparer.Ordinal);
        return node.ParentState == SourceRouteParentState.None && hasLoaderExposure
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
        while (visited.Add(current.Identity.CanonicalBasePath))
        {
            if (string.Equals(current.Identity.CanonicalBasePath, rootPath, StringComparison.Ordinal))
            {
                return true;
            }

            if (current.ParentState != SourceRouteParentState.Resolved)
            {
                break;
            }

            current = _nodesByPath[current.ParentPaths[0]];
            relativeDepth = checked(relativeDepth + 1);
        }

        relativeDepth = default;
        return false;
    }

    private static void ValidateRelationships(IReadOnlyDictionary<string, SourceRouteNode> byPath)
    {
        foreach (var node in byPath.Values)
        {
            foreach (var parentPath in node.ParentPaths)
            {
                if (!byPath.ContainsKey(parentPath))
                {
                    throw new ArgumentException("Every source route parent candidate must identify a topology node.", nameof(byPath));
                }
            }

            foreach (var childPath in node.ChildPaths)
            {
                if (!byPath.TryGetValue(childPath, out var child)
                    || child.ParentState != SourceRouteParentState.Resolved
                    || !child.ParentPaths.Contains(node.Identity.CanonicalBasePath, StringComparer.Ordinal))
                {
                    throw new ArgumentException("Source route child relationships must be reciprocal.", nameof(byPath));
                }
            }

            if (node.ParentState == SourceRouteParentState.Resolved)
            {
                var parent = byPath[node.ParentPaths[0]];
                if (!parent.ChildPaths.Contains(node.Identity.CanonicalBasePath, StringComparer.Ordinal))
                {
                    throw new ArgumentException("Source route parent relationships must be reciprocal.", nameof(byPath));
                }
            }
        }
    }

    private static void ValidateAcyclic(IReadOnlyDictionary<string, SourceRouteNode> byPath)
    {
        foreach (var node in byPath.Values)
        {
            var current = node;
            var visited = new HashSet<string>(StringComparer.Ordinal);
            while (current.ParentState == SourceRouteParentState.Resolved)
            {
                if (!visited.Add(current.Identity.CanonicalBasePath))
                {
                    throw new ArgumentException("Resolved source route topology cannot contain a parent cycle.", nameof(byPath));
                }

                current = byPath[current.ParentPaths[0]];
            }
        }
    }
}
