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

    // frontmatter, entrypoints and links in one run: a deleted entrypoint, whole frontmatter blocks and a comment link.
    [Fact(DisplayName = "Strip previews with frontmatter, entrypoints and links keep their text and JSON plan at every detail level"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task PreviewParts()
    {
        using var workspace = Seed("strip-output-parts");
        workspace.Write(Folder + "/game/_game.md", "---\nopen-forge:\n  description: Game\n  tags: [Docs]\n---\n# Game\n\n## Entries\n\n- [Rules](rules.md) - #Docs\n");
        workspace.Write(Folder + "/game/moves.md", "---\ntitle: Moves\n---\n# Moves\n\n<!-- @open-forge ./rules.md defines the board -->\n");

        var result = await workspace.StripAsync(workspace.Request(StripCommandKind.Strip, [StripPart.Frontmatter, StripPart.Entrypoints, StripPart.Links], Folder, dryRun: true));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Renderers().MatchDetails(result, "parts");
    }

    [Fact(DisplayName = "Strip collision reports keep their text and JSON at every detail level"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task Collision()
    {
        using var workspace = Seed("strip-output-collision");
        workspace.Write(Folder + "/other/rules.md", "# Other rules\n");
        workspace.Write(Folder + "/other/notes.md", "# Other notes\n");
        workspace.Write(Folder + "/game/notes.md", "# Game notes\n");

        var result = await workspace.StripAsync(workspace.Request(StripCommandKind.Strip, [StripPart.Metadata], Folder, dryRun: true));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(result.Collisions.Select(collision => collision.Path), result.Findings.Select(finding => finding.Path));
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
