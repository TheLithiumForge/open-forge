using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

internal sealed class RouteListTopologySelector
{
    internal RouteListTopologySelectionFacts SelectSources(
        RouteListTopologyInput input,
        SourceRouteFacts routeFacts,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(routeFacts);
        RouteListTopologyProjectionPolicy.Validate(input, routeFacts);
        return new RouteListTopologySelectionAccumulator(input, routeFacts, cancellationToken).Select();
    }
}

internal sealed class RouteListTopologySelectionAccumulator
{
    private readonly RouteListTopologyInput _input;
    private readonly SourceRouteFacts _routeFacts;
    private readonly SourceRouteTopology _topology;
    private readonly RouteListTopologySelectionState _state;
    private readonly RouteListTopologySelectionFindings _findings;
    private readonly RouteListTopologySelectionWalker _walker;
    private readonly RouteListTopologySelectionResultBuilder _resultBuilder;

    internal RouteListTopologySelectionAccumulator(
        RouteListTopologyInput input,
        SourceRouteFacts routeFacts,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(routeFacts);
        _input = input;
        _routeFacts = routeFacts;
        _topology = routeFacts.Topology;
        _state = new RouteListTopologySelectionState(cancellationToken);
        _findings = new RouteListTopologySelectionFindings(input, routeFacts, _state);
        _walker = new RouteListTopologySelectionWalker(input, routeFacts, _state, _findings);
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
        _state.SetSelectedRootPaths(roots.Select(root => root.Identity.CanonicalBasePath));
        var inventoryFindings = RouteListTopologyFindingPolicy.ReadRelevantInventoryFindings(
            _input,
            _topology,
            roots);
        foreach (var finding in inventoryFindings)
        {
            _state.AddFinding(finding.Finding, finding.RelativeDepth);
        }

        if (_routeFacts.IsCancelled
            && !_state.Findings.Any(finding => finding.Status == CliSemanticStatus.Interrupted))
        {
            _findings.AddInterruption(_findings.ReadSelectionSubject(), relativeDepth: 0);
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
                _findings.AddInterruption(root.Identity.CanonicalBasePath, relativeDepth: 0);
                break;
            }

            _walker.EnumerateRoot(root);
        }

        return _resultBuilder.FormFacts(roots.Count);
    }

    private IReadOnlyList<SourceRouteNode> ReadSelectedRoots()
    {
        var roots = new List<SourceRouteNode>();
        foreach (var source in _input.Selection.SelectedSources)
        {
            var routeFact = RouteListTopologyProjectionPolicy.ReadRouteFact(_input, _routeFacts, source);
            var node = _topology.FindByPath(source.CanonicalPath);
            if (node is null)
            {
                if (routeFact.State == SourceRouteState.Unrouted)
                {
                    _findings.AddUnsupportedSource(source);
                    continue;
                }

                if (routeFact.State == SourceRouteState.Unavailable)
                {
                    _findings.AddUnavailable(routeFact, relativeDepth: 0);
                    continue;
                }

                throw new InvalidOperationException("A routed or ambiguous selected source is missing from the immutable source topology.");
            }

            var projected = RouteListTopologyProjectionPolicy.ReadSource(_input, node);
            if (!ReferenceEquals(projected, source))
            {
                throw new InvalidOperationException("A selected topology root must retain its exact Route source projection.");
            }

            roots.Add(node);
        }

        return RouteListTopologyRootOrderer.Order(roots, _topology);
    }
}

internal static class RouteListTopologyProjectionPolicy
{
    internal static void Validate(
        RouteListTopologyInput input,
        SourceRouteFacts routeFacts)
    {
        if (input.LoaderRootSelection is not null
            && !input.LoaderRootPaths.SetEquals(routeFacts.Topology.LoaderRootPaths))
        {
            throw new ArgumentException("The source topology must retain the input Loader-root boundary.", nameof(routeFacts));
        }

        if (!routeFacts.AreLoaderRootFactsComplete
            && input.LoaderRootSelection?.State == RouteListSelectionResolutionState.Resolved)
        {
            throw new ArgumentException("Incomplete neutral Loader facts require an incomplete command Loader boundary.", nameof(routeFacts));
        }

        foreach (var node in routeFacts.Topology.Nodes)
        {
            _ = ReadProjection(input, node);
            _ = ReadRouteFact(input, routeFacts, node);
        }

        foreach (var fact in routeFacts.RouteFacts)
        {
            var projection = FindProjection(input, fact.Identity.CanonicalBasePath)
                ?? throw new ArgumentException("Every source route fact must retain one Route projection association.", nameof(routeFacts));
            if (!ReferenceEquals(projection.LogicalSource.Identity, fact.Identity))
            {
                throw new ArgumentException("Source route facts must retain the projected neutral identities.", nameof(routeFacts));
            }
        }

        foreach (var source in input.Selection.SelectedSources)
        {
            var retained = input.Inventory.ProjectionBuildResult.ProjectionSet.FindByPath(source.CanonicalPath);
            if (!ReferenceEquals(retained, source))
            {
                throw new ArgumentException("Selected topology sources must be exact Route projection-set members.", nameof(input));
            }

            _ = ReadRouteFact(input, routeFacts, source);
        }
    }

