using OpenForge.Cli.Core.Commands.Context.Models.Operation;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Projection;

internal sealed class ContextProjectionBuilder
{
    internal ContextProjectionFormation Build(
        IReadOnlyList<ContextSelectedGraphSource> selectedSources,
        ContextContentSelection content)
    {
        ArgumentNullException.ThrowIfNull(selectedSources);
        ArgumentNullException.ThrowIfNull(content);
        var state = new ProjectionAccumulator();
        var sources = new List<ContextSource>();
        var paths = new List<ContextPathProjection>();
        var pathPosition = 0;
        for (var sourceIndex = 0; sourceIndex < selectedSources.Count; sourceIndex++)
        {
            var selected = selectedSources[sourceIndex];
            var graphSource = selected.Source;
            var sourcePosition = sourceIndex + 1;
            var layers = new List<ContextLayer>();
            foreach (var graphLayer in graphSource.Layers)
            {
                pathPosition++;
                var layerReasons = LayerReasons(selected, graphLayer);
                if (content.Effective.Any(part => part.Kind == ContextContentPartKind.Paths))
                {
                    paths.Add(new ContextPathProjection(
                        position: pathPosition,
                        sourcePosition: sourcePosition,
                        id: graphSource.Id,
                        path: graphLayer.CanonicalPath,
                        layer: Layer(graphLayer.Kind),
                        inclusionReasons: layerReasons));
                }

                var projections = BuildLayerProjections(
                    graphSource,
                    graphLayer,
                    content,
                    state);
                layers.Add(new ContextLayer(
                    pathPosition: pathPosition,
                    kind: Layer(graphLayer.Kind),
                    path: graphLayer.CanonicalPath,
                    inclusionReasons: layerReasons,
                    projections: projections));
            }

            AddLogicalSectionFindings(graphSource, content, state);

            sources.Add(new ContextSource(
                position: sourcePosition,
                id: graphSource.Id,
                path: graphSource.CanonicalPath,
                routeState: RouteState(graphSource.RouteState),
                route: graphSource.Route,
                scope: null,
                inclusionReasons: selected.InclusionReasons,
                layers: layers));
        }

        return new ContextProjectionFormation
        {
            Paths = paths,
            Sources = sources,
            Findings = state.Findings,
            ProjectionComplete = !state.Incomplete,
            HasAttention = state.Attention,
        };
    }

    private static IReadOnlyList<ContextProjection> BuildLayerProjections(
        ContextGraphSource source,
        ContextGraphLayer layer,
        ContextContentSelection content,
        ProjectionAccumulator state)
    {
        var requested = content.Effective
            .Where(part => part.Kind is not (ContextContentPartKind.Metadata or ContextContentPartKind.Paths))
            .ToArray();
        if (requested.Length == 0)
        {
            if (layer.ReadState != FileReadState.Complete)
            {
                AddLayerFinding(source, layer, state);
            }

            return [];
        }

        if (layer.Document is not { } document || layer.Text is null)
        {
            AddLayerFinding(source, layer, state);
            return requested.Select(part => new ContextProjection(
                part: ProjectionPart(part.Kind),
                name: part.Name,
                state: ContextProjectionState.Unavailable,
                text: null,
                headings: [],
                location: null)).ToArray();
        }

        var projections = new List<(ContextProjection Projection, int Rank, int Order)>();
        foreach (var part in requested.Where(part => part.Kind != ContextContentPartKind.Section))
        {
            projections.Add((BuildFixedProjection(
                source,
                layer,
                document,
                part,
                state), ReadRank(part.Kind), 0));
        }

        var sectionOrder = 0;
        foreach (var part in requested.Where(part => part.Kind == ContextContentPartKind.Section))
        {
            var projection = BuildSectionProjection(source, layer, document, part, state);
            var documentOrder = projection.Location is null
                ? int.MaxValue - requested.Length + sectionOrder
                : checked((int)Math.Min(projection.Location.ByteOffset, int.MaxValue - requested.Length));
            projections.Add((projection, ReadRank(part.Kind), documentOrder));
            sectionOrder++;
        }

        return projections
            .OrderBy(value => value.Rank)
            .ThenBy(value => value.Order)
            .Select(value => value.Projection)
            .ToArray();
    }

