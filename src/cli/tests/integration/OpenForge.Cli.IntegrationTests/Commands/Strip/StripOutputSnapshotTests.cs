using OpenForge.Cli.Core.Commands.Shared.Stripping;
using OpenForge.Cli.Core.Commands.Shared.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Shared.Stripping.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Stripping;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Stripping;

namespace OpenForge.Cli.IntegrationTests.Commands.Strip;

public sealed class StripOutputSnapshotTests
{
    private const string Folder = ".agents/docs";

    [Fact(DisplayName = "Strip previews keep their text and JSON plan at every detail level"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task Preview()
    {
        using var workspace = Seed("strip-output-preview");

        var result = await workspace.StripAsync(workspace.Request(StripCommandKind.Strip, [StripPart.Metadata, StripPart.Links], Folder, dryRun: true));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Renderers().MatchDetails(result, "preview");
    }

    [Fact(DisplayName = "Strip collision reports keep their text and JSON at every detail level"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task Collision()
    {
        using var workspace = Seed("strip-output-collision");
        workspace.Write(Folder + "/other/rules.md", "# Other rules\n");

        var result = await workspace.StripAsync(workspace.Request(StripCommandKind.Strip, [StripPart.Metadata], Folder, dryRun: true));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Renderers().MatchDetails(result, "collision");
    }

    [Fact(DisplayName = "Strip invalid input keeps its text and JSON at every detail level"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public void InvalidInput()
    {
        var result = StripResultFactory.Invalid(StripCommandKind.Strip, null, "Each part must be metadata, entries or links.");

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Renderers().MatchDetails(result, "invalid");
    }

    private static CommandOutputRenderers<StripResult> Renderers() => CommandOutputRenderers<StripResult>.From(StripPresentation.Rendering);

    private static StripIntegrationWorkspace Seed(string purpose)
    {
        var workspace = StripIntegrationWorkspace.Create(purpose);
        workspace.Write(Folder + "/README.md", "---\nopen-forge:\n  description: Map\n  tags: [Docs]\n---\nStart at [Rules](game/rules.md).\n");
        workspace.Write(Folder + "/game/rules.md", "---\nopen-forge:\n  description: Rules\n  tags: [Docs]\n---\n# Rules\n\nSee the [map](../README.md).\n");
        return workspace;
    }
}
