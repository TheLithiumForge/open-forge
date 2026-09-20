using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Framework.GeneratedNavigation;

internal sealed partial class GeneratedNavigationFormationBuilder
{
    internal GeneratedNavigationFormation Build(SourceCatalogue observedCatalogue)
    {
        ArgumentNullException.ThrowIfNull(observedCatalogue);
        return Build(observedCatalogue, observedCatalogue.Sources);
    }

    internal GeneratedNavigationFormation Build(
        SourceCatalogue observedCatalogue,
        IReadOnlyList<SourceLogicalSource> intendedSources)
    {
        ArgumentNullException.ThrowIfNull(observedCatalogue);
        ArgumentNullException.ThrowIfNull(intendedSources);
        return new GeneratedNavigationFormation(new DerivedComponents(observedCatalogue, intendedSources));
    }

    private static IReadOnlyList<SourceLogicalSource> MaterializeIntendedSources(
        SourceCatalogue observedCatalogue,
        IReadOnlyList<SourceLogicalSource> intendedSources)
    {
        var ordered = intendedSources
            .Select(source => source ?? throw new ArgumentException(
                "Intended generated navigation sources cannot contain null members.",
                nameof(intendedSources)))
            .OrderBy(source => source.Identity.AutomaticId, StringComparer.Ordinal)
            .ThenBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
        if (ordered.Select(source => source.Identity.CanonicalBasePath)
            .Distinct(StringComparer.Ordinal)
            .Count() != ordered.Length)
        {
            throw new ArgumentException(
                "Intended generated navigation sources require unique canonical base paths.",
                nameof(intendedSources));
        }

        return ReferenceEquals(intendedSources, observedCatalogue.Sources)
            ? observedCatalogue.Sources
            : new ReadOnlyCollection<SourceLogicalSource>(ordered);
    }

    private static IReadOnlyList<GeneratedNavigationFormationAmbiguity> BuildAmbiguities(
        SourceRouteTopology topology,
        IReadOnlyDictionary<string, SourceLogicalSource> intendedByPath,
        IReadOnlyDictionary<string, SourceCandidate> observedCandidatesByPath,
        IReadOnlyDictionary<string, SourceLogicalSource> observedSourcesByCandidatePath,
        IReadOnlyList<GeneratedNavigationPhysicalAliasGroup> aliasGroups)
    {
        var ambiguities = new List<GeneratedNavigationFormationAmbiguity>();
        foreach (var node in topology.Nodes.Where(node => node.ParentState == SourceRouteParentState.Ambiguous))
        {
            var sources = node.ParentPaths.Select(path => intendedByPath[path]).ToArray();
            ambiguities.Add(new GeneratedNavigationFormationAmbiguity(
                kind: GeneratedNavigationFormationAmbiguityKind.RouteParent,
                subject: node.Identity.CanonicalBasePath,
                intendedSources: sources,
                observedCandidates: ReadObservedBaseCandidates(sources, observedCandidatesByPath)));
        }

        foreach (var group in aliasGroups.Where(group =>
                     group.Compatibility == GeneratedNavigationPhysicalAliasCompatibility.Incompatible))
        {
            var sources = group.Candidates
                .Select(candidate => observedSourcesByCandidatePath.GetValueOrDefault(candidate.CanonicalPath))
                .Where(source => source is not null)
                .Cast<SourceLogicalSource>()
                .DistinctBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
                .ToArray();
            ambiguities.Add(new GeneratedNavigationFormationAmbiguity(
                kind: GeneratedNavigationFormationAmbiguityKind.PhysicalAlias,
                subject: group.Candidates[0].CanonicalPath,
                intendedSources: sources,
                observedCandidates: group.Candidates));
        }

        return ambiguities;
    }

    private static IReadOnlyList<string> ReadStructuralRootPaths(
        IReadOnlyList<SourceLogicalSource> intendedSources,
        IReadOnlyDictionary<string, SourceCandidate> observedCandidatesByPath,
        IReadOnlyList<GeneratedNavigationPhysicalAliasGroup> aliasGroups,
        IReadOnlyList<GeneratedNavigationIntendedTargetCollision> intendedTargetCollisions,
        ICollection<GeneratedNavigationFormationAmbiguity> ambiguities)
    {
        var eligible = new List<SourceLogicalSource>();
        var rootCandidates = intendedSources
            .Select(source => (Source: source, Folder: ReadRepresentedRootFolder(source)))
            .Where(candidate => candidate.Folder is not null)
            .Select(candidate => (
                candidate.Source,
                Folder: candidate.Folder
                    ?? throw new InvalidOperationException("A structurally rooted entrypoint must retain its represented folder.")));
        foreach (var rootGroup in rootCandidates
                     .GroupBy(candidate => candidate.Folder, StringComparer.Ordinal)
                     .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            var sources = rootGroup
                .Select(candidate => candidate.Source)
                .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
                .ToArray();
            if (sources.Length > 1)
            {
                ambiguities.Add(new GeneratedNavigationFormationAmbiguity(
                    kind: GeneratedNavigationFormationAmbiguityKind.RootEntrypoint,
                    subject: rootGroup.Key,
                    intendedSources: sources,
                    observedCandidates: ReadObservedBaseCandidates(sources, observedCandidatesByPath)));
                continue;
            }

            eligible.Add(sources[0]);
        }

        var collidingTargetPaths = intendedTargetCollisions
            .Select(collision => collision.TargetPath)
            .ToHashSet(PhysicalIdentityTracker.PathComparer);
        return eligible
            .Where(source => !collidingTargetPaths.Contains(source.Base.PhysicalPath))
            .GroupBy(source => source.Base.PhysicalPath, PhysicalIdentityTracker.PathComparer)
            .Where(group => aliasGroups.All(alias =>
                !PhysicalIdentityTracker.PathComparer.Equals(alias.PhysicalPath, group.Key)
                || alias.Compatibility == GeneratedNavigationPhysicalAliasCompatibility.Compatible))
            .Select(group => group
                .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
                .First()
                .Identity.CanonicalBasePath)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<SourceCandidate> ReadObservedBaseCandidates(
        IEnumerable<SourceLogicalSource> sources,
        IReadOnlyDictionary<string, SourceCandidate> observedCandidatesByPath)
    {
        return sources
            .Select(source => observedCandidatesByPath.GetValueOrDefault(source.Identity.CanonicalBasePath))
            .Where(candidate => candidate is not null)
            .Cast<SourceCandidate>()
            .OrderBy(candidate => candidate.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
    }

    private static string? ReadRepresentedRootFolder(SourceLogicalSource source)
    {
        if (!SourceFormClassifier.IsEntrypoint(source.Base.Form))
        {
            return null;
        }

        var representedFolder = SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath);
        return SourceLogicalPath.ReadParent(representedFolder) == SourceLogicalPath.AgentsRoot
            ? representedFolder
            : null;
    }

}
