using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;

internal sealed class RouteDoctorGeneratedNavigationReader
{
    internal IReadOnlyList<DoctorGeneratedNavigationTargetObservation> Read(
        RouteSourceInspection inspection)
    {
        var formation = new GeneratedNavigationFormationBuilder().Build(inspection.Catalogue);
        var observations = inspection.Sources.ToDictionary(
            source => source.Source.Identity.CanonicalBasePath,
            StringComparer.Ordinal);
        var regions = formation.Sources.Select(source =>
        {
            var observation = observations.GetValueOrDefault(
                source.Identity.CanonicalBasePath);
            return observation?.Document is { } document
                ? new GeneratedNavigationRegionInput(source, document)
                : new GeneratedNavigationRegionInput(
                    source,
                    "The generated navigation source document is unavailable.");
        }).ToArray();
        var metadata = formation.Sources.Select(source =>
        {
            var observation = observations.GetValueOrDefault(
                source.Identity.CanonicalBasePath);
            return new GeneratedNavigationMetadata(
                source,
                observation?.AuthoredMetadata
                    ?? throw new InvalidOperationException(
                        "A generated-navigation formation source requires one retained observation."));
        }).ToArray();
        var projection = new GeneratedNavigationProjector().Project(
            new GeneratedNavigationProjectionRequest(formation, regions, metadata));
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
            ReadUnavailableState(reason),
            new DoctorGeneratedNavigationContent(
                source.GeneratedEntries,
                region.Entries,
                EntryComparisons: []),
            new DoctorGeneratedNavigationUnavailability(reason, region.Cause));
    }

    private static OperationalGeneratedNavigationState ReadUnavailableState(
        GeneratedNavigationRegionUnavailableReason reason)
        => reason switch
        {
            GeneratedNavigationRegionUnavailableReason.GeneratedRegionMissing =>
                OperationalGeneratedNavigationState.Missing,
            GeneratedNavigationRegionUnavailableReason.TopologyUnsafe
                or GeneratedNavigationRegionUnavailableReason.DestinationUnsafe
                or GeneratedNavigationRegionUnavailableReason.DestinationConflict =>
                OperationalGeneratedNavigationState.Blocked,
            GeneratedNavigationRegionUnavailableReason.RegionSourceUnsupported
                or GeneratedNavigationRegionUnavailableReason.SourceDocumentUnavailable
                or GeneratedNavigationRegionUnavailableReason.GeneratedRegionInvalid
                or GeneratedNavigationRegionUnavailableReason.GeneratedRegionUnavailable
                or GeneratedNavigationRegionUnavailableReason.GeneratedRegionLineEndingUnsupported
                or GeneratedNavigationRegionUnavailableReason.TopologyUnavailable
                or GeneratedNavigationRegionUnavailableReason.MetadataUnavailable
                or GeneratedNavigationRegionUnavailableReason.MetadataInvalid
                or GeneratedNavigationRegionUnavailableReason.MetadataUnrepresentable
                or GeneratedNavigationRegionUnavailableReason.ProjectionUnavailable =>
                OperationalGeneratedNavigationState.Unavailable,
            _ => throw new ArgumentOutOfRangeException(
                nameof(reason),
                reason,
                "The generated-navigation unavailable reason is not defined."),
        };
}