    private static ContextProjection BuildFixedProjection(
        ContextGraphSource source,
        ContextGraphLayer layer,
        MarkdownDocumentFacts document,
        ContextContentPart part,
        ProjectionAccumulator state)
        => part.Kind switch
        {
            ContextContentPartKind.Frontmatter => BuildFrontmatter(
                source,
                layer,
                document,
                part,
                state),
            ContextContentPartKind.Headings => new ContextProjection(
                part: ContextProjectionPart.Headings,
                name: null,
                state: ContextProjectionState.Available,
                text: null,
                headings: document.Headings.Select(heading => new ContextProjectedHeading(
                    text: heading.VisibleText,
                    level: heading.Level,
                    form: heading.Form == MarkdownHeadingForm.Atx ? ContextHeadingForm.Atx : ContextHeadingForm.Setext,
                    location: Location(document.Source, heading.Span),
                    canonical: heading.IsCanonical)),
                location: null),
            ContextContentPartKind.Body => TextProjection(
                ContextProjectionPart.Body,
                null,
                document,
                document.BodySpan ?? throw new InvalidOperationException("An available Markdown body requires its span.")),
            _ => throw new ArgumentOutOfRangeException(nameof(part), part.Kind, "The fixed Context projection part is not defined."),
        };

    private static ContextProjection BuildFrontmatter(
        ContextGraphSource source,
        ContextGraphLayer layer,
        MarkdownDocumentFacts document,
        ContextContentPart part,
        ProjectionAccumulator state)
    {
        if (document.Frontmatter.State == MarkdownFrontmatterState.Missing)
        {
            state.Attention = true;
            state.Findings.Add(Finding(
                code: ContextFindingCode.FrontmatterMissing,
                source: source,
                layer: layer,
                part: part,
                cause: "The selected physical layer has no authored frontmatter.",
                location: null));
            return new ContextProjection(
                part: ContextProjectionPart.Frontmatter,
                name: null,
                state: ContextProjectionState.Missing,
                text: null,
                headings: [],
                location: null);
        }

        if (document.Frontmatter.State != MarkdownFrontmatterState.Complete
            || document.Frontmatter.BlockSpan is not { } span)
        {
            state.Incomplete = true;
            state.Findings.Add(Finding(
                code: ContextFindingCode.ProjectionUnavailable,
                source: source,
                layer: layer,
                part: part,
                cause: "The authored frontmatter boundary could not be established completely.",
                location: null));
            return new ContextProjection(
                part: ContextProjectionPart.Frontmatter,
                name: null,
                state: ContextProjectionState.Unavailable,
                text: null,
                headings: [],
                location: null);
        }

        return TextProjection(ContextProjectionPart.Frontmatter, null, document, span);
    }

