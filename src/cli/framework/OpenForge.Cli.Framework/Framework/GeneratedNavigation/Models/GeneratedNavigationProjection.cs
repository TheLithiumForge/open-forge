using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;

internal sealed record GeneratedNavigationProjection
{
    internal GeneratedNavigationProjection(IEnumerable<GeneratedNavigationRegion> regions)
    {
        var ordered = regions
            .Select(region => region ?? throw new ArgumentException(
                "Generated navigation regions cannot contain null members.",
                nameof(regions)))
            .OrderBy(region => region.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        if (ordered.Select(region => region.CanonicalPath)
            .Distinct(StringComparer.Ordinal)
            .Count() != ordered.Length)
        {
            throw new ArgumentException(
                "A generated navigation projection requires unique canonical region paths.",
                nameof(regions));
        }

        if (ordered.Select(region => region.PhysicalPath)
            .Distinct(PhysicalIdentityTracker.PathComparer)
            .Count() != ordered.Length)
        {
            throw new ArgumentException(
                "A generated navigation projection requires unique physical region identities.",
                nameof(regions));
        }

        Regions = new ReadOnlyCollection<GeneratedNavigationRegion>(ordered);
    }

    internal IReadOnlyList<GeneratedNavigationRegion> Regions { get; }

    internal bool IsComplete => Regions.All(
        region => region.State == GeneratedNavigationRegionState.Available);
}
