using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Routing;

namespace OpenForge.Cli.Core.Framework.GeneratedNavigation;

internal sealed class GeneratedNavigationFormationBuilder
{
    internal GeneratedNavigationFormation Build(SourceCatalogue catalogue)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        return new GeneratedNavigationFormation(new DerivedComponents(catalogue));
    }

    private static IReadOnlyList<GeneratedNavigationPhysicalAliasGroup> BuildAliasGroups(
        SourceCatalogue catalogue,
        SourceRouteTopology topology)
    {
        return catalogue.Candidates
            .Where(candidate => candidate.PhysicalState == PhysicalPathState.Contained
                && candidate.PhysicalPath is not null)
            .GroupBy(
                candidate => candidate.PhysicalPath
                    ?? throw new InvalidOperationException("A contained alias candidate must retain its physical path."),
                PhysicalIdentityTracker.PathComparer)
            .Where(group => group.Count() > 1)
            .Select(group =>
            {
                var candidates = group.OrderBy(candidate => candidate.CanonicalPath, StringComparer.Ordinal).ToArray();
                var compatibility = candidates.Skip(1).All(candidate =>
                    AreCompatible(candidates[0], candidate, catalogue, topology))
                    ? GeneratedNavigationPhysicalAliasCompatibility.Compatible
                    : GeneratedNavigationPhysicalAliasCompatibility.Incompatible;
                return new GeneratedNavigationPhysicalAliasGroup(
                    physicalPath: candidates[0].PhysicalPath
                        ?? throw new InvalidOperationException("A contained alias candidate must retain its physical path."),
                    candidates: candidates,
                    compatibility: compatibility);
            })
            .OrderBy(group => group.Candidates[0].CanonicalPath, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool AreCompatible(
        SourceCandidate left,
        SourceCandidate right,
        SourceCatalogue catalogue,
        SourceRouteTopology topology)
    {
        if (left.Form is null
            || left.Form != right.Form
            || !PhysicalIdentityTracker.PathComparer.Equals(left.PhysicalParentPath, right.PhysicalParentPath))
        {
            return false;
        }

        var leftSource = catalogue.FindByPath(left.CanonicalPath);
        var rightSource = catalogue.FindByPath(right.CanonicalPath);
        if (leftSource is null
            || rightSource is null
            || leftSource.Base.Form != rightSource.Base.Form
            || IsBaseCandidate(left, leftSource) != IsBaseCandidate(right, rightSource))
        {
            return false;
        }

        var leftNode = topology.FindByPath(leftSource.Identity.CanonicalBasePath);
        var rightNode = topology.FindByPath(rightSource.Identity.CanonicalBasePath);
        if ((leftNode is null) != (rightNode is null))
        {
            return false;
        }

        if (leftNode is null || rightNode is null)
        {
            return true;
        }

        return leftNode.ParentState == rightNode.ParentState
            && PhysicalIdentitiesEqual(leftNode.ParentPaths, rightNode.ParentPaths, catalogue)
            && PhysicalIdentitiesEqual(leftNode.ChildPaths, rightNode.ChildPaths, catalogue)
            && string.Equals(ReadRepresentedRootFolder(leftSource), ReadRepresentedRootFolder(rightSource), StringComparison.Ordinal);
    }

    private static bool IsBaseCandidate(SourceCandidate candidate, SourceLogicalSource source)
    {
        return string.Equals(candidate.CanonicalPath, source.Base.CanonicalPath, StringComparison.Ordinal);
    }

    private static bool PhysicalIdentitiesEqual(
        IReadOnlyList<string> leftPaths,
        IReadOnlyList<string> rightPaths,
        SourceCatalogue catalogue)
    {
        var left = ReadPhysicalIdentities(leftPaths, catalogue);
        var right = ReadPhysicalIdentities(rightPaths, catalogue);
        return left.Count == right.Count
            && left.Zip(right).All(pair => PhysicalIdentityTracker.PathComparer.Equals(pair.First, pair.Second));
    }

    private static IReadOnlyList<string> ReadPhysicalIdentities(
        IReadOnlyList<string> paths,
        SourceCatalogue catalogue)
    {
        return paths
            .Select(path => catalogue.FindByPath(path)?.Base.PhysicalPath
                ?? throw new InvalidOperationException("A topology relationship must retain an exact catalogue source."))
            .OrderBy(path => path, PhysicalIdentityTracker.PathComparer)
            .ThenBy(path => path, StringComparer.Ordinal)
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

    private static IEnumerable<GeneratedNavigationFormationAmbiguity> BuildAmbiguities(
        SourceCatalogue catalogue,
        SourceRouteTopology topology,
        IReadOnlyList<GeneratedNavigationPhysicalAliasGroup> aliasGroups)
    {
        foreach (var node in topology.Nodes.Where(node => node.ParentState == SourceRouteParentState.Ambiguous))
        {
            yield return new GeneratedNavigationFormationAmbiguity(
                kind: GeneratedNavigationFormationAmbiguityKind.RouteParent,
                subject: node.Identity.CanonicalBasePath,
                candidates: node.ParentPaths.Select(path => RequireBaseCandidate(catalogue, path)));
        }

        foreach (var group in aliasGroups.Where(group =>
                     group.Compatibility == GeneratedNavigationPhysicalAliasCompatibility.Incompatible))
        {
            yield return new GeneratedNavigationFormationAmbiguity(
                kind: GeneratedNavigationFormationAmbiguityKind.PhysicalAlias,
                subject: group.Candidates[0].CanonicalPath,
                candidates: group.Candidates);
        }
    }

    private static IReadOnlyList<string> ReadStructuralRootPaths(
        SourceCatalogue catalogue,
        IReadOnlyList<GeneratedNavigationPhysicalAliasGroup> aliasGroups,
        ICollection<GeneratedNavigationFormationAmbiguity> ambiguities)
    {
        var eligible = new List<SourceLogicalSource>();
        var rootCandidates = catalogue.Sources
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
            var candidates = rootGroup
                .Select(candidate => RequireBaseCandidate(catalogue, candidate.Source.Identity.CanonicalBasePath))
                .OrderBy(candidate => candidate.CanonicalPath, StringComparer.Ordinal)
                .ToArray();
            if (candidates.Length > 1)
            {
                ambiguities.Add(new GeneratedNavigationFormationAmbiguity(
                    kind: GeneratedNavigationFormationAmbiguityKind.RootEntrypoint,
                    subject: rootGroup.Key,
                    candidates: candidates));
                continue;
            }

            eligible.Add(rootGroup.Single().Source);
        }

        var roots = new List<string>();
        foreach (var physicalGroup in eligible
                     .GroupBy(source => source.Base.PhysicalPath, PhysicalIdentityTracker.PathComparer)
                     .OrderBy(group => group.Min(source => source.Identity.CanonicalBasePath), StringComparer.Ordinal))
        {
            var aliasGroup = aliasGroups.FirstOrDefault(group =>
                PhysicalIdentityTracker.PathComparer.Equals(group.PhysicalPath, physicalGroup.Key));
            if (aliasGroup?.Compatibility == GeneratedNavigationPhysicalAliasCompatibility.Incompatible)
            {
                continue;
            }

            roots.Add(physicalGroup
                .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
                .First()
                .Identity.CanonicalBasePath);
        }

        return roots.OrderBy(path => path, StringComparer.Ordinal).ToArray();
    }

    private static SourceCandidate RequireBaseCandidate(SourceCatalogue catalogue, string canonicalPath)
    {
        var candidate = catalogue.FindCandidateByPath(canonicalPath);
        if (candidate is null)
        {
            throw new InvalidOperationException("A retained logical source must retain its exact base candidate.");
        }

        return candidate;
    }

    internal sealed class DerivedComponents
    {
        internal DerivedComponents(SourceCatalogue catalogue)
        {
            if (catalogue.IsCancelled)
            {
                throw new ArgumentException("Generated navigation formation rejects a cancelled source catalogue.", nameof(catalogue));
            }

            var topologyBuilder = new SourceRouteTopologyBuilder();
            var unrootedTopology = topologyBuilder.Build(catalogue.Sources, []);
            var aliasGroups = BuildAliasGroups(catalogue, unrootedTopology);
            var ambiguities = BuildAmbiguities(catalogue, unrootedTopology, aliasGroups).ToList();
            var loader = catalogue.FindByPath(SourceLogicalPath.LoaderPath);
            if (loader is not null && loader.Base.Form != SourceDocumentForm.Loader)
            {
                loader = null;
            }

            var structuralRootPaths = ReadStructuralRootPaths(catalogue, aliasGroups, ambiguities);
            var admittedRootPaths = loader is null
                ? []
                : structuralRootPaths;
            Catalogue = catalogue;
            Topology = topologyBuilder.Build(catalogue.Sources, admittedRootPaths);
            Loader = loader;
            PhysicalAliasGroups = new ReadOnlyCollection<GeneratedNavigationPhysicalAliasGroup>(
                aliasGroups
                    .OrderBy(group => group.Candidates[0].CanonicalPath, StringComparer.Ordinal)
                    .ToArray());
            Ambiguities = new ReadOnlyCollection<GeneratedNavigationFormationAmbiguity>(
                ambiguities
                    .OrderBy(ambiguity => ambiguity.Kind)
                    .ThenBy(ambiguity => ambiguity.Subject, StringComparer.Ordinal)
                    .ToArray());
        }

        internal SourceCatalogue Catalogue { get; }

        internal SourceRouteTopology Topology { get; }

        internal SourceLogicalSource? Loader { get; }

        internal IReadOnlyList<GeneratedNavigationPhysicalAliasGroup> PhysicalAliasGroups { get; }

        internal IReadOnlyList<GeneratedNavigationFormationAmbiguity> Ambiguities { get; }
    }
}
