using OpenForge.Cli.Core.Presentation.Route.Remove.Shared.Wording;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Route.Remove.Shared.Wording;

public sealed class RouteRemoveWordingTests
{
    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Route Remove confirmation wording pluralizes the reviewed file count")]
    [InlineData(1, "Apply these changes, including deleting 1 file? [y/N]")]
    [InlineData(2, "Apply these changes, including deleting 2 files? [y/N]")]
    [Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void ConfirmationUsesExactCountWording(int count, string expected)
        => Assert.Equal(expected, RouteRemoveWording.Confirmation(count));
}
