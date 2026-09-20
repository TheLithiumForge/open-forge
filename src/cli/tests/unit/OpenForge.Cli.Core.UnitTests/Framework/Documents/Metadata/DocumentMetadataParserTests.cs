using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Documents.Metadata;

public sealed class DocumentMetadataParserTests
{
    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Framework metadata reads flow and block tag sequences identically")]
    [InlineData("tags: [Docs, Architecture]")]
    [InlineData("tags:\n  - Docs\n  - Architecture")]
    [Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void ReadsSupportedTagSequenceStylesIdentically(string tags)
    {
        var facts = Parse(
            $"---\nopen-forge:\n  description: Route description\n  {tags}\n---\n");

        Assert.Equal(FrameworkDocumentMetadataState.Complete, facts.State);
        var metadata = Assert.IsType<FrameworkDocumentMetadata>(facts.Metadata);
        Assert.Equal(["Docs", "Architecture"], metadata.Tags);
        Assert.Equal(metadata.Tags, facts.ObservedTags);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Framework metadata recognizes only the Open Forge root")]
    [Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void RecognizesOnlyOpenForgeRoot()
    {
        var facts = Parse(
            "---\n"
                + "open-forge:\n"
                + "  description: '  Exact 描述  '\n"
                + "  tags: [Route-List, 工作2]\n"
                + "  responsibility: '  Exact owner  '\n"
                + "  extra: &opaque tolerated\n"
                + "  reference: *opaque\n"
                + "  nested:\n"
                + "    repeated: first\n"
                + "    repeated: second\n"
                + "unknown: tolerated\n"
                + "---\n");

        Assert.Equal(FrameworkDocumentMetadataState.Complete, facts.State);
        var metadata = Assert.IsType<FrameworkDocumentMetadata>(facts.Metadata);
        Assert.Equal("  Exact 描述  ", metadata.Description);
        Assert.Equal(["Route-List", "工作2"], metadata.Tags);
        Assert.Equal("  Exact owner  ", metadata.Responsibility);
        Assert.Equal(2, facts.TagSpans.Length);
        Assert.Equal(metadata.Tags, facts.ObservedTags);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Framework metadata ignores Rune as opaque unrelated YAML")]
    [Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void RuneOnlyMetadataIsAbsentEvenWithUnrelatedYamlFeatures()
    {
        var facts = Parse(
            "---\nrune:\n  description: &legacy Legacy\n  tags: [*legacy]\n  repeated: first\n  repeated: second\n---\n");

        Assert.Equal(FrameworkDocumentMetadataState.Missing, facts.State);
        Assert.Null(facts.Metadata);
        Assert.Empty(facts.TagSpans);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Framework metadata preserves only validated observed tags while incomplete")]
    [InlineData("---\nopen-forge:\n  tags: [Route-List, Évidence2]\n---\n", "Route-List|Évidence2")]
    [InlineData("---\nopen-forge:\n  description: Description\n---\n", "")]
    [InlineData("# Body\n", "")]
    [Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void PreservesObservedTagsWithoutRelaxingMetadataState(string source, string expectedObservedTags)
    {
        var facts = Parse(source);

        Assert.Equal(FrameworkDocumentMetadataState.Missing, facts.State);
        Assert.Null(facts.Metadata);
        Assert.Equal(expectedObservedTags, string.Join("|", facts.ObservedTags));
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Framework metadata reads Open Forge and ignores a sibling Rune root")]
    [Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void OpenForgeTakesSoleAuthorityWhenRuneIsAlsoPresent()
    {
        var facts = Parse(
            "---\nopen-forge:\n  description: Canonical\n  tags: [Canonical]\nrune:\n  description: &legacy Legacy\n  tags: [*legacy]\n---\n");

        Assert.Equal(FrameworkDocumentMetadataState.Complete, facts.State);
        var metadata = Assert.IsType<FrameworkDocumentMetadata>(facts.Metadata);
        Assert.Equal("Canonical", metadata.Description);
        Assert.Equal(["Canonical"], metadata.Tags);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Framework metadata reports absent or incomplete authored values as missing")]
    [InlineData("# Body\n")]
    [InlineData("---\nunknown: value\n---\n")]
    [InlineData("---\nopen-forge: null\n---\n")]
    [InlineData("---\nopen-forge: [value]\n---\n")]
    [InlineData("---\nopen-forge:\n  tags: [Tag]\n---\n")]
    [InlineData("---\nopen-forge:\n  description: '  '\n  tags: [Tag]\n---\n")]
    [InlineData("---\nopen-forge:\n  description: Value\n  tags: []\n---\n")]
    [InlineData("---\nopen-forge:\n  description: Value\n  tags: [Tag]\n  responsibility: '  '\n---\n")]
    [Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void MissingAuthoredValuesHaveNoImplicitMeaning(string source)
    {
        var facts = Parse(source);

        Assert.Equal(FrameworkDocumentMetadataState.Missing, facts.State);
        Assert.Null(facts.Metadata);
        Assert.Empty(facts.TagSpans);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Framework metadata fails closed for ambiguous or malformed authored values")]
    [InlineData("---\nopen-forge: [\n---\n")]
    [InlineData("---\nopen-forge:\n  description: &value Value\n  tags: [*value]\n---\n")]
    [InlineData("---\nopen-forge:\n  description: First\n  description: Second\n  tags: [Tag]\n---\n")]
    [InlineData("---\nopen-forge:\n  description: [Value]\n  tags: [Tag]\n---\n")]
    [InlineData("---\nopen-forge:\n  description: Value\n  tags: scalar\n---\n")]
    [InlineData("---\nopen-forge:\n  description: Value\n  tags: [[Tag]]\n---\n")]
    [InlineData("---\nopen-forge:\n  description: Value\n  tags: [Tag]\n  responsibility: [owner]\n---\n")]
    [InlineData("---\nopen-forge:\n  description: Value\n  tags: [1Invalid]\n---\n")]
    [Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void MalformedOrAmbiguousValuesHaveNoImplicitMeaning(string source)
    {
        var facts = Parse(source);

        Assert.Equal(FrameworkDocumentMetadataState.Malformed, facts.State);
        Assert.Null(facts.Metadata);
        Assert.Empty(facts.TagSpans);
        Assert.Empty(facts.ObservedTags);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Framework metadata preserves a safe description when required values are missing or malformed")]
    [InlineData("---\nopen-forge:\n  description: Value\n---\n", nameof(FrameworkDocumentMetadataState.Missing), "Value")]
    [InlineData("---\nopen-forge:\n  description: Value\n  tags: [1Invalid]\n---\n", nameof(FrameworkDocumentMetadataState.Malformed), "Value")]
    [InlineData("# Body\n", nameof(FrameworkDocumentMetadataState.Missing), null)]
    [Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void PreservesObservedDescriptionWithoutRelaxingMetadataState(
        string source,
        string expectedState,
        string? expectedObservedDescription)
    {
        var facts = Parse(source);

        Assert.Equal(Enum.Parse<FrameworkDocumentMetadataState>(expectedState), facts.State);
        Assert.Equal(expectedObservedDescription, facts.ObservedDescription);
        Assert.Null(facts.Metadata);
        Assert.Empty(facts.ObservedTags);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Framework metadata facts own and validate observed tags")]
    [Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void FactsOwnAndValidateObservedTags()
    {
        var tags = new List<string> { "First", "Second" };
        var facts = FrameworkDocumentMetadataFacts.WithoutValues(
            FrameworkDocumentMetadataState.Missing,
            observedTags: tags);
        tags.Clear();

        Assert.Equal(["First", "Second"], facts.ObservedTags);
        _ = Assert.Throws<ArgumentException>(() => FrameworkDocumentMetadataFacts.WithoutValues(
            FrameworkDocumentMetadataState.Missing,
            observedTags: ["1Invalid"]));
        _ = Assert.Throws<ArgumentException>(() => FrameworkDocumentMetadataFacts.WithoutValues(
            FrameworkDocumentMetadataState.Missing,
            observedTags: new[] { "Valid", (string)null! }));
    }

    private static FrameworkDocumentMetadataFacts Parse(string source)
    {
        var document = new MarkdownDocumentParser().Parse(source);
        return new FrameworkDocumentMetadataParser().Parse(document);
    }
}
