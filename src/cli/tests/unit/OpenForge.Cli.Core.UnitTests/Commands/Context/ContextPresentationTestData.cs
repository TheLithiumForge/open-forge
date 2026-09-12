using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Context;

internal static class ContextPresentationTestData
{
    internal const string GuideFrontmatter = "---\nopen-forge:\n  description: Guide\n  tags: [Guide]\n---\n";
    internal const string GuideBody = "\n# Guide\n\n## Rules\n\nBase rule.\n";

    private const string GuideReference = "projects/guide";
    private const string TopicPath = ".agents/docs/topic.md";
    private const string GuidePath = ".agents/projects/guide.md";
    private const string GuideOverwritePath = ".agents/projects/guide.overwrite.md";
    private const string ExternalDestination = "https://example.com";

    internal static ContextResult Create(
        ContextContentSelection content,
        ContextLinkExpansion? linkExpansion = null,
        IReadOnlyList<ContextFinding>? findings = null)
    {
        ArgumentNullException.ThrowIfNull(content);
        var effectiveLinkExpansion = linkExpansion ?? ContextLinkExpansion.None;
        var effectiveFindings = findings ?? [];
        var selectionCoverage = effectiveFindings.Count == 0
            ? ContextCoverageState.Complete
            : ContextCoverageState.Incomplete;
        var selectedReason = new ContextInclusionReason(
            kind: ContextInclusionReasonKind.SelectedSource,
            source: null,
            reference: GuideReference,
            depth: null,
            location: null);
        var loadNowReason = new ContextInclusionReason(
            kind: ContextInclusionReasonKind.LoadNow,
            source: null,
            reference: null,
            depth: null,
            location: null);
        var overwriteReason = new ContextInclusionReason(
            kind: ContextInclusionReasonKind.OverwriteCompanion,
            source: new ContextSourceIdentity(id: GuideReference, path: GuidePath),
            reference: null,
            depth: null,
            location: null);
        var topicLayer = new ContextLayer(
            pathPosition: 1,
            kind: ContextSourceLayerKind.Base,
            path: TopicPath,
            inclusionReasons: [loadNowReason],
            projections: []);
        var guideLayer = new ContextLayer(
            pathPosition: 2,
            kind: ContextSourceLayerKind.Base,
            path: GuidePath,
            inclusionReasons: [selectedReason],
            projections: GuideProjections(content));
        var overwriteLayer = new ContextLayer(
            pathPosition: 3,
            kind: ContextSourceLayerKind.Overwrite,
            path: GuideOverwritePath,
            inclusionReasons: [overwriteReason],
            projections: OverwriteProjections(content));
        ContextSource[] sources =
        [
            new ContextSource(
                position: 1,
                id: "docs/topic",
                path: TopicPath,
                routeState: ContextRouteState.Routed,
                route: "docs/topic",
                scope: "docs",
                inclusionReasons: [loadNowReason],
                layers: [topicLayer]),
            new ContextSource(
                position: 2,
                id: GuideReference,
                path: GuidePath,
                routeState: ContextRouteState.Routed,
                route: GuideReference,
                scope: "projects",
                inclusionReasons: [selectedReason],
                layers: [guideLayer, overwriteLayer]),
        ];
        ContextPathProjection[] paths =
        [
            new ContextPathProjection(
                position: 1,
                sourcePosition: 1,
                id: "docs/topic",
                path: TopicPath,
                layer: ContextSourceLayerKind.Base,
                inclusionReasons: [loadNowReason]),
            new ContextPathProjection(
                position: 2,
                sourcePosition: 2,
                id: GuideReference,
                path: GuidePath,
                layer: ContextSourceLayerKind.Base,
                inclusionReasons: [selectedReason]),
            new ContextPathProjection(
                position: 3,
                sourcePosition: 2,
                id: GuideReference,
                path: GuideOverwritePath,
                layer: ContextSourceLayerKind.Overwrite,
                inclusionReasons: [overwriteReason]),
        ];

        var workspaceRoot = Path.Combine(Path.GetTempPath(), "open-forge-context-presentation");
        return new ContextResult(
            workspace: new CliWorkspace(
                lexicalRoot: workspaceRoot,
                physicalRoot: workspaceRoot,
                selectedBy: CliWorkspaceSelectionMethod.CurrentDirectory),
            selection: new ContextSelection(
                requestedSources:
                [
                    new ContextRequestedSource(
                        supplied: GuideReference,
                        form: SourceReferenceKind.SourceId,
                        resolution: SourceReferenceResolutionState.Resolved,
                        source: new ContextSourceIdentity(id: GuideReference, path: GuidePath),
                        routeState: ContextRouteState.Routed,
                        candidates: []),
                ],
                startupIncluded: false,
                additionsOnly: true,
                linkExpansion: effectiveLinkExpansion,
                sourceCount: sources.Length),
            presentation: new ContextPresentation(
                suppliedView: null,
                effectiveView: CliView.Expanded,
                content: content),
            coverage: new ContextCoverage(
                state: selectionCoverage,
                selection: selectionCoverage,
                links: effectiveLinkExpansion.Mode == ContextLinkExpansionMode.None
                    ? ContextOptionalCoverageState.NotRequested
                    : ContextOptionalCoverageState.Complete,
                projection: ContextCoverageState.Complete),
            paths: content.Effective is [{ Kind: ContextContentPartKind.Paths }] ? paths : [],
            links: Links(effectiveLinkExpansion),
            sources: sources,
            findings: effectiveFindings,
            status: effectiveFindings.Count == 0
                ? CliSemanticStatus.Complete
                : CliSemanticStatus.Incomplete,
            next: null);
    }

