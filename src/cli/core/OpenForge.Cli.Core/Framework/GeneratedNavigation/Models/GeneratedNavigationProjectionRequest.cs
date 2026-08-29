using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;

internal sealed class GeneratedNavigationProjectionRequest
{
    private readonly IReadOnlyDictionary<string, SourceAuthoredMetadataFacts> _metadataByPath;
    private readonly IReadOnlyDictionary<string, SourceAuthoredMetadataFacts> _metadataByPhysical;

    internal GeneratedNavigationProjectionRequest(
        GeneratedNavigationFormation formation,
        IEnumerable<GeneratedNavigationRegionInput> regions,
        IEnumerable<GeneratedNavigationMetadata> metadata)
    {
        ArgumentNullException.ThrowIfNull(formation);

        var materializedRegions = regions
            .Select(region => region ?? throw new ArgumentException(
                "Generated navigation regions cannot contain null members.",
                nameof(regions)))
            .ToArray();
        if (materializedRegions.Any(region => formation.FindSource(
                region.Source.Identity.CanonicalBasePath) is not { } source
            || !ReferenceEquals(source, region.Source)))
        {
            throw new ArgumentException(
                "Generated navigation regions must retain source members from the request.",
                nameof(regions));
        }

        var materializedMetadata = metadata
            .Select(value => value ?? throw new ArgumentException(
                "Generated navigation metadata cannot contain null members.",
                nameof(metadata)))
            .OrderBy(value => value.Source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
        if (materializedMetadata.Any(value => formation.FindSource(
                value.Source.Identity.CanonicalBasePath) is not { } source
            || !ReferenceEquals(source, value.Source)))
        {
            throw new ArgumentException(
                "Generated navigation metadata must retain source members from the request.",
                nameof(metadata));
        }

        if (materializedMetadata.Select(value => value.Source.Identity.CanonicalBasePath)
            .Distinct(StringComparer.Ordinal)
            .Count() != materializedMetadata.Length)
        {
            throw new ArgumentException(
                "Generated navigation metadata requires one fact per canonical source path.",
                nameof(metadata));
        }

        Formation = formation;
        Regions = new ReadOnlyCollection<GeneratedNavigationRegionInput>(materializedRegions);
        Metadata = new ReadOnlyCollection<GeneratedNavigationMetadata>(materializedMetadata);
        _metadataByPath = new ReadOnlyDictionary<string, SourceAuthoredMetadataFacts>(
            materializedMetadata.ToDictionary(
                value => value.Source.Identity.CanonicalBasePath,
                value => value.Facts,
                StringComparer.Ordinal));
        _metadataByPhysical = new ReadOnlyDictionary<string, SourceAuthoredMetadataFacts>(
            materializedMetadata
                .GroupBy(
                    value => value.Source.Base.PhysicalPath,
                    PhysicalIdentityTracker.PathComparer)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderBy(value => value.Source.Identity.CanonicalBasePath, StringComparer.Ordinal)
                        .First()
                        .Facts,
                    PhysicalIdentityTracker.PathComparer));
    }

    internal GeneratedNavigationFormation Formation { get; }

    internal SourceRouteTopology Topology => Formation.Topology;

    internal IReadOnlyList<SourceLogicalSource> Sources => Formation.Sources;

    internal IReadOnlyList<GeneratedNavigationRegionInput> Regions { get; }

    internal IReadOnlyList<GeneratedNavigationMetadata> Metadata { get; }

    internal SourceLogicalSource? FindSource(string canonicalPath)
    {
        return Formation.FindSource(canonicalPath);
    }

    internal SourceAuthoredMetadataFacts? FindMetadata(SourceLogicalSource source)
    {
        if (_metadataByPath.TryGetValue(source.Identity.CanonicalBasePath, out var facts))
        {
            return facts;
        }

        return _metadataByPhysical.TryGetValue(source.Base.PhysicalPath, out facts)
            ? facts
            : null;
    }
}
