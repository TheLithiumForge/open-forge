using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Metadata;

public sealed class SourceAuthoredMetadataParserTests
{
    [Theory(DisplayName = "Authored metadata preserves Open Forge semantics for every Markdown base form")]
    [InlineData(nameof(SourceDocumentForm.Markdown))]
    [InlineData(nameof(SourceDocumentForm.CanonicalEntrypoint))]
    [InlineData(nameof(SourceDocumentForm.IndexEntrypoint))]
    [InlineData(nameof(SourceDocumentForm.UnderscoreIndexEntrypoint))]
    [InlineData(nameof(SourceDocumentForm.ReferencesEntrypoint))]
    [InlineData(nameof(SourceDocumentForm.UnderscoreReferencesEntrypoint))]
    [Trait("Feature", "source-metadata"), Trait("Evidence", "Unit")]
    public void OpenForgeFormsPreserveExactAuthoredValues(string formName)
    {
        var facts = Parse(
            "---\r\nopen-forge:\r\n  description: '  Exact 描述  '\r\n  tags: [Route-List, Évidence2, 工作]\r\n---\r\nbody",
            Enum.Parse<SourceDocumentForm>(formName));

        Assert.Equal(SourceAuthoredMetadataState.Complete, facts.State);
        Assert.Equal("  Exact 描述  ", facts.Description);
        Assert.Equal(["Route-List", "Évidence2", "工作"], facts.Tags);
    }

    [Theory(DisplayName = "Authored metadata keeps the accepted Open Forge missing and malformed classifications")]
    [InlineData("# body\n", nameof(SourceAuthoredMetadataState.Missing))]
    [InlineData("---\nopen-forge:\n  description: Value\n  tags: [Tag]\n", nameof(SourceAuthoredMetadataState.Malformed))]
    [InlineData("---\nopen-forge: [\n---\n", nameof(SourceAuthoredMetadataState.Malformed))]
    [InlineData("---\nopen-forge:\n  tags: [Tag]\n---\n", nameof(SourceAuthoredMetadataState.Missing))]
    [InlineData("---\nopen-forge:\n  description: Value\n  tags: []\n---\n", nameof(SourceAuthoredMetadataState.Missing))]
    [InlineData("---\nopen-forge:\n  description: Value\n  tags: [1Invalid]\n---\n", nameof(SourceAuthoredMetadataState.Malformed))]
    [InlineData("---\nopen-forge:\n  description: &value Value\n  tags: [*value]\n---\n", nameof(SourceAuthoredMetadataState.Malformed))]
    [Trait("Feature", "source-metadata"), Trait("Evidence", "Unit")]
    public void OpenForgeFormsRetainAcceptedStateSemantics(string source, string expectedState)
    {
        var facts = Parse(source, SourceDocumentForm.Markdown);

        Assert.Equal(Enum.Parse<SourceAuthoredMetadataState>(expectedState), facts.State);
        Assert.Null(facts.Description);
        Assert.Empty(facts.Tags);
    }

    [Theory(DisplayName = "Authored Skill metadata preserves native scalar aliases")]
    [InlineData("---\nname: &identity native-skill\ndescription: *identity\n---\n", "native-skill")]
    [Trait("Feature", "source-metadata"), Trait("Evidence", "Unit")]
    public void SkillPreservesNativeScalarAliases(string source, string expectedDescription)
    {
        var facts = Parse(source, SourceDocumentForm.Skill);

        Assert.Equal(SourceAuthoredMetadataState.Complete, facts.State);
        Assert.Equal(expectedDescription, facts.Description);
        Assert.Empty(facts.Tags);
    }

