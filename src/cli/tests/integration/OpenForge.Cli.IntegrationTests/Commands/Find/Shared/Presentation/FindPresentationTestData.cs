using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Commands.Find.Shared.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Find.Shared.Presentation;

internal static class FindPresentationTestData
{

    internal static FindResult CompleteResult()
        => RichResult(Workspace(), FindRequirement.All, CliDetail.Standard, AllContent());

    internal static FindContentSelection AllContent()
    {
        var parts = new FindContentPart[]
        {
            new(FindContentPartKind.Metadata, null, FindDefinitions.Metadata),
            new(FindContentPartKind.Frontmatter, null, FindDefinitions.Frontmatter),
            new(FindContentPartKind.Headings, null, FindDefinitions.Headings),
            new(FindContentPartKind.Body, null, FindDefinitions.Body),
            new(FindContentPartKind.Section, "Target", "section:Target"),
        };
        return new FindContentSelection(parts, parts, isRequested: true);
    }

    private static FindResult RichResult(
        CliWorkspace workspace,
        FindRequirement requirement,
        CliDetail view,
        FindContentSelection content)
    {
        var source = Source(withOverwrite: true);
        var query = RichQuery(requirement);
        return Build(
            workspace,
            new FindUniverseFilter([source.Identity.AutomaticId], []),
            query,
            Presentation(content, view, view),
            Universe(source),
            [Match(source, query)],
            content.Effective.Count == 0 ? [] : Projections(source),
            [],
            new FindStageCompletion(
                FindCoverageState.Complete,
                content.Effective.Count == 0
                    ? FindProjectionCoverageState.NotRequested
                    : FindProjectionCoverageState.Complete),
            null);
    }

    private static FindResult Build(
        CliWorkspace? workspace,
        FindUniverseFilter filter,
        FindQuery query,
        FindPresentationSelection presentation,
        FindUniverse? universe,
        IEnumerable<FindMatch> matches,
        IEnumerable<FindProjection> projections,
        IEnumerable<FindFinding> findings,
        FindStageCompletion completion,
        FindTerminalEvent? terminal)
        => new FindResultBuilder().Build(new FindResultInput(
            new FindRequestEcho(workspace, filter, query, presentation),
            universe,
            [],
            matches,
            projections,
            findings,
            completion,
            terminal));

    private static FindPresentationSelection Presentation(
        FindContentSelection content,
        CliDetail? suppliedDetail = CliDetail.Standard,
        CliDetail effectiveView = CliDetail.Standard)
        => new(suppliedDetail, effectiveView, content);

    private static FindQuery RichQuery(FindRequirement requirement)
    {
        var predicates = new FindPredicate[]
        {
            new(FindPredicateKind.Tag, LongTag, LongTag),
            new(FindPredicateKind.Heading, LongHeading, LongHeading),
        };
        return new FindQuery(
            predicates,
            predicates,
            requirement,
            new FindRegionSelection(
                [
                    new FindRegion(FindRegionKind.Frontmatter, null, FindDefinitions.Frontmatter),
                    new FindRegion(FindRegionKind.Body, null, FindDefinitions.Body),
                ],
                [new FindRegion(FindRegionKind.Frontmatter, null, FindDefinitions.Frontmatter)],
                [new FindRegion(FindRegionKind.Body, null, FindDefinitions.Body)]));
    }

    private static FindUniverse Universe(SourceLogicalSource source)
    {
        var identity = new FindSourceIdentity(
            source.Identity.AutomaticId,
            source.Identity.CanonicalBasePath);
        var selector = new FindSelector(
            source.Identity.AutomaticId,
            SourceReferenceKind.SourceId,
            FindSelectorResolution.Resolved,
            identity,
            FindSourceKind.Ordinary,
            FindSelectorExpansion.Source,
            []);
        return new FindUniverse(
            FindUniverseMode.Filtered,
            [selector],
            [],
            1,
            1,
            1);
    }

    private static FindMatch Match(SourceLogicalSource source, FindQuery query)
    {
        var basePath = source.Base.CanonicalPath;
        var overwritePath = source.Overwrite?.CanonicalPath
            ?? throw new InvalidOperationException("The rich Find fixture requires an overwrite layer.");
        return new FindMatch(
            1,
            source.Identity.AutomaticId,
            basePath,
            "A description with a tab\t, quote \" and slash \",",
            [
                new FindEvidence(
                    1,
                    FindPredicateKind.Tag,
                    query.EffectivePredicates[0].SuppliedValue,
                    query.EffectivePredicates[0].SuppliedValue,
                    new FindRegion(FindRegionKind.Frontmatter, null, FindDefinitions.Frontmatter),
                    SourceLayerKind.Base,
                    basePath,
                    new SourceLocation(2, 4, 12, 8),
                    1,
                    null),
                new FindEvidence(
                    1,
                    FindPredicateKind.Tag,
                    query.EffectivePredicates[0].SuppliedValue,
                    query.EffectivePredicates[0].SuppliedValue,
                    new FindRegion(FindRegionKind.Frontmatter, null, FindDefinitions.Frontmatter),
                    SourceLayerKind.Overwrite,
                    overwritePath,
                    new SourceLocation(2, 4, 14, 8),
                    1,
                    null),
                new FindEvidence(
                    2,
                    FindPredicateKind.Heading,
                    query.EffectivePredicates[1].SuppliedValue,
                    query.EffectivePredicates[1].SuppliedValue,
                    new FindRegion(FindRegionKind.Body, null, FindDefinitions.Body),
                    SourceLayerKind.Base,
                    basePath,
                    new SourceLocation(8, 1, 80, 20),
                    1,
                    new FindHeadingEvidence(2, MarkdownHeadingForm.Atx, true)),
                new FindEvidence(
                    2,
                    FindPredicateKind.Heading,
                    query.EffectivePredicates[1].SuppliedValue,
                    query.EffectivePredicates[1].SuppliedValue,
                    new FindRegion(FindRegionKind.Body, null, FindDefinitions.Body),
                    SourceLayerKind.Overwrite,
                    overwritePath,
                    new SourceLocation(8, 1, 82, 20),
                    1,
                    new FindHeadingEvidence(2, MarkdownHeadingForm.Setext, false)),
            ],
            []);
    }

