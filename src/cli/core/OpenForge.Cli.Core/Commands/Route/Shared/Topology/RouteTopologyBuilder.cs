using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Topology;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Topology;

internal sealed class RouteTopologyBuilder
{
    internal RouteTopologyFacts Build(
        IEnumerable<RouteSource> sources,
        IEnumerable<string> loaderRootPaths)
    {
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(loaderRootPaths);

        var materializedSources = sources
            .Select(source => source ?? throw new ArgumentException("Topology sources cannot contain null.", nameof(sources)))
            .OrderBy(source => source.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        if (materializedSources
            .Select(source => source.CanonicalPath)
            .Distinct(StringComparer.Ordinal)
            .Count() != materializedSources.Length)
        {
            throw new ArgumentException("Topology sources require unique canonical paths.", nameof(sources));
        }

        var entrypointsByDirectory = materializedSources
            .Where(source => source.Kind == RouteSourceKind.Entrypoint)
            .GroupBy(
                source => RouteLogicalPath.ReadParent(source.CanonicalPath),
                StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(source => source.CanonicalPath, StringComparer.Ordinal).ToArray(),
                StringComparer.Ordinal);

        var relationships = materializedSources.ToDictionary(
            source => source.CanonicalPath,
            source => ReadParentRelationship(source, entrypointsByDirectory),
            StringComparer.Ordinal);
        var childrenByParent = materializedSources
            .Select(source => (Source: source, Parent: relationships[source.CanonicalPath]))
            .Where(item => item.Parent.State == RouteTopologyParentState.Resolved)
            .GroupBy(item => item.Parent.Paths[0], StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(item => item.Source.CanonicalPath)
                    .OrderBy(path => path, StringComparer.Ordinal)
                    .ToArray(),
                StringComparer.Ordinal);

        var nodes = materializedSources.Select(source =>
        {
            var parent = relationships[source.CanonicalPath];
            return new RouteTopologyNode(
                source,
                parent.State,
                parent.Paths,
                childrenByParent.GetValueOrDefault(source.CanonicalPath) ?? []);
        });

        return new RouteTopologyFacts(nodes, loaderRootPaths);
    }

    private static RouteTopologyParentRelationship ReadParentRelationship(
        RouteSource source,
        IReadOnlyDictionary<string, RouteSource[]> entrypointsByDirectory)
    {
        var containingDirectory = RouteLogicalPath.ReadParent(source.CanonicalPath);
        var representedParentDirectory = source.Kind == RouteSourceKind.Markdown
            ? containingDirectory
            : RouteLogicalPath.ReadParent(containingDirectory);
        if (!entrypointsByDirectory.TryGetValue(representedParentDirectory, out var candidates))
        {
            return RouteTopologyParentRelationship.None;
        }

        var candidatePaths = candidates
            .Select(candidate => candidate.CanonicalPath)
            .Where(candidatePath => !string.Equals(candidatePath, source.CanonicalPath, StringComparison.Ordinal))
            .ToArray();
        return candidatePaths.Length switch
        {
            0 => RouteTopologyParentRelationship.None,
            1 => new RouteTopologyParentRelationship(RouteTopologyParentState.Resolved, candidatePaths),
            _ => new RouteTopologyParentRelationship(RouteTopologyParentState.Ambiguous, candidatePaths),
        };
    }

}
