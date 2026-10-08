using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models.Syntax;
using OpenForge.Cli.Core.Framework.Documents.Yaml;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Documents.Metadata;

public sealed class DocumentMetadataParserTests
{
    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Framework metadata reads quoted root and scoped applyTo patterns")]
    [InlineData("applyTo: '**/*.cs'\n", "Root")]
    [InlineData("applyTo: '{src,test}/**/*.{cs,ts}, docs/[,]*'\n", "Root")]
    [InlineData("open-forge:\n  description: Route description\n  tags: [Docs]\n  applyTo: ['**/*.cs', 'src/*.cs']\n", "OpenForge")]
    [Trait("Feature", "apply-to-metadata"), Trait("Evidence", "Unit")]
    public void ReadsRootAndScopedApplyTo(string frontmatter, string location)
    {
        var facts = Parse($"---\n{frontmatter}---\n");

        Assert.Equal(ApplyToMetadataState.Valid, facts.ApplyTo.State);
        Assert.Equal(location, Assert.Single(facts.ApplyTo.Declarations).Location.ToString());
        Assert.NotEmpty(facts.ApplyTo.Patterns);
        if (facts.Metadata is { } metadata)
        {
            Assert.Equal(facts.ApplyTo.Patterns, metadata.ApplyTo);
        }
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Framework metadata retains root applyTo while ordinary metadata is missing")]
    [Trait("Feature", "apply-to-metadata"), Trait("Evidence", "Unit")]
    public void RootApplyToSurvivesMissingOpenForgeMetadata()
    {
        var facts = Parse("---\napplyTo: '**/*.cs'\n---\n");

        Assert.Equal(FrameworkDocumentMetadataState.Missing, facts.State);
        Assert.Equal(ApplyToMetadataState.Valid, facts.ApplyTo.State);
        Assert.Equal("**/*.cs", Assert.Single(facts.ApplyTo.Patterns).Text);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Framework metadata keeps complete ordinary values when applyTo is invalid")]
    [Trait("Feature", "apply-to-metadata"), Trait("Evidence", "Unit")]
    public void InvalidApplyToDoesNotChangeCompleteOrdinaryMetadata()
    {
        var facts = Parse(
            "---\nopen-forge:\n"
                + "  description: Route description\n"
                + "  tags: [Docs]\n"
                + "  applyTo: '**//*.cs'\n"
                + "---\n");

        Assert.Equal(FrameworkDocumentMetadataState.Complete, facts.State);
        Assert.Equal(ApplyToMetadataState.Invalid, facts.ApplyTo.State);
        var metadata = Assert.IsType<FrameworkDocumentMetadata>(facts.Metadata);
        Assert.Equal("Route description", metadata.Description);
        Assert.Equal(["Docs"], metadata.Tags);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Equivalent root and scoped applyTo declarations form one normalized condition")]
    [Trait("Feature", "apply-to-metadata"), Trait("Evidence", "Unit")]
    public void EquivalentDualDeclarationsNormalizeOnceAndRetainBothLocations()
    {
        var facts = Parse(
            "---\n"
                + "applyTo: ' src/*.cs, **/*.cs, src/*.cs '\n"
                + "open-forge:\n  description: Route description\n  tags: [Docs]\n  applyTo: ['**/*.cs', 'src/*.cs']\n"
                + "---\n");

        Assert.Equal(FrameworkDocumentMetadataState.Complete, facts.State);
        Assert.Equal(ApplyToMetadataState.Valid, facts.ApplyTo.State);
        Assert.Equal(["**/*.cs", "src/*.cs"], facts.ApplyTo.Patterns.Select(pattern => pattern.Text));
        Assert.Equal(2, facts.ApplyTo.Declarations.Length);
        Assert.Equal(2, Assert.IsType<FrameworkDocumentMetadata>(facts.Metadata).ApplyTo.Length);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Conflicting root and scoped applyTo declarations preserve complete ordinary metadata")]
    [Trait("Feature", "apply-to-metadata"), Trait("Evidence", "Unit")]
    public void ConflictingDualDeclarationsFailWithTheirLocation()
    {
        var facts = Parse(
            "---\n"
                + "applyTo: '**/*.cs'\n"
                + "open-forge:\n  description: Route description\n  tags: [Docs]\n  applyTo: '**/*.ts'\n"
                + "---\n");

        Assert.Equal(FrameworkDocumentMetadataState.Complete, facts.State);
        Assert.Equal(ApplyToMetadataState.Invalid, facts.ApplyTo.State);
        Assert.Equal(ApplyToMetadataFailureKind.ConflictingFields, facts.ApplyTo.Failure?.Kind);
        Assert.Equal(2, facts.ApplyTo.Declarations.Length);
        var metadata = Assert.IsType<FrameworkDocumentMetadata>(facts.Metadata);
        Assert.Equal("Route description", metadata.Description);
        Assert.Equal(["Docs"], metadata.Tags);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Framework metadata rejects repeated applyTo keys at either location")]
    [InlineData("applyTo: '**/*.cs'\napplyTo: '**/*.ts'\n", false)]
    [InlineData(
        "open-forge:\n  description: Route description\n  tags: [Docs]\n  applyTo: '**/*.cs'\n  applyTo: '**/*.ts'\n",
        true)]
    [Trait("Feature", "apply-to-metadata"), Trait("Evidence", "Unit")]
    public void DuplicateApplyToKeysAreInvalid(string value, bool completeOrdinaryMetadata)
    {
        var facts = Parse($"---\n{value}---\n");
        var expectedOrdinaryState = completeOrdinaryMetadata
            ? FrameworkDocumentMetadataState.Complete
            : FrameworkDocumentMetadataState.Missing;

        Assert.Equal(expectedOrdinaryState, facts.State);
        Assert.Equal(ApplyToMetadataState.Invalid, facts.ApplyTo.State);
        Assert.Equal(ApplyToMetadataFailureKind.DuplicateField, facts.ApplyTo.Failure?.Kind);
        Assert.Equal(2, facts.ApplyTo.Declarations.Length);
        if (expectedOrdinaryState == FrameworkDocumentMetadataState.Complete)
        {
            var metadata = Assert.IsType<FrameworkDocumentMetadata>(facts.Metadata);
            Assert.Equal("Route description", metadata.Description);
            Assert.Equal(["Docs"], metadata.Tags);
        }
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Framework metadata retains duplicate scoped applyTo declarations across duplicate open-forge mappings")]
    [Trait("Feature", "apply-to-metadata"), Trait("Evidence", "Unit")]
    public void DuplicateOpenForgeMappingsRetainScopedApplyToFailure()
    {
        var facts = Parse(
            "---\n"
                + "open-forge:\n  applyTo: '**/*.cs'\n"
                + "open-forge:\n  applyTo: '**/*.ts'\n"
                + "---\n");

        Assert.Equal(FrameworkDocumentMetadataState.Malformed, facts.State);
        Assert.Equal(ApplyToMetadataState.Invalid, facts.ApplyTo.State);
        Assert.Equal(ApplyToMetadataFailureKind.DuplicateField, facts.ApplyTo.Failure?.Kind);
        Assert.Equal(2, facts.ApplyTo.Declarations.Length);
        Assert.Equal(["**/*.cs", "**/*.ts"], facts.ApplyTo.Patterns.Select(pattern => pattern.Text));
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Framework metadata keeps missing ordinary values separate from invalid applyTo")]
    [InlineData("applyTo: src/*.cs\n")]
    [InlineData("applyTo: []\n")]
    [InlineData("applyTo: null\n")]
    [InlineData("applyTo: 17\n")]
    [InlineData("applyTo: ['**/*.cs', src/*.cs]\n")]
    [InlineData("applyTo: ''\n")]
    [InlineData("applyTo: '**//*.cs'\n")]
    [InlineData("applyTo: ' , , '\n")]
    [InlineData("applyTo: ['  ']\n")]
    [InlineData("applyTo: 'good,{bad'\n")]
    [Trait("Feature", "apply-to-metadata"), Trait("Evidence", "Unit")]
    public void InvalidApplyToMakesMetadataMalformed(string value)
    {
        var facts = Parse($"---\n{value}---\n");

        Assert.Equal(FrameworkDocumentMetadataState.Missing, facts.State);
        Assert.Equal(ApplyToMetadataState.Invalid, facts.ApplyTo.State);
        Assert.NotNull(facts.ApplyTo.Failure);
    }

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

    [Theory(DisplayName = "Root and scoped ordinary metadata share validation and partial observations")]
    [InlineData("description: Example\ntags: [Évidence2, 工作-2]\nresponsibility: Owns examples\n", (int)FrameworkDocumentMetadataState.Complete)]
    [InlineData("description: >\n  An authored\n  description\ntags:\n  - Docs\n", (int)FrameworkDocumentMetadataState.Complete)]
    [InlineData("description: Example\n", (int)FrameworkDocumentMetadataState.Missing)]
    [InlineData("tags: [Docs]\n", (int)FrameworkDocumentMetadataState.Missing)]
    [InlineData("description: ''\ntags: [Docs]\n", (int)FrameworkDocumentMetadataState.Missing)]
    [InlineData("description: Example\ntags: []\n", (int)FrameworkDocumentMetadataState.Missing)]
    [InlineData("description: Example\ntags: [Docs]\nresponsibility: ''\n", (int)FrameworkDocumentMetadataState.Missing)]
    [InlineData("description: [Wrong]\ntags: [Docs]\n", (int)FrameworkDocumentMetadataState.Malformed)]
    [InlineData("description: Example\ntags: [Invalid--Tag]\n", (int)FrameworkDocumentMetadataState.Malformed)]
    [InlineData("description: Example\ntags: {wrong: Docs}\n", (int)FrameworkDocumentMetadataState.Malformed)]
    [InlineData("description: Example\ntags: [Docs]\nresponsibility: [Wrong]\n", (int)FrameworkDocumentMetadataState.Malformed)]
    [InlineData("description: &text Example\ntags: [Docs]\nresponsibility: *text\n", (int)FrameworkDocumentMetadataState.Malformed)]
    [Trait("Boundary", "Input"), Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void RootAndScopedValuesUseIdenticalGrammar(string fields, int expectedState)
    {
        var root = Parse($"---\n{fields}---\n");
        var scoped = Parse($"---\nopen-forge:\n  {fields.Replace("\n", "\n  ", StringComparison.Ordinal)}\n---\n");

        Assert.Equal((FrameworkDocumentMetadataState)expectedState, root.State);
        Assert.Equal(root.State, scoped.State);
        Assert.Equal(root.FailureKind, scoped.FailureKind);
        Assert.Equal(root.ObservedDescription, scoped.ObservedDescription);
        Assert.Equal(root.ObservedTags, scoped.ObservedTags);
        Assert.Equal(root.Metadata?.Description, scoped.Metadata?.Description);
        Assert.Equal(root.Metadata?.Tags, scoped.Metadata?.Tags);
        Assert.Equal(root.Metadata?.Responsibility, scoped.Metadata?.Responsibility);
        Assert.Equal(FrontmatterForm.Root, root.Syntax.AuthoredForm);
        Assert.Equal(FrontmatterForm.Scoped, scoped.Syntax.AuthoredForm);
    }

    [Theory(DisplayName = "A scoped mapping owns the complete ordinary set even when fields are missing")]
    [InlineData("{}", null)]
    [InlineData("\n  description: Scoped", "Scoped")]
    [InlineData("\n  tags: [Scoped]", null)]
    [Trait("Boundary", "Input"), Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void ScopedMappingOwnsEntireMetadataSet(string mapping, string? description)
    {
        var facts = Parse($"---\ndescription: Foreign\nresponsibility: [Foreign]\ntags: [Foreign]\nopen-forge: {mapping}\n---\n");

        Assert.Equal(FrameworkDocumentMetadataState.Missing, facts.State);
        Assert.Equal(description, facts.ObservedDescription);
        Assert.DoesNotContain("Foreign", facts.ObservedTags);
        Assert.All(facts.Syntax.Members, member => Assert.Equal(FrontmatterForm.Scoped, member.Form));
    }

    [Fact(DisplayName = "Unsupported root keys remain opaque to ordinary metadata validation")]
    [Trait("Boundary", "Input"), Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void UnknownRootValuesRemainOpaque()
    {
        var facts = Parse("---\ndescription: Example\ntags: [Docs]\nforeign: &foreign {key: value, key: other}\nforeign: *foreign\n? [complex, key]\n: [value]\n---\n");

        Assert.Equal(FrameworkDocumentMetadataState.Complete, facts.State);
        Assert.Equal(2, facts.Syntax.Members.Length);
        Assert.Equal(["Docs"], facts.Metadata?.Tags);
    }

    [Theory(DisplayName = "Selected metadata syntax retains partial fields and duplicate member coordinates")]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [Trait("Boundary", "Input"), Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void OriginsRetainPartialAndDuplicateCoordinates(bool scoped, bool duplicate)
    {
        var fields = "tags: [\"Caf\\u00E9\", 工作]\nresponsibility: Owner\n";
        if (duplicate)
        {
            fields += "tags: [Later]\n";
        }
        var yaml = scoped ? $"open-forge:\n  {fields.Replace("\n", "\n  ", StringComparison.Ordinal)}\n" : fields;
        var syntax = new YamlDocumentParser().Parse(yaml);
        var facts = new FrameworkDocumentMetadataParser().Read(syntax, FrameworkMetadataReadScope.RoutedSource);

        Assert.Equal(duplicate ? FrameworkDocumentMetadataState.Malformed : FrameworkDocumentMetadataState.Missing, facts.State);
        Assert.Empty(facts.TagSpans);
        Assert.Equal(scoped ? FrontmatterForm.Scoped : FrontmatterForm.Root, facts.Syntax.AuthoredForm);
        Assert.Equal(duplicate ? 3 : 2, facts.Syntax.Members.Length);
        var tags = facts.Syntax.Members[0];
        Assert.Equal(FrameworkMetadataField.Tags, tags.Field);
        Assert.Equal("tags", yaml[tags.KeySpan.Start..tags.KeySpan.End]);
        Assert.Equal("[\"Caf\\u00E9\", 工作]", yaml[tags.ValueSpan.Start..tags.ValueSpan.End]);
        Assert.Equal(["\"Caf\\u00E9\"", "工作"], tags.TagItemSpans.Select(span => yaml[span.Start..span.End]));
        var selected = Assert.IsType<YamlNode>(facts.Syntax.SelectedMapping);
        Assert.Same(selected.Mapping?[0], tags.Entry);
        if (scoped)
        {
            var entry = Assert.IsType<YamlMappingEntry>(facts.Syntax.ScopedEntry);
            Assert.Same(entry.Value, selected);
            Assert.Equal(entry.Key.Span, facts.Syntax.ScopedKeySpan);
            Assert.Equal(entry.Value.Span, facts.Syntax.ScopedValueSpan);
        }
        if (duplicate)
        {
            Assert.Equal(facts.Syntax.Members[2].KeySpan, facts.FailureSpan);
            Assert.Equal(FrameworkDocumentMetadataFailureKind.Duplicate, facts.FailureKind);
        }
        else
        {
            Assert.Equal(["Café", "工作"], facts.ObservedTags);
        }
    }

    [Fact(DisplayName = "Duplicate scoped containers retain the original later key failure coordinate")]
    [Trait("Boundary", "Input"), Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void DuplicateScopeRetainsItsFailureCoordinate()
    {
        const string yaml = "open-forge: {}\nopen-forge: {}\n";
        var facts = new FrameworkDocumentMetadataParser().Read(new YamlDocumentParser().Parse(yaml), FrameworkMetadataReadScope.RoutedSource);

        Assert.Equal(FrameworkDocumentMetadataFailureKind.Duplicate, facts.FailureKind);
        Assert.Equal(yaml.LastIndexOf("open-forge", StringComparison.Ordinal), facts.FailureSpan?.Start);
        Assert.Equal(FrontmatterForm.Scoped, facts.Syntax.AuthoredForm);
        Assert.NotNull(facts.Syntax.ScopedEntry);
    }

    [Theory(DisplayName = "Scoped-only reading ignores root ordinary values while retaining applicability")]
    [InlineData("description: Native\ntags: [Docs]\n")]
    [InlineData("description: [Native]\ntags: [invalid tag]\n")]
    [Trait("Boundary", "Input"), Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void ScopedOnlyReadingIgnoresRootOrdinaryFields(string fields)
    {
        var document = new MarkdownDocumentParser().Parse($"---\n{fields}applyTo: '**/*.cs'\n---\n");
        var facts = new FrameworkDocumentMetadataParser().Parse(document, FrameworkMetadataReadScope.ScopedOnly);

        Assert.Equal(FrameworkDocumentMetadataState.Missing, facts.State);
        Assert.Null(facts.ObservedDescription);
        Assert.Empty(facts.ObservedTags);
        Assert.Empty(facts.Syntax.Members);
        Assert.Equal(ApplyToMetadataState.Valid, facts.ApplyTo.State);
    }

    [Theory(DisplayName = "Explicit nonmapping scoped keys keep their existing state without root fallback")]
    [InlineData("", false)]
    [InlineData("null", false)]
    [InlineData("[]", false)]
    [InlineData("[Scoped]", false)]
    [InlineData("*foreign", true)]
    [Trait("Boundary", "Input"), Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void ExplicitNonMappingScopeKeyNeverFallsBackToRoot(string value, bool malformed)
    {
        var facts = Parse($"---\nforeign: &foreign {{description: Alias, tags: [Alias]}}\ndescription: Root\ntags: [Root]\nopen-forge: {value}\n---\n");

        Assert.Equal(malformed ? FrameworkDocumentMetadataState.Malformed : FrameworkDocumentMetadataState.Missing, facts.State);
        Assert.Null(facts.ObservedDescription);
        Assert.Empty(facts.ObservedTags);
        Assert.Equal(FrontmatterForm.Scoped, facts.Syntax.AuthoredForm);
        Assert.NotNull(facts.Syntax.ScopedEntry);
        Assert.Empty(facts.Syntax.Members);
    }

    [Fact(DisplayName = "Applicability alone and absent frontmatter do not establish ordinary metadata authorship")]
    [Trait("Boundary", "Input"), Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void MissingOrdinaryMetadataHasSyntaxWithoutAnAuthoredForm()
    {
        Assert.Null(Parse("# Body\n").Syntax.AuthoredForm);
        Assert.Null(Parse("---\napplyTo: '**/*.cs'\n---\n").Syntax.AuthoredForm);
    }

    private static FrameworkDocumentMetadataFacts Parse(string source)
    {
        var document = new MarkdownDocumentParser().Parse(source);
        return new FrameworkDocumentMetadataParser().Parse(document, FrameworkMetadataReadScope.RoutedSource);
    }
}
