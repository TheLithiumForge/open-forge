using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.List.Shared.Filesystem;

public sealed class RouteListMetadataParserTests
{
    public static TheoryData<string, object> OpenForgeMetadataCases => new()
    {
        { "# Body only\n", RouteListMetadataState.Missing },
        { "--- \nopen-forge:\n  description: Value\n  tags: [Tag]\n---\n", RouteListMetadataState.Missing },
        { "---\nopen-forge:\n  description: Value\n  tags: [Tag]\n", RouteListMetadataState.Malformed },
        { "---\nopen-forge: [\n---\n", RouteListMetadataState.Malformed },
        { "---\nopen-forge:\n  tags: [Tag]\n---\n", RouteListMetadataState.Missing },
        { "---\nopen-forge:\n  description: '   '\n  tags: [Tag]\n---\n", RouteListMetadataState.Missing },
        { "---\nopen-forge:\n  description: Value\n  tags: []\n---\n", RouteListMetadataState.Missing },
        { "---\nopen-forge:\n  description: Value\n  tags: [1Invalid]\n---\n", RouteListMetadataState.Malformed },
        { "---\nopen-forge:\n  description: Value\n  tags: [Invalid-]\n---\n", RouteListMetadataState.Malformed },
        { "---\nopen-forge:\n  description: Value\n  tags: [Invalid--Tag]\n---\n", RouteListMetadataState.Malformed },
    };

    public static TheoryData<string, object> SkillMetadataCases => new()
    {
        { "skill body", RouteListMetadataState.Missing },
        { "---\nname: skill\ndescription: Description\n", RouteListMetadataState.Malformed },
        { "---\nname: [\ndescription: Description\n---\n", RouteListMetadataState.Malformed },
        { "---\ndescription: Description\n---\n", RouteListMetadataState.Missing },
        { "---\nname: skill\ndescription: '  '\n---\n", RouteListMetadataState.Missing },
    };

    public static TheoryData<string, bool> TagCases => new()
    {
        { "Tag", true },
        { "Évidence2", true },
        { "Route-List", true },
        { "", false },
        { "2Route", false },
        { "-Route", false },
        { "Route-", false },
        { "Route--List", false },
        { "Route_List", false },
        { "#Route", false },
        { "Route List", false },
    };

    [Theory(DisplayName = "Route-list metadata parser distinguishes missing and malformed Open Forge metadata"),
        MemberData(nameof(OpenForgeMetadataCases))]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void OpenForgeMetadataUsesNarrowFrontmatterAndRequiredValues(
        string sourceBody,
        object expectedStateValue)
    {
        var expectedState = Assert.IsType<RouteListMetadataState>(expectedStateValue);
        var metadata = new RouteListMetadataParser().ParseOpenForge(
            sourceBody,
            isCompatibilityEntrypoint: false,
            isOverwritePresent: false);

        Assert.Equal(expectedState, metadata.State);
        Assert.Null(metadata.Description);
        Assert.Empty(metadata.Tags);
    }

    [Theory(DisplayName = "Route-list metadata parser distinguishes missing and malformed native Skill metadata"),
        MemberData(nameof(SkillMetadataCases))]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void SkillMetadataRequiresTopLevelNameAndDescription(
        string sourceBody,
        object expectedStateValue)
    {
        var expectedState = Assert.IsType<RouteListMetadataState>(expectedStateValue);
        var metadata = new RouteListMetadataParser().ParseSkill(sourceBody, isOverwritePresent: false);

        Assert.Equal(expectedState, metadata.State);
        Assert.Null(metadata.Description);
        Assert.Empty(metadata.Tags);
    }

    [Fact(DisplayName = "Route-list metadata parser preserves exact Open Forge description, tags, and source flags")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void CompleteOpenForgeMetadataPreservesExactValues()
    {
        var metadata = new RouteListMetadataParser().ParseOpenForge(
            "---\r\nopen-forge:\r\n  description: '  Exact description  '\r\n  tags: [Route-List, Évidence2]\r\n---\r\nbody",
            isCompatibilityEntrypoint: true,
            isOverwritePresent: true);

        Assert.Equal(RouteListMetadataState.Complete, metadata.State);
        Assert.Equal("  Exact description  ", metadata.Description);
        Assert.Equal(["Route-List", "Évidence2"], metadata.Tags);
        Assert.True(metadata.IsCompatibilityEntrypoint);
        Assert.True(metadata.IsOverwritePresent);
    }

    [Fact(DisplayName = "Route-list metadata parser preserves native Skill description with immutable empty tags")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void CompleteSkillMetadataUsesTopLevelValuesAndNoTags()
    {
        var metadata = new RouteListMetadataParser().ParseSkill(
            "---\nname: native-skill\ndescription: Exact native description.\n---\nbody",
            isOverwritePresent: true);

        Assert.Equal(RouteListMetadataState.Complete, metadata.State);
        Assert.Equal("Exact native description.", metadata.Description);
        Assert.Empty(metadata.Tags);
        Assert.False(metadata.IsCompatibilityEntrypoint);
        Assert.True(metadata.IsOverwritePresent);
    }

    [Theory(DisplayName = "Route-list metadata tags begin with a letter and contain only alphanumerics and internal hyphens"),
        MemberData(nameof(TagCases))]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void TagGrammarIsFinite(string tag, bool expected)
    {
        Assert.Equal(expected, RouteListMetadataParser.IsValidTag(tag));
    }

    [Fact(DisplayName = "Route-list source metadata snapshots exact tags")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void SourceMetadataOwnsTagSnapshot()
    {
        var tags = new List<string> { "First", "Second" };
        var metadata = RouteListSourceMetadata.Complete(
            "Description",
            tags,
            isCompatibilityEntrypoint: false,
            isOverwritePresent: false);
        tags.Clear();

        Assert.Equal(["First", "Second"], metadata.Tags);
    }
}