    internal static ContextContentSelection Content(params ContextContentPartKind[] kinds)
    {
        var values = kinds.Select(kind => new ContextContentPart(
            kind: kind,
            name: null,
            canonicalValue: kind.ToString().ToLowerInvariant())).ToArray();
        return new ContextContentSelection(
            supplied: values,
            effective: values.OrderBy(value => value.Kind));
    }

    private static IReadOnlyList<ContextProjection> GuideProjections(ContextContentSelection content)
    {
        var projections = new List<ContextProjection>();
        if (content.Effective.Any(part => part.Kind == ContextContentPartKind.Frontmatter))
        {
            projections.Add(Projection(ContextProjectionPart.Frontmatter, GuideFrontmatter));
        }

        if (content.Effective.Any(part => part.Kind == ContextContentPartKind.Body))
        {
            projections.Add(Projection(ContextProjectionPart.Body, GuideBody));
        }

        return projections;
    }

    private static IReadOnlyList<ContextProjection> OverwriteProjections(ContextContentSelection content)
        => content.Effective.Any(part => part.Kind == ContextContentPartKind.Body)
            ? [Projection(ContextProjectionPart.Body, "# Guide overwrite\n\nOverwrite rule.\n")]
            : [];

    private static ContextProjection Projection(ContextProjectionPart part, string text)
        => new(
            part: part,
            name: null,
            state: ContextProjectionState.Available,
            text: text,
            headings: [],
            location: new SourceLocation(line: 1, column: 1, byteOffset: 0, byteLength: text.Length));

    private static IReadOnlyList<ContextLink> Links(ContextLinkExpansion expansion)
        => expansion.Mode == ContextLinkExpansionMode.None
            ? []
            :
            [
                new ContextLink(
                    depth: 1,
                    source: new ContextLinkSource
                    {
                        Id = GuideReference,
                        Path = GuidePath,
                        Layer = ContextSourceLayerKind.Base,
                    },
                    location: new SourceLocation(line: 8, column: 1, byteOffset: 64, byteLength: 20),
                    destinationLocation: null,
                    rawDestination: "linked.md#details",
                    fragment: "details",
                    target: new ContextLinkTarget
                    {
                        Kind = ContextLinkTargetKind.Local,
                        Id = "projects/linked",
                        Path = ".agents/projects/linked.md",
                        Layer = ContextSourceLayerKind.Base,
                        Resolution = ContextLinkResolution.Complete,
                        Network = null,
                    },
                    disposition: ContextLinkDisposition.Selected),
                new ContextLink(
                    depth: 1,
                    source: new ContextLinkSource
                    {
                        Id = GuideReference,
                        Path = GuidePath,
                        Layer = ContextSourceLayerKind.Base,
                    },
                    location: new SourceLocation(line: 8, column: 32, byteOffset: 95, byteLength: 30),
                    destinationLocation: null,
                    rawDestination: ExternalDestination,
                    fragment: null,
                    target: new ContextLinkTarget
                    {
                        Kind = ContextLinkTargetKind.External,
                        Id = null,
                        Path = null,
                        Layer = null,
                        Resolution = ContextLinkResolution.ExternalUnchecked,
                        Network = ContextLinkNetwork.NetworkNotAttempted,
                    },
                    disposition: ContextLinkDisposition.ExternalUnchecked),
            ];
}
