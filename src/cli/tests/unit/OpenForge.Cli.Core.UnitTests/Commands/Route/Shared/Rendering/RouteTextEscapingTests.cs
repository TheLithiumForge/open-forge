using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Shared.Rendering;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Rendering;

public sealed class RouteTextEscapingTests
{
    [Theory(DisplayName = "Route bounded escaping retains whole encoded Unicode scalars")]
    [InlineData("a\\bcde", 5, "a...")]
    [InlineData("a\"bcde", 5, "a...")]
    [InlineData("a\u0001bcde", 5, "a...")]
    [InlineData("a\U0001F600bcde", 10, "a...")]
    [Trait("Feature", "route"), Trait("Evidence", "UnitBehavior")]
    public void BoundedEscapingRetainsWholeEncodedUnicodeScalars(
        string value,
        int maximumLength,
        string expected)
        => Assert.Equal(expected, RouteTextEscaping.Escape(value, maximumLength));

    [Fact(DisplayName = "Route unbounded escaping preserves JsonEncodedText bytes")]
    [Trait("Feature", "route"), Trait("Evidence", "UnitContract")]
    public void UnboundedEscapingPreservesJsonEncodedTextBytes()
    {
        const string value = "route\\\"\u0001\U0001F600";
        var encoded = JsonEncodedText.Encode(value);

        Assert.Equal(
            encoded.EncodedUtf8Bytes.ToArray(),
            Encoding.UTF8.GetBytes(RouteTextEscaping.Escape(value)));
        Assert.Equal(
            encoded.ToString(),
            RouteTextEscaping.Escape(value, int.MaxValue));
    }
}
