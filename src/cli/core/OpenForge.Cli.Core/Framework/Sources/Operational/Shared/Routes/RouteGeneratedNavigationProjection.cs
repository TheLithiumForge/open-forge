using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;

internal static class RouteGeneratedNavigationProjection
{
    internal static IReadOnlyDictionary<string, RouteSourceObservation> IndexSources(
        IReadOnlyList<RouteSourceObservation> sources)
        => sources.ToDictionary(
            source => source.Source.Identity.CanonicalBasePath,
            StringComparer.Ordinal);

    internal static GeneratedNavigationProjection Project(
        GeneratedNavigationFormation formation,
        IReadOnlyDictionary<string, RouteSourceObservation> observations,
        IEnumerable<SourceLogicalSource> sources)
    {
        var regions = sources.Select(source =>
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
        return new GeneratedNavigationProjector().Project(
            new GeneratedNavigationProjectionRequest(formation, regions, metadata));
    }

    internal static OperationalGeneratedNavigationState ReadUnavailableState(
        GeneratedNavigationRegionUnavailableReason reason,
        string parameterName)
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
                parameterName,
                reason,
                "The generated-navigation unavailable reason is not defined."),
        };
}
