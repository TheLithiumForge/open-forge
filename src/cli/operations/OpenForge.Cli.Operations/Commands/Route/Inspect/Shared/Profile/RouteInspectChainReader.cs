using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;

internal static class RouteInspectChainReader
{
    internal static IReadOnlyList<RouteSource>? Read(RouteInspectGraph graph, string path)
    {
        var topology = graph.RouteFacts.Topology;
        var current = topology.FindByPath(path);
        if (current is null)
        {
            return null;
        }

        var chain = new List<RouteSource>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        while (seen.Add(current.Identity.CanonicalBasePath))
        {
            var source = graph.ProjectionSet.FindByPath(current.Identity.CanonicalBasePath);
            if (source is null)
            {
                return null;
            }

            chain.Add(source);
            if (current.ParentState != SourceRouteParentState.Resolved)
            {
                return current.ParentState == SourceRouteParentState.None
                    ? chain.AsEnumerable().Reverse().ToArray()
                    : null;
            }

            current = topology.FindByPath(current.ParentPaths[0]);
            if (current is null)
            {
                return null;
            }
        }

        return null;
    }
}
