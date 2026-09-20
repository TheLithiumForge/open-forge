using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;

namespace OpenForge.Cli.Core.Framework.GeneratedNavigation;

internal sealed class GeneratedNavigationProjector
{
    private readonly GeneratedNavigationRegionPlanner _regionPlanner = new();

    internal GeneratedNavigationProjection Project(GeneratedNavigationProjectionRequest request)
    {
        var regions = request.Regions
            .OrderBy(region => region.Source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ThenBy(region => region.Document?.Source ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(region => region.UnavailableCause ?? string.Empty, StringComparer.Ordinal)
            .GroupBy(
                region => region.Source.Base.PhysicalPath,
                PhysicalIdentityTracker.PathComparer)
            .Select(group => group.First())
            .Select(region => _regionPlanner.Plan(request, region))
            .ToArray();

        return new GeneratedNavigationProjection(regions);
    }
}
