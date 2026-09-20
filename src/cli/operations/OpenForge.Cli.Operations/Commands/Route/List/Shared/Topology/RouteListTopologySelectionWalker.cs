using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

internal sealed class RouteListTopologySelectionWalker
{
    private readonly RouteListTopologyInput _input;
    private readonly SourceRouteFacts _routeFacts;
    private readonly SourceRouteTopology _topology;
    private readonly RouteListTopologyRowBuilder _rowBuilder;
    private readonly RouteListTopologySelectionState _state;
    private readonly RouteListTopologySelectionFindings _findings;

    internal RouteListTopologySelectionWalker(
        RouteListTopologyInput input,
        SourceRouteFacts routeFacts,
        RouteListTopologySelectionState state,
        RouteListTopologySelectionFindings findings)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(routeFacts);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(findings);
        _input = input;
        _routeFacts = routeFacts;
        _topology = routeFacts.Topology;
        _rowBuilder = new RouteListTopologyRowBuilder(input, routeFacts);
        _state = state;
        _findings = findings;
    }

    internal void EnumerateRoot(SourceRouteNode root)
    {
        var routeFact = RouteListTopologyProjectionPolicy.ReadRouteFact(_input, _routeFacts, root);
        if (routeFact.State == SourceRouteState.Ambiguous
            || root.ParentState == SourceRouteParentState.Ambiguous)
        {
            _findings.AddRouteAmbiguous(root, relativeDepth: 0, "The selected source has more than one possible routed parent.");
            return;
        }

        if (routeFact.State == SourceRouteState.Unavailable)
        {
            _findings.AddUnavailable(routeFact, relativeDepth: 0);
            return;
        }

        var source = RouteListTopologyProjectionPolicy.ReadSource(_input, root);
        if (source.Kind != RouteSourceKind.Entrypoint
            && root.ParentState == SourceRouteParentState.None)
        {
            _findings.AddUnsupportedSource(source);
            return;
        }

        var absoluteDepth = _topology.ReadAbsoluteDepth(root.Identity.CanonicalBasePath);
        var selectionProvenance = ReadRootProvenance(root, routeFact, absoluteDepth);
        if (selectionProvenance is null)
        {
            _findings.AddLoaderBoundaryIssues();
            return;
        }

        EnumerateNode(root, relativeDepth: 0, absoluteDepth, selectionProvenance.Value);
    }

    private RouteListSelectionProvenance? ReadRootProvenance(
        SourceRouteNode root,
        SourceRouteFact routeFact,
        int? absoluteDepth)
    {
        if (_input.Selection.Selection.Kind == RouteListSelectionKind.LoaderRoots)
        {
            if (!_input.LoaderRootPaths.Contains(root.Identity.CanonicalBasePath)
                || routeFact.State != SourceRouteState.Routed)
            {
                throw new InvalidOperationException("A Loader-root selection contains a source outside its Loader-root boundary.");
            }

            return RouteListSelectionProvenance.LoaderRoot;
        }

        if (routeFact.State == SourceRouteState.Routed)
        {
            if (absoluteDepth is null)
            {
                throw new InvalidOperationException("A routed source route fact requires an absolute Loader depth.");
            }

            return RouteListSelectionProvenance.ExplicitRoot;
        }

        if (routeFact.State != SourceRouteState.Unrouted || absoluteDepth is not null)
        {
            throw new InvalidOperationException("A detached source route fact must be unrouted and have no absolute Loader depth.");
        }

        return _input.LoaderRootBoundaryIsComplete
            ? RouteListSelectionProvenance.DetachedRoot
            : null;
    }

    private void EnumerateNode(
        SourceRouteNode node,
        int relativeDepth,
        int? absoluteDepth,
        RouteListSelectionProvenance rootProvenance)
    {
        if (_state.CancellationToken.IsCancellationRequested)
        {
            _findings.AddInterruption(node.Identity.CanonicalBasePath, relativeDepth);
            return;
        }

        var path = node.Identity.CanonicalBasePath;
        if (!_state.TryVisit(path))
        {
            return;
        }

        var routeFact = RouteListTopologyProjectionPolicy.ReadRouteFact(_input, _routeFacts, node);
        if (routeFact.State == SourceRouteState.Ambiguous
            || node.ParentState == SourceRouteParentState.Ambiguous)
        {
            _findings.AddRouteAmbiguous(node, relativeDepth, "The route has more than one possible routed parent.");
            return;
        }

        if (routeFact.State == SourceRouteState.Unavailable)
        {
            _findings.AddUnavailable(routeFact, relativeDepth);
            return;
        }

        if ((routeFact.State == SourceRouteState.Routed) != (absoluteDepth is not null))
        {
            throw new InvalidOperationException("Routed and unrouted source facts must preserve their absolute Loader-depth distinction.");
        }

        if (RouteListTopologyProjectionPolicy.IsAmbiguousEntrypoint(_input, node))
        {
            _findings.AddRouteAmbiguous(node, relativeDepth, "The route folder contains more than one recognized entrypoint.");
            return;
        }

        var source = RouteListTopologyProjectionPolicy.FindSource(_input, node);
        if (source is null)
        {
            _state.MarkUnresolved(relativeDepth);
            return;
        }

        _findings.InspectIdentity(node, routeFact, relativeDepth);
        if (source.Metadata.State != RouteSourceMetadataState.Complete)
        {
            _findings.EnsureMetadataFinding(node, relativeDepth);
            if (source.Metadata.State is not (
                    RouteSourceMetadataState.Missing
                    or RouteSourceMetadataState.Malformed))
            {
                return;
            }
        }

        var provenance = relativeDepth == 0
            ? rootProvenance
            : RouteListSelectionProvenance.Descendant;
        _state.AddRow(_rowBuilder.Build(node, relativeDepth, absoluteDepth, provenance));
        if (!_rowBuilder.CanDescend(node, relativeDepth))
        {
            return;
        }

        foreach (var childPath in node.ChildPaths)
        {
            if (_input.Selection.Selection.Kind == RouteListSelectionKind.LoaderRoots
                && _state.IsSelectedRoot(childPath))
            {
                var selectedChild = _topology.FindByPath(childPath)
                    ?? throw new InvalidOperationException("A selected topology child is missing from the immutable source graph.");
                EnumerateNode(
                    selectedChild,
                    relativeDepth: 0,
                    _topology.ReadAbsoluteDepth(childPath),
                    RouteListSelectionProvenance.LoaderRoot);
                continue;
            }

            var child = _topology.FindByPath(childPath)
                ?? throw new InvalidOperationException("A topology child is missing from the immutable source graph.");
            int? childAbsoluteDepth = absoluteDepth is null
                ? null
                : checked(absoluteDepth.Value + 1);
            EnumerateNode(
                child,
                checked(relativeDepth + 1),
                childAbsoluteDepth,
                rootProvenance);
        }
    }
}
