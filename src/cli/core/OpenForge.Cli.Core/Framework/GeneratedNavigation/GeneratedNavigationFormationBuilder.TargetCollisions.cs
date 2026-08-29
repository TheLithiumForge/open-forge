using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Framework.GeneratedNavigation;

internal sealed partial class GeneratedNavigationFormationBuilder
{
    private static IReadOnlyList<GeneratedNavigationIntendedTargetCollision> BuildIntendedTargetCollisions(
        IReadOnlyList<SourceLogicalSource> intendedSources,
        IReadOnlyList<SourceCandidate> retainedObservedCandidates,
        IEnumerable<string> exactMemberCandidatePaths)
    {
        var exactMemberPaths = exactMemberCandidatePaths.ToHashSet(StringComparer.Ordinal);
        var containedCandidates = retainedObservedCandidates
            .Where(candidate => candidate.PhysicalState == PhysicalPathState.Contained
                && candidate.PhysicalPath is not null)
            .ToArray();
        var collisions = new List<GeneratedNavigationIntendedTargetCollision>();
        foreach (var targetGroup in intendedSources
                     .SelectMany(ReadLayers)
                     .GroupBy(layer => layer.PhysicalPath, PhysicalIdentityTracker.PathComparer))
        {
            var layers = targetGroup
                .OrderBy(layer => layer.CanonicalPath, StringComparer.Ordinal)
                .ToArray();
            if (layers.All(layer => exactMemberPaths.Contains(layer.CanonicalPath)))
            {
                continue;
            }

            var intendedLayerPaths = layers
                .Select(layer => layer.CanonicalPath)
                .ToHashSet(StringComparer.Ordinal);
            var observedOccupants = containedCandidates
                .Where(candidate => !intendedLayerPaths.Contains(candidate.CanonicalPath)
                    && PhysicalIdentityTracker.PathComparer.Equals(candidate.PhysicalPath, targetGroup.Key))
                .OrderBy(candidate => candidate.CanonicalPath, StringComparer.Ordinal)
                .ToArray();
            if (layers.Length < 2 && observedOccupants.Length == 0)
            {
                continue;
            }

            collisions.Add(new GeneratedNavigationIntendedTargetCollision(
                targetPath: targetGroup.Key,
                layers: layers,
                observedCandidates: observedOccupants));
        }

        return new ReadOnlyCollection<GeneratedNavigationIntendedTargetCollision>(
            collisions
                .OrderBy(collision => collision.Layers[0].CanonicalPath, StringComparer.Ordinal)
                .ToArray());
    }

    private static IEnumerable<SourceLayer> ReadLayers(SourceLogicalSource source)
    {
        yield return source.Base;
        if (source.Overwrite is { } overwrite)
        {
            yield return overwrite;
        }
    }
}
