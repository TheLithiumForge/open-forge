using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Routing;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Routing;

public sealed class SourceLoaderParserTests
{
    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Neutral Loader destination parsing decodes UTF-8 and percent escapes exactly once"),
        InlineData("root/_root.md", "root/_root.md", ".agents/root/_root.md"),
        InlineData("root%2F_root.md", "root/_root.md", ".agents/root/_root.md"),
        InlineData("project%20alpha/_project%20alpha.md", "project alpha/_project alpha.md", ".agents/project alpha/_project alpha.md"),
        InlineData("%E5%B7%A5%E4%BD%9C/_%E5%B7%A5%E4%BD%9C.md", "工作/_工作.md", ".agents/工作/_工作.md")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void ValidDestinationPreservesOneDecodedCanonicalValue(
        string attempted,
        string decoded,
        string canonical)
    {
        var result = SourceLoaderDestinationParser.Parse(attempted);

        Assert.Equal(SourceLoaderDestinationParseState.Valid, result.State);
        Assert.Equal(attempted, result.AttemptedDestination);
        Assert.Equal(decoded, result.DecodedDestination);
        Assert.Equal(canonical, result.CanonicalPath);
        Assert.Null(result.Cause);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Neutral Loader destination parsing rejects malformed percent and UTF-8 sequences"),
        InlineData("root%/_root.md"),
        InlineData("root%2/_root.md"),
        InlineData("root%GG/_root.md"),
        InlineData("root%C3%28/_root.md"),
        InlineData("root/space name/_root.md"),
        InlineData("root/space\tname/_root.md")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void MalformedDestinationRetainsCause(string attempted)
    {
        var result = SourceLoaderDestinationParser.Parse(attempted);

        Assert.Equal(SourceLoaderDestinationParseState.Malformed, result.State);
        Assert.Equal(attempted, result.AttemptedDestination);
        Assert.Null(result.DecodedDestination);
        Assert.Null(result.CanonicalPath);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Neutral Loader destination parsing blocks decoded separators traversal controls and rooted forms"),
        InlineData("root%5C_root.md", "root\\_root.md"),
        InlineData("root/%00source.md", "root/\0source.md"),
        InlineData("root/%1Fsource.md", "root/\u001Fsource.md"),
        InlineData("root%3Fquery/_root.md", "root?query/_root.md"),
        InlineData("root%23fragment/_root.md", "root#fragment/_root.md"),
        InlineData("root%3Astream/_root.md", "root:stream/_root.md"),
        InlineData("/root/_root.md", "/root/_root.md"),
        InlineData("//root/_root.md", "//root/_root.md"),
        InlineData("file:/root/_root.md", "file:/root/_root.md"),
        InlineData("C:/root/_root.md", "C:/root/_root.md"),
        InlineData("root%2F%2F_root.md", "root//_root.md"),
        InlineData("root/%2E/_root.md", "root/./_root.md"),
        InlineData("root/%2E%2E/_root.md", "root/../_root.md"),
        InlineData("root//_root.md", "root//_root.md"),
        InlineData("root/./_root.md", "root/./_root.md"),
        InlineData("root/../_root.md", "root/../_root.md")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void UnsafeDestinationRetainsDecodedEvidence(
        string attempted,
        string decoded)
    {
        var result = SourceLoaderDestinationParser.Parse(attempted);

        Assert.Equal(SourceLoaderDestinationParseState.Unsafe, result.State);
        Assert.Equal(attempted, result.AttemptedDestination);
        Assert.Equal(decoded, result.DecodedDestination);
        Assert.Null(result.CanonicalPath);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Neutral Loader destination parsing never decodes an encoded percent twice")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void EncodedPercentIsDecodedExactlyOnce()
    {
        var result = SourceLoaderDestinationParser.Parse("encoded%2520name/_encoded%2520name.md");

        Assert.Equal(SourceLoaderDestinationParseState.Valid, result.State);
        Assert.Equal("encoded%20name/_encoded%20name.md", result.DecodedDestination);
        Assert.Equal(".agents/encoded%20name/_encoded%20name.md", result.CanonicalPath);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Neutral Loader declaration parsing retains exact generated entry declarations")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void DeclarationParserReturnsOneDestinationAndNoCause()
    {
        var parsed = SourceLoaderDeclarationParser.TryParse(
            "- [Root](root/_root.md) - #Root",
            out var destination,
            out var cause);

        Assert.True(parsed);
        Assert.Equal("root/_root.md", destination);
        Assert.Null(cause);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Neutral Loader Entries parsing preserves declaration order and partial malformed evidence")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void EntriesParserPreservesOrderAndEarlierSafeDestinations()
    {
        var result = SourceLoaderEntriesParser.Parse(LoaderContents(
            "\n- [Second](second/_second.md) - #Second\n"
            + "- [First](first/_first.md) - #First\n"));

        Assert.Equal(SourceLoaderEntriesParseState.Valid, result.State);
        Assert.Equal(
            [".agents/second/_second.md", ".agents/first/_first.md"],
            result.Destinations.Select(destination => destination.CanonicalPath));
        Assert.All(result.Destinations, destination =>
            Assert.Equal(SourceLoaderDestinationParseState.Valid, destination.State));
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Neutral Loader Entries parsing accepts the valid empty forms"),
        InlineData(""),
        InlineData("- none - No entries - #Empty")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void EmptyEntriesAreACompleteEmptySet(string entriesBody)
    {
        var result = SourceLoaderEntriesParser.Parse(LoaderContents(entriesBody));

        Assert.Equal(SourceLoaderEntriesParseState.Valid, result.State);
        Assert.Empty(result.Destinations);
        Assert.Null(result.Cause);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Neutral Loader Entries parsing rejects missing or duplicate sections and malformed declarations"),
        InlineData("# Open Forge Loader\n"),
        InlineData("# Open Forge Loader\n\n## Entries\n\n## Entries\n"),
        InlineData("# Open Forge Loader\n\n## Entries\n\n- Invalid declaration\n"),
        InlineData("# Open Forge Loader\n\n## Entries\n\n- [Root](root/_root.md) #Root\n"),
        InlineData("# Open Forge Loader\n\n## Entries\n\n- [   ](root/_root.md) - #Root\n")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void MalformedEntriesRetainAParseCause(string contents)
    {
        var result = SourceLoaderEntriesParser.Parse(contents);

        Assert.Equal(SourceLoaderEntriesParseState.Malformed, result.State);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    private static string LoaderContents(string entriesBody)
    {
        return $"""
            # Open Forge Loader

            ## Entries
            {entriesBody}
            """;
    }
}
