using OpenForge.Cli.Core.Commands.Context.Shared.Selection;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability.Models;
using OpenForge.Cli.Core.Commands.Context.Shared.Links.Models;
using OpenForge.Cli.Core.Commands.Context.Models.Operation;
using OpenForge.Cli.Core.Commands.Context.Models.Request;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Shell.Definitions;
using static OpenForge.Cli.Core.Commands.Context.Shared.Links.ContextLinkResultFactory;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Links;

internal sealed class ContextLinkExpander
{
    private readonly ContextLinkedSourceReader _linkedSourceReader = new();
    private readonly SourceLinkDestinationResolver _destinationResolver;

    internal ContextLinkExpander()
    {
        var physicalPathResolver = new PhysicalPathResolver();
        var markdownParser = new MarkdownDocumentParser();
        _destinationResolver = new SourceLinkDestinationResolver(
            (workspace, lexicalPath) => physicalPathResolver.ResolveCandidate(
                workspace.LexicalRoot,
                workspace.PhysicalRoot,
                lexicalPath),
            StrictUtf8FileReader.ReadAsync,
            markdownParser.Parse);
    }

    internal async ValueTask<ContextLinkExpansionFormation> ExpandAsync(
        ContextRequest request,
        ContextGraph graph,
        ContextClosureResolution closure,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(closure);
        if (request.LinkExpansion.Mode == ContextLinkExpansionMode.None)
        {
            return ContextLinkExpansionFormation.NotRequested(closure);
        }

        var startup = await ExpandSetAsync(
            request,
            graph,
            closure.StartupSources,
            captureEvidence: false,
            cancellationToken).ConfigureAwait(false);
        var combined = await ExpandSetAsync(
            request,
            graph,
            closure.CombinedSources,
            captureEvidence: true,
            cancellationToken).ConfigureAwait(false);
        var result = request.AdditionsOnly
            ? Subtract(combined.Sources, startup.Sources)
            : combined.Sources;
        return ContextLinkExpansionFormation.Expanded(
            startupSources: startup.Sources,
            combined: combined,
            resultSources: result);
    }

