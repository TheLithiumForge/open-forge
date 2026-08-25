using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Routing;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Routing;

public sealed class SourceLoaderEntriesParserListRegressionTests
{
    [Fact(DisplayName = "Loader Entries parser accepts an ordered canonical declaration set")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void EntriesParserPreservesDeclarationOrder()
    {
        var result = SourceLoaderEntriesParser.Parse(LoaderContents(
            "\n- [Second](second/_second.md) - #Second\n"
            + "- [First](first/_first.md) - #First\n",
            "## Axioms\n\n- inherited - No local axioms.\n\n"));

        Assert.Equal(SourceLoaderEntriesParseState.Valid, result.State);
        Assert.Equal(
            [".agents/second/_second.md", ".agents/first/_first.md"],
            result.Destinations.Select(destination => destination.CanonicalPath));
        Assert.All(result.Destinations, destination =>
            Assert.Equal(SourceLoaderDestinationParseState.Valid, destination.State));
        Assert.Null(result.Cause);
    }

    [Theory(DisplayName = "Loader Entries parser accepts both valid empty forms as zero destinations"),
        InlineData(""),
        InlineData("- none - No entries - #Empty")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void EntriesParserAcceptsEmptyMarkerRegionAndSentinel(string markerBody)
    {
        var result = SourceLoaderEntriesParser.Parse(LoaderContents(markerBody));

        Assert.Equal(SourceLoaderEntriesParseState.Valid, result.State);
        Assert.Empty(result.Destinations);
        Assert.Null(result.Cause);
    }

    [Fact(DisplayName = "Loader Entries parser keeps unsafe destinations for the resolver to block")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void EntriesParserRetainsUnsafeDestinationResults()
    {
        var result = SourceLoaderEntriesParser.Parse(LoaderContents(
            "- [Unsafe](root%2F%2F_root.md) - #Root"));

        Assert.Equal(SourceLoaderEntriesParseState.Valid, result.State);
        var destination = Assert.Single(result.Destinations);
        Assert.Equal(SourceLoaderDestinationParseState.Unsafe, destination.State);
        Assert.Equal("root//_root.md", destination.DecodedDestination);
    }

    [Theory(DisplayName = "Loader Entries parser rejects every non-canonical section or declaration shape"),
        InlineData("# Loader\n"),
        InlineData("# Loader\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n- [Root](root/_root.md) - #Root\n<!-- open-forge:generated-index:end -->\n\n## Notes\n"),
        InlineData("# Loader\n\n## Entries\n\n## Entries\n"),
        InlineData("# Loader\n\n## Entries\n\n<!-- open-forge:generated-index:end -->\n<!-- open-forge:generated-index:start -->\n"),
        InlineData("# Loader\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n<!-- open-forge:generated-index:start -->\n<!-- open-forge:generated-index:end -->\n"),
        InlineData("# Loader\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\nAuthored prose\n<!-- open-forge:generated-index:end -->\n"),
        InlineData("# Loader\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n- [Root](root/\n_root.md) - #Root\n<!-- open-forge:generated-index:end -->\n"),
        InlineData("# Loader\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n- none - No entries - #Empty\n- [Root](root/_root.md) - #Root\n<!-- open-forge:generated-index:end -->\n"),
        InlineData("# Loader\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n- [Root](root/_root.md) #Root\n<!-- open-forge:generated-index:end -->\n"),
        InlineData("# Loader\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n- [   ](root/_root.md) - #Root\n<!-- open-forge:generated-index:end -->\n"),
        InlineData("# Loader\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n- [Root](root/_root.md) - #1\n<!-- open-forge:generated-index:end -->\n"),
        InlineData("# Loader\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n- [Root](root/_root.md) - #A/B\n<!-- open-forge:generated-index:end -->\n"),
        InlineData("# Loader\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n- [Root](root/_root.md) - #Root)\n<!-- open-forge:generated-index:end -->\n"),
        InlineData("# Loader\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n- [Root](root/_root.md) - #Root-\n<!-- open-forge:generated-index:end -->\n"),
        InlineData("# Loader\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n- [forged [Root](root/_root.md) - #Root\n<!-- open-forge:generated-index:end -->\n")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void EntriesParserRejectsMalformedSections(string loaderContents)
    {
        var result = SourceLoaderEntriesParser.Parse(loaderContents);

        Assert.Equal(SourceLoaderEntriesParseState.Malformed, result.State);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    [Fact(DisplayName = "Loader Entries parser reports malformed destination encoding as malformed input")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void EntriesParserRejectsMalformedDestinationEncoding()
    {
        var result = SourceLoaderEntriesParser.Parse(LoaderContents(
            "- [Root](root%2/_root.md) - #Root"));

        Assert.Equal(SourceLoaderEntriesParseState.Malformed, result.State);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    private static string LoaderContents(string markerBody, string earlierSections = "")
    {
        return $"""
            # Open Forge Loader

            {earlierSections}
            ## Entries

            <!-- open-forge:generated-index:start -->
            {markerBody}
            <!-- open-forge:generated-index:end -->
            """;
    }
}
