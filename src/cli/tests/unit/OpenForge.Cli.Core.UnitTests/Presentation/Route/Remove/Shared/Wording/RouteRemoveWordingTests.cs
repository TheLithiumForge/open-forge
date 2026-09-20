using OpenForge.Cli.Core.Presentation.Route.Remove.Shared.Wording;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Route.Remove.Shared.Wording;

public sealed class RouteRemoveWordingTests
{
    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Route Remove confirmation wording pluralizes the reviewed file count")]
    [InlineData(1, "Delete the 1 file listed above? [y/N]")]
    [InlineData(2, "Delete the 2 files listed above? [y/N]")]
    [Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void ConfirmationUsesExactCountWording(int count, string expected)
        => Assert.Equal(expected, RouteRemoveWording.Confirmation(count));
}
