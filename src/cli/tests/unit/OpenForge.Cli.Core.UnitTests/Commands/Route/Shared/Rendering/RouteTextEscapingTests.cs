using OpenForge.Cli.Core.Presentation.Shared.Text;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Rendering;

public sealed class RouteTextEscapingTests
{
    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Route text preserves printable characters and escapes only control values"), Trait("Feature", "route"), Trait("Evidence", "UnitContract")]
    [InlineData("route\\\"\u0001😀", "route\\\"\\u0001😀")]
    [InlineData("<route>&\n\r\t", "<route>&\\n\\r\\t")]
    public void PrimaryRouteTextUsesSharedEscaping(string value, string expected)
        => Assert.Equal(expected, CliText.Escape(value));
}
