using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

internal sealed class RouteListTopologySelector
{
    internal RouteListTopologySelectionFacts Select(
        RouteListTopologyInput input,
        RouteListTopologyFacts topology,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(topology);
        if (!input.LoaderRootPaths.SetEquals(topology.LoaderRootPaths))
        {
            throw new ArgumentException("The topology must retain the input Loader-root boundary.", nameof(topology));
        }

        var accumulator = new RouteListTopologySelectionAccumulator(input, topology, cancellationToken);
        return accumulator.Select();
    }
}

internal sealed class RouteListTopologySelectionAccumulator
{
    private readonly RouteListTopologyInput _input;
    private readonly RouteListTopologyFacts _topology;
    private readonly RouteListTopologyRowBuilder _rowBuilder;
    private readonly CancellationToken _cancellationToken;
    private readonly List<RouteListRow> _rows = [];
    private readonly List<RouteListFinding> _findings = [];
    private readonly Dictionary<string, string> _selectedPathsById = new(StringComparer.Ordinal);
    private readonly HashSet<string> _visitedPaths = new(StringComparer.Ordinal);
    private IReadOnlySet<string> _selectedRootPaths = new HashSet<string>(StringComparer.Ordinal);
    private int? _firstUnresolvedDepth;

    internal RouteListTopologySelectionAccumulator(
        RouteListTopologyInput input,
        RouteListTopologyFacts topology,
        CancellationToken cancellationToken)
    {
        _input = input;
        _topology = topology;
        _rowBuilder = new RouteListTopologyRowBuilder(input, topology);
        _cancellationToken = cancellationToken;
    }

    internal RouteListTopologySelectionFacts Select()
    {
        AddSelectionIssues(_input.Selection.Issues);
        if (_input.Selection.State == RouteListSelectionResolutionState.Invalid)
        {
            return FormFacts(selectedRootCount: 0);
        }

        var roots = ReadSelectedRoots();
        _selectedRootPaths = roots
            .Select(root => root.Source.Source.CanonicalPath)
            .ToHashSet(StringComparer.Ordinal);
        var inventoryFindings = RouteListTopologyFindingPolicy.ReadRelevantInventoryFindings(
            _input,
            _topology,
            roots);
        foreach (var finding in inventoryFindings)
        {
            AddFinding(finding.Finding, finding.RelativeDepth);
        }

        if (_cancellationToken.IsCancellationRequested)
        {
            AddInterruption(ReadSelectionSubject(), relativeDepth: 0);
            return FormFacts(roots.Count);
        }

        foreach (var root in roots)
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                AddInterruption(root.Source.Source.CanonicalPath, relativeDepth: 0);
                break;
            }

