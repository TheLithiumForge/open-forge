using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Source;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Profile;

public sealed class RouteInspectGeneratedEntriesReaderTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route inspect accepts blank whitespace inside generated Entries")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void WhitespaceOnlyLinesRemainAvailable()
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            "## Entries\n"
            + "   \n"
            + "- none - No entries - #Empty\n"
            );

        var result = RouteInspectGeneratedEntriesReader.Read(loader);

        Assert.True(result.IsAvailable);
    }

    [Theory]
    [InlineData("Complete", true)]
    [InlineData("Missing", true)]
    [InlineData("Malformed", false)]
    [InlineData("ReadUnavailable", false)]
    [Trait("Boundary", "Processing"), Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void EntrypointEntriesDistinguishAbsentOptionalMetadataFromUnavailableMetadata(string metadataState, bool available)
    {
        var source = RouteInspectSourceTestData.Source(new RouteInspectSourceSpec
        {
            Path = ".agents/root/_root.md",
            Kind = RouteSourceKind.Entrypoint,
            MetadataState = Enum.Parse<RouteSourceMetadataState>(metadataState),
            Body = "## Entries\n\n- [Child](child.md) - #LoadNow\n",
        });

        var result = RouteInspectGeneratedEntriesReader.Read(source);

        Assert.Equal(available, result.IsAvailable);
        if (available)
        {
            var entry = Assert.Single(result.Entries);
            Assert.Equal("child.md", entry.Destination);
            Assert.True(entry.HasTag("LoadNow"));
        }
    }

    [Theory]
    [InlineData("Native", "Complete", "## Entries\n\n- none - No entries - #Empty\n")]
    [InlineData("Entrypoint", "AccessDenied", "## Entries\n\n- none - No entries - #Empty\n")]
    [InlineData("Entrypoint", "Complete", "# Root\n")]
    [Trait("Boundary", "Processing"), Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void MissingMetadataDoesNotHideRequiredNativeReadOrEntriesFailures(string kind, string readState, string body)
    {
        var source = RouteInspectSourceTestData.Source(new RouteInspectSourceSpec
        {
            Path = kind == "Native" ? ".agents/skills/example/SKILL.md" : ".agents/root/_root.md",
            Kind = Enum.Parse<RouteSourceKind>(kind),
            MetadataState = RouteSourceMetadataState.Missing,
            DocumentReadState = Enum.Parse<FileReadState>(readState),
            Body = body,
        });

        Assert.False(RouteInspectGeneratedEntriesReader.Read(source).IsAvailable);
    }
}
