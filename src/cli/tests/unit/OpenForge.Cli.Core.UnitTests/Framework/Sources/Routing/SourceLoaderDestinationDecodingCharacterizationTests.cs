using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Routing;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Routing;

public sealed class SourceLoaderDestinationDecodingCharacterizationTests
{
    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Loader destinations retain exact decoding failure causes and first-failure precedence"),
        InlineData("", "A Loader destination cannot be empty."),
        InlineData("space name.md", "Unencoded whitespace is not valid in a Loader destination."),
        InlineData("space\tname.md", "Unencoded whitespace is not valid in a Loader destination."),
        InlineData("space\u00A0name.md", "Unencoded whitespace is not valid in a Loader destination."),
        InlineData("name%", "Every percent sign must begin a valid percent triplet."),
        InlineData("name%2", "Every percent sign must begin a valid percent triplet."),
        InlineData("name%GG", "Every percent sign must begin a valid percent triplet."),
        InlineData("%C3", "Percent-encoded bytes must form strict UTF-8."),
        InlineData("%E2%82", "Percent-encoded bytes must form strict UTF-8."),
        InlineData("%F0%9F%98", "Percent-encoded bytes must form strict UTF-8."),
        InlineData("%80", "Percent-encoded bytes must form strict UTF-8."),
        InlineData("%C0%AF", "Percent-encoded bytes must form strict UTF-8."),
        InlineData("%ED%A0%80", "Percent-encoded bytes must form strict UTF-8."),
        InlineData("%F4%90%80%80", "Percent-encoded bytes must form strict UTF-8."),
        InlineData("%C3é%A9", "Percent-encoded bytes must form strict UTF-8."),
        InlineData("valid%C3%A9/%FF.md", "Percent-encoded bytes must form strict UTF-8."),
        InlineData("%C3%GG", "Every percent sign must begin a valid percent triplet."),
        InlineData("%C3x%GG", "Percent-encoded bytes must form strict UTF-8."),
        InlineData(" %GG", "Unencoded whitespace is not valid in a Loader destination."),
        InlineData("%GG ", "Every percent sign must begin a valid percent triplet."),
        InlineData("%FF ", "Percent-encoded bytes must form strict UTF-8.")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void FailuresPreserveExactCause(string attempted, string expectedCause)
    {
        var result = SourceLoaderDestinationParser.Parse(attempted);

        Assert.Equal(SourceLoaderDestinationParseState.Malformed, result.State);
        Assert.Equal(attempted, result.AttemptedDestination);
        Assert.Null(result.DecodedDestination);
        Assert.Null(result.CanonicalPath);
        Assert.Equal(expectedCause, result.Cause);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Loader destinations preserve mixed literal and escaped Unicode"),
        InlineData("space%C2%A0name.md", "space\u00A0name.md", ".agents/space\u00A0name.md"),
        InlineData("caf%C3%A9-雪-%F0%9F%98%80.md", "café-雪-😀.md", ".agents/café-雪-😀.md"),
        InlineData("caf%c3%a9.md", "café.md", ".agents/café.md"),
        InlineData("café-雪-😀.md", "café-雪-😀.md", ".agents/café-雪-😀.md")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void UnicodePreservesFrozenDecodedAndCanonicalValues(string attempted, string expectedDecoded, string expectedPath)
    {
        var result = SourceLoaderDestinationParser.Parse(attempted);

        Assert.Equal(SourceLoaderDestinationParseState.Valid, result.State);
        Assert.Equal(attempted, result.AttemptedDestination);
        Assert.Equal(expectedDecoded, result.DecodedDestination);
        Assert.Equal(expectedPath, result.CanonicalPath);
        Assert.Null(result.Cause);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Loader destination path rejection retains exact causes after successful decoding"),
        InlineData("./child//../leaf.md", "./child//../leaf.md", "The decoded Loader destination contains an empty or traversal segment."),
        InlineData("%2E%2E/leaf.md", "../leaf.md", "The decoded Loader destination contains an empty or traversal segment."),
        InlineData("leaf%09.md", "leaf\t.md", "The decoded Loader destination contains an unsafe path character."),
        InlineData("leaf%23fragment.md", "leaf#fragment.md", "The decoded Loader destination contains an unsafe path character.")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void PathPolicyPreservesDecodedEvidence(string attempted, string expectedDecoded, string expectedCause)
    {
        var result = SourceLoaderDestinationParser.Parse(attempted);

        Assert.Equal(SourceLoaderDestinationParseState.Unsafe, result.State);
        Assert.Equal(attempted, result.AttemptedDestination);
        Assert.Equal(expectedDecoded, result.DecodedDestination);
        Assert.Null(result.CanonicalPath);
        Assert.Equal(expectedCause, result.Cause);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Loader destinations preserve literal unpaired UTF-16 surrogates"), Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void LiteralSurrogatesAreNotReencoded()
    {
        var high = SourceLoaderDestinationParser.Parse("\uD800.md");
        var low = SourceLoaderDestinationParser.Parse("\uDC00.md");

        Assert.Equal(SourceLoaderDestinationParseState.Valid, high.State);
        Assert.Equal("\uD800.md", high.DecodedDestination);
        Assert.Equal(".agents/\uD800.md", high.CanonicalPath);
        Assert.Null(high.Cause);
        Assert.Equal(SourceLoaderDestinationParseState.Valid, low.State);
        Assert.Equal("\uDC00.md", low.DecodedDestination);
        Assert.Equal(".agents/\uDC00.md", low.CanonicalPath);
        Assert.Null(low.Cause);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "An encoded space remains a nonempty lexical Loader destination"), Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void EncodedWhitespaceOnlyRetainsDecodedLexicalDestination()
    {
        var result = SourceLoaderDestinationParser.Parse("%20");

        Assert.Equal(SourceLoaderDestinationParseState.Valid, result.State);
        Assert.Equal("%20", result.AttemptedDestination);
        Assert.Equal(" ", result.DecodedDestination);
        Assert.Equal(".agents/ ", result.CanonicalPath);
        Assert.Null(result.Cause);
    }
}
