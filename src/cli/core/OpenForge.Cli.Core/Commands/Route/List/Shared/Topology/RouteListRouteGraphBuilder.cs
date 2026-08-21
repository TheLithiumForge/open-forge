using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

internal sealed class RouteListRouteGraphBuilder
{
    internal RouteListTopologyFacts Build(RouteListTopologyInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var sources = input.Inventory.Sources
            .Where(source => source.Source.Kind is (
                RouteListSourceKind.Entrypoint
                or RouteListSourceKind.RoutedLeaf
                or RouteListSourceKind.RoutedNative))
            .OrderBy(source => source.Source.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        var entrypointsByDirectory = sources
            .Where(source => source.Source.Kind == RouteListSourceKind.Entrypoint)
            .GroupBy(
                source => RouteListLogicalPath.ReadParent(source.Source.CanonicalPath),
                StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(
                    source => source.Source.CanonicalPath,
                    StringComparer.Ordinal).ToArray(),
                StringComparer.Ordinal);

        var relationships = sources.ToDictionary(
            source => source.Source.CanonicalPath,
            source => ReadParentRelationship(source, entrypointsByDirectory),
            StringComparer.Ordinal);
        var childrenByParent = sources
            .Select(source => (Source: source, Parent: relationships[source.Source.CanonicalPath]))
            .Where(item => item.Parent.State == RouteListTopologyParentState.Resolved)
            .GroupBy(item => item.Parent.Paths[0], StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(item => item.Source.Source.CanonicalPath)
                    .OrderBy(path => path, StringComparer.Ordinal)
                    .ToArray(),
                StringComparer.Ordinal);
        var nodes = sources.Select(source =>
        {
            var path = source.Source.CanonicalPath;
            var parent = relationships[path];
            return new RouteListTopologyNode(
                source,
                parent.State,
                parent.Paths,
                childrenByParent.GetValueOrDefault(path) ?? []);
        });

        var identityCollisions = input.Inventory.Sources
            .GroupBy(source => source.Source.Id, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => new RouteListTopologyIdentityCollision(
                group.Key,
                group.Select(source => source.Source.CanonicalPath)));
        return new RouteListTopologyFacts(
            nodes,
            input.LoaderRootPaths,
            identityCollisions);
    }

    private static RouteListParentRelationship ReadParentRelationship(
        RouteListInventorySource source,
        IReadOnlyDictionary<string, RouteListInventorySource[]> entrypointsByDirectory)
    {
        var path = source.Source.CanonicalPath;
        var containingDirectory = RouteListLogicalPath.ReadParent(path);
        var representedParentDirectory = source.Source.Kind == RouteListSourceKind.RoutedLeaf
            ? containingDirectory
            : RouteListLogicalPath.ReadParent(containingDirectory);
        if (!entrypointsByDirectory.TryGetValue(representedParentDirectory, out var candidates))
        {
            return RouteListParentRelationship.None;
        }

        var candidatePaths = candidates
            .Select(candidate => candidate.Source.CanonicalPath)
            .Where(candidatePath => !string.Equals(candidatePath, path, StringComparison.Ordinal))
            .ToArray();
        return candidatePaths.Length switch
        {
            0 => RouteListParentRelationship.None,
            1 => new RouteListParentRelationship(
                RouteListTopologyParentState.Resolved,
                candidatePaths),
            _ => new RouteListParentRelationship(
                RouteListTopologyParentState.Ambiguous,
                candidatePaths),
        };
    }

    private sealed record RouteListParentRelationship(
        RouteListTopologyParentState State,
        IReadOnlyList<string> Paths)
    {
        internal static RouteListParentRelationship None { get; } = new(
            RouteListTopologyParentState.None,
            []);
    }
}
