using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Source;

public sealed class RouteMetadataParserListRegressionTests
{
    public static TheoryData<string, object> OpenForgeMetadataCases => new()
    {
        { "# Body only\n", RouteSourceMetadataState.Missing },
        { "---\nopen-forge:\n  description: Value\n  tags: [Tag]\n", RouteSourceMetadataState.Malformed },
        { "---\nopen-forge: [\n---\n", RouteSourceMetadataState.Malformed },
        { "---\nopen-forge:\n  tags: [Tag]\n---\n", RouteSourceMetadataState.Missing },
        { "---\nopen-forge:\n  description: '   '\n  tags: [Tag]\n---\n", RouteSourceMetadataState.Missing },
        { "---\nopen-forge:\n  description: Value\n  tags: []\n---\n", RouteSourceMetadataState.Missing },
        { "---\nopen-forge:\n  description: Value\n  tags: [1Invalid]\n---\n", RouteSourceMetadataState.Malformed },
        { "---\nopen-forge:\n  description: Value\n  tags: [Invalid-]\n---\n", RouteSourceMetadataState.Malformed },
        { "---\nopen-forge:\n  description: Value\n  tags: [Invalid--Tag]\n---\n", RouteSourceMetadataState.Malformed },
    };

    public static TheoryData<string, object> SkillMetadataCases => new()
    {
        { "skill body", RouteSourceMetadataState.Missing },
        { "---\nname: skill\ndescription: Description\n", RouteSourceMetadataState.Malformed },
        { "---\nname: [\ndescription: Description\n---\n", RouteSourceMetadataState.Malformed },
        { "---\ndescription: Description\n---\n", RouteSourceMetadataState.Missing },
        { "---\nname: skill\ndescription: '  '\n---\n", RouteSourceMetadataState.Missing },
    };

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Route-list metadata parser accepts fences with trailing whitespace and a byte order mark")]
    // Both are invisible and both are written by ordinary editors — a Windows editor writes
    // the mark by default. Rejecting either reported valid metadata as missing, naming the
    // wrong problem in a file whose frontmatter is visibly correct.
    [InlineData("--- \nopen-forge:\n  description: Value\n  tags: [Tag]\n---\n")]
    [InlineData("---\nopen-forge:\n  description: Value\n  tags: [Tag]\n--- \n")]
    [InlineData("﻿---\nopen-forge:\n  description: Value\n  tags: [Tag]\n---\n")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void OpenForgeMetadataAcceptsToleratedFences(string sourceBody)
    {
        var metadata = Parse(
            sourceBody,
            SourceDocumentForm.Markdown,
            isCompatibilityEntrypoint: false,
            isOverwritePresent: false);

        Assert.Equal(RouteSourceMetadataState.Complete, metadata.State);
        Assert.Equal("Value", metadata.Description);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Route-list metadata parser distinguishes missing and malformed Open Forge metadata"),
        MemberData(nameof(OpenForgeMetadataCases))]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void OpenForgeMetadataUsesNarrowFrontmatterAndRequiredValues(
        string sourceBody,
        object expectedStateValue)
    {
        var expectedState = Assert.IsType<RouteSourceMetadataState>(expectedStateValue);
        var metadata = Parse(
            sourceBody,
            SourceDocumentForm.Markdown,
            isCompatibilityEntrypoint: false,
            isOverwritePresent: false);

        Assert.Equal(expectedState, metadata.State);
        Assert.Null(metadata.Description);
        Assert.Empty(metadata.Tags);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Route-list metadata parser distinguishes missing and malformed native Skill metadata"),
        MemberData(nameof(SkillMetadataCases))]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void SkillMetadataRequiresTopLevelNameAndDescription(
        string sourceBody,
        object expectedStateValue)
    {
        var expectedState = Assert.IsType<RouteSourceMetadataState>(expectedStateValue);
        var metadata = Parse(
            sourceBody,
            SourceDocumentForm.Skill,
            isCompatibilityEntrypoint: false,
            isOverwritePresent: false);

        Assert.Equal(expectedState, metadata.State);
        Assert.Null(metadata.Description);
        Assert.Empty(metadata.Tags);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Route-list metadata carries safe descriptions for warning rows")]
    [InlineData("---\nopen-forge:\n  description: Value\n---\n", nameof(SourceDocumentForm.Markdown), nameof(RouteSourceMetadataState.Missing), "Value")]
    [InlineData("---\nopen-forge:\n  description: Value\n  tags: [1Invalid]\n---\n", nameof(SourceDocumentForm.Markdown), nameof(RouteSourceMetadataState.Malformed), "Value")]
    [InlineData("---\nopen-forge:\n  tags: [Tag]\n---\n", nameof(SourceDocumentForm.Markdown), nameof(RouteSourceMetadataState.Missing), null)]
    [InlineData("---\nname: skill\ndescription: '  '\n---\n", nameof(SourceDocumentForm.Skill), nameof(RouteSourceMetadataState.Missing), null)]
    [InlineData("---\nname: [skill]\ndescription: Description\n---\n", nameof(SourceDocumentForm.Skill), nameof(RouteSourceMetadataState.Malformed), "Description")]
    [InlineData("---\ndescription: Description\nname: [skill]\n---\n", nameof(SourceDocumentForm.Skill), nameof(RouteSourceMetadataState.Malformed), "Description")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void WarningMetadataPreservesObservedDescription(
        string sourceBody,
        string formName,
        string expectedStateName,
        string? expectedObservedDescription)
    {
        var metadata = Parse(
            sourceBody,
            Enum.Parse<SourceDocumentForm>(formName),
            isCompatibilityEntrypoint: false,
            isOverwritePresent: false);

        Assert.Equal(Enum.Parse<RouteSourceMetadataState>(expectedStateName), metadata.State);
        Assert.Equal(expectedObservedDescription, metadata.ObservedDescription);
        Assert.Null(metadata.Description);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Route-list metadata parser preserves exact Open Forge description, tags, and source flags")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void CompleteOpenForgeMetadataPreservesExactValues()
    {
        var metadata = Parse(
            "---\r\nopen-forge:\r\n  description: '  Exact description  '\r\n  tags: [Route-List, Évidence2]\r\n---\r\nbody",
            SourceDocumentForm.IndexEntrypoint,
            isCompatibilityEntrypoint: true,
            isOverwritePresent: true);

        Assert.Equal(RouteSourceMetadataState.Complete, metadata.State);
        Assert.Equal("  Exact description  ", metadata.Description);
        Assert.Equal(["Route-List", "Évidence2"], metadata.Tags);
        Assert.True(metadata.IsCompatibilityEntrypoint);
        Assert.True(metadata.IsOverwritePresent);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Route-list metadata parser preserves native Skill description with immutable empty tags")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void CompleteSkillMetadataUsesTopLevelValuesAndNoTags()
    {
        var metadata = Parse(
            "---\nname: native-skill\ndescription: Exact native description.\n---\nbody",
            SourceDocumentForm.Skill,
            isCompatibilityEntrypoint: false,
            isOverwritePresent: true);

        Assert.Equal(RouteSourceMetadataState.Complete, metadata.State);
        Assert.Equal("Exact native description.", metadata.Description);
        Assert.Empty(metadata.Tags);
        Assert.False(metadata.IsCompatibilityEntrypoint);
        Assert.True(metadata.IsOverwritePresent);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Route-list source metadata snapshots exact tags")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void SourceMetadataOwnsTagSnapshot()
    {
        var tags = new List<string> { "First", "Second" };
        var metadata = RouteSourceMetadata.Complete(
            "Description",
            tags,
            isCompatibilityEntrypoint: false,
            isOverwritePresent: false);
        tags.Clear();

        Assert.Equal(["First", "Second"], metadata.Tags);
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
