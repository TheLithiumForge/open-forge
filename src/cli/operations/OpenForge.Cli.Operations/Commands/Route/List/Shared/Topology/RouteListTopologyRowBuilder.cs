using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

internal sealed class RouteListTopologyRowBuilder
{
    private readonly RouteListTopologyInput _input;
    private readonly SourceRouteFacts _routeFacts;
    private readonly SourceRouteTopology _topology;

    internal RouteListTopologyRowBuilder(
        RouteListTopologyInput input,
        SourceRouteFacts routeFacts)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(routeFacts);
        _input = input;
        _routeFacts = routeFacts;
        _topology = routeFacts.Topology;
    }

    internal RouteListRow Build(
        SourceRouteNode node,
        int relativeDepth,
        int? absoluteDepth,
        RouteListSelectionProvenance selectionProvenance)
    {
        var source = RouteListTopologyProjectionPolicy.ReadSource(_input, node);
        string? parentId = null;
        string? parentPath = null;
        if (node.ParentState == SourceRouteParentState.Resolved)
        {
            parentPath = node.ParentPaths[0];
            var parent = _topology.FindByPath(parentPath)
                ?? throw new InvalidOperationException("A resolved topology parent is missing from the immutable source graph.");
            parentId = RouteListTopologyProjectionPolicy.ReadSource(_input, parent).Id;
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
            source.Overwrite is not null);
        var metadata = source.Metadata;
        if (metadata.State is not (
                RouteSourceMetadataState.Complete
                or RouteSourceMetadataState.Missing
                or RouteSourceMetadataState.Malformed))
        {
            throw new InvalidOperationException(
                "A route-list row requires complete or known-incomplete source metadata.");
        }

        var description = metadata.Description ?? "(no description)";

        if (source.Kind == RouteSourceKind.Entrypoint)
        {
            return RouteListRow.Entrypoint(
                source.Id,
                source.CanonicalPath,
                parentId,
                parentPath,
                absoluteDepth,
                relativeDepth,
                description,
                metadata.Tags,
                ReadDirectChildCount(node, relativeDepth),
                provenance);
        }

        return RouteListRow.RoutedLeaf(
            source.Id,
            source.CanonicalPath,
            parentId,
            parentPath,
            absoluteDepth,
            relativeDepth,
            description,
            metadata.Tags,
            provenance);
    }

    internal bool CanDescend(SourceRouteNode node, int relativeDepth)
    {
        var source = RouteListTopologyProjectionPolicy.FindSource(_input, node);
        if (source?.Kind != RouteSourceKind.Entrypoint)
        {
            return false;
        }

        var requestedDepth = _input.Request.RequestedDepth;
        return requestedDepth.Kind == RouteListDepthKind.All
            || relativeDepth < requestedDepth.FiniteValue;
    }

    private int? ReadDirectChildCount(SourceRouteNode node, int relativeDepth)
    {
        var source = RouteListTopologyProjectionPolicy.ReadSource(_input, node);
        if (!CanDescend(node, relativeDepth))
        {
            return node.ChildPaths.Count;
        }

        if (node.ChildPaths.Any(path =>
            {
                var child = _topology.FindByPath(path)
                    ?? throw new InvalidOperationException("A topology child is missing from the immutable source graph.");
                var childFact = RouteListTopologyProjectionPolicy.ReadRouteFact(_input, _routeFacts, child);
                return childFact.State is SourceRouteState.Ambiguous or SourceRouteState.Unavailable
                    || RouteListTopologyProjectionPolicy.FindSource(_input, child) is null
                    || RouteListTopologyProjectionPolicy.IsAmbiguousEntrypoint(_input, child);
            })
            || _input.Inventory.Findings.Any(finding =>
                RouteListTopologyFindingPolicy.AffectsDirectChildren(node, source, finding)))
        {
            return null;
        }

        return node.ChildPaths.Count;
    }
}
