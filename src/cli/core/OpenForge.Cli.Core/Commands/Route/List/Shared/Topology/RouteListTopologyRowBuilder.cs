using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Topology;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

internal sealed class RouteListTopologyRowBuilder
{
    private readonly RouteListTopologyInput _input;
    private readonly RouteTopologyFacts _topology;

    internal RouteListTopologyRowBuilder(
        RouteListTopologyInput input,
        RouteTopologyFacts topology)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(topology);
        _input = input;
        _topology = topology;
    }

    internal RouteListRow Build(
        RouteTopologyNode node,
        int relativeDepth,
        int? absoluteDepth,
        RouteListSelectionProvenance selectionProvenance)
    {
        ArgumentNullException.ThrowIfNull(node);
        var source = node.Source;
        string? parentId = null;
        if (node.ParentPath is { } parentPath)
        {
            parentId = _topology.FindByPath(parentPath)?.Source.Id
                ?? throw new InvalidOperationException("A resolved topology parent is missing from the immutable graph.");
        }

        var sourceProvenance = source.Kind switch
        {
            RouteSourceKind.Entrypoint => RouteListSourceProvenance.AuthoredEntrypoint,
            RouteSourceKind.Markdown => RouteListSourceProvenance.AuthoredLeaf,
            RouteSourceKind.Native => RouteListSourceProvenance.RoutedNative,
            _ => throw new ArgumentOutOfRangeException(nameof(node), source.Kind, "The routed source kind is not defined."),
        };
        var provenance = new RouteListProvenance(
            selectionProvenance,
            sourceProvenance,
            node.Source.Overwrite is not null);
        var metadata = node.Source.Metadata;
        if (source.Kind == RouteSourceKind.Entrypoint)
        {
            return RouteListRow.Entrypoint(
                source.Id,
                source.CanonicalPath,
                parentId,
                node.ParentPath,
                absoluteDepth,
                relativeDepth,
                metadata.Description!,
                metadata.Tags,
                ReadDirectChildCount(node, relativeDepth),
                provenance);
        }

        return RouteListRow.RoutedLeaf(
            source.Id,
            source.CanonicalPath,
            parentId,
            node.ParentPath,
            absoluteDepth,
            relativeDepth,
            metadata.Description!,
            metadata.Tags,
            provenance);
    }

    internal bool CanDescend(RouteTopologyNode node, int relativeDepth)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (node.Source.Kind != RouteSourceKind.Entrypoint)
        {
            return false;
        }

        return _input.Request.RequestedDepth.Kind == RouteListDepthKind.All
            || relativeDepth < _input.Request.RequestedDepth.Value!.Value;
    }

    private int? ReadDirectChildCount(RouteTopologyNode node, int relativeDepth)
    {
        if (!CanDescend(node, relativeDepth)
            || node.ChildPaths.Any(path =>
                _topology.FindByPath(path)!.Source.IsRouteAmbiguous)
            || _input.Inventory.Findings.Any(finding =>
                RouteListTopologyFindingPolicy.AffectsDirectChildren(node, finding)))
        {
            return null;
        }

        return node.ChildPaths.Count;
    }
}
