using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Routing;

internal sealed class SourceRouteFacts
{
    internal SourceRouteFacts(
        SourceRouteTopology topology,
        IEnumerable<SourceRouteFact> routeFacts,
        IEnumerable<SourceRouteIssue> issues,
        bool areLoaderRootFactsComplete,
        bool isCancelled)
    {
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(routeFacts);
        ArgumentNullException.ThrowIfNull(issues);

        var orderedRouteFacts = routeFacts
            .Select(fact => fact ?? throw new ArgumentException("Source route facts cannot contain null.", nameof(routeFacts)))
            .OrderBy(fact => fact.Identity.AutomaticId, StringComparer.Ordinal)
            .ThenBy(fact => fact.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
        if (orderedRouteFacts.Select(fact => fact.Identity.CanonicalBasePath)
            .Distinct(StringComparer.Ordinal)
            .Count() != orderedRouteFacts.Length)
        {
            throw new ArgumentException("Source route facts require unique canonical base paths.", nameof(routeFacts));
        }

        var orderedIssues = issues
            .Select(issue => issue ?? throw new ArgumentException("Source route issues cannot contain null.", nameof(issues)))
            .OrderBy(issue => issue.Code)
            .ThenBy(issue => issue.CanonicalPath, StringComparer.Ordinal)
            .ThenBy(issue => string.Join("\u001f", issue.RelatedPaths), StringComparer.Ordinal)
            .ThenBy(issue => issue.Occurrence)
            .ToArray();

        Topology = topology;
        RouteFacts = new ReadOnlyCollection<SourceRouteFact>(orderedRouteFacts);
        Issues = new ReadOnlyCollection<SourceRouteIssue>(orderedIssues);
        AreLoaderRootFactsComplete = areLoaderRootFactsComplete;
        IsCancelled = isCancelled;
    }

    internal SourceRouteTopology Topology { get; }

    internal IReadOnlyList<SourceRouteFact> RouteFacts { get; }

    internal IReadOnlyList<SourceRouteIssue> Issues { get; }

    internal bool AreLoaderRootFactsComplete { get; }

    internal bool IsCancelled { get; }
}
