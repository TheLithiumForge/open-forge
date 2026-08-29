using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.Core.Commands.Index.Models.Projection;

internal sealed record IndexProjectionContext
{
    public required GeneratedNavigationFormation Formation { get; init; }

    public required IndexSelectionResolution Selection { get; init; }

    public required SourceDocumentReader Reader { get; init; }
}

internal sealed record IndexProjectionAssembly
{
    public required GeneratedNavigationFormation Formation { get; init; }

    public required IndexSelectionResolution Selection { get; init; }

    public required IEnumerable<IndexProjectionRegionInput> Regions { get; init; }

    public required IEnumerable<GeneratedNavigationMetadata> Metadata { get; init; }

    public required IndexProjectionReadiness Readiness { get; init; }
}

internal sealed record IndexProjectionRegionInput
{
    private IndexProjectionRegionInput(
        GeneratedNavigationRegionInput navigation,
        int? beforeEntryCount)
    {
        if (beforeEntryCount is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(beforeEntryCount));
        }

        Navigation = navigation;
        BeforeEntryCount = beforeEntryCount;
    }

    internal GeneratedNavigationRegionInput Navigation { get; }

    internal int? BeforeEntryCount { get; }

    internal static IndexProjectionRegionInput FromDocument(
        SourceLogicalSource source,
        MarkdownDocumentFacts document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return new IndexProjectionRegionInput(
            new GeneratedNavigationRegionInput(source, document),
            ReadBeforeEntryCount(document));
    }

    internal static IndexProjectionRegionInput Unavailable(
        SourceLogicalSource source,
        string unavailableCause)
        => new(
            new GeneratedNavigationRegionInput(source, unavailableCause),
            beforeEntryCount: null);

    private static int? ReadBeforeEntryCount(MarkdownDocumentFacts document)
    {
        var entries = SourceGeneratedEntriesParser.Parse(document);
        return entries.State switch
        {
            SourceGeneratedEntriesState.Complete => entries.Entries.Count,
            SourceGeneratedEntriesState.Absent or SourceGeneratedEntriesState.Unavailable => null,
            _ => throw new ArgumentOutOfRangeException(
                nameof(document),
                entries.State,
                "The generated Entries state is not defined."),
        };
    }
}

internal sealed record IndexProjectedRegion
{
    internal IndexProjectedRegion(
        GeneratedNavigationRegion region,
        IndexLogicalSource source,
        int? beforeEntryCount)
    {
        ArgumentNullException.ThrowIfNull(region);
        ArgumentNullException.ThrowIfNull(source);
        if (beforeEntryCount is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(beforeEntryCount));
        }
        if (!string.Equals(source.Path, region.CanonicalPath, StringComparison.Ordinal)
            || !string.Equals(source.Id, region.Source.Identity.AutomaticId, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "An Index projected region requires the generated region's canonical source identity.",
                nameof(source));
        }

        Region = region;
        Source = source;
        BeforeEntryCount = beforeEntryCount;
        ExpectedEntryCount = ReadExpectedEntryCount(region.State, region.Entries.Count);
    }

    internal GeneratedNavigationRegion Region { get; }

    internal IndexLogicalSource Source { get; }

    internal int? BeforeEntryCount { get; }

    internal int? ExpectedEntryCount { get; }

    internal static int? ReadExpectedEntryCount(
        GeneratedNavigationRegionState state,
        int entryCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(entryCount);
        return state switch
        {
            GeneratedNavigationRegionState.Available => entryCount,
            GeneratedNavigationRegionState.Unavailable => null,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The generated navigation region state is not defined."),
        };
    }
}

internal sealed record IndexProjectionReadiness
{
    internal IndexProjectionReadiness(
        IEnumerable<IndexFinding> findings,
        IEnumerable<string> unavailableTargetPaths)
    {
        ArgumentNullException.ThrowIfNull(findings);
        ArgumentNullException.ThrowIfNull(unavailableTargetPaths);
        var findingValues = findings
            .Select(finding => finding ?? throw new ArgumentException(
                "Index projection readiness findings cannot contain null members.",
                nameof(findings)))
            .ToArray();
        var pathValues = unavailableTargetPaths
            .Select(path => path ?? throw new ArgumentException(
                "Index projection readiness targets cannot contain null members.",
                nameof(unavailableTargetPaths)))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        if (pathValues.Any(path => !SourceLogicalPath.IsCanonical(path))
            || pathValues.Distinct(StringComparer.Ordinal).Count() != pathValues.Length)
        {
            throw new ArgumentException(
                "Index projection readiness targets require unique canonical paths.",
                nameof(unavailableTargetPaths));
        }
        if ((findingValues.Length == 0) != (pathValues.Length == 0))
        {
            throw new ArgumentException(
                "Index projection readiness findings must exactly identify unavailable targets.",
                nameof(unavailableTargetPaths));
        }

        Findings = new ReadOnlyCollection<IndexFinding>(IndexResult.OrderFindings(findingValues).ToArray());
        UnavailableTargetPaths = new ReadOnlyCollection<string>(pathValues);
    }

    internal IReadOnlyList<IndexFinding> Findings { get; }

    internal IReadOnlyList<string> UnavailableTargetPaths { get; }
}

internal sealed record IndexProjectionFormation
{
    internal IndexProjectionFormation(
        IndexSelectionResolution selection,
        GeneratedNavigationProjection projection,
        IEnumerable<IndexProjectedRegion> regions,
        IEnumerable<IndexFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(regions);
        ArgumentNullException.ThrowIfNull(findings);
        if (!selection.IsComplete)
        {
            throw new ArgumentException("Index projection requires a complete target selection.", nameof(selection));
        }

        var findingValues = findings
            .Select(finding => finding ?? throw new ArgumentException(
                "Index projection findings cannot contain null members.",
                nameof(findings)))
            .ToArray();
        var targetPaths = selection.Targets
            .Select(target => target.Identity.CanonicalBasePath)
            .OrderBy(path => path, StringComparer.Ordinal);
        var regionPaths = projection.Regions.Select(region => region.CanonicalPath);
        if (!targetPaths.SequenceEqual(regionPaths, StringComparer.Ordinal))
        {
            throw new ArgumentException(
                "Index projection regions must exactly cover the selected target closure.",
                nameof(projection));
        }

        var regionValues = regions
            .Select(region => region ?? throw new ArgumentException(
                "Index projected regions cannot contain null members.",
                nameof(regions)))
            .ToArray();
        if (regionValues.Length != projection.Regions.Count
            || regionValues.Where((region, index) => !ReferenceEquals(
                region.Region,
                projection.Regions[index])).Any())
        {
            throw new ArgumentException(
                "Index projected-region count facts must correspond one-to-one with the generated projection.",
                nameof(regions));
        }

        if ((findingValues.Length == 0) != projection.IsComplete)
        {
            throw new ArgumentException(
                "Index projection findings must exactly describe projection completeness.",
                nameof(findings));
        }

        Selection = selection;
        Projection = projection;
        Regions = new ReadOnlyCollection<IndexProjectedRegion>(regionValues);
        Findings = new ReadOnlyCollection<IndexFinding>(IndexResult.OrderFindings(findingValues).ToArray());
    }

    internal IndexSelectionResolution Selection { get; }

    internal GeneratedNavigationProjection Projection { get; }

    internal IReadOnlyList<IndexProjectedRegion> Regions { get; }

    internal IReadOnlyList<IndexFinding> Findings { get; }

    internal bool IsComplete => Findings.Count == 0;
}
