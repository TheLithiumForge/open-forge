using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.UnitTests.Framework.GeneratedNavigation;

public sealed class GeneratedNavigationSkillProjectionTests
{
    [Fact(DisplayName = "Generated navigation projects native Skills with one synthetic classification tag in stable order")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void NativeSkillsUseSyntheticClassificationWithoutAuthoredTags()
    {
        var parent = Source(
            ".agents/skills/_skills.md",
            "skills",
            SourceDocumentForm.CanonicalEntrypoint);
        var alpha = Source(
            ".agents/skills/alpha/SKILL.md",
            "skills/alpha",
            SourceDocumentForm.Skill);
        var zeta = Source(
            ".agents/skills/zeta/SKILL.md",
            "skills/zeta",
            SourceDocumentForm.Skill,
            withOverwrite: true);
        var topology = new SourceRouteTopology(
            nodes:
            [
                Node(parent, SourceRouteParentState.None, [], [zeta.Identity.CanonicalBasePath, alpha.Identity.CanonicalBasePath]),
                Node(alpha, SourceRouteParentState.Resolved, [parent.Identity.CanonicalBasePath], []),
                Node(zeta, SourceRouteParentState.Resolved, [parent.Identity.CanonicalBasePath], []),
            ],
            loaderRootPaths: []);
        var alphaFacts = SourceAuthoredMetadataFacts.Complete("Alpha skill", []);
        var zetaFacts = SourceAuthoredMetadataFacts.Complete("Zeta skill", []);
        var request = Request(
            topology,
            [parent, zeta, alpha],
            [new GeneratedNavigationRegionInput(parent, Document("stale"))],
            [new GeneratedNavigationMetadata(zeta, zetaFacts), new GeneratedNavigationMetadata(alpha, alphaFacts)]);

        var projection = new GeneratedNavigationProjector().Project(request);
        var repeated = new GeneratedNavigationProjector().Project(request);
        var region = Assert.Single(projection.Regions);
        var repeatedRegion = Assert.Single(repeated.Regions);

        Assert.Empty(alphaFacts.Tags);
        Assert.Empty(zetaFacts.Tags);
        Assert.Equal(
            ["alpha/SKILL.md", "zeta/SKILL.md"],
            region.Entries.Select(entry => entry.Destination));
        Assert.All(region.Entries, entry => Assert.Equal(["Skill"], entry.Tags));
        Assert.Equal(
            [
                "- [Alpha skill](alpha/SKILL.md) - #Skill",
                "- [Zeta skill](zeta/SKILL.md) - #Skill",
            ],
            region.Entries.Select(entry => entry.Line));
        Assert.NotNull(zeta.Overwrite);
        Assert.DoesNotContain("overwrite", region.ExpectedBody, StringComparison.Ordinal);
        Assert.Equal(region.ExpectedBody, repeatedRegion.ExpectedBody);
        Assert.Equal(region.Change?.ExpectedDocumentBytes, repeatedRegion.Change?.ExpectedDocumentBytes);
    }

    [Theory(DisplayName = "Generated navigation blocks absent, missing, and malformed native Skill metadata")]
    [InlineData("absent")]
    [InlineData(nameof(SourceAuthoredMetadataState.Missing))]
    [InlineData(nameof(SourceAuthoredMetadataState.Malformed))]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void NativeSkillRequiresCompleteMetadata(string metadataState)
    {
        var parent = Source(
            ".agents/skills/_skills.md",
            "skills",
            SourceDocumentForm.CanonicalEntrypoint);
        var skill = Source(
            ".agents/skills/example/SKILL.md",
            "skills/example",
            SourceDocumentForm.Skill);
        var topology = new SourceRouteTopology(
            nodes:
            [
                Node(parent, SourceRouteParentState.None, [], [skill.Identity.CanonicalBasePath]),
                Node(skill, SourceRouteParentState.Resolved, [parent.Identity.CanonicalBasePath], []),
            ],
            loaderRootPaths: []);
        IReadOnlyList<GeneratedNavigationMetadata> metadata = [];
        if (metadataState != "absent")
        {
            var state = Enum.Parse<SourceAuthoredMetadataState>(metadataState);
            metadata =
            [
                new GeneratedNavigationMetadata(
                    skill,
                    SourceAuthoredMetadataFacts.WithoutValues(state)),
            ];
        }

        var request = Request(
            topology,
            [parent, skill],
            [new GeneratedNavigationRegionInput(parent, Document("stale"))],
            metadata);

        if (metadataState == "absent")
        {
            Assert.Null(request.FindMetadata(skill));
        }

        var region = Assert.Single(new GeneratedNavigationProjector().Project(request).Regions);

        Assert.Equal(GeneratedNavigationRegionState.Unavailable, region.State);
        Assert.Empty(region.Entries);
        Assert.Null(region.Change);
        Assert.Contains("requires complete authored", region.Cause, StringComparison.Ordinal);
    }

    private static GeneratedNavigationProjectionRequest Request(
        SourceRouteTopology topology,
        IEnumerable<SourceLogicalSource> sources,
        IEnumerable<GeneratedNavigationRegionInput> regions,
        IEnumerable<GeneratedNavigationMetadata> metadata)
    {
        return new GeneratedNavigationProjectionRequest(
            topology: topology,
            sources: sources,
            regions: regions,
            metadata: metadata);
    }

    private static SourceRouteNode Node(
        SourceLogicalSource source,
        SourceRouteParentState parentState,
        IEnumerable<string> parentPaths,
        IEnumerable<string> childPaths)
    {
        return new SourceRouteNode(
            identity: source.Identity,
            parentState: parentState,
            parentPaths: parentPaths,
            childPaths: childPaths);
    }

    private static SourceLogicalSource Source(
        string canonicalPath,
        string automaticId,
        SourceDocumentForm form,
        bool withOverwrite = false)
    {
        var physicalPath = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "generated-navigation-skill-unit",
            canonicalPath.Replace('/', Path.DirectorySeparatorChar)));
        var baseLayer = new SourceLayer(
            canonicalPath: canonicalPath,
            physicalPath: physicalPath,
            form: form,
            kind: SourceLayerKind.Base);
        var overwrite = withOverwrite
            ? new SourceLayer(
                canonicalPath: canonicalPath[..^".md".Length] + ".overwrite.md",
                physicalPath: physicalPath[..^".md".Length] + ".overwrite.md",
                form: SourceDocumentForm.OverwriteCompanion,
                kind: SourceLayerKind.Overwrite)
            : null;
        return new SourceLogicalSource(
            new SourceLogicalIdentity(automaticId, canonicalPath),
            baseLayer,
            overwrite);
    }

    private static MarkdownDocumentFacts Document(string body)
    {
        var source = $"""
                # Skills

                ## Entries

                <!-- open-forge:generated-index:start -->
                {body}
                <!-- open-forge:generated-index:end -->

                """;
        return new MarkdownDocumentParser().Parse(source);
    }
}
