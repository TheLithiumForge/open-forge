using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Presentation.Stripping;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Stripping;

namespace OpenForge.Cli.IntegrationTests.Commands.Strip;

public sealed class StripOutputSnapshotTests
{
    private const string Folder = WorkspaceLayout.DefaultMainRoot + "/docs";

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

    private static CommandOutputRenderers<StripResult> Renderers() => CommandOutputRenderers<StripResult>.From(StripPresentation.Rendering);

    private static StripIntegrationWorkspace Seed(string purpose)
    {
        var workspace = StripIntegrationWorkspace.Create(purpose);
        workspace.Write(Folder + "/README.md", "---\nopen-forge:\n  description: Map\n  tags: [Docs]\n---\nStart at [Rules](game/rules.md).\n");
        workspace.Write(Folder + "/game/rules.md", "---\nopen-forge:\n  description: Rules\n  tags: [Docs]\n---\n# Rules\n\nSee the [map](../README.md).\n");
        return workspace;
    }
}
