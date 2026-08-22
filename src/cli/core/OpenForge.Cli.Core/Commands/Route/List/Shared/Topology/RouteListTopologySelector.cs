using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Topology;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

internal sealed class RouteListTopologySelector
{
    internal RouteListTopologySelectionFacts Select(
        RouteListTopologyInput input,
        RouteTopologyFacts topology,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(topology);
        if (!input.LoaderRootPaths.SetEquals(topology.LoaderRootPaths))
        {
            throw new ArgumentException("The topology must retain the input Loader-root boundary.", nameof(topology));
        }

        return new RouteListTopologySelectionAccumulator(input, topology, cancellationToken).Select();
    }
}

internal sealed class RouteListTopologySelectionAccumulator
{
    private readonly RouteListTopologyInput _input;
    private readonly RouteTopologyFacts _topology;
    private readonly RouteListTopologySelectionState _state;
    private readonly RouteListTopologySelectionFindings _findings;
    private readonly RouteListTopologySelectionWalker _walker;
    private readonly RouteListTopologySelectionResultBuilder _resultBuilder;

    internal RouteListTopologySelectionAccumulator(
        RouteListTopologyInput input,
        RouteTopologyFacts topology,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(topology);
        _input = input;
        _topology = topology;
        _state = new RouteListTopologySelectionState(cancellationToken);
        _findings = new RouteListTopologySelectionFindings(input, _state);
        _walker = new RouteListTopologySelectionWalker(input, topology, _state, _findings);
        _resultBuilder = new RouteListTopologySelectionResultBuilder(input, _state);
    }

    internal RouteListTopologySelectionFacts Select()
    {
        _findings.AddSelectionIssues(_input.Selection.Issues);
        if (_input.Selection.State == RouteListSelectionResolutionState.Invalid)
        {
            return _resultBuilder.FormFacts(selectedRootCount: 0);
        }

        var roots = ReadSelectedRoots();
        _state.SetSelectedRootPaths(roots.Select(root => root.Source.CanonicalPath));
        var inventoryFindings = RouteListTopologyFindingPolicy.ReadRelevantInventoryFindings(
            _input,
            _topology,
            roots);
        foreach (var finding in inventoryFindings)
        {
            _state.AddFinding(finding.Finding, finding.RelativeDepth);
        }

        if (_state.CancellationToken.IsCancellationRequested)
        {
            _findings.AddInterruption(_findings.ReadSelectionSubject(), relativeDepth: 0);
            return _resultBuilder.FormFacts(roots.Count);
        }

        foreach (var root in roots)
        {
            if (_state.CancellationToken.IsCancellationRequested)
            {
                _findings.AddInterruption(root.Source.CanonicalPath, relativeDepth: 0);
                break;
            }

            _walker.EnumerateRoot(root);
        }

        return _resultBuilder.FormFacts(roots.Count);
    }

    private IReadOnlyList<RouteTopologyNode> ReadSelectedRoots()
    {
        var roots = new List<RouteTopologyNode>();
        foreach (var source in _input.Selection.SelectedSources)
        {
            var node = _topology.FindByPath(source.CanonicalPath)
                ?? throw new InvalidOperationException("A selected routed source is missing from the immutable topology.");
            roots.Add(node);
        }

        return RouteListTopologyRootOrderer.Order(roots, _topology);
    }
}
