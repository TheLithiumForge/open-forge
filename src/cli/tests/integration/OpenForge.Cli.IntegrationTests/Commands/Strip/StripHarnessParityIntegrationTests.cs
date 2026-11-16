using OpenForge.Cli.Core.Commands.Shared.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Shared.Stripping.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Stripping;

namespace OpenForge.Cli.IntegrationTests.Commands.Strip;

// The demo-evals harness at 6edb7de66 strips by hand in src/evals/materialize. These fixtures encode its
// plain-links.test.ts and frontmatter.test.ts cases, plus the cases where Open Forge's own rules differ.
public sealed class StripHarnessParityIntegrationTests
{
    private const string Note = ".agents/notes/note.md";
    private const string Other = ".agents/notes/rules.md";

    [Fact(DisplayName = "Strip links keeps text, external links and code exactly as the eval harness does"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task LinksMatchTheHarnessCases()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-links-harness");
        workspace.Write(Other, "# Rules\n");
        workspace.Write(Note, string.Join('\n',
            "See the [rules](rules.md) and the [moves section](rules.md#moves), or [this heading](#local).",
            "React lists its [stable releases](https://react.dev/versions).",
            "```",
            "[kept](literal.md)",
            "```"));

        var result = await workspace.StripAsync(workspace.Request(StripCommandKind.Strip, [StripPart.Links]));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(string.Join('\n',
            "See the rules and the moves section, or this heading.",
            "React lists its [stable releases](https://react.dev/versions).",
            "```",
            "[kept](literal.md)",
            "```"), workspace.Read(Note));
    }

    [Fact(DisplayName = "Strip links follows Markdown where the eval harness's regular expression does not"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task LinksFollowMarkdownWhereTheHarnessDiffers()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-links-differences");
        workspace.Write(Other, "# Rules\n");
        workspace.Write(Note, string.Join('\n',
            "Code `[x](rules.md)` stays, and so does ![chart](chart.png).",
            "The [app](../../src/app.ts) stays linked, [**bold rules**](rules.md \"Rules\") and [the rules][r] lose theirs.",
            "<!-- @open-forge ./rules.md explains this -->",
            "~~~~",
            "```",
            "[fenced](rules.md)",
            "~~~~",
            "",
            "[r]: rules.md",
            ""));

        var result = await workspace.StripAsync(workspace.Request(StripCommandKind.Strip, [StripPart.Links]));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(string.Join('\n',
            "Code `[x](rules.md)` stays, and so does ![chart](chart.png).",
            "The [app](../../src/app.ts) stays linked, **bold rules** and the rules lose theirs.",
            "<!-- @open-forge ./rules.md explains this -->",
            "~~~~",
            "```",
            "[fenced](rules.md)",
            "~~~~",
            "",
            ""), workspace.Read(Note));
    }

    [Theory(DisplayName = "Strip metadata keeps the exact UTF-8 body bytes from the eval harness cases"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    [InlineData("# Game\nFind the pairs.\n")]
    [InlineData("\r\n# Café\r\nKeep these bytes.\r\n")]
    [InlineData("# Cards\n🙂\n\n")]
    [InlineData("# No final newline")]
    public async Task MetadataKeepsTheBody(string body)
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-metadata-body");
        workspace.Write(Note, "---\nopen-forge:\n  description: Game requirements\n  tags: [LoadNow, Game]\n---\n" + body);

        var result = await workspace.StripAsync(workspace.Request(StripCommandKind.Strip, [StripPart.Metadata]));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(body, workspace.Read(Note));
    }

    [Fact(DisplayName = "Strip metadata keeps foreign YAML byte for byte where the harness serializes it again"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task MetadataKeepsForeignYaml()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-metadata-foreign");
        workspace.Write(Note, "---\r\ntitle: \"Public note\"\r\nopen-forge:\r\n  description: Note\r\n  tags: [Docs]\r\ncustom:\r\n  flags: [true, false]\r\napplyTo: [\"src/**\"]\r\n---\r\n# Note\r\nBody.\r\n");
        workspace.Write(Other, "---\ndescription: Rules\ntags: [Docs]\nowner: team\n---\n# Rules\n");

        var result = await workspace.StripAsync(workspace.Request(StripCommandKind.Strip, [StripPart.Metadata]));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal("---\r\ntitle: \"Public note\"\r\ncustom:\r\n  flags: [true, false]\r\n---\r\n# Note\r\nBody.\r\n", workspace.Read(Note));
        Assert.Equal("---\nowner: team\n---\n# Rules\n", workspace.Read(Other));
    }

    [Fact(DisplayName = "Strip entries removes a generated section and keeps a section the user wrote in"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task EntriesKeepUserText()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-entries");
        workspace.Write(".agents/notes/_notes.md", "# Notes\n\nIntro.\n\n## Entries\n\n- [Rules](rules.md) - #Docs\n");
        workspace.Write(".agents/guide/_guide.md", "# Guide\n\n## Entries\n\nRead these first.\n\n- [Rules](../notes/rules.md) - #Docs\n");
        workspace.Write(Other, "# Rules\n");

        var result = await workspace.StripAsync(workspace.Request(StripCommandKind.Strip, [StripPart.Entries, StripPart.Links]));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal("# Notes\n\nIntro.\n", workspace.Read(".agents/notes/_notes.md"));
        Assert.Equal("# Guide\n\n## Entries\n\nRead these first.\n\n", workspace.Read(".agents/guide/_guide.md"));
    }

    [Fact(DisplayName = "Strip flatten moves documents and rewrites kept links to their new places"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task FlattenRewritesKeptLinks()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-flatten");
        workspace.Write(".agents/docs/README.md", "Start at [Rules](game/rules.md#moves).\n");
        workspace.Write(".agents/docs/game/rules.md", "See [Input](../ui/input.md), ![board](board.png) and [README](../README.md).\n");
        workspace.Write(".agents/docs/game/board.png", "png");
        workspace.Write(".agents/docs/ui/input.md", "Back to [Rules](../game/rules.md).\n<!-- @rune ../game/rules.md defines moves -->\n");

        var result = await workspace.StripAsync(workspace.Request(StripCommandKind.Strip, [], flatten: ".agents/docs"));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal("Start at [Rules](rules.md#moves).\n", workspace.Read(".agents/docs/README.md"));
        Assert.Equal("See [Input](input.md), ![board](game/board.png) and [README](README.md).\n", workspace.Read(".agents/docs/rules.md"));
        Assert.Equal("Back to [Rules](rules.md).\n<!-- @rune ./rules.md defines moves -->\n", workspace.Read(".agents/docs/input.md"));
        Assert.True(workspace.Exists(".agents/docs/game/board.png"));
        Assert.False(workspace.Exists(".agents/docs/ui"));
    }

    [Fact(DisplayName = "Strip flatten reports every name collision and writes nothing"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task FlattenCollisionWritesNothing()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-flatten-collision");
        workspace.Write(".agents/docs/a/notes.md", "A\n");
        workspace.Write(".agents/docs/b/Notes.md", "B\n");
        var before = workspace.SnapshotHashes();

        var result = await workspace.StripAsync(workspace.Request(StripCommandKind.Strip, [StripPart.Metadata], flatten: ".agents/docs"));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(StripFindingCode.FlattenCollision, Assert.Single(result.Findings).Code);
        Assert.Equal([".agents/docs/a/notes.md", ".agents/docs/b/Notes.md"], Assert.Single(result.Collisions).Sources);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Strip preview writes nothing and lists the plan that applying then carries out"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task PreviewMatchesApplication()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-preview-parity");
        workspace.Write(Note, "---\nopen-forge:\n  description: Note\n  tags: [Docs]\n---\nSee [Rules](rules.md).\n");
        workspace.Write(Other, "# Rules\n");
        var before = workspace.SnapshotHashes();
        var request = workspace.Request(StripCommandKind.Strip, [StripPart.Metadata, StripPart.Links]);

        var preview = await workspace.StripAsync(request with { DryRun = true });
        Assert.Equal(before, workspace.SnapshotHashes());
        var applied = await workspace.StripAsync(request);
        var repeated = await workspace.StripAsync(request);

        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.True(applied.Verified);
        Assert.Equal(preview.Plan.Select(entry => (entry.Action, entry.Path, entry.Before, entry.After)), applied.Plan.Select(entry => (entry.Action, entry.Path, entry.Before, entry.After)));
        Assert.True(repeated.NoOp);
    }
}
