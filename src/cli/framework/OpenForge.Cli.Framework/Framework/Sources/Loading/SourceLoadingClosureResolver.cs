using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Loading;

internal sealed class SourceLoadingClosureResolver
{
    internal SourceLoadingClosureResolution Resolve(SourceLoadingClosureRequest request)
        => new ClosureBuilder(request).Build();

    internal static bool ShouldLoadAutomatically(bool hasLoadingTag, SourceApplicabilityState? applicability)
        => applicability switch
        {
            null or SourceApplicabilityState.Unconditioned or SourceApplicabilityState.Matched => hasLoadingTag,
            SourceApplicabilityState.Unmatched or SourceApplicabilityState.Pending or SourceApplicabilityState.Invalid => false,
            _ => throw new ArgumentOutOfRangeException(nameof(applicability), applicability, "The source applicability state is not defined."),
        };

    private sealed class ClosureBuilder
    {
        private readonly List<string> _continuityPaths = [];
        private readonly HashSet<string> _continuitySet = new(StringComparer.Ordinal);
        private readonly List<SourceLoadingClosureIssue> _issues = [];
        private readonly List<SourceApplyToCondition> _pendingConditions = [];
        private readonly HashSet<string> _pendingConditionSources = new(StringComparer.Ordinal);
        private readonly SourceLoadingClosureGraph _graph;
        private readonly SelectionAccumulator _selection = new();
        private readonly SourceLoadingClosureRequest _request;

        internal ClosureBuilder(SourceLoadingClosureRequest request)
        {
            _request = request;
            _graph = new SourceLoadingClosureGraph(request);
        }

        internal SourceLoadingClosureResolution Build()
        {
            _selection.Add(
                _graph.WorkspaceEntryPath,
                new SourceLoadingClosureReason(
                    SourceLoadingClosureReasonKind.WorkspaceEntry,
                    sourcePath: null));
            var loader = _graph.Sources.SingleOrDefault(source => source.IsLoader);
            if (loader is null)
            {
                AddIssue(
                    SourceLoadingClosureIssueKind.LoaderUnavailable,
                    SourceLogicalPath.LoaderPath,
                    "The canonical Loader source is unavailable.");
                return Result();
            }

            _selection.Add(
                loader.Path,
                new SourceLoadingClosureReason(
                    SourceLoadingClosureReasonKind.Loader,
                    sourcePath: null));
            Traverse(loader, parentIsContinuity: false, addSelections: true);
            return Result();
        }

        private void Traverse(
            SourceLoadingClosureSource root,
            bool parentIsContinuity,
            bool addSelections)
        {
            var traversed = new HashSet<(string Path, bool IsContinuity, bool AddSelections)>();
            var stack = new Stack<TraversalFrame>();
            stack.Push(new TraversalFrame(root, parentIsContinuity, addSelections));
            while (stack.TryPeek(out var frame))
            {
                if (frame.NextEntryIndex == 0)
                {
                    if (!traversed.Add((frame.Source.Path, frame.IsContinuity, frame.AddSelections)))
                    {
                        stack.Pop();
                        continue;
                    }

                    if (frame.Source.GeneratedEntries.State != SourceGeneratedEntriesState.Complete)
                    {
                        if (frame.Source.IsEntrypoint || frame.Source.IsLoader)
                        {
                            AddIssue(
                                SourceLoadingClosureIssueKind.GeneratedEntriesUnavailable,
                                frame.Source.Path,
                                frame.Source.GeneratedEntries.Cause
                                    ?? "The generated Entries facts are unavailable.");
                        }

                        stack.Pop();
                        continue;
                    }
                }

                if (frame.NextEntryIndex >= frame.Source.GeneratedEntries.Entries.Count)
                {
                    stack.Pop();
                    continue;
                }

                var generated = frame.Source.GeneratedEntries.Entries[frame.NextEntryIndex++];
                var visible = ReadVisible(frame.Source, generated);
                if (visible is null || !visible.AutoSelect)
                {
                    continue;
                }

                var addedToSelection = false;
                if (frame.AddSelections && visible.LoadNow)
                {
                    addedToSelection = _selection.Add(visible.Target.Path,
                        new SourceLoadingClosureReason(SourceLoadingClosureReasonKind.LoadNow, frame.Source.Path),
                        visible.Applicability);
                }

                if (frame.AddSelections && visible.KeepInMind)
                {
                    addedToSelection |= _selection.Add(visible.Target.Path,
                        new SourceLoadingClosureReason(SourceLoadingClosureReasonKind.KeepInMind, frame.Source.Path),
                        visible.Applicability);
                }

                var isContinuity = frame.IsContinuity || visible.KeepInMind;
                var addedToContinuity = isContinuity && AddContinuity(visible.Target.Path);
                if (visible.Target.IsEntrypoint
                    && (addedToSelection || addedToContinuity))
                {
                    stack.Push(new TraversalFrame(
                        visible.Target,
                        isContinuity,
                        addSelections: addedToSelection));
                }
            }
        }