    private static IReadOnlyList<FindProjection> Projections(
        SourceLogicalSource source,
        int position = 1,
        FindRouteState routeState = FindRouteState.Routed)
    {
        var basePath = source.Base.CanonicalPath;
        var metadataLayers = new List<FindMetadataLayer>
        {
            new(SourceLayerKind.Base, basePath),
        };
        if (source.Overwrite is { } overwrite)
        {
            metadataLayers.Add(new FindMetadataLayer(SourceLayerKind.Overwrite, overwrite.CanonicalPath));
        }

        var projections = new List<FindProjection>
        {
            new(
                FindContentPartKind.Metadata,
                null,
                null,
                null,
                FindProjectionState.Available,
                new FindMetadata(
                    position,
                    source.Identity.AutomaticId,
                    basePath,
                    routeState,
                    routeState == FindRouteState.Routed ? source.Identity.AutomaticId : null,
                    metadataLayers),
                null,
                [],
                null),
            Projection(source, FindContentPartKind.Frontmatter, null, SourceLayerKind.Base, FindProjectionState.Available, "base frontmatter"),
            Projection(source, FindContentPartKind.Headings, null, SourceLayerKind.Base, FindProjectionState.Available, null),
            Projection(source, FindContentPartKind.Body, null, SourceLayerKind.Base, FindProjectionState.Available, "base body"),
            Projection(source, FindContentPartKind.Section, "Target", SourceLayerKind.Base, FindProjectionState.Available, "base section"),
        };
        if (source.Overwrite is not null)
        {
            projections.AddRange(
            [
                Projection(source, FindContentPartKind.Frontmatter, null, SourceLayerKind.Overwrite, FindProjectionState.Available, "overwrite frontmatter"),
                Projection(source, FindContentPartKind.Headings, null, SourceLayerKind.Overwrite, FindProjectionState.Available, null),
                Projection(source, FindContentPartKind.Body, null, SourceLayerKind.Overwrite, FindProjectionState.Available, "overwrite body"),
                Projection(source, FindContentPartKind.Section, "Target", SourceLayerKind.Overwrite, FindProjectionState.Available, "overwrite section"),
            ]);
        }

        return projections;
    }

    private static FindProjection Projection(
        SourceLogicalSource source,
        FindContentPartKind part,
        string? name,
        SourceLayerKind layer,
        FindProjectionState state,
        string? text)
    {
        var path = layer == SourceLayerKind.Base
            ? source.Base.CanonicalPath
            : source.Overwrite?.CanonicalPath
                ?? throw new InvalidOperationException("The Find projection fixture requires its overwrite layer.");
        var location = new SourceLocation(
            layer == SourceLayerKind.Base ? 5 : 15,
            1,
            layer == SourceLayerKind.Base ? 40 : 140,
            text?.Length ?? 0);
        FindProjectedHeading[] headings = part == FindContentPartKind.Headings && state == FindProjectionState.Available
            ? [new FindProjectedHeading(
                "Projected heading",
                2,
                layer == SourceLayerKind.Base ? MarkdownHeadingForm.Atx : MarkdownHeadingForm.Setext,
                new SourceLocation(
                    layer == SourceLayerKind.Base ? 8 : 18,
                    1,
                    layer == SourceLayerKind.Base ? 80 : 180,
                    18),
                layer == SourceLayerKind.Base)]
            : [];
        return new FindProjection(
            part,
            name,
            layer,
            path,
            state,
            null,
            state == FindProjectionState.Available && part != FindContentPartKind.Headings ? text : null,
            headings,
            state == FindProjectionState.Available && part is not FindContentPartKind.Headings ? location : null);
    }

    private static SourceLogicalSource Source(bool withOverwrite = false)
        => Source(".agents/docs.md", withOverwrite);

    private static SourceLogicalSource Source(string basePath, bool withOverwrite = false)
    {
        var root = Workspace().PhysicalRoot;
        var id = SourceIdentity.DeriveId(basePath)
            ?? throw new InvalidOperationException("The Find fixture source must have a derived identity.");
        var baseLayer = new SourceLayer(
            basePath,
            Physical(root, basePath),
            SourceDocumentForm.Markdown,
            SourceLayerKind.Base);
        var overwrite = withOverwrite
            ? new SourceLayer(
                $"{basePath[..^3]}.overwrite.md",
                Physical(root, $"{basePath[..^3]}.overwrite.md"),
                SourceDocumentForm.OverwriteCompanion,
                SourceLayerKind.Overwrite)
            : null;
        return new SourceLogicalSource(
            new SourceLogicalIdentity(id, basePath),
            baseLayer,
            overwrite);
    }

    private static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-find-presentation-red"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    private static string Physical(string root, string logicalPath)
        => Path.GetFullPath(Path.Combine(root, logicalPath.Replace('/', Path.DirectorySeparatorChar)));

    private static string LongTag
        => $"Tag-{new string('t', 600)}";

    private static string LongHeading
        => $"Heading-{new string('h', 600)}\\\"";

}

