using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

internal enum RouteListTopologyParentState
{
    None,
    Resolved,
    Ambiguous,
}

internal sealed class RouteListTopologyNode
{
    internal RouteListTopologyNode(
        RouteListInventorySource source,
        RouteListTopologyParentState parentState,
        IEnumerable<string> parentPaths,
        IEnumerable<string> childPaths)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!Enum.IsDefined(parentState))
        {
            throw new ArgumentOutOfRangeException(nameof(parentState), parentState, "The topology parent state is not defined.");
        }

        if (source.Source.Kind is not (
            RouteListSourceKind.Entrypoint
            or RouteListSourceKind.RoutedLeaf
            or RouteListSourceKind.RoutedNative))
        {
            throw new ArgumentException("A topology node requires a routed inventory source.", nameof(source));
        }

        var parents = MaterializePaths(parentPaths, nameof(parentPaths));
        if (parentState == RouteListTopologyParentState.None && parents.Count != 0
            || parentState == RouteListTopologyParentState.Resolved && parents.Count != 1
            || parentState == RouteListTopologyParentState.Ambiguous && parents.Count < 2)
        {
            throw new ArgumentException("The topology parent paths do not match the parent state.", nameof(parentPaths));
        }

        var children = MaterializePaths(childPaths, nameof(childPaths));
        if (source.Source.Kind != RouteListSourceKind.Entrypoint && children.Count != 0)
        {
            throw new ArgumentException("Only an entrypoint can have routed children.", nameof(childPaths));
        }

        var path = source.Source.CanonicalPath;
        if (parents.Contains(path, StringComparer.Ordinal) || children.Contains(path, StringComparer.Ordinal))
        {
            throw new ArgumentException("A topology node cannot be its own parent or child.");
        }

        Source = source;
        ParentState = parentState;
        ParentPaths = parents;
        ChildPaths = children;
    }

    internal RouteListInventorySource Source { get; }

    internal RouteListTopologyParentState ParentState { get; }

    internal IReadOnlyList<string> ParentPaths { get; }

    internal string? ParentPath => ParentState == RouteListTopologyParentState.Resolved
        ? ParentPaths[0]
        : null;

    internal IReadOnlyList<string> ChildPaths { get; }

    internal bool HasCompleteMetadata => Source.Metadata.State == RouteListMetadataState.Complete;

    private static IReadOnlyList<string> MaterializePaths(
        IEnumerable<string> paths,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(paths, parameterName);
        var materialized = paths.ToArray();
        if (materialized.Any(path => !RouteListLogicalPath.IsCanonical(path))
            || materialized.Distinct(StringComparer.Ordinal).Count() != materialized.Length)
        {
            throw new ArgumentException(
                "Topology relationship paths must be unique canonical logical paths.",
                parameterName);
        }

        return new ReadOnlyCollection<string>(
            materialized.OrderBy(path => path, StringComparer.Ordinal).ToArray());
    }
}

internal sealed class RouteListTopologyIdentityCollision
{
    internal RouteListTopologyIdentityCollision(string id, IEnumerable<string> paths)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(paths);
        var materialized = paths
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        if (materialized.Length < 2
            || materialized.Any(path => !RouteListLogicalPath.IsCanonical(path))
            || materialized.Distinct(StringComparer.Ordinal).Count() != materialized.Length)
        {
            throw new ArgumentException("A topology identity collision requires at least two unique canonical paths.", nameof(paths));
        }

        Id = id;
        Paths = new ReadOnlyCollection<string>(materialized);
    }

    internal string Id { get; }

    internal IReadOnlyList<string> Paths { get; }
}

internal sealed class RouteListTopologyFacts
{
    private readonly IReadOnlyDictionary<string, RouteListTopologyNode> _nodesByPath;

