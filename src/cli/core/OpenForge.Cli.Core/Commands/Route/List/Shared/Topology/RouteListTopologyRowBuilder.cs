using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

internal sealed class RouteListTopologyRowBuilder
{
    private readonly RouteListTopologyInput _input;
    private readonly RouteListTopologyFacts _topology;

    internal RouteListTopologyRowBuilder(
        RouteListTopologyInput input,
        RouteListTopologyFacts topology)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(topology);
        _input = input;
        _topology = topology;
    }

    internal RouteListRow Build(
        RouteListTopologyNode node,
        int relativeDepth,
        int? absoluteDepth,
        RouteListSelectionProvenance selectionProvenance)
    {
        ArgumentNullException.ThrowIfNull(node);
        var source = node.Source.Source;
        string? parentId = null;
        if (node.ParentPath is { } parentPath)
        {
            parentId = _topology.FindByPath(parentPath)?.Source.Source.Id
                ?? throw new InvalidOperationException("A resolved topology parent is missing from the immutable graph.");
        }

        var sourceProvenance = source.Kind switch
        {
            RouteListSourceKind.Entrypoint => RouteListSourceProvenance.AuthoredEntrypoint,
            RouteListSourceKind.RoutedLeaf => RouteListSourceProvenance.AuthoredLeaf,
            RouteListSourceKind.RoutedNative => RouteListSourceProvenance.RoutedNative,
            _ => throw new ArgumentOutOfRangeException(nameof(node), source.Kind, "The routed source kind is not defined."),
        };
        var provenance = new RouteListProvenance(
            selectionProvenance,
            sourceProvenance,
            node.Source.Overwrite is not null);
        var metadata = node.Source.Metadata;
        if (source.Kind == RouteListSourceKind.Entrypoint)
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

    internal bool CanDescend(RouteListTopologyNode node, int relativeDepth)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (node.Source.Source.Kind != RouteListSourceKind.Entrypoint)
        {
            return false;
        }

        return _input.Request.RequestedDepth.Kind == RouteListDepthKind.All
            || relativeDepth < _input.Request.RequestedDepth.Value!.Value;
    }

    private int? ReadDirectChildCount(RouteListTopologyNode node, int relativeDepth)
    {
        if (!CanDescend(node, relativeDepth)
            || node.ChildPaths.Any(path =>
                _topology.FindByPath(path)!.Source.Source.IsRouteAmbiguous)
            || _input.Inventory.Findings.Any(finding =>
                RouteListTopologyFindingPolicy.AffectsDirectChildren(node, finding)))
        {
            return null;
        }

        return node.ChildPaths.Count;
    }
}
