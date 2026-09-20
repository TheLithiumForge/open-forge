using OpenForge.Cli.Core.Commands.Context.Models.Operation;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Commands.Context.Shared.Projection;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.UnitTests.Commands.Context.Shared.Projection;

public sealed class ContextEntryDocumentProjectionTests
{
    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Entry documents without frontmatter produce no Context attention finding"), Trait("Feature", "context"), Trait("Evidence", "Unit")]
    [InlineData("AGENTS.md")]
    [InlineData(".agents/loader.md")]
    public void EntryDocumentsWithoutFrontmatterAreExempt(string canonicalPath)
    {
        var formation = Build(canonicalPath);

        Assert.Empty(formation.Findings);
        Assert.False(formation.HasAttention);
        Assert.Equal(
            ContextProjectionState.Missing,
            Assert.Single(Assert.Single(Assert.Single(formation.Sources).Layers).Projections).State);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "A routed source without frontmatter still reports the Context attention finding"), Trait("Feature", "context"), Trait("Evidence", "Unit")]
    [InlineData(".agents/guidance/_guidance.md")]
    [InlineData(".agents/guidance/guide.md")]
    public void RoutedSourcesWithoutFrontmatterStillReport(string canonicalPath)
    {
        var formation = Build(canonicalPath);

        Assert.True(formation.HasAttention);
        var finding = Assert.Single(formation.Findings);
        Assert.Equal(ContextFindingCode.FrontmatterMissing, finding.Code);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "A bare overwrite inherits the base frontmatter contract without changing body or order"), Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public void BareOverwriteInheritsBaseFrontmatterContract()
    {
        const string basePath = ".agents/guidance/guide.md";
        const string overwritePath = ".agents/guidance/guide.overwrite.md";
        const string baseBody = "# Base guide\n\nBase body.\n";
        const string overwriteBody = "# Local adjustment\n\nOverwrite body.\n";
        const string baseText = "---\ndescription: Guide\n---\n" + baseBody;
        var frontmatter = new ContextContentPart(ContextContentPartKind.Frontmatter, null, "frontmatter");
        var body = new ContextContentPart(ContextContentPartKind.Body, null, "body");
        var formation = Build(
            basePath,
            layers:
            [
                Layer(SourceLayerKind.Base, basePath, baseText),
                Layer(SourceLayerKind.Overwrite, overwritePath, overwriteBody),
            ],
            metadata: SourceAuthoredMetadataFacts.Complete("Guide", []),
            contentParts: [frontmatter, body]);

        Assert.True(formation.ProjectionComplete);
        Assert.False(formation.HasAttention);
        Assert.Empty(formation.Findings);

        var source = Assert.Single(formation.Sources);
        Assert.Equal([basePath, overwritePath], source.Layers.Select(layer => layer.Path));
        Assert.Equal([1, 2], source.Layers.Select(layer => layer.PathPosition));
        Assert.Equal(
            [ContextProjectionPart.Frontmatter, ContextProjectionPart.Body],
            source.Layers[0].Projections.Select(projection => projection.Part));
        Assert.Equal(
            [ContextProjectionPart.Frontmatter, ContextProjectionPart.Body],
            source.Layers[1].Projections.Select(projection => projection.Part));

        var overwriteFrontmatter = Assert.Single(
            source.Layers[1].Projections,
            projection => projection.Part == ContextProjectionPart.Frontmatter);
        Assert.Equal(ContextProjectionState.Missing, overwriteFrontmatter.State);
        Assert.Null(overwriteFrontmatter.Text);
        var overwriteBodyProjection = Assert.Single(
            source.Layers[1].Projections,
            projection => projection.Part == ContextProjectionPart.Body);
        Assert.Equal(overwriteBody, overwriteBodyProjection.Text);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "A malformed overwrite frontmatter remains an incomplete projection"), Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public void MalformedOverwriteFrontmatterRemainsIncomplete()
    {
        const string basePath = ".agents/guidance/guide.md";
        const string overwritePath = ".agents/guidance/guide.overwrite.md";
        const string baseText = "---\ndescription: Guide\n---\n# Base guide\n";
        const string malformedOverwrite = "---\ndescription: Local adjustment\n# Unclosed frontmatter\n";
        var formation = Build(
            basePath,
            layers:
            [
                Layer(SourceLayerKind.Base, basePath, baseText),
                Layer(SourceLayerKind.Overwrite, overwritePath, malformedOverwrite),
            ],
            metadata: SourceAuthoredMetadataFacts.Complete("Guide", []));

        Assert.False(formation.ProjectionComplete);
        var finding = Assert.Single(formation.Findings);
        Assert.Equal(ContextFindingCode.ProjectionUnavailable, finding.Code);
        Assert.Equal(ContextSourceLayerKind.Overwrite, finding.Layer);
        var overwriteFrontmatter = Assert.Single(
            Assert.Single(formation.Sources).Layers[1].Projections,
            projection => projection.Part == ContextProjectionPart.Frontmatter);
        Assert.Equal(ContextProjectionState.Unavailable, overwriteFrontmatter.State);
    }

    private static ContextProjectionFormation Build(
        string canonicalPath,
        IReadOnlyList<ContextGraphLayer>? layers = null,
        SourceAuthoredMetadataFacts? metadata = null,
        IReadOnlyList<ContextContentPart>? contentParts = null)
    {
        var content = contentParts ??
        [
            new ContextContentPart(ContextContentPartKind.Frontmatter, null, "frontmatter"),
        ];
        return new ContextProjectionBuilder().Build(
            [
                new ContextSelectedGraphSource
                {
                    Source = Source(canonicalPath, layers, metadata),
                    InclusionReasons = [],
                },
            ],
            new ContextContentSelection(content, content));
    }

    private static ContextGraphSource Source(
        string canonicalPath,
        IReadOnlyList<ContextGraphLayer>? layers = null,
        SourceAuthoredMetadataFacts? metadata = null)
    {
        const string text = "# Heading\n\nNo authored frontmatter.\n";
        return new ContextGraphSource(
            logicalSource: null,
            id: null,
            canonicalPath: canonicalPath,
            form: SourceDocumentForm.Markdown,
            routeState: SourceRouteState.Unrouted,
            route: null,
            metadata: metadata ?? SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Missing),
            generatedEntries: SourceGeneratedEntriesFacts.Absent,
            layers: layers ?? [Layer(SourceLayerKind.Base, canonicalPath, text)]);
    }

    private static ContextGraphLayer Layer(
        SourceLayerKind kind,
        string canonicalPath,
        string text)
        => new()
        {
            Kind = kind,
            CanonicalPath = canonicalPath,
            PhysicalPath = PhysicalPath(canonicalPath),
            ReadState = FileReadState.Complete,
            Text = text,
            Document = new MarkdownDocumentParser().Parse(text),
        };

    private static string PhysicalPath(string canonicalPath)
        => Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "open-forge-context-entry",
            canonicalPath.Replace('/', Path.DirectorySeparatorChar)));
}
