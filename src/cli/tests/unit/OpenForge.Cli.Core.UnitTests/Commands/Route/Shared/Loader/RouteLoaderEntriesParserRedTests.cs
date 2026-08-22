using OpenForge.Cli.Core.Commands.Route.Shared.Loader;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Loader;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Loader;

public sealed class RouteLoaderEntriesParserRedTests
{
    [Fact(DisplayName = "Loader Entries parsing preserves exact declaration order and canonical destinations")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void DeclarationsRemainInAuthoredOrder()
    {
        var result = RouteLoaderEntriesParser.Parse(LoaderContents(
            "\n- [Second](second/_second.md) - #Second\n"
            + "- [First](first/_first.md) - #First\n"));

        Assert.Equal(RouteLoaderEntriesParseState.Valid, result.State);
        Assert.Equal(
            [".agents/second/_second.md", ".agents/first/_first.md"],
            result.Destinations.Select(destination => destination.CanonicalPath));
        Assert.All(result.Destinations, destination =>
            Assert.Equal(RouteLoaderDestinationParseState.Valid, destination.State));
        Assert.Null(result.Cause);
        Assert.Null(result.AttemptedDestination);
    }

    [Theory(DisplayName = "Loader Entries parsing accepts an empty marker region and its exact empty sentinel"),
        InlineData(""),
        InlineData("- none - No entries - #Empty")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void EmptyDeclarationsProduceAnEmptyValidSet(string markerBody)
    {
        var result = RouteLoaderEntriesParser.Parse(LoaderContents(markerBody));

        Assert.Equal(RouteLoaderEntriesParseState.Valid, result.State);
        Assert.Empty(result.Destinations);
        Assert.Null(result.Cause);
    }

    [Fact(DisplayName = "Loader Entries parsing retains an unsafe destination for the boundary resolver")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void UnsafeDestinationRemainsAParsedDestination()
    {
        var result = RouteLoaderEntriesParser.Parse(
            LoaderContents("- [Unsafe](root%2F%2F_root.md) - #Root"));

        Assert.Equal(RouteLoaderEntriesParseState.Valid, result.State);
        var destination = Assert.Single(result.Destinations);
        Assert.Equal(RouteLoaderDestinationParseState.Unsafe, destination.State);
        Assert.Equal("root//_root.md", destination.DecodedDestination);
    }

    [Theory(DisplayName = "Loader Entries parsing rejects missing markers duplicate markers prose and declaration-shape drift"),
        InlineData("# Open Forge Loader\n"),
        InlineData("# Open Forge Loader\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n- [Root](root/_root.md) - #Root\n<!-- open-forge:generated-index:end -->\n\n## Notes\n"),
        InlineData("# Open Forge Loader\n\n## Entries\n\n## Entries\n"),
        InlineData("# Open Forge Loader\n\n## Entries\n\n<!-- open-forge:generated-index:end -->\n<!-- open-forge:generated-index:start -->\n"),
        InlineData("# Open Forge Loader\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\nAuthored prose\n<!-- open-forge:generated-index:end -->\n"),
        InlineData("# Open Forge Loader\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n- [Root](root/_root.md) #Root\n<!-- open-forge:generated-index:end -->\n"),
        InlineData("# Open Forge Loader\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n- [   ](root/_root.md) - #Root\n<!-- open-forge:generated-index:end -->\n"),
        InlineData("# Open Forge Loader\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n- [Root](root/_root.md) - #1\n<!-- open-forge:generated-index:end -->\n"),
        InlineData("# Open Forge Loader\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n- none - No entries - #Empty\n- [Root](root/_root.md) - #Root\n<!-- open-forge:generated-index:end -->\n")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void MalformedEntriesRetainAParseCause(string contents)
    {
        var result = RouteLoaderEntriesParser.Parse(contents);

        Assert.Equal(RouteLoaderEntriesParseState.Malformed, result.State);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    [Fact(DisplayName = "Loader Entries parsing reports malformed destination encoding as malformed input")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void MalformedDestinationEncodingInvalidatesTheEntriesRegion()
    {
        var result = RouteLoaderEntriesParser.Parse(
            LoaderContents("- [Root](root%2/_root.md) - #Root"));

        Assert.Equal(RouteLoaderEntriesParseState.Malformed, result.State);
        Assert.Equal("root%2/_root.md", result.AttemptedDestination);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    private static string LoaderContents(string markerBody)
    {
        return $"""
            # Open Forge Loader

            ## Entries

            <!-- open-forge:generated-index:start -->
            {markerBody}
            <!-- open-forge:generated-index:end -->
            """;
    }
}
