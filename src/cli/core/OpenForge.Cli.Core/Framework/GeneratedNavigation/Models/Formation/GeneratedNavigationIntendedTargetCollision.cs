using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;

internal sealed class GeneratedNavigationIntendedTargetCollision
{
    internal GeneratedNavigationIntendedTargetCollision(
        string targetPath,
        IEnumerable<SourceLayer> layers,
        IEnumerable<SourceCandidate> observedCandidates)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);
        var normalizedTargetPath = Path.GetFullPath(targetPath);
        if (!Path.IsPathRooted(targetPath)
            || !string.Equals(normalizedTargetPath, targetPath, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "An intended target collision requires an absolute normalized target path.",
                nameof(targetPath));
        }

        ArgumentNullException.ThrowIfNull(layers);
        var orderedLayers = layers
            .Select(layer => layer ?? throw new ArgumentException(
                "An intended target collision cannot contain a null layer.",
                nameof(layers)))
            .OrderBy(layer => layer.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        if (orderedLayers.Length == 0
            || orderedLayers.Select(layer => layer.CanonicalPath)
                .Distinct(StringComparer.Ordinal)
                .Count() != orderedLayers.Length
            || orderedLayers.Any(layer => !PhysicalIdentityTracker.PathComparer.Equals(
                layer.PhysicalPath,
                normalizedTargetPath)))
        {
            throw new ArgumentException(
                "An intended target collision requires unique intended source layers for one target path.",
                nameof(layers));
        }

        ArgumentNullException.ThrowIfNull(observedCandidates);
        var intendedLayerPaths = orderedLayers
            .Select(layer => layer.CanonicalPath)
            .ToHashSet(StringComparer.Ordinal);
        var orderedCandidates = observedCandidates
            .Select(candidate => candidate ?? throw new ArgumentException(
                "An intended target collision cannot contain a null observed candidate.",
                nameof(observedCandidates)))
            .OrderBy(candidate => candidate.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        if (orderedCandidates.Select(candidate => candidate.CanonicalPath)
                .Distinct(StringComparer.Ordinal)
                .Count() != orderedCandidates.Length
            || orderedCandidates.Any(candidate => intendedLayerPaths.Contains(candidate.CanonicalPath)
                || candidate.PhysicalState != PhysicalPathState.Contained
                || candidate.PhysicalPath is null
                || !PhysicalIdentityTracker.PathComparer.Equals(candidate.PhysicalPath, normalizedTargetPath)))
        {
            throw new ArgumentException(
                "An intended target collision requires unique additional observed occupants of the same target path.",
                nameof(observedCandidates));
        }

        if (orderedLayers.Length < 2 && orderedCandidates.Length == 0)
        {
            throw new ArgumentException(
                "An intended target collision requires at least two intended or observed target occupants.",
                nameof(observedCandidates));
        }

        TargetPath = normalizedTargetPath;
        Layers = new ReadOnlyCollection<SourceLayer>(orderedLayers);
        ObservedCandidates = new ReadOnlyCollection<SourceCandidate>(orderedCandidates);
    }

    internal string TargetPath { get; }

    internal IReadOnlyList<SourceLayer> Layers { get; }

    internal IReadOnlyList<SourceCandidate> ObservedCandidates { get; }
}
