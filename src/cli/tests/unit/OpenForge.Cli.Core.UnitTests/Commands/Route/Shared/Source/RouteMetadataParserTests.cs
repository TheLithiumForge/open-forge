using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Source;

public sealed class RouteMetadataParserTests
{
    [Theory(DisplayName = "Open Forge metadata parsing classifies missing and malformed frontmatter"),
        InlineData("# body\n", nameof(RouteSourceMetadataState.Missing)),
        InlineData("---\nopen-forge:\n  description: Value\n  tags: [Tag]\n", nameof(RouteSourceMetadataState.Malformed)),
        InlineData("---\nopen-forge: [\n---\n", nameof(RouteSourceMetadataState.Malformed)),
        InlineData("---\nopen-forge:\n  tags: [Tag]\n---\n", nameof(RouteSourceMetadataState.Missing)),
        InlineData("---\nopen-forge:\n  description: Value\n  tags: []\n---\n", nameof(RouteSourceMetadataState.Missing)),
        InlineData("---\nopen-forge:\n  description: Value\n  tags: [1Invalid]\n---\n", nameof(RouteSourceMetadataState.Malformed)),
        InlineData("---\nopen-forge:\n  description: Value\n  tags: [Invalid--Tag]\n---\n", nameof(RouteSourceMetadataState.Malformed))]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void OpenForgeMetadataUsesTheStrictAcceptedShape(
        string body,
        string expectedState)
    {
        var metadata = Parse(
            body,
            SourceDocumentForm.Markdown,
            isCompatibilityEntrypoint: false,
            isOverwritePresent: false);

        Assert.Equal(Enum.Parse<RouteSourceMetadataState>(expectedState), metadata.State);
        Assert.Null(metadata.Description);
        Assert.Empty(metadata.Tags);
    }

    [Fact(DisplayName = "Open Forge metadata parsing preserves Unicode descriptions, tags, and source flags")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void CompleteOpenForgeMetadataPreservesAuthoredValues()
    {
        var metadata = Parse(
            "---\r\nopen-forge:\r\n  description: '  Exact 描述  '\r\n  tags: [Route-List, Évidence2, 工作]\r\n---\r\nbody",
            SourceDocumentForm.IndexEntrypoint,
            isCompatibilityEntrypoint: true,
            isOverwritePresent: true);

        Assert.Equal(RouteSourceMetadataState.Complete, metadata.State);
        Assert.Equal("  Exact 描述  ", metadata.Description);
        Assert.Equal(["Route-List", "Évidence2", "工作"], metadata.Tags);
        Assert.True(metadata.IsCompatibilityEntrypoint);
        Assert.True(metadata.IsOverwritePresent);
    }

    [Theory(DisplayName = "Skill metadata parsing distinguishes complete, missing, and malformed native metadata"),
        InlineData("skill body", nameof(RouteSourceMetadataState.Missing)),
        InlineData("---\nname: skill\ndescription: Description\n", nameof(RouteSourceMetadataState.Malformed)),
        InlineData("---\nname: [\ndescription: Description\n---\n", nameof(RouteSourceMetadataState.Malformed)),
        InlineData("---\ndescription: Description\n---\n", nameof(RouteSourceMetadataState.Missing)),
        InlineData("---\nname: skill\ndescription: '  '\n---\n", nameof(RouteSourceMetadataState.Missing))]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void SkillMetadataUsesTopLevelNameAndDescription(
        string body,
        string expectedState)
    {
        var metadata = Parse(
            body,
            SourceDocumentForm.Skill,
            isCompatibilityEntrypoint: false,
            isOverwritePresent: false);

        Assert.Equal(Enum.Parse<RouteSourceMetadataState>(expectedState), metadata.State);
        Assert.Null(metadata.Description);
        Assert.Empty(metadata.Tags);
    }

    [Fact(DisplayName = "Skill metadata parsing preserves Unicode native name and description")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void CompleteSkillMetadataPreservesUnicodeValues()
    {
        var metadata = Parse(
            "---\nname: 工作技能\ndescription: 'Native 描述'\n---\nbody",
            SourceDocumentForm.Skill,
            isCompatibilityEntrypoint: false,
            isOverwritePresent: true);

        Assert.Equal(RouteSourceMetadataState.Complete, metadata.State);
        Assert.Equal("Native 描述", metadata.Description);
        Assert.Empty(metadata.Tags);
        Assert.False(metadata.IsCompatibilityEntrypoint);
        Assert.True(metadata.IsOverwritePresent);
    }

    private static RouteSourceMetadata Parse(
        string source,
        SourceDocumentForm form,
        bool isCompatibilityEntrypoint,
        bool isOverwritePresent)
    {
        var document = new MarkdownDocumentParser().Parse(source);
        var facts = new SourceAuthoredMetadataParser().Parse(document, form);
        return new RouteMetadataParser().Parse(
            facts,
            isCompatibilityEntrypoint,
            isOverwritePresent);
    }
}
