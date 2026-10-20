using OpenForge.Cli.Core.Framework.GeneratedNavigation;

namespace OpenForge.Cli.Core.UnitTests.Framework.GeneratedNavigation;

public sealed class LoaderMembershipTests
{
    [Fact(DisplayName = "An operation turns on only the roots it creates and the roots the user selected")]
    [Trait("Feature", "loader-membership"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void TurnsOnCreatedAndSelectedRootsOnly()
    {
        string[] structural = [".agents/directives/_directives.md", ".agents/guidance/_guidance.md", ".agents/maps/_maps.md"];
        var existing = new HashSet<string>([".agents/directives/_directives.md", ".agents/guidance/_guidance.md"], StringComparer.Ordinal);

        var turnedOn = LoaderMembership.ReadTurnedOn(structural, existing.Contains, [".agents/guidance/_guidance.md"]);

        Assert.Equal([".agents/guidance/_guidance.md", ".agents/maps/_maps.md"], turnedOn.Order(StringComparer.Ordinal));
        Assert.Empty(LoaderMembership.ReadTurnedOn(structural, _ => true));
    }
}
