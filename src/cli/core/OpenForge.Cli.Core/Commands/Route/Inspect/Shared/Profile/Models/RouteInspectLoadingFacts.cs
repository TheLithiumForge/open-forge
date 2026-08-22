using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile.Models;

internal sealed class RouteInspectLoadingFacts
{
    internal bool StartupApplicable { get; init; }

    internal bool SelectedApplicable { get; init; }

    internal bool NarrowDescendantsApplicable { get; init; }

    internal bool StartupAvailable { get; init; }

    internal bool SelectedAvailable { get; init; }

    internal bool NarrowDescendantsAvailable { get; init; }

    internal bool ReadingAvailable { get; init; }

    internal IReadOnlySet<string> StartupPaths { get; init; } = new HashSet<string>(StringComparer.Ordinal);

    internal IReadOnlySet<string> SelectedPaths { get; init; } = new HashSet<string>(StringComparer.Ordinal);

    internal IReadOnlySet<string> LoadNowDescendantPaths { get; init; } = new HashSet<string>(StringComparer.Ordinal);

    internal IReadOnlySet<string> RequiredAncestorPaths { get; init; } = new HashSet<string>(StringComparer.Ordinal);

    internal IReadOnlyDictionary<string, IReadOnlyList<RouteInspectVisibleEntry>> VisibleEntries { get; init; }
        = new ReadOnlyDictionary<string, IReadOnlyList<RouteInspectVisibleEntry>>(
            new Dictionary<string, IReadOnlyList<RouteInspectVisibleEntry>>(StringComparer.Ordinal));
}

internal sealed class RouteInspectLoadingFactInputs
{
    internal bool StartupApplicable { get; init; }

    internal bool SelectedApplicable { get; init; }

    internal bool NarrowDescendantsApplicable { get; init; }

    internal IEnumerable<string> StartupPaths { get; init; } = [];

    internal IEnumerable<string> SelectedPaths { get; init; } = [];

    internal IEnumerable<string> LoadNowDescendantPaths { get; init; } = [];

    internal IEnumerable<string> RequiredAncestorPaths { get; init; } = [];
}

internal sealed class RouteInspectVisibleEntry
{
    internal RouteInspectVisibleEntry(
        string parentPath,
        string targetPath,
        bool loadNow,
        bool keepInMind)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parentPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);
        ParentPath = parentPath;
        TargetPath = targetPath;
        LoadNow = loadNow;
        KeepInMind = keepInMind;
    }

    internal string ParentPath { get; }

    internal string TargetPath { get; }

    internal bool LoadNow { get; }

    internal bool KeepInMind { get; }
}

internal sealed class RouteInspectVisibleEntriesRead
{
    internal bool IsAvailable { get; init; }

    internal string Reason { get; init; } = "The visible Entries facts are unavailable.";

    internal IReadOnlyList<RouteInspectVisibleEntry> Entries { get; init; } = [];
}
