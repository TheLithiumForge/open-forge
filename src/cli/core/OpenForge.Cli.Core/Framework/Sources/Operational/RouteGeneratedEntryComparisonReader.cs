using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational;

internal static class RouteGeneratedEntryComparisonReader
{
    internal static IReadOnlyList<RouteGeneratedEntryComparison> Read(
        SourceLogicalSource parent,
        SourceGeneratedEntriesFacts current,
        IReadOnlyList<GeneratedNavigationEntry> expected,
        Utf8SourceMap? locations)
    {
        if (current.State != SourceGeneratedEntriesState.Complete || locations is null)
        {
            return [];
        }

        var expectedBySource = expected.ToDictionary(entry => entry.CanonicalPath, StringComparer.Ordinal);
        var matched = new HashSet<string>(StringComparer.Ordinal);
        var currentOrder = new List<string>();
        var comparisons = new List<RouteGeneratedEntryComparison>();
        foreach (var entry in current.Entries)
        {
            var location = locations.Map(entry.Span.Start, entry.Span.Length);
            var resolved = SourceGeneratedDestinationResolver.Resolve(
                parent.Identity.CanonicalBasePath,
                parent.Base.Form == SourceDocumentForm.Loader,
                entry.Destination);
            if (resolved is null || !expectedBySource.TryGetValue(resolved, out var expectedEntry))
            {
                comparisons.Add(new RouteGeneratedEntryComparison(
                    RouteGeneratedEntryComparisonKind.Extra,
                    expected: null,
                    entry.Destination,
                    location));
                continue;
            }

            matched.Add(resolved);
            currentOrder.Add(resolved);
            if (!string.Equals(entry.Destination, expectedEntry.Destination, StringComparison.Ordinal))
            {
                comparisons.Add(new RouteGeneratedEntryComparison(
                    RouteGeneratedEntryComparisonKind.Path,
                    expectedEntry.Destination,
                    entry.Destination,
                    location));
            }

            if (!string.Equals(entry.Description, expectedEntry.Description, StringComparison.Ordinal))
            {
                comparisons.Add(new RouteGeneratedEntryComparison(
                    RouteGeneratedEntryComparisonKind.Description,
                    expectedEntry.Description,
                    entry.Description,
                    location));
            }

            if (!entry.Tags.SequenceEqual(expectedEntry.Tags, StringComparer.Ordinal))
            {
                comparisons.Add(new RouteGeneratedEntryComparison(
                    RouteGeneratedEntryComparisonKind.Tags,
                    string.Join(",", expectedEntry.Tags),
                    string.Join(",", entry.Tags),
                    location));
            }
        }

        foreach (var entry in expected.Where(entry => !matched.Contains(entry.CanonicalPath)))
        {
            comparisons.Add(new RouteGeneratedEntryComparison(
                RouteGeneratedEntryComparisonKind.Missing,
                entry.Destination,
                actual: null,
                location: null));
        }

        var expectedOrder = expected.Where(entry => matched.Contains(entry.CanonicalPath))
            .Select(entry => entry.CanonicalPath)
            .ToArray();
        if (currentOrder.Count == expectedOrder.Length
            && !currentOrder.SequenceEqual(expectedOrder, StringComparer.Ordinal))
        {
            comparisons.Add(new RouteGeneratedEntryComparison(
                RouteGeneratedEntryComparisonKind.Order,
                string.Join("|", expectedOrder),
                string.Join("|", currentOrder),
                locations.Map(current.Entries[0].Span.Start, current.Entries[^1].Span.End - current.Entries[0].Span.Start)));
        }

        return comparisons
            .OrderBy(comparison => comparison.Kind)
            .ThenBy(comparison => comparison.Expected, StringComparer.Ordinal)
            .ThenBy(comparison => comparison.Actual, StringComparer.Ordinal)
            .ToArray();
    }
}