    internal RouteListTopologyFacts(
        IEnumerable<RouteListTopologyNode> nodes,
        IEnumerable<string> loaderRootPaths,
        IEnumerable<RouteListTopologyIdentityCollision> identityCollisions)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(loaderRootPaths);
        ArgumentNullException.ThrowIfNull(identityCollisions);
        var materialized = nodes
            .Select(node => node ?? throw new ArgumentException("Topology nodes cannot contain null.", nameof(nodes)))
            .OrderBy(node => node.Source.Source.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        if (materialized
            .Select(node => node.Source.Source.CanonicalPath)
            .Distinct(StringComparer.Ordinal)
            .Count() != materialized.Length)
        {
            throw new ArgumentException("Topology nodes require unique canonical source paths.", nameof(nodes));
        }

        var byPath = materialized.ToDictionary(
            node => node.Source.Source.CanonicalPath,
            StringComparer.Ordinal);
        var roots = loaderRootPaths.OrderBy(path => path, StringComparer.Ordinal).ToArray();
        if (roots.Distinct(StringComparer.Ordinal).Count() != roots.Length
                || roots.Any(path => !byPath.TryGetValue(path, out var node)
                || node.Source.Source.Kind != RouteListSourceKind.Entrypoint))
        {
            throw new ArgumentException("Every Loader root must identify one entrypoint node.", nameof(loaderRootPaths));
        }

        ValidateRelationships(byPath);
        ValidateAcyclic(byPath);

        _nodesByPath = new ReadOnlyDictionary<string, RouteListTopologyNode>(byPath);
        Nodes = new ReadOnlyCollection<RouteListTopologyNode>(materialized);
        LoaderRootPaths = new ReadOnlyCollection<string>(roots);
        IdentityCollisions = new ReadOnlyCollection<RouteListTopologyIdentityCollision>(identityCollisions
            .Select(collision => collision
                ?? throw new ArgumentException("Topology identity collisions cannot contain null.", nameof(identityCollisions)))
            .OrderBy(collision => collision.Id, StringComparer.Ordinal)
            .ToArray());
    }

    internal IReadOnlyList<RouteListTopologyNode> Nodes { get; }

    internal IReadOnlyList<string> LoaderRootPaths { get; }

    internal IReadOnlyList<RouteListTopologyIdentityCollision> IdentityCollisions { get; }

    internal RouteListTopologyNode? FindByPath(string canonicalPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalPath);
        return _nodesByPath.TryGetValue(canonicalPath, out var node)
            ? node
            : null;
    }

    internal int? ReadAbsoluteDepth(string canonicalPath)
    {
        var node = FindByPath(canonicalPath)
            ?? throw new ArgumentException("The source path is not present in the topology.", nameof(canonicalPath));
        var depth = 0;
        var hasLoaderExposure = false;
        while (node.ParentState == RouteListTopologyParentState.Resolved)
        {
            hasLoaderExposure |= LoaderRootPaths.Contains(
                node.Source.Source.CanonicalPath,
                StringComparer.Ordinal);
            node = _nodesByPath[node.ParentPath!];
            depth = checked(depth + 1);
        }

        hasLoaderExposure |= LoaderRootPaths.Contains(
            node.Source.Source.CanonicalPath,
            StringComparer.Ordinal);
        return node.ParentState == RouteListTopologyParentState.None && hasLoaderExposure
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
        while (visited.Add(current.Source.Source.CanonicalPath))
        {
            if (string.Equals(current.Source.Source.CanonicalPath, rootPath, StringComparison.Ordinal))
            {
                return true;
            }

            if (current.ParentState != RouteListTopologyParentState.Resolved)
            {
                break;
            }

            current = _nodesByPath[current.ParentPath!];
            relativeDepth = checked(relativeDepth + 1);
        }

        relativeDepth = default;
        return false;
    }

    internal static string ReadRouteDirectory(RouteListTopologyNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return RouteListLogicalPath.ReadParent(node.Source.Source.CanonicalPath);
    }

    private static void ValidateRelationships(
        IReadOnlyDictionary<string, RouteListTopologyNode> byPath)
    {
        foreach (var node in byPath.Values)
        {
            foreach (var parentPath in node.ParentPaths)
            {
                if (!byPath.ContainsKey(parentPath))
                {
                    throw new ArgumentException("Every topology parent path must identify another node.", nameof(byPath));
                }
            }

            foreach (var childPath in node.ChildPaths)
            {
                if (!byPath.TryGetValue(childPath, out var child)
                    || child.ParentState != RouteListTopologyParentState.Resolved
                    || !string.Equals(child.ParentPath, node.Source.Source.CanonicalPath, StringComparison.Ordinal))
                {
                    throw new ArgumentException("Topology child relationships must be reciprocal.", nameof(byPath));
                }
            }

            if (node.ParentState == RouteListTopologyParentState.Resolved
                && !byPath[node.ParentPath!].ChildPaths.Contains(
                    node.Source.Source.CanonicalPath,
                    StringComparer.Ordinal))
            {
                throw new ArgumentException("Topology parent relationships must be reciprocal.", nameof(byPath));
            }
        }
    }

    private static void ValidateAcyclic(
        IReadOnlyDictionary<string, RouteListTopologyNode> byPath)
    {
        foreach (var node in byPath.Values)
        {
            var current = node;
            var visited = new HashSet<string>(StringComparer.Ordinal);
            while (current.ParentState == RouteListTopologyParentState.Resolved)
            {
                if (!visited.Add(current.Source.Source.CanonicalPath))
                {
                    throw new ArgumentException("Resolved route topology cannot contain a parent cycle.", nameof(byPath));
                }

                current = byPath[current.ParentPath!];
            }
        }
    }
}
