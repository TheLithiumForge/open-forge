using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;

internal sealed class RouteInspectTopologyCounts
{
    internal RouteInspectTopologyCounts(
        int directRoutedFileCount,
        int directEntrypointCount,
        int descendantRoutedFileCount,
        int descendantEntrypointCount)
    {
        if (directRoutedFileCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(directRoutedFileCount),
                directRoutedFileCount,
                "Direct routed-file count cannot be negative.");
        }

        if (directEntrypointCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(directEntrypointCount),
                directEntrypointCount,
                "Direct entrypoint count cannot be negative.");
        }

        if (descendantRoutedFileCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(descendantRoutedFileCount),
                descendantRoutedFileCount,
                "Descendant routed-file count cannot be negative.");
        }

        if (descendantEntrypointCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(descendantEntrypointCount),
                descendantEntrypointCount,
                "Descendant entrypoint count cannot be negative.");
        }

        DirectRoutedFileCount = directRoutedFileCount;
        DirectEntrypointCount = directEntrypointCount;
        DescendantRoutedFileCount = descendantRoutedFileCount;
        DescendantEntrypointCount = descendantEntrypointCount;
    }

    internal int DirectRoutedFileCount { get; }

    internal int DirectEntrypointCount { get; }

    internal int DescendantRoutedFileCount { get; }

    internal int DescendantEntrypointCount { get; }
}

internal sealed class RouteInspectTopology
{
    internal RouteInspectTopology(
        string rootRoute,
        IEnumerable<string> routeChain,
        string? parentId,
        int depth,
        RouteInspectFact<RouteInspectTopologyCounts> counts)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootRoute);
        if (parentId is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(parentId);
        }

        if (depth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(depth), depth, "Route depth must be positive.");
        }

        ArgumentNullException.ThrowIfNull(counts);
        RootRoute = rootRoute;
        RouteChain = MaterializeRouteChain(routeChain);
        ParentId = parentId;
        Depth = depth;
        Counts = counts;
        ValidateDepthAndParent();
    }

    internal string RootRoute { get; }

    internal IReadOnlyList<string> RouteChain { get; }

    internal string? ParentId { get; }

    internal int Depth { get; }

    internal RouteInspectFact<RouteInspectTopologyCounts> Counts { get; }

    private static IReadOnlyList<string> MaterializeRouteChain(IEnumerable<string> routeChain)
    {
        ArgumentNullException.ThrowIfNull(routeChain);
        var materialized = routeChain.ToArray();
        if (materialized.Length == 0)
        {
            throw new ArgumentException("A route-inspect topology requires a nonempty route chain.", nameof(routeChain));
        }

        foreach (var route in materialized)
        {
            if (route is null)
            {
                throw new ArgumentException("Route chains cannot contain null.", nameof(routeChain));
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(route);
        }

        return new ReadOnlyCollection<string>(materialized);
    }

    private void ValidateDepthAndParent()
    {
        if (Depth != RouteChain.Count)
        {
            throw new ArgumentException("Route depth must equal the route-chain count.", nameof(Depth));
        }

        if (!string.Equals(RootRoute, RouteChain[0], StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The root route must equal the first route-chain segment.",
                nameof(RootRoute));
        }

        if (Depth == 1 && ParentId is not null)
        {
            throw new ArgumentException("A root route cannot have a parent ID.", nameof(ParentId));
        }

        if (Depth > 1 && ParentId is null)
        {
            throw new ArgumentException("A nested route requires a parent ID.", nameof(ParentId));
        }

        if (Depth > 1)
        {
            var expectedParentId = string.Join('/', RouteChain.Take(RouteChain.Count - 1));
            if (!string.Equals(ParentId, expectedParentId, StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "A nested route parent ID must equal its preceding route segments.",
                    nameof(ParentId));
            }
        }
    }
}