    private async ValueTask<ContextLinkExpansionRun> ExpandSetAsync(
        ContextRequest request,
        ContextGraph graph,
        IReadOnlyList<ContextSelectedGraphSource> seeds,
        bool captureEvidence,
        CancellationToken cancellationToken)
    {
        var selected = new ContextSelectionAccumulator();
        foreach (var seed in seeds)
        {
            foreach (var reason in seed.InclusionReasons)
            {
                selected.Add(seed.Source, reason, seed.Applicability);
            }
        }
        var links = new List<ContextLink>();
        var findings = new List<ContextFinding>();
        var pendingConditions = new List<ContextPendingCondition>();
        var pendingSources = new HashSet<string>(StringComparer.Ordinal);
        var externalSources = new Dictionary<string, ContextGraphSource>(StringComparer.Ordinal);
        var queue = new Queue<TraversalNode>(seeds.Select(seed => new TraversalNode
        {
            Source = seed.Source,
            Depth = 0,
            Lineage = new HashSet<string>([seed.Source.CanonicalPath], StringComparer.Ordinal),
        }));
        var complete = true;
        var blocked = false;
        while (queue.TryDequeue(out var node))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var depth = node.Depth + 1;
            if (request.LinkExpansion is
                {
                    Mode: ContextLinkExpansionMode.Bounded,
                    Depth: { } maximumDepth,
                }
                && depth > maximumDepth)
            {
                continue;
            }

            foreach (var layer in node.Source.Layers)
            {
                if (layer.Document is not { } document)
                {
                    continue;
                }

                var map = new Utf8SourceMap(document.Source);
                foreach (var authored in ReadAuthoredLinks(document))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var location = map.Map(authored.Span.Start, authored.Span.Length);
                    var destinationLocation = authored.DestinationSpan is { } destinationSpan
                        ? map.Map(destinationSpan.Start, destinationSpan.Length)
                        : null;
                    var input = new SourceLinkDestinationInput
                    {
                        Workspace = request.Workspace,
                        Catalogue = graph.Catalogue,
                        SourceCanonicalPath = layer.CanonicalPath,
                        RawDestination = authored.RawDestination,
                    };
                    var evidence = new ContextAuthoredLinkEvidence
                    {
                        Source = node.Source,
                        Layer = layer,
                        Authored = authored,
                        Location = location,
                        DestinationLocation = destinationLocation,
                    };
                    var initial = await _destinationResolver.ResolveAsync(
                        input,
                        cancellationToken).ConfigureAwait(false);
                    var destination = await ContextLinkCaseMismatchResolver.ResolveAsync(
                        input,
                        initial,
                        _destinationResolver,
                        cancellationToken).ConfigureAwait(false);
                    var facts = destination.Facts;
                    var finding = facts.Finding is null
                        ? null
                        : Finding(evidence, facts);
                    if (captureEvidence && finding is not null)
                    {
                        findings.Add(finding);
                    }

                    if (captureEvidence && destination.CaseMismatch)
                    {
                        findings.Add(CaseMismatchFinding(
                            evidence,
                            facts.Target.Path));
                    }

                    if (finding is not null && finding.Status is not (CliSemanticStatus.Complete or CliSemanticStatus.Attention))
                    {
                        complete = false;
                        blocked |= finding.Status == CliSemanticStatus.Blocked;
                    }

                    var targetSource = facts.Target.Resolution == SourceLinkTargetResolution.Complete
                        ? await _linkedSourceReader.ReadAsync(
                            graph,
                            facts.Target,
                            externalSources,
                            cancellationToken).ConfigureAwait(false)
                        : null;
                    ContextLinkDisposition disposition;
                    if (facts.Target.Kind == SourceLinkTargetKind.External)
                    {
                        disposition = ContextLinkDisposition.ExternalUnchecked;
                    }
                    else if (facts.Target.Resolution != SourceLinkTargetResolution.Complete || targetSource is null)
                    {
                        disposition = ContextLinkDisposition.Unresolved;
                        if (facts.Target.Resolution == SourceLinkTargetResolution.Complete && finding is null)
                        {
                            complete = false;
                            if (captureEvidence)
                            {
                                findings.Add(UnavailableTargetFinding(
                                    evidence,
                                    facts.Target.Path));
                            }
                        }
                    }
                    else
                    {
                        var chain = ContextApplicabilityProjector.ReadChain(graph, targetSource);
                        if (chain is null && captureEvidence)
                        {
                            complete = false;
                            findings.Add(new ContextFinding(
                                code: ContextFindingCode.ClosureUnavailable,
                                subject: targetSource.CanonicalPath,
                                cause: string.Empty,
                                reference: null,
                                source: Identity(targetSource),
                                layer: null,
                                path: targetSource.CanonicalPath,
                                part: null,
                                location: null,
                                destinationLocation: null,
                                candidates: []));
                        }

                        var applicability = ContextApplicabilityProjector.Evaluate(
                            graph,
                            targetSource,
                            request.WorkingPaths);
                        if (captureEvidence)
                        {
                            RecordApplicability(
                                applicability,
                                targetSource,
                                pendingConditions,
                                pendingSources,
                                findings,
                                ref complete);
                        }

                        if (chain is not null)
                        {
                            var targetIdentity = Identity(targetSource);
                            foreach (var ancestor in chain.Take(chain.Count - 1).Where(item => item.IsEntrypoint))
                            {
                                var ancestorApplicability = ContextApplicabilityProjector.Evaluate(
                                    graph,
                                    ancestor,
                                    request.WorkingPaths);
                                if (captureEvidence)
                                {
                                    RecordApplicability(
                                        ancestorApplicability,
                                        ancestor,
                                        pendingConditions,
                                        pendingSources,
                                        findings,
                                        ref complete);
                                }

                                selected.Add(
                                    ancestor,
                                    new ContextInclusionReason(
                                        kind: ContextInclusionReasonKind.AncestorRequired,
                                        source: targetIdentity,
                                        reference: null,
                                        depth: null,
                                        location: null),
                                    ContextApplicabilityProjector.Project(
                                        ancestorApplicability,
                                        request.WorkingPathsSupplied));
                            }
                        }

                        var reason = new ContextInclusionReason(
                            kind: ContextInclusionReasonKind.LinkedSource,
                            source: Identity(node.Source),
                            reference: null,
                            depth: depth,
                            location: location);
                        var added = selected.Add(
                            targetSource,
                            reason,
                            ContextApplicabilityProjector.Project(applicability, request.WorkingPathsSupplied));
                        disposition = ReadDisposition(added, node.Lineage, targetSource);
                        if (added)
                        {
                            var lineage = new HashSet<string>(node.Lineage, StringComparer.Ordinal)
                            {
                                targetSource.CanonicalPath,
                            };
                            queue.Enqueue(new TraversalNode
                            {
                                Source = targetSource,
                                Depth = depth,
                                Lineage = lineage,
                            });
                        }
                    }

                    if (captureEvidence)
                    {
                        links.Add(new ContextLink(
                            depth: depth,
                            source: new ContextLinkSource
                            {
                                Id = node.Source.Id,
                                Path = layer.CanonicalPath,
                                Layer = Layer(layer.Kind),
                            },
                            location: location,
                            destinationLocation: destinationLocation,
                            rawDestination: authored.RawDestination,
                            fragment: facts.Fragment,
                            target: Target(facts.Target, targetSource, destination.CaseMismatch),
                            disposition: disposition));
                    }
                }
            }
        }

        return new ContextLinkExpansionRun
        {
            Sources = selected.Sources,
            Links = links,
            Findings = findings,
            PendingConditions = pendingConditions,
            Complete = complete,
            Blocked = blocked,
        };
    }

    private static void RecordApplicability(
        SourceApplicabilityResult? applicability,
        ContextGraphSource source,
        ICollection<ContextPendingCondition> pendingConditions,
        ISet<string> pendingSources,
        ICollection<ContextFinding> findings,
        ref bool complete)
    {
        if (applicability is null)
        {
            return;
        }

        if (applicability.State == SourceApplicabilityState.Pending)
        {
            complete = false;
            foreach (var condition in ContextApplicabilityProjector.PendingConditions(applicability))
            {
                if (pendingSources.Add(condition.Source))
                {
                    pendingConditions.Add(condition);
                }
            }

            if (!findings.Any(finding => finding.Code == ContextFindingCode.ApplicabilityPending))
            {
                findings.Add(new ContextFinding(
                    code: ContextFindingCode.ApplicabilityPending,
                    subject: source.CanonicalPath,
                    cause: string.Empty,
                    reference: null,
                    source: Identity(source),
                    layer: null,
                    path: source.CanonicalPath,
                    part: null,
                    location: null,
                    destinationLocation: null,
                    candidates: []));
            }
        }
        else if (applicability.State == SourceApplicabilityState.Invalid)
        {
            complete = false;
            if (!findings.Any(finding => finding.Code == ContextFindingCode.ApplicabilityInvalid
                && string.Equals(finding.Path, source.CanonicalPath, StringComparison.Ordinal)))
            {
                findings.Add(new ContextFinding(
                    code: ContextFindingCode.ApplicabilityInvalid,
                    subject: source.CanonicalPath,
                    cause: string.Empty,
                    reference: null,
                    source: Identity(source),
                    layer: null,
                    path: source.CanonicalPath,
                    part: null,
                    location: null,
                    destinationLocation: null,
                    candidates: []));
            }
        }
    }

    private static ContextLinkDisposition ReadDisposition(
        bool added,
        IReadOnlySet<string> lineage,
        ContextGraphSource target)
    {
        if (added)
        {
            return ContextLinkDisposition.Selected;
        }

        return lineage.Contains(target.CanonicalPath)
            ? ContextLinkDisposition.Cycle
            : ContextLinkDisposition.AlreadySelected;
    }

    private static IReadOnlyList<MarkdownLinkFact> ReadAuthoredLinks(MarkdownDocumentFacts document)
    {
        var content = document.GeneratedRegion.State == MarkdownGeneratedRegionState.Complete
            ? document.GeneratedRegion.EntriesBlock?.Span
            : null;
        return document.Links
            .Where(link => content is null
                || link.Span.Start < content.Start
                || link.Span.End > content.End)
            .ToArray();
    }

    private static IReadOnlyList<ContextSelectedGraphSource> Subtract(
        IReadOnlyList<ContextSelectedGraphSource> combined,
        IReadOnlyList<ContextSelectedGraphSource> startup)
    {
        var startupPaths = startup
            .Select(source => source.Source.CanonicalPath)
            .ToHashSet(StringComparer.Ordinal);
        return combined.Where(source => !startupPaths.Contains(source.Source.CanonicalPath)).ToArray();
    }

    private sealed record TraversalNode
    {
        public required ContextGraphSource Source { get; init; }

        public required int Depth { get; init; }

        public required IReadOnlySet<string> Lineage { get; init; }
    }
}