    [Theory(DisplayName = "Authored Skill metadata keeps native missing and malformed classifications")]
    [InlineData("skill body", nameof(SourceAuthoredMetadataState.Missing))]
    [InlineData("---\n---\n", nameof(SourceAuthoredMetadataState.Missing))]
    [InlineData("---\nnull\n---\n", nameof(SourceAuthoredMetadataState.Missing))]
    [InlineData("---\n~\n---\n", nameof(SourceAuthoredMetadataState.Missing))]
    [InlineData("---\nname: skill\ndescription: Description\n", nameof(SourceAuthoredMetadataState.Malformed))]
    [InlineData("---\ndescription: Description\n---\n", nameof(SourceAuthoredMetadataState.Missing))]
    [InlineData("---\nname: skill\ndescription: '  '\n---\n", nameof(SourceAuthoredMetadataState.Missing))]
    [InlineData("---\nname: null\ndescription: Description\n---\n", nameof(SourceAuthoredMetadataState.Missing))]
    [InlineData("---\nname: NULL\ndescription: Description\n---\n", nameof(SourceAuthoredMetadataState.Missing))]
    [InlineData("---\nname: skill\ndescription: ~\n---\n", nameof(SourceAuthoredMetadataState.Missing))]
    [InlineData("---\nname: &missing null\ndescription: *missing\n---\n", nameof(SourceAuthoredMetadataState.Missing))]
    [InlineData("---\nname: skill\ndescription: Description\nunmatched: value\n---\n", nameof(SourceAuthoredMetadataState.Malformed))]
    [InlineData("---\nname: first\nname: second\ndescription: Description\n---\n", nameof(SourceAuthoredMetadataState.Malformed))]
    [InlineData("---\nname: [skill]\ndescription: Description\n---\n", nameof(SourceAuthoredMetadataState.Malformed))]
    [InlineData("---\nname: skill\ndescription: { value: Description }\n---\n", nameof(SourceAuthoredMetadataState.Malformed))]
    [InlineData("---\nname: skill\ndescription: *unknown\n---\n", nameof(SourceAuthoredMetadataState.Malformed))]
    [InlineData("---\ndescription: *identity\nname: &identity native-skill\n---\n", nameof(SourceAuthoredMetadataState.Malformed))]
    [InlineData("---\nname: &identity [skill]\ndescription: *identity\n---\n", nameof(SourceAuthoredMetadataState.Malformed))]
    [Trait("Feature", "source-metadata"), Trait("Evidence", "Unit")]
    public void SkillRetainsAcceptedStateSemantics(string source, string expectedState)
    {
        var facts = Parse(source, SourceDocumentForm.Skill);

        Assert.Equal(Enum.Parse<SourceAuthoredMetadataState>(expectedState), facts.State);
        Assert.Null(facts.Description);
        Assert.Empty(facts.Tags);
    }

    [Fact(DisplayName = "Authored Skill metadata keeps quoted null spellings as scalar values")]
    [Trait("Feature", "source-metadata"), Trait("Evidence", "Unit")]
    public void SkillKeepsQuotedNullValues()
    {
        var facts = Parse(
            "---\nname: \"null\"\ndescription: 'null'\n---\n",
            SourceDocumentForm.Skill);

        Assert.Equal(SourceAuthoredMetadataState.Complete, facts.State);
        Assert.Equal("null", facts.Description);
        Assert.Empty(facts.Tags);
    }

    [Fact(DisplayName = "Authored metadata treats loaders as not applicable without interpreting content")]
    [Trait("Feature", "source-metadata"), Trait("Evidence", "Unit")]
    public void LoaderMetadataIsNotApplicable()
    {
        var facts = Parse("---\ninvalid: [\n---\n", SourceDocumentForm.Loader);

        Assert.Equal(SourceAuthoredMetadataState.NotApplicable, facts.State);
        Assert.Null(facts.Description);
        Assert.Empty(facts.Tags);
    }

    [Fact(DisplayName = "Authored metadata rejects overwrite companions and undefined forms as logical bases")]
    [Trait("Feature", "source-metadata"), Trait("Evidence", "Unit")]
    public void InvalidLogicalBaseFormsFailFast()
    {
        var document = new MarkdownDocumentParser().Parse("# body\n");
        var parser = new SourceAuthoredMetadataParser();

        _ = Assert.Throws<ArgumentOutOfRangeException>(
            () => parser.Parse(document, SourceDocumentForm.OverwriteCompanion));
        _ = Assert.Throws<ArgumentOutOfRangeException>(
            () => parser.Parse(document, (SourceDocumentForm)int.MaxValue));
    }

    [Fact(DisplayName = "Authored metadata owns an immutable authored-tag snapshot")]
    [Trait("Feature", "source-metadata"), Trait("Evidence", "Unit")]
    public void FactsOwnTheirTagSnapshot()
    {
        var tags = new List<string> { "First", "Second" };
        var facts = SourceAuthoredMetadataFacts.Complete("Description", tags);
        tags.Clear();

        Assert.Equal(["First", "Second"], facts.Tags);
    }

    [Theory(DisplayName = "Authored Open Forge tags use the accepted source grammar")]
    [InlineData("Tag", true)]
    [InlineData("Évidence2", true)]
    [InlineData("工作-2", true)]
    [InlineData("", false)]
    [InlineData("2Route", false)]
    [InlineData("-Route", false)]
    [InlineData("Route-", false)]
    [InlineData("Route--List", false)]
    [InlineData("Route_List", false)]
    [InlineData("#Route", false)]
    [InlineData("Route List", false)]
    [InlineData("Route%20List", false)]
    [Trait("Feature", "source-metadata"), Trait("Evidence", "Unit")]
    public void TagGrammarRemainsAuthoritative(string tag, bool expected)
    {
        Assert.Equal(expected, SourceOpenForgeMetadataParser.IsValidTag(tag));
    }

    private static SourceAuthoredMetadataFacts Parse(string source, SourceDocumentForm form)
    {
        var document = new MarkdownDocumentParser().Parse(source);
        return new SourceAuthoredMetadataParser().Parse(document, form);
    }
}
