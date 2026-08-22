using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Topology;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;

internal sealed class RouteInspectGraph
{
    internal RouteInspectGraph(RouteSourceCatalogue catalogue, RouteTopologyFacts topology)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(topology);
        foreach (var node in topology.Nodes)
        {
            var catalogueSource = catalogue.FindByPath(node.Source.CanonicalPath);
            if (!ReferenceEquals(catalogueSource, node.Source))
            {
                throw new ArgumentException(
                    "Topology sources must be reference-identical members of the source catalogue.",
                    nameof(topology));
            }
        }

        Catalogue = catalogue;
        Topology = topology;
    }

    internal RouteSourceCatalogue Catalogue { get; }

    internal RouteTopologyFacts Topology { get; }
}
