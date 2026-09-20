using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Routing;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Routing;

public sealed class SourceLoaderDestinationParserListRegressionTests
{
    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Loader destination parser accepts canonical and exactly-once decoded destinations"),
        InlineData("root/_root.md", "root/_root.md", ".agents/root/_root.md"),
        InlineData("root%2F_root.md", "root/_root.md", ".agents/root/_root.md"),
        InlineData("project%20alpha/_project%20alpha.md", "project alpha/_project alpha.md", ".agents/project alpha/_project alpha.md"),
        InlineData("%E5%B7%A5%E4%BD%9C/_%E5%B7%A5%E4%BD%9C.md", "工作/_工作.md", ".agents/工作/_工作.md"),
        InlineData("encoded%2520name/_encoded%2520name.md", "encoded%20name/_encoded%20name.md", ".agents/encoded%20name/_encoded%20name.md")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void ValidDestinationDecodesOnceAndCanonicalizes(
        string attemptedDestination,
        string decodedDestination,
        string canonicalPath)
    {
        var result = SourceLoaderDestinationParser.Parse(attemptedDestination);

        Assert.Equal(SourceLoaderDestinationParseState.Valid, result.State);
        Assert.Equal(attemptedDestination, result.AttemptedDestination);
        Assert.Equal(decodedDestination, result.DecodedDestination);
        Assert.Equal(canonicalPath, result.CanonicalPath);
        Assert.Null(result.Cause);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Loader destination parser rejects malformed percent triplets and UTF-8"),
        InlineData("root%/_root.md"),
        InlineData("root%2/_root.md"),
        InlineData("root%GG/_root.md"),
        InlineData("root%G0/_root.md"),
        InlineData("root%0G/_root.md"),
        InlineData("root%C3%28/_root.md"),
        InlineData("root/space name/_root.md"),
        InlineData("root/space\tname/_root.md")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void MalformedDestinationRetainsAttemptAndCause(string attemptedDestination)
    {
        var result = SourceLoaderDestinationParser.Parse(attemptedDestination);

        Assert.Equal(SourceLoaderDestinationParseState.Malformed, result.State);
        Assert.Equal(attemptedDestination, result.AttemptedDestination);
        Assert.Null(result.DecodedDestination);
        Assert.Null(result.CanonicalPath);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Loader destination parser blocks every decoded path-boundary hazard"),
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
        string attemptedDestination,
        string decodedDestination)
    {
        var result = SourceLoaderDestinationParser.Parse(attemptedDestination);

        Assert.Equal(SourceLoaderDestinationParseState.Unsafe, result.State);
        Assert.Equal(attemptedDestination, result.AttemptedDestination);
        Assert.Equal(decodedDestination, result.DecodedDestination);
        Assert.Null(result.CanonicalPath);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Neutral Loader destination parser does not decode an encoded percent twice")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void EncodedPercentIsDecodedExactlyOnce()
    {
        var result = SourceLoaderDestinationParser.Parse("encoded%2520name/_encoded%2520name.md");

        Assert.Equal(SourceLoaderDestinationParseState.Valid, result.State);
        Assert.Equal("encoded%20name/_encoded%20name.md", result.DecodedDestination);
        Assert.DoesNotContain("encoded name", result.DecodedDestination, StringComparison.Ordinal);
        Assert.Equal(".agents/encoded%20name/_encoded%20name.md", result.CanonicalPath);
    }
}
