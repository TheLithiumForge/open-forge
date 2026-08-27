using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Source;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Profile;

public sealed class RouteInspectGeneratedEntriesReaderTests
{
    [Fact(DisplayName = "Route inspect rejects whitespace-only lines before generated Entries markers")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void WhitespaceOnlyLinesBeforeStartMarkerRemainUnavailable()
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            "## Entries\n"
            + "   \n"
            + "<!-- open-forge:generated-index:start -->\n"
            + "- none - No entries - #Empty\n"
            + "<!-- open-forge:generated-index:end -->\n");

        var result = RouteInspectGeneratedEntriesReader.Read(loader);

        Assert.False(result.IsAvailable);
        Assert.Equal(
            "Only blank lines may occur outside the entrypoint Entries markers.",
            result.ReadReason());
    }
}
