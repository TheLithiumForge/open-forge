using OpenForge.Cli.Core.Presentation.Route.Shared.Wording;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Route.Shared.Wording;

public sealed class RouteSourceSelectionWordingTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route source selection wording keeps the shared catalogue sentence")]
    [Trait("Feature", "route-interaction"), Trait("Evidence", "UnitContract")]
    public void SourceSelectionUsesSharedCatalogueSentence()
    {
        Assert.Equal(
            "docs/guide matches 2 sources. Which one?",
            RouteSourceSelectionWording.SourceSelection("docs/guide", 2));
    }
}
