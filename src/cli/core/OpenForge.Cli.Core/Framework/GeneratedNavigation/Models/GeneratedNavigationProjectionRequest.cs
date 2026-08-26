using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;

internal sealed class GeneratedNavigationProjectionRequest
{
    private readonly IReadOnlyDictionary<string, SourceLogicalSource> _sourcesByPath;
    private readonly IReadOnlyDictionary<string, SourceAuthoredMetadataFacts> _metadataByPath;
    private readonly IReadOnlyDictionary<string, SourceAuthoredMetadataFacts> _metadataByPhysical;

    internal GeneratedNavigationProjectionRequest(
        SourceRouteTopology topology,
        IEnumerable<SourceLogicalSource> sources,
        IEnumerable<GeneratedNavigationRegionInput> regions,
        IEnumerable<GeneratedNavigationMetadata> metadata)
    {
        var orderedSources = sources
            .Select(source => source ?? throw new ArgumentException(
                "Generated navigation sources cannot contain null members.",
                nameof(sources)))
            .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
        if (orderedSources.Select(source => source.Identity.CanonicalBasePath)
            .Distinct(StringComparer.Ordinal)
            .Count() != orderedSources.Length)
        {
            throw new ArgumentException(
                "Generated navigation sources require unique canonical paths.",
                nameof(sources));
        }

        var sourceMap = orderedSources.ToDictionary(
            source => source.Identity.CanonicalBasePath,
            StringComparer.Ordinal);
        if (topology.Nodes.Any(node => !sourceMap.ContainsKey(node.Identity.CanonicalBasePath))
            || topology.LoaderRootPaths.Any(path => !sourceMap.ContainsKey(path)))
        {
            throw new ArgumentException(
                "Generated navigation topology must retain source members for every node and Loader root.",
                nameof(topology));
        }

        var materializedRegions = regions
            .Select(region => region ?? throw new ArgumentException(
                "Generated navigation regions cannot contain null members.",
                nameof(regions)))
            .ToArray();
        if (materializedRegions.Any(region => !sourceMap.TryGetValue(
                region.Source.Identity.CanonicalBasePath,
                out var source)
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
        if (materializedMetadata.Any(value => !sourceMap.TryGetValue(
                value.Source.Identity.CanonicalBasePath,
                out var source)
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

        Topology = topology;
        Sources = new ReadOnlyCollection<SourceLogicalSource>(orderedSources);
        Regions = new ReadOnlyCollection<GeneratedNavigationRegionInput>(materializedRegions);
        Metadata = new ReadOnlyCollection<GeneratedNavigationMetadata>(materializedMetadata);
        _sourcesByPath = new ReadOnlyDictionary<string, SourceLogicalSource>(sourceMap);
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

    internal SourceRouteTopology Topology { get; }

    internal IReadOnlyList<SourceLogicalSource> Sources { get; }

    internal IReadOnlyList<GeneratedNavigationRegionInput> Regions { get; }

    internal IReadOnlyList<GeneratedNavigationMetadata> Metadata { get; }

    internal SourceLogicalSource? FindSource(string canonicalPath)
    {
        return _sourcesByPath.TryGetValue(canonicalPath, out var source)
            ? source
            : null;
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
