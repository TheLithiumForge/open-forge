using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Metadata;

public sealed class SourceAuthoredMetadataParserTests
{
    [Trait("Boundary", "Input")]
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
        Assert.Equal(facts.Tags, facts.ObservedTags);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Authored metadata ignores Rune as unrelated YAML")]
    [Trait("Feature", "source-metadata"), Trait("Evidence", "Unit")]
    public void RuneRootHasNoOpenForgeMetadataMeaning()
    {
        var facts = Parse(
            "---\nrune:\n  description: Legacy route\n  tags: [Legacy, 工作2]\n  responsibility: Ignored by source facts\n---\n",
            SourceDocumentForm.Markdown);

        Assert.Equal(SourceAuthoredMetadataState.Missing, facts.State);
        Assert.Null(facts.Description);
        Assert.Empty(facts.Tags);
        Assert.Empty(facts.ObservedTags);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Authored metadata reads Open Forge and ignores a sibling Rune root")]
    [Trait("Feature", "source-metadata"), Trait("Evidence", "Unit")]
    public void OpenForgeRemainsAuthoritativeAlongsideRune()
    {
        var facts = Parse(
            "---\n"
                + "open-forge:\n  description: Canonical\n  tags: [Canonical]\n"
                + "rune:\n  description: Legacy\n  tags: [Legacy]\n"
                + "---\n",
            SourceDocumentForm.Markdown);

        Assert.Equal(SourceAuthoredMetadataState.Complete, facts.State);
        Assert.Equal("Canonical", facts.Description);
        Assert.Equal(["Canonical"], facts.Tags);
        Assert.Equal(facts.Tags, facts.ObservedTags);
    }

    [Trait("Boundary", "Input")]
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

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Authored Open Forge metadata separates complete tags from observed tags")]
    [InlineData(
        "---\nopen-forge:\n  tags: [Route-List, Évidence2]\n---\n",
        nameof(SourceAuthoredMetadataState.Missing),
        null,
        null,
        "",
        "Route-List|Évidence2")]
    [InlineData(
        "---\nopen-forge:\n  description: Description\n---\n",
        nameof(SourceAuthoredMetadataState.Missing),
        null,
        "Description",
        "",
        "")]
    [InlineData("# body\n", nameof(SourceAuthoredMetadataState.Missing), null, null, "", "")]
    [InlineData(
        "---\nopen-forge:\n  description: Description\n  tags: [Route-List, Évidence2]\n---\n",
        nameof(SourceAuthoredMetadataState.Complete),
        "Description",
        "Description",
        "Route-List|Évidence2",
        "Route-List|Évidence2")]
    [Trait("Feature", "source-metadata"), Trait("Evidence", "Unit")]
    public void OpenForgeMetadataPreservesObservedTagsWithoutDeclaringCompleteness(
        string source,
        string expectedState,
        string? expectedDescription,
        string? expectedObservedDescription,
        string expectedTags,
        string expectedObservedTags)
    {
        var facts = Parse(source, SourceDocumentForm.Markdown);

        Assert.Equal(Enum.Parse<SourceAuthoredMetadataState>(expectedState), facts.State);
        Assert.Equal(expectedDescription, facts.Description);
        Assert.Equal(expectedObservedDescription, facts.ObservedDescription);
        Assert.Equal(expectedObservedTags, string.Join("|", facts.ObservedTags));
        if (string.IsNullOrEmpty(expectedTags))
        {
            Assert.Empty(facts.Tags);
        }
        else
        {
            Assert.Equal(expectedTags.Split('|'), facts.Tags);
        }
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Malformed Open Forge metadata never retains observed tags")]
    [InlineData("---\nopen-forge:\n  description: First\n  description: Second\n  tags: [Tag]\n---\n")]
    [InlineData("---\nopen-forge:\n  description: &value Value\n  tags: [*value]\n---\n")]
    [InlineData("---\nopen-forge:\n  description: Value\n  tags: scalar\n---\n")]
    [InlineData("---\nopen-forge:\n  description: Value\n  tags: [1Invalid]\n---\n")]
    [Trait("Feature", "source-metadata"), Trait("Evidence", "Unit")]
    public void MalformedOpenForgeMetadataHasNoObservedTags(string source)
    {
        var facts = Parse(source, SourceDocumentForm.Markdown);

        Assert.Equal(SourceAuthoredMetadataState.Malformed, facts.State);
        Assert.Empty(facts.ObservedTags);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Authored Skill metadata preserves native scalar aliases")]
    [InlineData("---\nname: &identity native-skill\ndescription: *identity\n---\n", "native-skill")]
    [Trait("Feature", "source-metadata"), Trait("Evidence", "Unit")]
    public void SkillPreservesNativeScalarAliases(string source, string expectedDescription)
    {
        var facts = Parse(source, SourceDocumentForm.Skill);

        Assert.Equal(SourceAuthoredMetadataState.Complete, facts.State);
        Assert.Equal(expectedDescription, facts.Description);
        Assert.Empty(facts.Tags);
        Assert.Empty(facts.ObservedTags);
    }

    [Trait("Boundary", "Input")]
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
        Assert.Empty(facts.ObservedTags);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Authored Skill metadata preserves a safe description across malformed names")]
    [InlineData("---\nname: skill\ndescription: '  '\n---\n", nameof(SourceAuthoredMetadataState.Missing), null)]
    [InlineData("---\nname: [skill]\ndescription: Description\n---\n", nameof(SourceAuthoredMetadataState.Malformed), "Description")]
    [InlineData("---\ndescription: Description\nname: [skill]\n---\n", nameof(SourceAuthoredMetadataState.Malformed), "Description")]
    [Trait("Feature", "source-metadata"), Trait("Evidence", "Unit")]
    public void SkillObservedDescriptionIsIndependentOfNameOrder(
        string source,
        string expectedState,
        string? expectedObservedDescription)
    {
        var facts = Parse(source, SourceDocumentForm.Skill);

        Assert.Equal(Enum.Parse<SourceAuthoredMetadataState>(expectedState), facts.State);
        Assert.Equal(expectedObservedDescription, facts.ObservedDescription);
        Assert.Null(facts.Description);
        Assert.Empty(facts.ObservedTags);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Authored Skill metadata reads name and description and ignores every other member")]
    // A Skill keeps the metadata its own runtime requires. Rejecting an unrecognized member
    // made a standard SKILL.md carrying `license` or `allowed-tools` block the workspace.
    [InlineData("---\nname: skill\ndescription: Description\nunmatched: value\n---\n")]
    [InlineData("---\nname: skill\ndescription: Description\nlicense: Apache-2.0\n---\n")]
    [InlineData("---\nname: skill\ndescription: Description\nallowed-tools: Read, Bash\n---\n")]
    [InlineData("---\nname: skill\ndescription: Description\nmetadata:\n  nested: true\n---\n")]
    [InlineData("---\nlicense: Apache-2.0\nname: skill\ndescription: Description\n---\n")]
    [Trait("Feature", "source-metadata"), Trait("Evidence", "Unit")]
    public void SkillIgnoresUnrecognizedMembers(string source)
    {
        var facts = Parse(source, SourceDocumentForm.Skill);

        Assert.Equal(SourceAuthoredMetadataState.Complete, facts.State);
        Assert.Equal("Description", facts.Description);
        Assert.Empty(facts.Tags);
        Assert.Equal(facts.Tags, facts.ObservedTags);
    }

    [Trait("Boundary", "Input")]
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
        Assert.Empty(facts.ObservedTags);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Authored metadata treats loaders as not applicable without interpreting content")]
    [Trait("Feature", "source-metadata"), Trait("Evidence", "Unit")]
    public void LoaderMetadataIsNotApplicable()
    {
        var facts = Parse("---\ninvalid: [\n---\n", SourceDocumentForm.Loader);

        Assert.Equal(SourceAuthoredMetadataState.NotApplicable, facts.State);
        Assert.Null(facts.Description);
        Assert.Empty(facts.Tags);
        Assert.Empty(facts.ObservedTags);
    }

    [Trait("Boundary", "Input")]
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

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Authored metadata owns an immutable authored-tag snapshot")]
    [Trait("Feature", "source-metadata"), Trait("Evidence", "Unit")]
    public void FactsOwnTheirTagSnapshot()
    {
        var tags = new List<string> { "First", "Second" };
        var facts = SourceAuthoredMetadataFacts.Complete("Description", tags);
        tags.Clear();

        Assert.Equal(["First", "Second"], facts.Tags);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Authored metadata facts own and validate observed tags")]
    [Trait("Feature", "source-metadata"), Trait("Evidence", "Unit")]
    public void FactsOwnAndValidateObservedTagSnapshot()
    {
        var tags = new List<string> { "First", "Second" };
        var facts = SourceAuthoredMetadataFacts.WithoutValues(
            SourceAuthoredMetadataState.Missing,
            observedTags: tags);
        tags.Clear();

        Assert.Equal(["First", "Second"], facts.ObservedTags);
        _ = Assert.Throws<ArgumentException>(() => SourceAuthoredMetadataFacts.WithoutValues(
            SourceAuthoredMetadataState.Missing,
            observedTags: ["1Invalid"]));
        _ = Assert.Throws<ArgumentException>(() => SourceAuthoredMetadataFacts.WithoutValues(
            SourceAuthoredMetadataState.Missing,
            observedTags: new[] { "Valid", (string)null! }));
    }

    [Trait("Boundary", "Processing")]
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
