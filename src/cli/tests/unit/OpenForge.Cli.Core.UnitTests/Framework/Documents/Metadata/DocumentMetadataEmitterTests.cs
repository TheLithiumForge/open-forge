using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Documents.Metadata;

public sealed class DocumentMetadataEmitterTests
{
    [Fact(DisplayName = "Framework metadata emits one ordered Open Forge root without absent values")]
    [Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void EmitsCanonicalOpenForgeDocumentWithoutResponsibility()
    {
        var yaml = new FrameworkDocumentMetadataEmitter().Emit(
            new FrameworkDocumentMetadata("Route description", ["NeedsAuthoring", "工作2"], null));

        Assert.Equal(
            "open-forge:\n"
                + "  description: Route description\n"
                + "  tags: [NeedsAuthoring, 工作2]\n",
            yaml);
        Assert.DoesNotContain("rune", yaml, StringComparison.Ordinal);
        Assert.DoesNotContain("responsibility", yaml, StringComparison.Ordinal);
        Assert.DoesNotContain("\r", yaml, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Framework metadata emits responsibility after ordered tags")]
    [Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void EmitsCanonicalOpenForgeDocumentWithResponsibility()
    {
        var yaml = new FrameworkDocumentMetadataEmitter().Emit(
            new FrameworkDocumentMetadata(
                "Route description",
                ["NeedsAuthoring"],
                "Owns route documentation."));

        Assert.Equal(
            "open-forge:\n"
                + "  description: Route description\n"
                + "  tags: [NeedsAuthoring]\n"
                + "  responsibility: Owns route documentation.\n",
            yaml);
    }

    [Fact(DisplayName = "Framework metadata round trips Unicode and YAML-sensitive scalars")]
    [Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void EmittedYamlRoundTripsThroughCanonicalParser()
    {
        var expected = new FrameworkDocumentMetadata(
            "true: # 路由",
            ["Évidence2", "工作-2"],
            "null: # ownership");
        var yaml = new FrameworkDocumentMetadataEmitter().Emit(expected);
        var document = new MarkdownDocumentParser().Parse($"---\n{yaml}---\n# Body\n");

        var facts = new FrameworkDocumentMetadataParser().Parse(document);

        Assert.Equal(FrameworkDocumentMetadataState.Complete, facts.State);
        var actual = Assert.IsType<FrameworkDocumentMetadata>(facts.Metadata);
        Assert.Equal(expected.Description, actual.Description);
        Assert.Equal(expected.Tags, actual.Tags);
        Assert.Equal(expected.Responsibility, actual.Responsibility);
    }
}
