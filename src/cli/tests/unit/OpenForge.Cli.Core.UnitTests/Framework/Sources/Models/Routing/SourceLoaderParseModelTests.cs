using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Models.Routing;

public sealed class SourceLoaderParseModelTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "A valid Loader destination rejects a null decoded value"), Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void ValidDestinationRejectsNullDecodedValue()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            SourceLoaderDestinationParseResult.Valid(
                attemptedDestination: "root/_root.md",
                decodedDestination: null!,
                canonicalPath: ".agents/root/_root.md"));

        Assert.Equal("decodedDestination", exception.ParamName);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "A valid Loader destination rejects an empty decoded value"), Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void ValidDestinationRejectsEmptyDecodedValue()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            SourceLoaderDestinationParseResult.Valid(
                attemptedDestination: "root/_root.md",
                decodedDestination: string.Empty,
                canonicalPath: ".agents/root/_root.md"));

        Assert.Equal("decodedDestination", exception.ParamName);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Neutral Loader destination results retain valid, malformed, and unsafe field shapes")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void DestinationResultFactoriesAreTyped()
    {
        var valid = SourceLoaderDestinationParseResult.Valid(
            "root%2F_root.md",
            "root/_root.md",
            ".agents/root/_root.md");
        Assert.Equal(SourceLoaderDestinationParseState.Valid, valid.State);
        Assert.Equal("root%2F_root.md", valid.AttemptedDestination);
        Assert.Equal("root/_root.md", valid.DecodedDestination);
        Assert.Equal(".agents/root/_root.md", valid.CanonicalPath);
        Assert.Null(valid.Cause);

        var malformed = SourceLoaderDestinationParseResult.Malformed("root%2", "bad percent");
        Assert.Equal(SourceLoaderDestinationParseState.Malformed, malformed.State);
        Assert.Null(malformed.DecodedDestination);
        Assert.Null(malformed.CanonicalPath);
        Assert.Equal("bad percent", malformed.Cause);

        var unsafeResult = SourceLoaderDestinationParseResult.Unsafe(
            "root/../_root.md",
            "root/../_root.md",
            "traversal");
        Assert.Equal(SourceLoaderDestinationParseState.Unsafe, unsafeResult.State);
        Assert.Equal("root/../_root.md", unsafeResult.DecodedDestination);
        Assert.Null(unsafeResult.CanonicalPath);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Neutral Loader Entries results retain authored destinations and partial malformed progress")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void EntriesResultFactoriesRetainPartialProgress()
    {
        var safe = SourceLoaderDestinationParseResult.Valid(
            "root/_root.md",
            "root/_root.md",
            ".agents/root/_root.md");
        var valid = SourceLoaderEntriesParseResult.Valid([safe]);
        Assert.Equal(SourceLoaderEntriesParseState.Valid, valid.State);
        Assert.Same(safe, Assert.Single(valid.Destinations));
        Assert.Null(valid.Cause);
        Assert.Null(valid.AttemptedDestination);

        var malformed = SourceLoaderEntriesParseResult.Malformed(
            "later destination is malformed",
            "broken%2",
            [safe]);
        Assert.Equal(SourceLoaderEntriesParseState.Malformed, malformed.State);
        Assert.Same(safe, Assert.Single(malformed.Destinations));
        Assert.Equal("broken%2", malformed.AttemptedDestination);
        Assert.Equal("later destination is malformed", malformed.Cause);
    }
}