    internal static RouteSourceProjection ReadProjection(
        RouteListTopologyInput input,
        SourceRouteNode node)
    {
        var projection = FindProjection(input, node.Identity.CanonicalBasePath)
            ?? throw new InvalidOperationException("A source topology node is missing its Route projection association.");
        if (!ReferenceEquals(projection.LogicalSource.Identity, node.Identity))
        {
            throw new InvalidOperationException("A source topology node must retain its exact projected neutral identity.");
        }

        return projection;
    }

    internal static RouteSource? FindSource(
        RouteListTopologyInput input,
        SourceRouteNode node)
    {
        var projection = ReadProjection(input, node);
        var source = projection.Source;
        if (source is not null
            && !ReferenceEquals(
                input.Inventory.ProjectionBuildResult.ProjectionSet.FindByPath(node.Identity.CanonicalBasePath),
                source))
        {
            throw new InvalidOperationException("A source topology node must map to the Route source retained by its projection set.");
        }

        return source;
    }

    internal static RouteSource ReadSource(
        RouteListTopologyInput input,
        SourceRouteNode node)
    {
        return FindSource(input, node)
            ?? throw new InvalidOperationException("Route-list row policy requires an available Route source projection.");
    }

    internal static SourceRouteFact ReadRouteFact(
        RouteListTopologyInput input,
        SourceRouteFacts routeFacts,
        SourceRouteNode node)
    {
        var fact = ReadRouteFact(input, routeFacts, node.Identity.CanonicalBasePath);
        if (!ReferenceEquals(fact.Identity, node.Identity))
        {
            throw new InvalidOperationException("A source topology node must retain its exact neutral route fact identity.");
        }

        return fact;
    }

    internal static SourceRouteFact ReadRouteFact(
        RouteListTopologyInput input,
        SourceRouteFacts routeFacts,
        RouteSource source)
    {
        var projection = FindProjection(input, source.CanonicalPath)
            ?? throw new InvalidOperationException("A selected Route source is missing its projection association.");
        if (!ReferenceEquals(projection.Source, source))
        {
            throw new InvalidOperationException("A selected Route source must be the exact source retained by its projection.");
        }

        var fact = ReadRouteFact(input, routeFacts, source.CanonicalPath);
        if (!ReferenceEquals(fact.Identity, projection.LogicalSource.Identity))
        {
            throw new InvalidOperationException("A selected Route source must retain its exact neutral route fact identity.");
        }

        return fact;
    }

    internal static bool IsAmbiguousEntrypoint(
        RouteListTopologyInput input,
        SourceRouteNode node)
    {
        var projection = ReadProjection(input, node);
        if (!SourceFormClassifier.IsEntrypoint(projection.LogicalSource.Base.Form))
        {
            return false;
        }

        var directory = SourceLogicalPath.ReadParent(
            projection.LogicalSource.Identity.CanonicalBasePath);
        return input.Inventory.ProjectionBuildResult.ProjectionSet.Projections.Count(candidate =>
            SourceFormClassifier.IsEntrypoint(candidate.LogicalSource.Base.Form)
            && string.Equals(
                SourceLogicalPath.ReadParent(
                    candidate.LogicalSource.Identity.CanonicalBasePath),
                directory,
                StringComparison.Ordinal)) > 1;
    }

    private static RouteSourceProjection? FindProjection(
        RouteListTopologyInput input,
        string canonicalPath)
    {
        return input.Inventory.ProjectionBuildResult.ProjectionSet.Projections.SingleOrDefault(projection =>
            string.Equals(
                projection.LogicalSource.Identity.CanonicalBasePath,
                canonicalPath,
                StringComparison.Ordinal));
    }

    private static SourceRouteFact ReadRouteFact(
        RouteListTopologyInput input,
        SourceRouteFacts routeFacts,
        string canonicalPath)
    {
        return routeFacts.RouteFacts.SingleOrDefault(fact => string.Equals(
                fact.Identity.CanonicalBasePath,
                canonicalPath,
                StringComparison.Ordinal))
            ?? throw new InvalidOperationException("A projected source is missing its neutral route fact.");
    }
}
