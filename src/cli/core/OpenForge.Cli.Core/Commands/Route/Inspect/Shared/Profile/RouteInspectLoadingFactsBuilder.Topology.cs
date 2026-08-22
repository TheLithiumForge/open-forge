using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Topology;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;

internal sealed partial class RouteInspectLoadingFactsBuilder
{
    private IReadOnlyList<RouteSource>? ReadChain(string path)
    {
        var current = _graph.Topology.FindByPath(path);
        if (current is null)
        {
            return null;
        }

        var chain = new List<RouteSource>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        while (seen.Add(current.Source.CanonicalPath))
        {
            chain.Add(current.Source);
            if (current.ParentState != RouteTopologyParentState.Resolved)
            {
                return current.ParentState == RouteTopologyParentState.None
                    ? chain.AsEnumerable().Reverse().ToArray()
                    : null;
            }

            current = _graph.Topology.FindByPath(current.ParentPath!);
            if (current is null)
            {
                return null;
            }
        }

        return null;
    }
}
