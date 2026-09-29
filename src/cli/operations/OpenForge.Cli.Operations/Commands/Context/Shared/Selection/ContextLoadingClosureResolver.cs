using OpenForge.Cli.Core.Commands.Context.Models.Operation;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Selection;

internal sealed class ContextLoadingClosureResolver
{
    private readonly List<ContextFinding> _findings = [];
    private readonly List<ContextPendingCondition> _pendingConditions = [];
    private readonly HashSet<string> _pendingConditionSources = new(StringComparer.Ordinal);
    private IReadOnlyList<string> _workingPaths = [];
    private bool _workingPathsSupplied;
    private bool _incomplete;
    private bool _blocked;

    internal ContextLoadingClosureResolution Resolve(
        ContextGraph graph,
        IReadOnlyList<ContextResolvedRequest> requested,
        ContextSelectionAccumulator startup,
        IReadOnlyList<string>? workingPaths = null,
        bool workingPathsSupplied = false)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(requested);
        ArgumentNullException.ThrowIfNull(startup);
        _workingPaths = workingPaths ?? [];
        _workingPathsSupplied = workingPathsSupplied;
        AddRootFindings(graph);
        ResolveStartup(graph, startup);
        var combined = startup.Clone();
        ResolveExplicit(requested, graph, combined);
        return new ContextLoadingClosureResolution
        {
            Selection = new ContextLoadingSelection
            {
                StartupSources = startup.Sources,
                CombinedSources = combined.Sources,
            },
            Findings = _findings.ToArray(),
            PendingConditions = _pendingConditions.ToArray(),
            Incomplete = _incomplete,
            Blocked = _blocked,
        };
    }

    private void ResolveStartup(
        ContextGraph graph,
        ContextSelectionAccumulator selected)
    {
        var loading = new SourceLoadingClosureResolver().Resolve(new SourceLoadingClosureRequest
        {
            WorkspaceEntryPath = graph.WorkspaceEntry.CanonicalPath,
            Sources = graph.Sources.Select(source => new SourceLoadingClosureSource
            {
                Path = source.CanonicalPath,
                Form = source.Form,
                RouteState = source.RouteState,
                ParentPath = ReadParentPath(graph, source),
                Metadata = source.Metadata,
                GeneratedEntries = source.GeneratedEntries,
            }).ToArray(),
            LoaderRootPaths = graph.RouteFacts.Topology.LoaderRootPaths,
            EvaluateApplicability = true,
            WorkingPathsSupplied = _workingPathsSupplied,
            WorkingPaths = _workingPaths,
        });
        foreach (var selection in loading.Startup)
        {
            var source = graph.FindByPath(selection.Path)
                ?? throw new InvalidOperationException(
                    "A resolved source loading selection must belong to the Context graph.");
            foreach (var reason in selection.Reasons)
            {
                selected.Add(source, ProjectReason(graph, reason), ProjectApplicability(selection.Applicability));
            }
        }

        foreach (var issue in loading.Issues)
        {
            if (issue.Kind == SourceLoadingClosureIssueKind.ApplicabilityInvalid)
            {
                AddApplicabilityInvalidFinding(issue.Path);
            }
            else
            {
                AddClosureFinding(issue.Path, issue.Cause);
            }
        }

        foreach (var condition in loading.PendingConditions)
        {
            AddPendingCondition(condition);
        }
    }

    private static string? ReadParentPath(
        ContextGraph graph,
        ContextGraphSource source)
    {
        var node = graph.RouteFacts.Topology.FindByPath(source.CanonicalPath);
        if (node?.ParentState == SourceRouteParentState.Resolved)
        {
            return node.ParentPaths[0];
        }

        return null;
    }

    private static ContextInclusionReason ProjectReason(
        ContextGraph graph,
        SourceLoadingClosureReason reason)
    {
        var source = reason.SourcePath is null
            ? null
            : graph.FindByPath(reason.SourcePath)
                ?? throw new InvalidOperationException(
                    "A source loading inclusion reason must identify one Context graph source.");
        return new ContextInclusionReason(
            kind: reason.Kind switch
            {
                SourceLoadingClosureReasonKind.WorkspaceEntry => ContextInclusionReasonKind.WorkspaceEntry,
                SourceLoadingClosureReasonKind.Loader => ContextInclusionReasonKind.Loader,
                SourceLoadingClosureReasonKind.LoadNow => ContextInclusionReasonKind.LoadNow,
                SourceLoadingClosureReasonKind.KeepInMind => ContextInclusionReasonKind.KeepInMind,
                SourceLoadingClosureReasonKind.AncestorRequired => ContextInclusionReasonKind.AncestorRequired,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(reason),
                    reason.Kind,
                    "The source loading closure reason kind is not defined."),
            },
            source: source is null ? null : Identity(source),
            reference: null,
            depth: null,
            location: null);
    }

    private void ResolveExplicit(
        IEnumerable<ContextResolvedRequest> requested,
        ContextGraph graph,
        ContextSelectionAccumulator selected)
    {
        foreach (var value in requested)
        {
            if (value.Resolution.State != SourceReferenceResolutionState.Resolved
                || value.Source is not { } source)
            {
                continue;
            }

            if (source.RouteState == SourceRouteState.Ambiguous)
            {
                _blocked = true;
                _findings.Add(Finding(
                    code: ContextFindingCode.SourceAmbiguous,
                    subject: value.Reference,
                    cause: "The selected source has ambiguous route meaning.",
                    reference: value.Reference,
                    source: Identity(source),
                    path: source.CanonicalPath));
                continue;
            }

            if (source.RouteState == SourceRouteState.Unavailable)
            {
                AddClosureFinding(source.CanonicalPath, "The selected source route facts are unavailable.");
            }

            IReadOnlyList<ContextGraphSource> chain = [];
            if (source.RouteState == SourceRouteState.Routed)
            {
                var resolvedChain = ContextApplicabilityProjector.ReadChain(graph, source);
                if (resolvedChain is null)
                {
                    AddClosureFinding(source.CanonicalPath, "The selected source route chain is unavailable.");
                }
                else
                {
                    chain = resolvedChain;
                    var identity = Identity(source);
                    foreach (var ancestor in chain.Take(chain.Count - 1).Where(item => item.IsEntrypoint))
                    {
                        var applicability = ReadApplicability(graph, ancestor);
                        selected.Add(
                            ancestor,
                            new ContextInclusionReason(
                                kind: ContextInclusionReasonKind.AncestorRequired,
                                source: identity,
                                reference: value.Reference,
                                depth: null,
                                location: null),
                            ProjectApplicability(applicability));
                    }
                }
            }

            var sourceApplicability = ReadApplicability(graph, source);

            selected.Add(
                source,
                new ContextInclusionReason(
                    kind: ContextInclusionReasonKind.SelectedSource,
                    source: null,
                    reference: value.Reference,
                    depth: null,
                    location: null),
                ProjectApplicability(sourceApplicability));
            var selectedEntrypoints = chain.Where(item => item.IsEntrypoint).ToArray();
            if (selectedEntrypoints.Length == 0 && source.IsEntrypoint)
            {
                selectedEntrypoints = [source];
            }

            if (selectedEntrypoints.Length != 0)
            {
                TraverseSelected(
                    graph,
                    selectedEntrypoints,
                    selected,
                    value.Reference,
                    source.RouteState == SourceRouteState.Routed && chain.Count != 0);
            }
        }
    }

    private void TraverseSelected(
        ContextGraph graph,
        IEnumerable<ContextGraphSource> seeds,
        ContextSelectionAccumulator selected,
        string reference,
        bool chainAvailable)
    {
        if (!chainAvailable)
        {
            return;
        }

        var queue = new Queue<ContextGraphSource>(seeds);
        var traversed = new HashSet<string>(StringComparer.Ordinal);
        while (queue.TryDequeue(out var parent))
        {
            if (!traversed.Add(parent.CanonicalPath))
            {
                continue;
            }

            var parentApplicability = ReadApplicability(graph, parent);
            if (parentApplicability is not null
                && parentApplicability.State is not (SourceApplicabilityState.Unconditioned or SourceApplicabilityState.Matched))
            {
                continue;
            }

            foreach (var entry in ReadVisibleEntries(graph, parent).Where(entry => entry.AutoSelect))
            {
                var added = false;
                if (entry.LoadNow)
                {
                    added = selected.Add(
                        entry.Target,
                        ParentReason(ContextInclusionReasonKind.LoadNow, parent, reference),
                        ProjectApplicability(entry.Applicability));
                }

                if (entry.KeepInMind)
                {
                    added |= selected.Add(
                        entry.Target,
                        ParentReason(ContextInclusionReasonKind.KeepInMind, parent, reference),
                        ProjectApplicability(entry.Applicability));
                }

                if (added && entry.Target.IsEntrypoint)
                {
                    queue.Enqueue(entry.Target);
                }
            }
        }
    }

    private static ContextInclusionReason ParentReason(
        ContextInclusionReasonKind kind,
        ContextGraphSource parent,
        string reference)
        => new(kind: kind, source: Identity(parent), reference: reference, depth: null, location: null);

    private IReadOnlyList<VisibleEntry> ReadVisibleEntries(ContextGraph graph, ContextGraphSource parent)
    {
        if (parent.GeneratedEntries.State != SourceGeneratedEntriesState.Complete)
        {
            if (parent.IsEntrypoint || parent.IsLoader)
            {
                AddClosureFinding(
                    parent.CanonicalPath,
                    parent.GeneratedEntries.Cause ?? "The generated Entries facts are unavailable.");
            }

            return [];
        }

        var entries = new List<VisibleEntry>();
        foreach (var generated in parent.GeneratedEntries.Entries)
        {
            var targetPath = SourceGeneratedDestinationResolver.Resolve(
                parent.CanonicalPath,
                parent.IsLoader,
                generated.Destination);
            var target = targetPath is null ? null : graph.FindByPath(targetPath);
            if (target is null || !IsDirectChild(graph, parent, target))
            {
                continue;
            }

            var loadNow = generated.HasTag("LoadNow");
            var keepInMind = generated.HasTag("KeepInMind");
            if (!parent.IsLoader && (loadNow || keepInMind))
            {
                if (target.Metadata.State != SourceAuthoredMetadataState.Complete)
                {
                    AddClosureFinding(target.CanonicalPath, "A visible child source has unavailable loading metadata.");
                    continue;
                }

                loadNow &= target.Metadata.Tags.Contains("LoadNow", StringComparer.Ordinal);
                keepInMind &= target.Metadata.Tags.Contains("KeepInMind", StringComparer.Ordinal);
            }

            var applicability = ReadApplicability(graph, target, reportPending: loadNow || keepInMind);
            var autoSelect = SourceLoadingClosureResolver.ShouldLoadAutomatically(loadNow || keepInMind, applicability?.State);

            entries.Add(new VisibleEntry
            {
                Target = target,
                LoadNow = loadNow,
                KeepInMind = keepInMind,
                AutoSelect = autoSelect,
                Applicability = applicability,
            });
        }

        return entries;
    }

    private SourceApplicabilityResult? ReadApplicability(ContextGraph graph, ContextGraphSource source, bool reportPending = false)
    {
        var result = ContextApplicabilityProjector.Evaluate(graph, source, _workingPaths);
        if (result is null)
        {
            return null;
        }

        switch (result.State)
        {
            case SourceApplicabilityState.Pending:
                foreach (var condition in result.Conditions)
                {
                    if (reportPending && condition.Metadata.State == ApplyToMetadataState.Valid)
                    {
                        AddPendingCondition(condition);
                    }
                }

                break;
            case SourceApplicabilityState.Invalid:
                AddApplicabilityInvalidFinding(source.CanonicalPath);
                break;
            case SourceApplicabilityState.Unconditioned:
            case SourceApplicabilityState.Matched:
            case SourceApplicabilityState.Unmatched:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result.State, "The source applicability state is not defined.");
        }

        return result;
    }

    private void AddPendingCondition(SourceApplyToCondition condition)
    {
        if (_pendingConditionSources.Add(condition.CanonicalSourcePath))
        {
            _pendingConditions.Add(new ContextPendingCondition(
                condition.CanonicalSourcePath,
                condition.Metadata.Patterns.Select(pattern => pattern.Text)));
        }

        _incomplete = true;
        if (!_findings.Any(finding => finding.Code == ContextFindingCode.ApplicabilityPending))
        {
            _findings.Add(Finding(
                code: ContextFindingCode.ApplicabilityPending,
                subject: null,
                cause: string.Empty,
                reference: null,
                source: null,
                path: null));
        }
    }

    private void AddApplicabilityInvalidFinding(string path)
    {
        _incomplete = true;
        if (_findings.Any(finding => finding.Code == ContextFindingCode.ApplicabilityInvalid
            && string.Equals(finding.Path, path, StringComparison.Ordinal)))
        {
            return;
        }

        _findings.Add(Finding(
            code: ContextFindingCode.ApplicabilityInvalid,
            subject: path,
            cause: string.Empty,
            reference: null,
            source: null,
            path: path));
    }

    private ContextApplicability? ProjectApplicability(SourceApplicabilityResult? result)
        => ContextApplicabilityProjector.Project(result, _workingPathsSupplied);

    private static bool IsDirectChild(ContextGraph graph, ContextGraphSource parent, ContextGraphSource target)
    {
        if (parent.IsLoader)
        {
            return graph.RouteFacts.Topology.LoaderRootPaths.Contains(target.CanonicalPath, StringComparer.Ordinal);
        }

        var node = graph.RouteFacts.Topology.FindByPath(parent.CanonicalPath);
        return node is not null && node.ChildPaths.Contains(target.CanonicalPath, StringComparer.Ordinal);
    }

    private void AddRootFindings(ContextGraph graph)
    {
        foreach (var issue in graph.Catalogue.Issues.Where(issue => issue.Stage == SourceCatalogueIssueStage.Root))
        {
            if (issue.Code == SourceCatalogueIssueCode.RootUnsafe)
            {
                _blocked = true;
                _findings.Add(Finding(
                    code: ContextFindingCode.WorkspaceUnsafe,
                    subject: issue.AttemptedCanonicalPath,
                    cause: "The .agents source root is unsafe.",
                    reference: null,
                    source: null,
                    path: issue.AttemptedCanonicalPath));
            }
            else
            {
                AddClosureFinding(issue.AttemptedCanonicalPath, "The .agents source root is unavailable.");
            }
        }
    }

    private void AddClosureFinding(string path, string cause)
    {
        _incomplete = true;
        if (_findings.Any(finding => finding.Code == ContextFindingCode.ClosureUnavailable
            && string.Equals(finding.Path, path, StringComparison.Ordinal)))
        {
            return;
        }

        _findings.Add(Finding(
            code: ContextFindingCode.ClosureUnavailable,
            subject: path,
            cause: cause,
            reference: null,
            source: null,
            path: path));
    }

    private static ContextFinding Finding(
        ContextFindingCode code,
        string? subject,
        string cause,
        string? reference,
        ContextSourceIdentity? source,
        string? path)
        => new(
            code: code,
            subject: subject,
            cause: cause,
            reference: reference,
            source: source,
            layer: null,
            path: path,
            part: null,
            location: null,
            destinationLocation: null,
            candidates: []);

    private static ContextSourceIdentity Identity(ContextGraphSource source)
        => new(id: source.Id, path: source.CanonicalPath);

    private sealed record VisibleEntry
    {
        public required ContextGraphSource Target { get; init; }

        public required bool LoadNow { get; init; }

        public required bool KeepInMind { get; init; }

        public required bool AutoSelect { get; init; }

        public required SourceApplicabilityResult? Applicability { get; init; }
    }
}
