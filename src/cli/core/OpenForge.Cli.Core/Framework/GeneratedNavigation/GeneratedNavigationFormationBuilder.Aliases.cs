using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Framework.GeneratedNavigation;

internal sealed partial class GeneratedNavigationFormationBuilder
{
    private static IReadOnlyList<GeneratedNavigationPhysicalAliasGroup> BuildAliasGroups(
        IReadOnlyList<SourceCandidate> observedCandidates,
        IReadOnlyDictionary<string, SourceLogicalSource> observedSourcesByCandidatePath,
        IReadOnlyDictionary<string, SourceLogicalSource> intendedSourcesByPath,
        SourceRouteTopology topology)
    {
        var groups = observedCandidates
            .Where(candidate => candidate.PhysicalState == PhysicalPathState.Contained
                && candidate.PhysicalPath is not null)
            .GroupBy(
                candidate => candidate.PhysicalPath
                    ?? throw new InvalidOperationException("A contained alias candidate must retain its physical path."),
                PhysicalIdentityTracker.PathComparer)
            .Where(group => group.Count() > 1)
            .Select(group => BuildAliasGroup(
                group,
                observedSourcesByCandidatePath,
                intendedSourcesByPath,
                topology))
            .OrderBy(group => group.Candidates[0].CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        return new ReadOnlyCollection<GeneratedNavigationPhysicalAliasGroup>(groups);
    }

    private static GeneratedNavigationPhysicalAliasGroup BuildAliasGroup(
        IEnumerable<SourceCandidate> group,
        IReadOnlyDictionary<string, SourceLogicalSource> observedSourcesByCandidatePath,
        IReadOnlyDictionary<string, SourceLogicalSource> intendedSourcesByPath,
        SourceRouteTopology topology)
    {
        var candidates = group.OrderBy(candidate => candidate.CanonicalPath, StringComparer.Ordinal).ToArray();
        var compatibility = candidates.Skip(1).All(candidate =>
            AreCompatible(
                candidates[0],
                candidate,
                observedSourcesByCandidatePath,
                intendedSourcesByPath,
                topology))
            ? GeneratedNavigationPhysicalAliasCompatibility.Compatible
            : GeneratedNavigationPhysicalAliasCompatibility.Incompatible;
        return new GeneratedNavigationPhysicalAliasGroup(
            physicalPath: candidates[0].PhysicalPath
                ?? throw new InvalidOperationException("A contained alias candidate must retain its physical path."),
            candidates: candidates,
            compatibility: compatibility);
    }

    private static bool AreCompatible(
        SourceCandidate left,
        SourceCandidate right,
        IReadOnlyDictionary<string, SourceLogicalSource> observedSourcesByCandidatePath,
        IReadOnlyDictionary<string, SourceLogicalSource> intendedSourcesByPath,
        SourceRouteTopology topology)
    {
        if (left.Form is null
            || left.Form != right.Form
            || !PhysicalIdentityTracker.PathComparer.Equals(left.PhysicalParentPath, right.PhysicalParentPath))
        {
            return false;
        }

        var leftSource = observedSourcesByCandidatePath.GetValueOrDefault(left.CanonicalPath);
        var rightSource = observedSourcesByCandidatePath.GetValueOrDefault(right.CanonicalPath);
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
            && PhysicalIdentitiesEqual(leftNode.ParentPaths, rightNode.ParentPaths, intendedSourcesByPath)
            && PhysicalIdentitiesEqual(leftNode.ChildPaths, rightNode.ChildPaths, intendedSourcesByPath)
            && string.Equals(
                ReadRepresentedRootFolder(leftSource),
                ReadRepresentedRootFolder(rightSource),
                StringComparison.Ordinal);
    }

    private static bool IsBaseCandidate(SourceCandidate candidate, SourceLogicalSource source)
    {
        return string.Equals(candidate.CanonicalPath, source.Base.CanonicalPath, StringComparison.Ordinal);
    }

    private static bool PhysicalIdentitiesEqual(
        IReadOnlyList<string> leftPaths,
        IReadOnlyList<string> rightPaths,
        IReadOnlyDictionary<string, SourceLogicalSource> intendedSourcesByPath)
    {
        var left = ReadPhysicalIdentities(leftPaths, intendedSourcesByPath);
        var right = ReadPhysicalIdentities(rightPaths, intendedSourcesByPath);
        return left.Count == right.Count
            && left.Zip(right).All(pair => PhysicalIdentityTracker.PathComparer.Equals(pair.First, pair.Second));
    }

    private static IReadOnlyList<string> ReadPhysicalIdentities(
        IReadOnlyList<string> paths,
        IReadOnlyDictionary<string, SourceLogicalSource> intendedSourcesByPath)
    {
        return paths
            .Select(path => intendedSourcesByPath.GetValueOrDefault(path)?.Base.PhysicalPath
                ?? throw new InvalidOperationException("A topology relationship must retain an exact intended source."))
            .OrderBy(path => path, PhysicalIdentityTracker.PathComparer)
            .ThenBy(path => path, StringComparer.Ordinal)
            .ToArray();
    }
}
