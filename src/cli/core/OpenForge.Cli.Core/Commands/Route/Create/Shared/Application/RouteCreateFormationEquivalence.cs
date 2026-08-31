using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Application;

internal static class RouteCreateFormationEquivalence
{
    internal static bool Matches(
        GeneratedNavigationFormation expected,
        GeneratedNavigationFormation actual)
        => MatchesSources(expected.Sources, actual.Sources)
            && MatchesIssues(expected.Issues, actual.Issues)
            && MatchesTopology(expected.Topology, actual.Topology)
            && MatchesOptionalSource(expected.Loader, actual.Loader)
            && MatchesAliasGroups(
                expected.PhysicalAliasGroups,
                actual.PhysicalAliasGroups)
            && MatchesCollisions(
                expected.IntendedTargetCollisions,
                actual.IntendedTargetCollisions)
            && MatchesAmbiguities(expected.Ambiguities, actual.Ambiguities);

    private static bool MatchesSources(
        IReadOnlyList<SourceLogicalSource> expected,
        IReadOnlyList<SourceLogicalSource> actual)
    {
        if (expected.Count != actual.Count)
        {
            return false;
        }

        for (var index = 0; index < expected.Count; index++)
        {
            if (!RouteCreatePlanEquivalence.MatchesSource(
                    expected[index],
                    actual[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesOptionalSource(
        SourceLogicalSource? expected,
        SourceLogicalSource? actual)
        => expected is null || actual is null
            ? expected is null && actual is null
            : RouteCreatePlanEquivalence.MatchesSource(expected, actual);

    private static bool MatchesIssues(
        IReadOnlyList<SourceCatalogueIssue> expected,
        IReadOnlyList<SourceCatalogueIssue> actual)
    {
        if (expected.Count != actual.Count)
        {
            return false;
        }

        for (var index = 0; index < expected.Count; index++)
        {
            var before = expected[index];
            var after = actual[index];
            if (before.Stage != after.Stage
                || before.Code != after.Code
                || !string.Equals(
                    before.AttemptedCanonicalPath,
                    after.AttemptedCanonicalPath,
                    StringComparison.Ordinal)
                || !before.RelatedPaths.SequenceEqual(after.RelatedPaths)
                || !PhysicalIdentityTracker.PathComparer.Equals(
                    before.ScopePhysicalPath,
                    after.ScopePhysicalPath))
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesTopology(
        SourceRouteTopology expected,
        SourceRouteTopology actual)
    {
        if (!expected.LoaderRootPaths.SequenceEqual(actual.LoaderRootPaths)
            || expected.Nodes.Count != actual.Nodes.Count)
        {
            return false;
        }

        for (var index = 0; index < expected.Nodes.Count; index++)
        {
            var before = expected.Nodes[index];
            var after = actual.Nodes[index];
            if (!MatchesIdentity(before, after)
                || before.ParentState != after.ParentState
                || !before.ParentPaths.SequenceEqual(after.ParentPaths)
                || !before.ChildPaths.SequenceEqual(after.ChildPaths))
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesIdentity(
        SourceRouteNode expected,
        SourceRouteNode actual)
        => string.Equals(
                expected.Identity.AutomaticId,
                actual.Identity.AutomaticId,
                StringComparison.Ordinal)
            && string.Equals(
                expected.Identity.CanonicalBasePath,
                actual.Identity.CanonicalBasePath,
                StringComparison.Ordinal);

    private static bool MatchesAliasGroups(
        IReadOnlyList<GeneratedNavigationPhysicalAliasGroup> expected,
        IReadOnlyList<GeneratedNavigationPhysicalAliasGroup> actual)
    {
        if (expected.Count != actual.Count)
        {
            return false;
        }

        for (var index = 0; index < expected.Count; index++)
        {
            var before = expected[index];
            var after = actual[index];
            if (before.Compatibility != after.Compatibility
                || !PhysicalIdentityTracker.PathComparer.Equals(
                    before.PhysicalPath,
                    after.PhysicalPath)
                || !MatchesCandidates(before.Candidates, after.Candidates))
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesCollisions(
        IReadOnlyList<GeneratedNavigationIntendedTargetCollision> expected,
        IReadOnlyList<GeneratedNavigationIntendedTargetCollision> actual)
    {
        if (expected.Count != actual.Count)
        {
            return false;
        }

        for (var index = 0; index < expected.Count; index++)
        {
            var before = expected[index];
            var after = actual[index];
            if (!PhysicalIdentityTracker.PathComparer.Equals(
                    before.TargetPath,
                    after.TargetPath)
                || !MatchesLayers(before.Layers, after.Layers)
                || !MatchesCandidates(
                    before.ObservedCandidates,
                    after.ObservedCandidates))
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesAmbiguities(
        IReadOnlyList<GeneratedNavigationFormationAmbiguity> expected,
        IReadOnlyList<GeneratedNavigationFormationAmbiguity> actual)
    {
        if (expected.Count != actual.Count)
        {
            return false;
        }

        for (var index = 0; index < expected.Count; index++)
        {
            var before = expected[index];
            var after = actual[index];
            if (before.Kind != after.Kind
                || !string.Equals(before.Subject, after.Subject, StringComparison.Ordinal)
                || !MatchesSources(before.IntendedSources, after.IntendedSources)
                || !MatchesCandidates(before.Candidates, after.Candidates))
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesLayers(
        IReadOnlyList<SourceLayer> expected,
        IReadOnlyList<SourceLayer> actual)
    {
        if (expected.Count != actual.Count)
        {
            return false;
        }

        for (var index = 0; index < expected.Count; index++)
        {
            var before = expected[index];
            var after = actual[index];
            if (!string.Equals(before.CanonicalPath, after.CanonicalPath, StringComparison.Ordinal)
                || !PhysicalIdentityTracker.PathComparer.Equals(
                    before.PhysicalPath,
                    after.PhysicalPath)
                || before.Form != after.Form
                || before.Kind != after.Kind)
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesCandidates(
        IReadOnlyList<SourceCandidate> expected,
        IReadOnlyList<SourceCandidate> actual)
    {
        if (expected.Count != actual.Count)
        {
            return false;
        }

        for (var index = 0; index < expected.Count; index++)
        {
            var before = expected[index];
            var after = actual[index];
            if (!string.Equals(before.CanonicalPath, after.CanonicalPath, StringComparison.Ordinal)
                || before.Form != after.Form
                || !string.Equals(before.AutomaticId, after.AutomaticId, StringComparison.Ordinal)
                || before.PhysicalState != after.PhysicalState
                || !PhysicalIdentityTracker.PathComparer.Equals(
                    before.PhysicalPath,
                    after.PhysicalPath)
                || !PhysicalIdentityTracker.PathComparer.Equals(
                    before.PhysicalParentPath,
                    after.PhysicalParentPath))
            {
                return false;
            }
        }

        return true;
    }
}