        private VisibleEntry? ReadVisible(
            SourceLoadingClosureSource parent,
            SourceGeneratedEntry generated)
        {
            var target = _graph.FindDirectChild(parent, generated.Destination);
            if (target is null)
            {
                return null;
            }

            var loadNow = generated.HasTag("LoadNow");
            var keepInMind = generated.HasTag("KeepInMind");
            if (!parent.IsLoader && (loadNow || keepInMind))
            {
                if (target.Metadata.State != SourceAuthoredMetadataState.Complete)
                {
                    AddIssue(
                        SourceLoadingClosureIssueKind.VisibleMetadataUnavailable,
                        target.Path,
                        "A visible child source has unavailable loading metadata.");
                    return null;
                }

                loadNow &= target.Metadata.Tags.Contains("LoadNow", StringComparer.Ordinal);
                keepInMind &= target.Metadata.Tags.Contains("KeepInMind", StringComparer.Ordinal);
            }

            var applicability = ReadApplicability(target, reportPending: loadNow || keepInMind);
            var autoSelect = ShouldLoadAutomatically(loadNow || keepInMind, applicability?.State);
            return new VisibleEntry(target, loadNow, keepInMind, autoSelect, applicability);
        }

        private SourceApplicabilityResult? ReadApplicability(SourceLoadingClosureSource target, bool reportPending)
        {
            if (!_request.EvaluateApplicability)
            {
                return null;
            }

            SourceApplicabilityResult result;
            if (target.RouteState == SourceRouteState.Ambiguous)
            {
                result = new SourceApplicabilityResult(
                    SourceApplicabilityState.Invalid,
                    ImmutableArray<SourceApplyToCondition>.Empty,
                    ImmutableArray<string>.Empty);
            }
            else
            {
                result = SourceApplicabilityEvaluator.Evaluate(
                    ReadConditionChain(target),
                    _request.WorkingPaths);
            }

            switch (result.State)
            {
                case SourceApplicabilityState.Pending:
                    foreach (var condition in result.Conditions)
                    {
                        if (reportPending && condition.Metadata.State == ApplyToMetadataState.Valid
                            && _pendingConditionSources.Add(condition.CanonicalSourcePath))
                        {
                            _pendingConditions.Add(condition);
                        }
                    }

                    break;
                case SourceApplicabilityState.Invalid:
                    AddIssue(
                        SourceLoadingClosureIssueKind.ApplicabilityInvalid,
                        target.Path,
                        "The source has an invalid applyTo condition or ambiguous route ancestry.");
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

        private IReadOnlyList<SourceApplyToCondition> ReadConditionChain(SourceLoadingClosureSource source)
        {
            var chain = new List<SourceLoadingClosureSource>();
            var current = source;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            while (seen.Add(current.Path))
            {
                chain.Add(current);
                if (current.ParentPath is not { } parentPath)
                {
                    break;
                }

                var parent = _graph.Sources.FirstOrDefault(item => string.Equals(item.Path, parentPath, StringComparison.Ordinal));
                if (parent is null)
                {
                    break;
                }

                current = parent;
            }

            chain.Reverse();
            return chain.Select(item => new SourceApplyToCondition(item.Path, item.Metadata.ApplyTo)).ToArray();
        }

        private bool AddContinuity(string path)
        {
            if (!_continuitySet.Add(path))
            {
                return false;
            }

            _continuityPaths.Add(path);
            return true;
        }

        private void AddIssue(
            SourceLoadingClosureIssueKind kind,
            string path,
            string cause)
            => _issues.Add(new SourceLoadingClosureIssue(kind, path, cause));

        private SourceLoadingClosureResolution Result()
            => new()
            {
                Startup = _selection.Freeze(),
                ContinuityPaths = _continuityPaths.ToArray(),
                Issues = _issues.ToArray(),
                PendingConditions = _pendingConditions.ToArray(),
            };

        private sealed class TraversalFrame
        {
            internal TraversalFrame(
                SourceLoadingClosureSource source,
                bool isContinuity,
                bool addSelections)
            {
                Source = source;
                IsContinuity = isContinuity;
                AddSelections = addSelections;
            }

            internal SourceLoadingClosureSource Source { get; }

            internal bool IsContinuity { get; }

            internal bool AddSelections { get; }

            internal int NextEntryIndex { get; set; }
        }

        private sealed record VisibleEntry(
            SourceLoadingClosureSource Target,
            bool LoadNow,
            bool KeepInMind,
            bool AutoSelect,
            SourceApplicabilityResult? Applicability);

        private sealed class SelectionAccumulator
        {
            private readonly Dictionary<string, MutableSelection> _byPath = new(StringComparer.Ordinal);
            private readonly List<MutableSelection> _values = [];

            internal bool Add(
                string path,
                SourceLoadingClosureReason reason,
                SourceApplicabilityResult? applicability = null)
            {
                if (_byPath.TryGetValue(path, out var existing))
                {
                    if (!existing.Reasons.Contains(reason))
                    {
                        existing.Reasons.Add(reason);
                    }

                    existing.Applicability ??= applicability;

                    return false;
                }

                var added = new MutableSelection
                {
                    Path = path,
                    Reasons = [reason],
                    Applicability = applicability,
                };
                _byPath.Add(path, added);
                _values.Add(added);
                return true;
            }

            internal IReadOnlyList<SourceLoadingClosureSelection> Freeze()
                => _values.Select(value => new SourceLoadingClosureSelection
                {
                    Path = value.Path,
                    Reasons = value.Reasons.ToArray(),
                    Applicability = value.Applicability,
                }).ToArray();

            private sealed record MutableSelection
            {
                public required string Path { get; init; }

                public required List<SourceLoadingClosureReason> Reasons { get; init; }

                public SourceApplicabilityResult? Applicability { get; set; }
            }
        }
    }
}
