using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;

internal enum GeneratedNavigationRegionState
{
    Available,
    Unavailable,
}

internal sealed record GeneratedNavigationRegion
{
    private GeneratedNavigationRegion(
        SourceLogicalSource source,
        GeneratedNavigationRegionState state,
        IEnumerable<GeneratedNavigationEntry> entries,
        GeneratedNavigationBoundedChange? change,
        string? cause)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The generated navigation region state is not defined.");
        }

        var values = entries
            .Select(entry => entry ?? throw new ArgumentException(
                "Generated navigation entries cannot contain null members.",
                nameof(entries)))
            .ToArray();
        var ordered = values
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

        if (state == GeneratedNavigationRegionState.Available
            && (change is null || cause is not null))
        {
            throw new ArgumentException(
                "An available generated navigation region requires bounded change facts.");
        }

        if (state == GeneratedNavigationRegionState.Unavailable
            && (ordered.Length != 0 || change is not null || string.IsNullOrWhiteSpace(cause)))
        {
            throw new ArgumentException(
                "An unavailable generated navigation region cannot carry projection values.");
        }

        Source = source;
        State = state;
        Entries = new ReadOnlyCollection<GeneratedNavigationEntry>(ordered);
        Change = change;
        Cause = cause;
    }

    internal SourceLogicalSource Source { get; }

    internal string CanonicalPath => Source.Identity.CanonicalBasePath;

    internal string PhysicalPath => Source.Base.PhysicalPath;

    internal GeneratedNavigationRegionState State { get; }

    internal IReadOnlyList<GeneratedNavigationEntry> Entries { get; }

    internal GeneratedNavigationBoundedChange? Change { get; }

    internal string? Cause { get; }

    internal string? ExpectedBody => Change?.ExpectedBody;

    internal static GeneratedNavigationRegion Available(
        SourceLogicalSource source,
        IEnumerable<GeneratedNavigationEntry> entries,
        GeneratedNavigationBoundedChange change)
        => new(source, GeneratedNavigationRegionState.Available, entries, change, null);

    internal static GeneratedNavigationRegion Unavailable(
        SourceLogicalSource source,
        string cause)
        => new(source, GeneratedNavigationRegionState.Unavailable, [], null, cause);
}
