using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Topology;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Topology;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal sealed class RouteInspectTopologyResolver
{
    internal RouteTopologyFacts Build(
        RouteSourceCatalogue catalogue,
        IReadOnlyList<string> loaderRootPaths)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(loaderRootPaths);
        var entrypointDirectories = catalogue.Sources
            .Where(source => source.Kind == RouteSourceKind.Entrypoint)
            .Select(source => RouteLogicalPath.ReadParent(source.CanonicalPath))
            .ToHashSet(StringComparer.Ordinal);
        var sources = catalogue.Sources
            .Where(source => source.Kind != RouteSourceKind.Loader)
            .Where(source => source.Kind == RouteSourceKind.Entrypoint
                || entrypointDirectories.Contains(ReadRepresentedParentDirectory(source)))
            .OrderBy(source => source.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        return new RouteTopologyBuilder().Build(sources, loaderRootPaths);
    }

    internal RouteInspectRouteState ReadRouteState(
        RouteSource source,
        RouteTopologyFacts topology,
        bool areLoaderRootFactsComplete)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(topology);
        var node = topology.FindByPath(source.CanonicalPath);
        if (node is null)
        {
            return areLoaderRootFactsComplete
                ? RouteInspectRouteState.NotRouted
                : RouteInspectRouteState.Unresolved;
        }

        if (HasAmbiguousRoute(node, topology))
        {
            return RouteInspectRouteState.Ambiguous;
        }

        if (topology.ReadAbsoluteDepth(source.CanonicalPath) is not null)
        {
            return RouteInspectRouteState.Routed;
        }

        if (!areLoaderRootFactsComplete)
        {
            return RouteInspectRouteState.Unresolved;
        }

        return source.Kind == RouteSourceKind.Entrypoint
            ? RouteInspectRouteState.Detached
            : RouteInspectRouteState.NotRouted;
    }

    internal static IReadOnlyList<RouteInspectPhysicalLayer> ReadPhysicalLayers(RouteSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var layers = new List<RouteInspectPhysicalLayer>
        {
            new(source.CanonicalPath, source.PhysicalPath, RouteInspectLayerRole.Base),
        };
        if (source.Overwrite is not null)
        {
            layers.Add(new RouteInspectPhysicalLayer(
                source.Overwrite.CanonicalLogicalPath,
                source.Overwrite.PhysicalPath,
                RouteInspectLayerRole.Overwrite));
        }

        return layers;
    }

    private static string ReadRepresentedParentDirectory(RouteSource source)
    {
        var containingDirectory = RouteLogicalPath.ReadParent(source.CanonicalPath);
        return source.Kind == RouteSourceKind.Markdown
            ? containingDirectory
            : RouteLogicalPath.ReadParent(containingDirectory);
    }

    private static bool HasAmbiguousRoute(RouteTopologyNode node, RouteTopologyFacts topology)
    {
        var current = node;
        var visited = new HashSet<string>(StringComparer.Ordinal);
        while (visited.Add(current.Source.CanonicalPath))
        {
            if (current.Source.IsRouteAmbiguous || current.ParentState == RouteTopologyParentState.Ambiguous)
            {
                return true;
            }

            if (current.ParentState != RouteTopologyParentState.Resolved)
            {
                return false;
            }

            current = topology.FindByPath(current.ParentPath!)
                ?? throw new InvalidOperationException("The topology parent is not present in the topology graph.");
        }

        return true;
    }
}
