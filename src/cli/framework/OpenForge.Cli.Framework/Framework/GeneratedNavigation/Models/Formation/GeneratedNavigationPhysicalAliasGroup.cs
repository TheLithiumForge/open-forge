using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;

internal enum GeneratedNavigationPhysicalAliasCompatibility
{
    Compatible,
    Incompatible,
}

internal sealed class GeneratedNavigationPhysicalAliasGroup
{
    internal GeneratedNavigationPhysicalAliasGroup(
        string physicalPath,
        IEnumerable<SourceCandidate> candidates,
        GeneratedNavigationPhysicalAliasCompatibility compatibility)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(physicalPath);
        var normalizedPhysicalPath = Path.GetFullPath(physicalPath);
        if (!Path.IsPathRooted(physicalPath)
            || !string.Equals(normalizedPhysicalPath, physicalPath, StringComparison.Ordinal))
        {
            throw new ArgumentException("A physical alias group requires an absolute normalized physical path.", nameof(physicalPath));
        }

        ArgumentNullException.ThrowIfNull(candidates);
        var orderedCandidates = candidates
            .Select(candidate => candidate ?? throw new ArgumentException(
                "A physical alias group cannot contain a null candidate.",
                nameof(candidates)))
            .OrderBy(candidate => candidate.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        if (orderedCandidates.Length < 2
            || orderedCandidates.Select(candidate => candidate.CanonicalPath)
                .Distinct(StringComparer.Ordinal)
                .Count() != orderedCandidates.Length
            || orderedCandidates.Any(candidate => candidate.PhysicalState != PhysicalPathState.Contained
                || candidate.PhysicalPath is null
                || !PhysicalIdentityTracker.PathComparer.Equals(candidate.PhysicalPath, normalizedPhysicalPath)))
        {
            throw new ArgumentException(
                "A physical alias group requires at least two unique contained candidates for one physical path.",
                nameof(candidates));
        }

        if (!Enum.IsDefined(compatibility))
        {
            throw new ArgumentOutOfRangeException(nameof(compatibility), compatibility, "The physical alias compatibility is not defined.");
        }

        PhysicalPath = normalizedPhysicalPath;
        Candidates = new ReadOnlyCollection<SourceCandidate>(orderedCandidates);
        Compatibility = compatibility;
    }

    internal string PhysicalPath { get; }

    internal IReadOnlyList<SourceCandidate> Candidates { get; }

    internal GeneratedNavigationPhysicalAliasCompatibility Compatibility { get; }
}
