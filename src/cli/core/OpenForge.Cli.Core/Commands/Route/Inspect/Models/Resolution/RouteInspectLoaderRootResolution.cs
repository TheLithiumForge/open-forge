using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;

internal sealed class RouteInspectLoaderRootResolution
{
    internal RouteInspectLoaderRootResolution(
        IEnumerable<string> rootPaths,
        IEnumerable<RouteInspectResolutionIssue> issues,
        bool interrupted,
        bool areLoaderRootFactsComplete)
    {
        ArgumentNullException.ThrowIfNull(rootPaths);
        ArgumentNullException.ThrowIfNull(issues);
        var materializedRoots = rootPaths.OrderBy(path => path, StringComparer.Ordinal).ToArray();
        var materializedIssues = issues.ToArray();
        if (materializedRoots.Any(path => string.IsNullOrWhiteSpace(path))
            || materializedRoots.Distinct(StringComparer.Ordinal).Count() != materializedRoots.Length)
        {
            throw new ArgumentException("Loader root paths must be non-empty and unique.", nameof(rootPaths));
        }

        if (materializedIssues.Any(issue => issue is null))
        {
            throw new ArgumentException("Loader root issues cannot contain null.", nameof(issues));
        }

        if (areLoaderRootFactsComplete && (interrupted || materializedIssues.Length != 0))
        {
            throw new ArgumentException("Complete Loader root facts cannot be interrupted or contain issues.");
        }

        RootPaths = new ReadOnlyCollection<string>(materializedRoots);
        Issues = new ReadOnlyCollection<RouteInspectResolutionIssue>(materializedIssues);
        Interrupted = interrupted;
        AreLoaderRootFactsComplete = areLoaderRootFactsComplete;
    }

    internal IReadOnlyList<string> RootPaths { get; }

    internal IReadOnlyList<RouteInspectResolutionIssue> Issues { get; }

    internal bool Interrupted { get; }

    internal bool AreLoaderRootFactsComplete { get; }
}
