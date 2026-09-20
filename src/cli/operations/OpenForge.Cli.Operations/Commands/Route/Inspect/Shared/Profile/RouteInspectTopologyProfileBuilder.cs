using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;

internal sealed class RouteInspectTopologyProfileBuilder
{
    private readonly RouteInspectResolution _resolution;
    private readonly CancellationToken _cancellationToken;

    internal RouteInspectTopologyProfileBuilder(
        RouteInspectResolution resolution,
        CancellationToken cancellationToken)
    {
        _resolution = resolution;
        _cancellationToken = cancellationToken;
    }

    internal RouteInspectFact<RouteInspectTopology> Build()
    {
        var identity = _resolution.ReadIdentity();
        if (identity.RouteState == RouteInspectRouteState.NotRouted)
        {
            return RouteInspectFact<RouteInspectTopology>.NotApplicable(
                "Route topology does not apply to an unrouted source.");
        }

        if (identity.RouteState == RouteInspectRouteState.Unresolved)
        {
            return RouteInspectFact<RouteInspectTopology>.Unavailable(
                "The Loader-rooted route topology is unavailable.");
        }

        var graph = _resolution.ReadGraph();
        var selected = graph.ProjectionSet.FindByPath(identity.CanonicalWorkspaceRelativePath);
        var chain = selected is null ? null : RouteInspectChainReader.Read(graph, selected.CanonicalPath);
        if (selected is null || chain is null)
        {
            return RouteInspectFact<RouteInspectTopology>.Unavailable(
                "The selected route topology is unavailable.");
        }

        var counts = selected.Kind == RouteSourceKind.Entrypoint
            ? ReadCounts(graph, selected.CanonicalPath)
            : RouteInspectFact<RouteInspectTopologyCounts>.NotApplicable(
                "Child topology counts do not apply to an ordinary source.");
        if (counts is null)
        {
            return RouteInspectFact<RouteInspectTopology>.Unavailable(
                "The selected route topology is unavailable.");
        }

        var routeChain = chain
            .Select(source => source.Id[(source.Id.LastIndexOf('/') + 1)..])
            .ToArray();
        var topology = new RouteInspectTopology(
            routeChain[0],
            routeChain,
            routeChain.Length > 1
                ? string.Join('/', routeChain.Take(routeChain.Length - 1))
                : null,
            routeChain.Length,
            counts);
        return RouteInspectFact<RouteInspectTopology>.Available(topology);
    }

    private RouteInspectFact<RouteInspectTopologyCounts>? ReadCounts(
        RouteInspectGraph graph,
        string selectedPath)
    {
        var topology = graph.RouteFacts.Topology;
        var selected = topology.FindByPath(selectedPath);
        if (selected is null)
        {
            return null;
        }

        var directFiles = 0;
        var directEntrypoints = 0;
        foreach (var childPath in selected.ChildPaths)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            var child = topology.FindByPath(childPath);
            if (child is null)
            {
                return null;
            }

            var childSource = graph.ProjectionSet.FindByPath(child.Identity.CanonicalBasePath);
            if (childSource is null)
            {
                return null;
            }

            if (childSource.Kind == RouteSourceKind.Entrypoint)
            {
                directEntrypoints++;
            }
            else
            {
                directFiles++;
            }
        }

        var descendants = new HashSet<string>(StringComparer.Ordinal);
        var queue = new Queue<string>(selected.ChildPaths);
        while (queue.TryDequeue(out var path))
        {
            _cancellationToken.ThrowIfCancellationRequested();
            if (!descendants.Add(path))
            {
                continue;
            }

            var node = topology.FindByPath(path);
            if (node is null)
            {
                return null;
            }

            foreach (var childPath in node.ChildPaths)
            {
                queue.Enqueue(childPath);
            }
        }

        var descendantEntrypoints = 0;
        foreach (var path in descendants)
        {
            var node = topology.FindByPath(path);
            var source = node is null
                ? null
                : graph.ProjectionSet.FindByPath(node.Identity.CanonicalBasePath);
            if (source is null)
            {
                return null;
            }

            if (source.Kind == RouteSourceKind.Entrypoint)
            {
                descendantEntrypoints++;
            }
        }

        return RouteInspectFact<RouteInspectTopologyCounts>.Available(
            new RouteInspectTopologyCounts(
                directFiles,
                directEntrypoints,
                descendants.Count - descendantEntrypoints,
                descendantEntrypoints));
    }

}
