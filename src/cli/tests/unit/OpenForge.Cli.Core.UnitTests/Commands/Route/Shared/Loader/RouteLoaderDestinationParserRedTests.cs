using OpenForge.Cli.Core.Commands.Route.Shared.Loader;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Loader;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Loader;

public sealed class RouteLoaderDestinationParserRedTests
{
    [Theory(DisplayName = "Loader destination parsing decodes UTF-8 and percent escapes exactly once"),
        InlineData("root/_root.md", "root/_root.md", ".agents/root/_root.md"),
        InlineData("root%2F_root.md", "root/_root.md", ".agents/root/_root.md"),
        InlineData("project%20alpha/_project%20alpha.md", "project alpha/_project alpha.md", ".agents/project alpha/_project alpha.md"),
        InlineData("%E5%B7%A5%E4%BD%9C/_%E5%B7%A5%E4%BD%9C.md", "工作/_工作.md", ".agents/工作/_工作.md")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void ValidDestinationPreservesOneDecodedCanonicalValue(
        string attempted,
        string decoded,
        string canonical)
    {
        var result = RouteLoaderDestinationParser.Parse(attempted);

        Assert.Equal(RouteLoaderDestinationParseState.Valid, result.State);
        Assert.Equal(attempted, result.AttemptedDestination);
        Assert.Equal(decoded, result.DecodedDestination);
        Assert.Equal(canonical, result.CanonicalPath);
        Assert.Null(result.Cause);
    }

    [Theory(DisplayName = "Loader destination parsing rejects malformed percent and UTF-8 sequences"),
        InlineData("root%/_root.md"),
        InlineData("root%2/_root.md"),
        InlineData("root%GG/_root.md"),
        InlineData("root%C3%28/_root.md"),
        InlineData("root/space name/_root.md"),
        InlineData("root/space\tname/_root.md")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void MalformedDestinationRetainsCause(string attempted)
    {
        var result = RouteLoaderDestinationParser.Parse(attempted);

        Assert.Equal(RouteLoaderDestinationParseState.Malformed, result.State);
        Assert.Equal(attempted, result.AttemptedDestination);
        Assert.Null(result.DecodedDestination);
        Assert.Null(result.CanonicalPath);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    [Theory(DisplayName = "Loader destination parsing blocks separators traversal controls query fragments and colon forms"),
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
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void UnsafeDestinationRetainsDecodedEvidence(string attempted, string decoded)
    {
        var result = RouteLoaderDestinationParser.Parse(attempted);

        Assert.Equal(RouteLoaderDestinationParseState.Unsafe, result.State);
        Assert.Equal(attempted, result.AttemptedDestination);
        Assert.Equal(decoded, result.DecodedDestination);
        Assert.Null(result.CanonicalPath);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    [Fact(DisplayName = "Loader destination parsing never decodes an encoded percent twice")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void EncodedPercentIsDecodedExactlyOnce()
    {
        var result = RouteLoaderDestinationParser.Parse("encoded%2520name/_encoded%2520name.md");

        Assert.Equal(RouteLoaderDestinationParseState.Valid, result.State);
        Assert.Equal("encoded%20name/_encoded%20name.md", result.DecodedDestination);
        Assert.DoesNotContain("encoded name", result.DecodedDestination, StringComparison.Ordinal);
        Assert.Equal(".agents/encoded%20name/_encoded%20name.md", result.CanonicalPath);
    }
}
