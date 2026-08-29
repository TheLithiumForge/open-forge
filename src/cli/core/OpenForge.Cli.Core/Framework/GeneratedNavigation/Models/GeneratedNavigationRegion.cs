using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;

internal enum GeneratedNavigationRegionState
{
    Available,
    Unavailable,
}

internal enum GeneratedNavigationRegionUnavailableReason
{
    RegionSourceUnsupported,
    SourceDocumentUnavailable,
    GeneratedRegionMissing,
    GeneratedRegionInvalid,
    GeneratedRegionUnavailable,
    GeneratedRegionLineEndingUnsupported,
    TopologyUnavailable,
    TopologyUnsafe,
    MetadataUnavailable,
    MetadataInvalid,
    MetadataUnrepresentable,
    DestinationUnsafe,
    DestinationConflict,
    ProjectionUnavailable,
}

internal sealed record GeneratedNavigationRegion
{
    private GeneratedNavigationRegion(
        SourceLogicalSource source,
        IEnumerable<GeneratedNavigationEntry> entries,
        GeneratedNavigationBoundedChange change)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(change);
        var ordered = entries
            .Select(entry => entry ?? throw new ArgumentException(
                "Generated navigation entries cannot contain null members.",
                nameof(entries)))
            .OrderBy(entry => entry.Destination, StringComparer.Ordinal)
            .ToArray();
        if (ordered.Select(entry => entry.PhysicalPath)
            .Distinct(PhysicalIdentityTracker.PathComparer)
            .Count() != ordered.Length)
        {
            throw new ArgumentException(
                "A generated navigation region requires unique physical entry targets.",
                nameof(entries));
        }

        Source = source;
        State = GeneratedNavigationRegionState.Available;
        Entries = new ReadOnlyCollection<GeneratedNavigationEntry>(ordered);
        Change = change;
    }

    private GeneratedNavigationRegion(
        SourceLogicalSource source,
        GeneratedNavigationRegionUnavailableReason unavailableReason,
        string cause)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!Enum.IsDefined(unavailableReason))
        {
            throw new ArgumentOutOfRangeException(
                nameof(unavailableReason),
                unavailableReason,
                "The generated navigation unavailable reason is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cause);

        Source = source;
        State = GeneratedNavigationRegionState.Unavailable;
        Entries = Array.Empty<GeneratedNavigationEntry>();
        UnavailableReason = unavailableReason;
        Cause = cause;
    }

    internal SourceLogicalSource Source { get; }

    internal string CanonicalPath => Source.Identity.CanonicalBasePath;

    internal string PhysicalPath => Source.Base.PhysicalPath;

    internal GeneratedNavigationRegionState State { get; }

    internal IReadOnlyList<GeneratedNavigationEntry> Entries { get; }

    internal GeneratedNavigationBoundedChange? Change { get; }

    internal GeneratedNavigationRegionUnavailableReason? UnavailableReason { get; }

    internal string? Cause { get; }

    internal string? ExpectedBody => Change?.ExpectedBody;

    internal static GeneratedNavigationRegion Available(
        SourceLogicalSource source,
        IEnumerable<GeneratedNavigationEntry> entries,
        GeneratedNavigationBoundedChange change)
        => new(source, entries, change);

    internal static GeneratedNavigationRegion Unavailable(
        SourceLogicalSource source,
        GeneratedNavigationRegionUnavailableReason reason,
        string cause)
        => new(source, reason, cause);
}
