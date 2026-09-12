using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;

internal sealed class RouteDoctorGeneratedNavigationReader
{
    internal IReadOnlyList<DoctorGeneratedNavigationTargetObservation> Read(
        RouteSourceInspection inspection)
    {
        var formation = new GeneratedNavigationFormationBuilder().Build(inspection.Catalogue);
        var observations = RouteGeneratedNavigationProjection.IndexSources(inspection.Sources);
        var projection = RouteGeneratedNavigationProjection.Project(formation, observations, formation.Sources);
        return projection.Regions
            .OrderBy(region => region.CanonicalPath, StringComparer.Ordinal)
            .Select(region => Project(region, observations[region.CanonicalPath]))
            .ToArray();
    }

    private static DoctorGeneratedNavigationTargetObservation Project(
        GeneratedNavigationRegion region,
        RouteSourceObservation source)
    {
        if (region.State == GeneratedNavigationRegionState.Available)
        {
            var content = new DoctorGeneratedNavigationContent(
                source.GeneratedEntries,
                region.Entries,
                RouteGeneratedEntryComparisonReader.Read(
                    source.Source,
                    source.GeneratedEntries,
                    region.Entries,
                    source.Layers[0].Text is { } text ? new Sources.Locations.Utf8SourceMap(text) : null));
            var state = region.Change?.IsUnchanged == true
                ? OperationalGeneratedNavigationState.Current
                : OperationalGeneratedNavigationState.Changed;
            return DoctorGeneratedNavigationTargetObservation.Available(
                region.CanonicalPath,
                state,
                content);
        }

        var reason = region.UnavailableReason
            ?? throw new InvalidOperationException(
                "An unavailable generated-navigation region requires a typed reason.");
        return DoctorGeneratedNavigationTargetObservation.Unavailable(
            region.CanonicalPath,
            RouteGeneratedNavigationProjection.ReadUnavailableState(reason, nameof(reason)),
            new DoctorGeneratedNavigationContent(
                source.GeneratedEntries,
                region.Entries,
                EntryComparisons: []),
            new DoctorGeneratedNavigationUnavailability(reason, region.Cause));
    }
}