            EnumerateRoot(root);
        }

        return FormFacts(roots.Count);
    }

    private IReadOnlyList<RouteListTopologyNode> ReadSelectedRoots()
    {
        var roots = new List<RouteListTopologyNode>();
        foreach (var source in _input.Selection.SelectedSources)
        {
            var node = _topology.FindByPath(source.CanonicalPath)
                ?? throw new InvalidOperationException("A selected routed source is missing from the immutable topology.");
            roots.Add(node);
        }

        return OrderRootsParentFirst(roots);
    }

    private void EnumerateRoot(RouteListTopologyNode root)
    {
        if (root.ParentState == RouteListTopologyParentState.Ambiguous)
        {
            AddRouteAmbiguous(root, relativeDepth: 0, "The selected source has more than one possible routed parent.");
            return;
        }

        if (root.Source.Source.Kind != RouteListSourceKind.Entrypoint
            && root.ParentState == RouteListTopologyParentState.None)
        {
            AddFinding(
                new RouteListFinding(
                    RouteListFindingCode.UnsupportedSource,
                    CliSemanticStatus.Invalid,
                    root.Source.Source.CanonicalPath,
                    "The selected source is not a routed sibling of an entrypoint."),
                relativeDepth: 0);
            return;
        }

        var absoluteDepth = _topology.ReadAbsoluteDepth(root.Source.Source.CanonicalPath);
        var selectionProvenance = ReadRootProvenance(root, absoluteDepth);
        if (selectionProvenance is null)
        {
            AddLoaderBoundaryIssues();
            return;
        }

        EnumerateNode(root, relativeDepth: 0, absoluteDepth, selectionProvenance.Value);
    }

    private RouteListSelectionProvenance? ReadRootProvenance(
        RouteListTopologyNode root,
        int? absoluteDepth)
    {
        if (_input.Selection.Selection.Kind == RouteListSelectionKind.LoaderRoots)
        {
            if (!_input.LoaderRootPaths.Contains(root.Source.Source.CanonicalPath))
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
        RouteListTopologyNode node,
        int relativeDepth,
        int? absoluteDepth,
        RouteListSelectionProvenance rootProvenance)
    {
        if (_cancellationToken.IsCancellationRequested)
        {
            AddInterruption(node.Source.Source.CanonicalPath, relativeDepth);
            return;
        }

        var path = node.Source.Source.CanonicalPath;
        if (!_visitedPaths.Add(path))
        {
            return;
        }

        if (node.ParentState == RouteListTopologyParentState.Ambiguous)
        {
            AddRouteAmbiguous(node, relativeDepth, "The route has more than one possible routed parent.");
            return;
        }

        if (node.Source.Source.IsRouteAmbiguous)
        {
            AddRouteAmbiguous(node, relativeDepth, "The route folder contains more than one recognized entrypoint.");
            return;
        }

        InspectIdentity(node, relativeDepth);
        if (!node.HasCompleteMetadata)
        {
            EnsureMetadataFinding(node, relativeDepth);
            return;
        }

        var provenance = relativeDepth == 0
            ? rootProvenance
            : RouteListSelectionProvenance.Descendant;
        _rows.Add(_rowBuilder.Build(node, relativeDepth, absoluteDepth, provenance));
        if (!_rowBuilder.CanDescend(node, relativeDepth))
        {
            return;
        }

        foreach (var childPath in node.ChildPaths)
        {
            if (_input.Selection.Selection.Kind == RouteListSelectionKind.LoaderRoots
                && _selectedRootPaths.Contains(childPath))
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

    private void InspectIdentity(RouteListTopologyNode node, int relativeDepth)
    {
        var source = node.Source.Source;
        var collision = _topology.IdentityCollisions.SingleOrDefault(candidate =>
            candidate.Paths.Contains(source.CanonicalPath, StringComparer.Ordinal));
        if (collision is not null
            && !_selectedPathsById.ContainsKey(source.Id))
        {
            AddFinding(
                new RouteListFinding(
                    RouteListFindingCode.IdentityCollision,
                    CliSemanticStatus.Attention,
                    source.CanonicalPath,
                    $"The source ID also identifies {string.Join(", ", collision.Paths.Where(path => !string.Equals(path, source.CanonicalPath, StringComparison.Ordinal)))}."),
                relativeDepth);
        }

        if (_selectedPathsById.TryGetValue(source.Id, out var firstPath)
            && !string.Equals(firstPath, source.CanonicalPath, StringComparison.Ordinal))
        {
            AddFinding(
                new RouteListFinding(
                    RouteListFindingCode.IdentityCollision,
                    CliSemanticStatus.Blocked,
                    source.CanonicalPath,
                    $"The selected route ID also identifies {firstPath}."),
                relativeDepth);
            return;
        }

        _selectedPathsById[source.Id] = source.CanonicalPath;
    }

    private void EnsureMetadataFinding(RouteListTopologyNode node, int relativeDepth)
    {
        var source = node.Source.Source;
        if (_findings.Any(finding =>
                string.Equals(finding.Subject, source.CanonicalPath, StringComparison.Ordinal)
                && finding.Code is (
                    RouteListFindingCode.MetadataMissing
                    or RouteListFindingCode.MetadataMalformed
                    or RouteListFindingCode.ReadUnavailable)))
        {
            MarkUnresolved(relativeDepth);
            return;
        }

        var (code, cause) = node.Source.Metadata.State switch
        {
            RouteListMetadataState.Missing => (
                RouteListFindingCode.MetadataMissing,
                "Required source metadata is missing."),
            RouteListMetadataState.Malformed => (
                RouteListFindingCode.MetadataMalformed,
                "The source frontmatter or metadata shape is malformed."),
            RouteListMetadataState.ReadUnavailable => (
                RouteListFindingCode.ReadUnavailable,
                "The source metadata could not be read."),
            _ => (
                RouteListFindingCode.MetadataMalformed,
                "The source does not contain complete route metadata."),
        };
        AddFinding(
            new RouteListFinding(
                code,
                CliSemanticStatus.Incomplete,
                source.CanonicalPath,
                cause),
            relativeDepth);
    }

    private void AddSelectionIssues(IEnumerable<RouteListSelectionIssue> issues)
    {
        foreach (var issue in issues)
        {
            AddFinding(RouteListTopologyFindingPolicy.FromSelectionIssue(issue), relativeDepth: 0);
        }
    }

    private void AddLoaderBoundaryIssues()
    {
        var loaderSelection = _input.LoaderRootSelection
            ?? throw new InvalidOperationException("The Loader-root boundary is unavailable for a resolved explicit selection.");
        if (loaderSelection.Issues.Count == 0)
        {
            throw new InvalidOperationException("An incomplete Loader-root boundary requires a typed issue.");
        }

        AddSelectionIssues(loaderSelection.Issues);
    }

    private void AddRouteAmbiguous(
        RouteListTopologyNode node,
        int relativeDepth,
        string cause)
    {
        AddFinding(
            new RouteListFinding(
                RouteListFindingCode.RouteAmbiguous,
                CliSemanticStatus.Blocked,
                node.Source.Source.CanonicalPath,
                cause,
                node.ParentState == RouteListTopologyParentState.Ambiguous
                    ? node.ParentPaths
                    : null),
            relativeDepth);
    }

    private void AddInterruption(string subject, int relativeDepth)
    {
        AddFinding(
            new RouteListFinding(
                RouteListFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                subject,
                "Route topology selection was interrupted."),
            relativeDepth);
    }

    private void AddFinding(RouteListFinding finding, int relativeDepth)
    {
        _findings.Add(finding);
        if (finding.Status != CliSemanticStatus.Attention)
        {
            MarkUnresolved(relativeDepth);
        }
    }

    private void MarkUnresolved(int relativeDepth)
    {
        _firstUnresolvedDepth = _firstUnresolvedDepth is null
            ? relativeDepth
            : Math.Min(_firstUnresolvedDepth.Value, relativeDepth);
    }

    private RouteListTopologySelectionFacts FormFacts(int selectedRootCount)
    {
        var orderedFindings = RouteListTopologyFindingPolicy.OrderDistinct(_findings);
        var status = RouteListTopologyFindingPolicy.ReadAggregateStatus(
            _input.Selection.State,
            orderedFindings);
        selectedRootCount = status == CliSemanticStatus.Invalid
            ? 0
            : selectedRootCount;
        var statusRule = RouteListDefinitions.ReadFindingResultStatusRule(status);
        RouteListFinding[] findings = statusRule.RequiredFindingStatus is null
            ? []
            : orderedFindings
                .Where(finding => statusRule.AllowedFindingStatuses.Contains(finding.Status))
                .ToArray();
        var rows = status == CliSemanticStatus.Invalid
            ? []
            : _rows.ToArray();
        var effectiveDepth = ReadEffectiveDepth(status);
        string[] evidence = status is CliSemanticStatus.Complete or CliSemanticStatus.Attention
            ? ["The selected route roots and requested structural depth were confirmed."]
            : [$"{rows.Length} safe route rows were confirmed."];
        var unresolvedBoundaries = findings
            .Where(finding => finding.Status != CliSemanticStatus.Attention)
            .Select(RouteListTopologyFindingPolicy.ReadBoundary)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return new RouteListTopologySelectionFacts(
            status,
            _input.Selection.Selection,
            selectedRootCount,
            effectiveDepth,
            rows,
            findings,
            evidence,
            unresolvedBoundaries);
    }

    private RouteListDepth? ReadEffectiveDepth(CliSemanticStatus status)
    {
        if (status is CliSemanticStatus.Complete or CliSemanticStatus.Attention)
        {
            return _input.Request.RequestedDepth;
        }

        if (status is CliSemanticStatus.Invalid or CliSemanticStatus.Failed
            || _firstUnresolvedDepth is null or <= 0)
        {
            return null;
        }

        var completeDepth = _firstUnresolvedDepth.Value - 1;
        if (_input.Request.RequestedDepth is { Kind: RouteListDepthKind.Finite, Value: { } requested })
        {
            completeDepth = Math.Min(completeDepth, requested);
        }

        return RouteListDepth.Finite(completeDepth);
    }

    private string ReadSelectionSubject()
    {
        return _input.Selection.Selection.AttemptedId
            ?? _input.Selection.Selection.AttemptedPath
            ?? ".agents/loader.md";
    }

    private IReadOnlyList<RouteListTopologyNode> OrderRootsParentFirst(
        IReadOnlyList<RouteListTopologyNode> roots)
    {
        var selectedByPath = roots.ToDictionary(
            root => root.Source.Source.CanonicalPath,
            StringComparer.Ordinal);
        var childrenBySelectedParent = new Dictionary<string, List<RouteListTopologyNode>>(StringComparer.Ordinal);
        var topLevel = new List<RouteListTopologyNode>();
        foreach (var root in roots)
        {
            var selectedParent = ReadNearestSelectedParent(root, selectedByPath);
            if (selectedParent is null)
            {
                topLevel.Add(root);
                continue;
            }

            if (!childrenBySelectedParent.TryGetValue(selectedParent, out var children))
            {
                children = [];
                childrenBySelectedParent.Add(selectedParent, children);
            }

            children.Add(root);
        }

        var ordered = new List<RouteListTopologyNode>(roots.Count);
        foreach (var root in topLevel.OrderBy(
            node => node.Source.Source.CanonicalPath,
            StringComparer.Ordinal))
        {
            AddRootAndSelectedDescendants(root, childrenBySelectedParent, ordered);
        }

        return ordered;
    }

    private string? ReadNearestSelectedParent(
        RouteListTopologyNode root,
        IReadOnlyDictionary<string, RouteListTopologyNode> selectedByPath)
    {
        var current = root;
        while (current.ParentState == RouteListTopologyParentState.Resolved)
        {
            var parentPath = current.ParentPath!;
            if (selectedByPath.ContainsKey(parentPath))
            {
                return parentPath;
            }

            current = _topology.FindByPath(parentPath)!;
        }

        return null;
    }

    private static void AddRootAndSelectedDescendants(
        RouteListTopologyNode root,
        IReadOnlyDictionary<string, List<RouteListTopologyNode>> childrenBySelectedParent,
        ICollection<RouteListTopologyNode> ordered)
    {
        ordered.Add(root);
        var path = root.Source.Source.CanonicalPath;
        if (!childrenBySelectedParent.TryGetValue(path, out var children))
        {
            return;
        }

        foreach (var child in children.OrderBy(
            node => node.Source.Source.CanonicalPath,
            StringComparer.Ordinal))
        {
            AddRootAndSelectedDescendants(child, childrenBySelectedParent, ordered);
        }
    }
}