    private static ContextProjection BuildSectionProjection(
        ContextGraphSource source,
        ContextGraphLayer layer,
        MarkdownDocumentFacts document,
        ContextContentPart part,
        ProjectionAccumulator state)
    {
        var matches = document.Sections
            .Where(section => string.Equals(section.Heading.VisibleText, part.Name, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (matches.Length == 0)
        {
            return new ContextProjection(
                part: ContextProjectionPart.Section,
                name: part.Name,
                state: ContextProjectionState.Missing,
                text: null,
                headings: [],
                location: null);
        }

        if (matches.Length > 1)
        {
            state.Incomplete = true;
            state.Findings.Add(Finding(
                code: ContextFindingCode.SectionAmbiguous,
                source: source,
                layer: layer,
                part: part,
                cause: "Several headings establish the requested section in the same physical layer.",
                location: Location(document.Source, matches[0].Span)));
            return new ContextProjection(
                part: ContextProjectionPart.Section,
                name: part.Name,
                state: ContextProjectionState.Ambiguous,
                text: null,
                headings: [],
                location: null);
        }

        return TextProjection(ContextProjectionPart.Section, part.Name, document, matches[0].Span);
    }

    private static void AddLogicalSectionFindings(
        ContextGraphSource source,
        ContextContentSelection content,
        ProjectionAccumulator state)
    {
        foreach (var part in content.Effective.Where(part => part.Kind == ContextContentPartKind.Section))
        {
            var inspectionComplete = true;
            var matchEstablished = false;
            foreach (var layer in source.Layers)
            {
                if (layer.Document is not { } document || layer.Text is null)
                {
                    inspectionComplete = false;
                    continue;
                }

                matchEstablished |= document.Sections.Any(section => string.Equals(
                    section.Heading.VisibleText,
                    part.Name,
                    StringComparison.OrdinalIgnoreCase));
            }

            if (matchEstablished || !inspectionComplete)
            {
                continue;
            }

            state.Attention = true;
            state.Findings.Add(new ContextFinding(
                code: ContextFindingCode.SectionMissing,
                subject: part.CanonicalValue,
                cause: "The requested section is absent from the completely inspected logical source.",
                reference: null,
                source: new ContextSourceIdentity(id: source.Id, path: source.CanonicalPath),
                layer: null,
                path: source.CanonicalPath,
                part: part,
                location: null,
                destinationLocation: null,
                candidates: []));
        }
    }

    private static ContextProjection TextProjection(
        ContextProjectionPart part,
        string? name,
        MarkdownDocumentFacts document,
        MarkdownTextSpan span)
        => new(
            part: part,
            name: name,
            state: ContextProjectionState.Available,
            text: document.Source[span.Start..span.End],
            headings: [],
            location: Location(document.Source, span));

    private static void AddLayerFinding(
        ContextGraphSource source,
        ContextGraphLayer layer,
        ProjectionAccumulator state)
    {
        state.Incomplete = true;
        var code = layer.ReadState == FileReadState.InvalidEncoding
            ? ContextFindingCode.InvalidEncoding
            : ContextFindingCode.LayerUnavailable;
        state.Findings.Add(Finding(
            code: code,
            source: source,
            layer: layer,
            part: null,
            cause: layer.ReadState == FileReadState.InvalidEncoding
                ? "The selected physical layer is not strict UTF-8."
                : "The selected physical layer could not be read completely.",
            location: null));
    }

    private static ContextFinding Finding(
        ContextFindingCode code,
        ContextGraphSource source,
        ContextGraphLayer layer,
        ContextContentPart? part,
        string cause,
        SourceLocation? location)
        => new(
            code: code,
            subject: part?.CanonicalValue ?? layer.CanonicalPath,
            cause: cause,
            reference: null,
            source: new ContextSourceIdentity(id: source.Id, path: source.CanonicalPath),
            layer: Layer(layer.Kind),
            path: layer.CanonicalPath,
            part: part,
            location: location,
            destinationLocation: null,
            candidates: []);

    private static IReadOnlyList<ContextInclusionReason> LayerReasons(
        ContextSelectedGraphSource selected,
        ContextGraphLayer layer)
    {
        var reasons = selected.InclusionReasons.ToList();
        if (layer.Kind == SourceLayerKind.Overwrite)
        {
            reasons.Add(new ContextInclusionReason(
                kind: ContextInclusionReasonKind.OverwriteCompanion,
                source: new ContextSourceIdentity(
                    id: selected.Source.Id,
                    path: selected.Source.CanonicalPath),
                reference: null,
                depth: null,
                location: null));
        }

        return reasons;
    }

    private static SourceLocation Location(
        string source,
        MarkdownTextSpan span)
        => new Utf8SourceMap(source).Map(span.Start, span.Length);

    private static int ReadRank(ContextContentPartKind kind)
        => kind switch
        {
            ContextContentPartKind.Frontmatter => 2,
            ContextContentPartKind.Headings => 3,
            ContextContentPartKind.Body => 4,
            ContextContentPartKind.Section => 5,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The physical Context content part is not defined."),
        };

    private static ContextProjectionPart ProjectionPart(ContextContentPartKind kind)
        => kind switch
        {
            ContextContentPartKind.Frontmatter => ContextProjectionPart.Frontmatter,
            ContextContentPartKind.Headings => ContextProjectionPart.Headings,
            ContextContentPartKind.Body => ContextProjectionPart.Body,
            ContextContentPartKind.Section => ContextProjectionPart.Section,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Context projection part is not defined."),
        };

    private sealed class ProjectionAccumulator
    {
        internal List<ContextFinding> Findings { get; } = [];

        internal bool Incomplete { get; set; }

        internal bool Attention { get; set; }
    }

    private static ContextSourceLayerKind Layer(SourceLayerKind kind)
        => kind switch
        {
            SourceLayerKind.Base => ContextSourceLayerKind.Base,
            SourceLayerKind.Overwrite => ContextSourceLayerKind.Overwrite,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The source layer kind is not defined."),
        };

    private static ContextRouteState RouteState(SourceRouteState state)
        => state switch
        {
            SourceRouteState.Routed => ContextRouteState.Routed,
            SourceRouteState.Unrouted => ContextRouteState.Unrouted,
            SourceRouteState.Ambiguous => ContextRouteState.Ambiguous,
            SourceRouteState.Unavailable => ContextRouteState.Unavailable,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The source route state is not defined."),
        };
}
