using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Framework.Sources.Routing;

internal sealed class SourceRouteTopologyBuilder
{
    internal SourceRouteTopology Build(
        IEnumerable<SourceLogicalSource> sources,
        IReadOnlyList<string> loaderRootPaths)
    {
        var candidateSources = sources
            .Select(source => source ?? throw new ArgumentException("Topology sources cannot contain null.", nameof(sources)))
            .Where(IsTopologySource)
            .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
        if (candidateSources
            .Select(source => source.Identity.CanonicalBasePath)
            .Distinct(StringComparer.Ordinal)
            .Count() != candidateSources.Length)
        {
            throw new ArgumentException("Topology sources require unique canonical paths.", nameof(sources));
        }

        var entrypointsByDirectory = candidateSources
            .Where(source => SourceFormClassifier.IsEntrypoint(source.Base.Form))
            .GroupBy(
                source => SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath),
                StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
                    .ToArray(),
                StringComparer.Ordinal);
        var materializedSources = candidateSources
            .Where(source => IsAdmitted(source, entrypointsByDirectory))
            .ToArray();
        var relationships = materializedSources.ToDictionary(
            source => source.Identity.CanonicalBasePath,
            source => ReadParentRelationship(source, entrypointsByDirectory),
            StringComparer.Ordinal);
        var childrenByParent = materializedSources
            .Select(source => (Source: source, Parent: relationships[source.Identity.CanonicalBasePath]))
            .Where(item => item.Parent.State == SourceRouteParentState.Resolved)
            .GroupBy(item => item.Parent.Paths[0], StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(item => item.Source.Identity.CanonicalBasePath)
                    .OrderBy(path => path, StringComparer.Ordinal)
                    .ToArray(),
                StringComparer.Ordinal);

        var nodes = materializedSources.Select(source =>
        {
            var path = source.Identity.CanonicalBasePath;
            var parent = relationships[path];
            return new SourceRouteNode(
                source.Identity,
                parent.State,
                parent.Paths,
                childrenByParent.GetValueOrDefault(path) ?? []);
        });

        return new SourceRouteTopology(nodes, loaderRootPaths);
    }

    private static bool IsTopologySource(SourceLogicalSource source)
    {
        return source.Base.Form is not (SourceDocumentForm.Loader or SourceDocumentForm.OverwriteCompanion);
    }

    private static bool IsAdmitted(
        SourceLogicalSource source,
        IReadOnlyDictionary<string, SourceLogicalSource[]> entrypointsByDirectory)
    {
        if (SourceFormClassifier.IsEntrypoint(source.Base.Form))
        {
            return true;
        }

        var containingDirectory = SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath);
        var representedParentDirectory = source.Base.Form == SourceDocumentForm.Markdown
            ? containingDirectory
            : SourceLogicalPath.ReadParent(containingDirectory);
        return entrypointsByDirectory.ContainsKey(representedParentDirectory);
    }

    private static SourceRouteParentRelationship ReadParentRelationship(
        SourceLogicalSource source,
        IReadOnlyDictionary<string, SourceLogicalSource[]> entrypointsByDirectory)
    {
        var containingDirectory = SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath);
        var representedParentDirectory = source.Base.Form == SourceDocumentForm.Markdown
            ? containingDirectory
            : SourceLogicalPath.ReadParent(containingDirectory);
        if (!entrypointsByDirectory.TryGetValue(representedParentDirectory, out var candidates))
        {
            return SourceRouteParentRelationship.None;
        }

        var candidatePaths = candidates
            .Select(candidate => candidate.Identity.CanonicalBasePath)
            .Where(candidatePath => !string.Equals(
                candidatePath,
                source.Identity.CanonicalBasePath,
                StringComparison.Ordinal))
            .ToArray();
        return candidatePaths.Length switch
        {
            0 => SourceRouteParentRelationship.None,
            1 => new SourceRouteParentRelationship(SourceRouteParentState.Resolved, candidatePaths),
            _ => new SourceRouteParentRelationship(SourceRouteParentState.Ambiguous, candidatePaths),
        };
    }

    private sealed class SourceRouteParentRelationship
    {
        internal SourceRouteParentRelationship(
            SourceRouteParentState state,
            IEnumerable<string> paths)
        {
            State = state;
            Paths = paths.ToArray();
        }

        internal SourceRouteParentState State { get; }

        internal IReadOnlyList<string> Paths { get; }

        internal static SourceRouteParentRelationship None { get; } =
            new(SourceRouteParentState.None, []);
    }
}
