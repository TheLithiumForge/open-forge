using OpenForge.Cli.Core.Commands.Route.Shared.Loader;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Loader;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Loader;

public sealed class RouteLoaderEntriesParserListRegressionTests
{
    [Fact(DisplayName = "Loader Entries parser accepts an ordered canonical declaration set")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void EntriesParserPreservesDeclarationOrder()
    {
        var result = RouteLoaderEntriesParser.Parse(LoaderContents(
            "\n- [Second](second/_second.md) - #Second\n"
            + "- [First](first/_first.md) - #First\n",
            "## Axioms\n\n- inherited - No local axioms.\n\n"));

        Assert.Equal(RouteLoaderEntriesParseState.Valid, result.State);
        Assert.Equal(
            [".agents/second/_second.md", ".agents/first/_first.md"],
            result.Destinations.Select(destination => destination.CanonicalPath));
        Assert.All(result.Destinations, destination =>
            Assert.Equal(RouteLoaderDestinationParseState.Valid, destination.State));
        Assert.Null(result.Cause);
    }

    [Theory(DisplayName = "Loader Entries parser accepts both valid empty forms as zero destinations"),
        InlineData(""),
        InlineData("- none - No entries - #Empty")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void EntriesParserAcceptsEmptyMarkerRegionAndSentinel(string markerBody)
    {
        var result = RouteLoaderEntriesParser.Parse(LoaderContents(markerBody));

        Assert.Equal(RouteLoaderEntriesParseState.Valid, result.State);
        Assert.Empty(result.Destinations);
        Assert.Null(result.Cause);
    }

    [Fact(DisplayName = "Loader Entries parser keeps unsafe destinations for the resolver to block")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void EntriesParserRetainsUnsafeDestinationResults()
    {
        var result = RouteLoaderEntriesParser.Parse(LoaderContents(
            "- [Unsafe](root%2F%2F_root.md) - #Root"));

        Assert.Equal(RouteLoaderEntriesParseState.Valid, result.State);
        var destination = Assert.Single(result.Destinations);
        Assert.Equal(RouteLoaderDestinationParseState.Unsafe, destination.State);
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
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void EntriesParserRejectsMalformedSections(string loaderContents)
    {
        var result = RouteLoaderEntriesParser.Parse(loaderContents);

        Assert.Equal(RouteLoaderEntriesParseState.Malformed, result.State);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    [Fact(DisplayName = "Loader Entries parser reports malformed destination encoding as malformed input")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void EntriesParserRejectsMalformedDestinationEncoding()
    {
        var result = RouteLoaderEntriesParser.Parse(LoaderContents(
            "- [Root](root%2/_root.md) - #Root"));

        Assert.Equal(RouteLoaderEntriesParseState.Malformed, result.State);
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
