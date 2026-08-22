using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Topology;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

internal sealed class RouteListTopologySelectionWalker
{
    private readonly RouteListTopologyInput _input;
    private readonly RouteTopologyFacts _topology;
    private readonly RouteListTopologyRowBuilder _rowBuilder;
    private readonly RouteListTopologySelectionState _state;
    private readonly RouteListTopologySelectionFindings _findings;

    internal RouteListTopologySelectionWalker(
        RouteListTopologyInput input,
        RouteTopologyFacts topology,
        RouteListTopologySelectionState state,
        RouteListTopologySelectionFindings findings)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(findings);
        _input = input;
        _topology = topology;
        _rowBuilder = new RouteListTopologyRowBuilder(input, topology);
        _state = state;
        _findings = findings;
    }

    internal void EnumerateRoot(RouteTopologyNode root)
    {
        if (root.ParentState == RouteTopologyParentState.Ambiguous)
        {
            _findings.AddRouteAmbiguous(root, relativeDepth: 0, "The selected source has more than one possible routed parent.");
            return;
        }

        if (root.Source.Kind != RouteSourceKind.Entrypoint
            && root.ParentState == RouteTopologyParentState.None)
        {
            _state.AddFinding(
                new RouteListFinding(
                    RouteListFindingCode.UnsupportedSource,
                    CliSemanticStatus.Invalid,
                    root.Source.CanonicalPath,
                    "The selected source is not a routed sibling of an entrypoint."),
                relativeDepth: 0);
            return;
        }

        var absoluteDepth = _topology.ReadAbsoluteDepth(root.Source.CanonicalPath);
        var selectionProvenance = ReadRootProvenance(root, absoluteDepth);
        if (selectionProvenance is null)
        {
            _findings.AddLoaderBoundaryIssues();
            return;
        }

        EnumerateNode(root, relativeDepth: 0, absoluteDepth, selectionProvenance.Value);
    }

    private RouteListSelectionProvenance? ReadRootProvenance(
        RouteTopologyNode root,
        int? absoluteDepth)
    {
        if (_input.Selection.Selection.Kind == RouteListSelectionKind.LoaderRoots)
        {
            if (!_input.LoaderRootPaths.Contains(root.Source.CanonicalPath))
            {
                throw new InvalidOperationException("A Loader-root selection contains a source outside its Loader-root boundary.");
            }

            return RouteListSelectionProvenance.LoaderRoot;
        }

        if (absoluteDepth is not null)
        {
            return RouteListSelectionProvenance.ExplicitRoot;
        }

        return _input.LoaderRootBoundaryIsComplete
            ? RouteListSelectionProvenance.DetachedRoot
            : null;
    }

    private void EnumerateNode(
        RouteTopologyNode node,
        int relativeDepth,
        int? absoluteDepth,
        RouteListSelectionProvenance rootProvenance)
    {
        if (_state.CancellationToken.IsCancellationRequested)
        {
            _findings.AddInterruption(node.Source.CanonicalPath, relativeDepth);
            return;
        }

        var path = node.Source.CanonicalPath;
        if (!_state.TryVisit(path))
        {
            return;
        }

        if (node.ParentState == RouteTopologyParentState.Ambiguous)
        {
            _findings.AddRouteAmbiguous(node, relativeDepth, "The route has more than one possible routed parent.");
            return;
        }

        if (node.Source.IsRouteAmbiguous)
        {
            _findings.AddRouteAmbiguous(node, relativeDepth, "The route folder contains more than one recognized entrypoint.");
            return;
        }

        _findings.InspectIdentity(node, relativeDepth);
        if (!node.HasCompleteMetadata)
        {
            _findings.EnsureMetadataFinding(node, relativeDepth);
            return;
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
                    ?? throw new InvalidOperationException("A selected topology child is missing from the immutable graph.");
                EnumerateNode(
                    selectedChild,
                    relativeDepth: 0,
                    _topology.ReadAbsoluteDepth(childPath),
                    RouteListSelectionProvenance.LoaderRoot);
                continue;
            }

            var child = _topology.FindByPath(childPath)
                ?? throw new InvalidOperationException("A topology child is missing from the immutable graph.");
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
