using System.Text;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Stripping;

namespace OpenForge.Cli.IntegrationTests.Commands.Strip;

// Covers the strip Behavior contract's Required Evidence for single parts, folder links and every document refusal.
public sealed class StripDocumentBoundaryIntegrationTests
{
    private const string Docs = "docs";
    private const string Notes = WorkspaceLayout.DefaultMainRoot + "/notes";
    private const string Note = Notes + "/note.md";
    private const string Chart = "chart.png";
    private const string MemoryCategory = WorkspaceLayout.DefaultMainRoot + "/memory/crystallized";

    // Each case names its part by the wire name the contract gives it.
    public static TheoryData<string, string> Refusals => new()
    {
        { "---\nopen-forge: [unclosed\n---\n# Note\n", StrippingDefinitions.PartName(StripPart.Metadata) },
        { "---\nopen-forge:\n  description: A\nopen-forge:\n  description: B\n---\n# Note\n", StrippingDefinitions.PartName(StripPart.Metadata) },
        { "---\n{description: Note, title: Keep}\n---\n# Note\n", StrippingDefinitions.PartName(StripPart.Metadata) },
        { "---\ntitle: Note\n# Note\n", StrippingDefinitions.PartName(StripPart.Frontmatter) },
        { "# Note\n\n<!-- @open-forge ./other.md explains this\n", StrippingDefinitions.PartName(StripPart.Links) },
    };

    // The failure cases of the harness's frontmatter.test.ts at 6edb7de66. Strip refuses where the bytes cannot be
    // read safely and leaves foreign YAML it has no key to remove from, which the harness rejects.
    public static TheoryData<byte[], bool> HarnessFailureCases => new()
    {
        { Encoding.UTF8.GetBytes("---\nname: x\n"), true },
        { Encoding.UTF8.GetBytes("---\nname: [\n---\n"), true },
        { Encoding.UTF8.GetBytes("---\nname: x\nname: y\n---\n"), false },
        { Encoding.UTF8.GetBytes("---\n- list\n---\n"), false },
        { [255], true },
    };

    [Fact(DisplayName = "Strip entrypoints alone deletes every entrypoint, managed ones included, and keeps the loader, metadata and links"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task EntrypointsAlone()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-entrypoints-alone");
        await workspace.InstallAsync();
        workspace.Write(Docs + "/rules/rules.md", "---\nopen-forge:\n  description: Rules\n  tags: [Game]\n---\n# Rules\n\nBack to the [docs](../_docs.md).\n");
        await workspace.IncludeAsync(Docs);
        Assert.Equal(CliSemanticStatus.Complete, (await workspace.RunAsync([IndexDefinitions.CommandIdentity])).Status);
        var catalogue = Docs + "/_" + Docs + ".md";
        workspace.Write(catalogue, workspace.Read(catalogue) + "\nNotes the user wrote.\n");
        var managed = MemoryCategory + "/" + Path.GetFileName(Assert.Single(Directory.GetFiles(workspace.Combine(MemoryCategory))));
        var loader = WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
        var rules = workspace.Read(Docs + "/rules/rules.md");

        var result = await workspace.StripAsync(workspace.Request(StripCommandKind.Strip, [StripPart.Entrypoints]));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.All(result.Plan.Where(entry => entry.Action != StripPlanAction.DeleteDirectory), entry =>
            Assert.Equal((StripPlanAction.Delete, StripPlanReason.Strip, StripRewritePart.Entrypoints), (entry.Action, entry.Reason, Assert.Single(entry.Parts))));
        Assert.Contains(result.Plan, entry => entry.Path == catalogue);
        Assert.Contains(result.Plan, entry => entry.Path == managed);
        Assert.False(workspace.Exists(catalogue));
        Assert.False(workspace.Exists(managed));
        Assert.True(workspace.Exists(loader));
        Assert.Equal(rules, workspace.Read(Docs + "/rules/rules.md"));
    }

    // Entrypoints the same run deletes no longer make flatten refuse the folder that holds them.
    [Fact(DisplayName = "Strip entrypoints with flatten flattens a Framework category whose only managed file is its entrypoint"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task FlattenWithEntrypoints()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-flatten-entrypoints");
        await workspace.InstallAsync();
        var entrypoint = MemoryCategory + "/" + Path.GetFileName(Assert.Single(Directory.GetFiles(workspace.Combine(MemoryCategory))));
        workspace.Write(MemoryCategory + "/team/note.md", "# Note\n");

        var result = await workspace.StripAsync(workspace.Request(StripCommandKind.Strip, [StripPart.Entrypoints], MemoryCategory));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.False(workspace.Exists(entrypoint));
        Assert.Equal("# Note\n", workspace.Read(MemoryCategory + "/note.md"));
        Assert.False(workspace.Exists(MemoryCategory + "/team"));
    }

    [Fact(DisplayName = "Strip frontmatter removes the whole block with foreign keys, wins over metadata and keeps a SKILL.md block"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task FrontmatterRemovesTheWholeBlock()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-frontmatter");
        var skill = WorkspaceLayout.NativeSkillsPath + "/notes/" + SourceFormClassifier.NativeSkillFileName;
        const string SkillDocument = "---\nname: notes\ndescription: Notes\n---\n# Notes\n";
        workspace.Write(Note, "\uFEFF---\r\ntitle: \"Public\"\r\nopen-forge:\r\n  description: Note\r\n---\r\n# Note\r\n");
        workspace.Write(Notes + "/broken.md", "---\nname: [\n---\n# Broken\n");
        workspace.Write(skill, SkillDocument);

        var result = await workspace.StripAsync(workspace.Request(StripCommandKind.Strip, [StripPart.Metadata, StripPart.Frontmatter]));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.All(result.Plan, entry => Assert.Equal(StripRewritePart.Frontmatter, Assert.Single(entry.Parts)));
        Assert.Equal(Encoding.UTF8.GetBytes("\uFEFF# Note\r\n"), workspace.ReadBytes(Note));
        Assert.Equal("# Broken\n", workspace.Read(Notes + "/broken.md"));
        Assert.Equal(SkillDocument, workspace.Read(skill));
    }

    // Links between documents go everywhere, generated Entries lists included, and so does the whole comment around a
    // comment link. A comment alone on its lines takes those lines with it.
    [Fact(DisplayName = "Strip links removes links in Entries lists and the whole comment around each comment link"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task LinksRemoveEntriesListsAndComments()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-links-comments");
        workspace.Write(Notes + "/other.md", "# Other\n");
        workspace.Write(Notes + "/_notes.md", "# Notes\n\n## Entries\n\n- [Other](other.md) - #Docs\n");
        workspace.Write(Note, "# Note\n\n<!-- @open-forge ./other.md explains this -->\nKept text <!-- @rune other.md defines it --> stays.\n<!--\n  @open-forge ./other.md first\n  @rune ./other.md second\n-->\nEnd.\n");

        var result = await workspace.StripAsync(workspace.Request(StripCommandKind.Strip, [StripPart.Links]));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal("# Notes\n\n## Entries\n\n- Other - #Docs\n", workspace.Read(Notes + "/_notes.md"));
        Assert.Equal("# Note\n\nKept text  stays.\nEnd.\n", workspace.Read(Note));
    }

    [Fact(DisplayName = "Strip links removes a link to a folder, which reads as its entrypoint, and keeps a link to another file"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task FolderLinks()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-folder-links");
        workspace.Write(Notes + "/rules/rules.md", "# Rules\n");
        workspace.Write(Note, $"# Note\n\nSee [the rules](rules/) and [the chart]({Chart}).\n");

        var result = await workspace.StripAsync(workspace.Request(StripCommandKind.Strip, [StripPart.Links]));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var link = Assert.Single(new MarkdownDocumentParser().Parse(workspace.Read(Note)).Links);
        Assert.Equal(Chart, link.RawDestination);
    }

    [Fact(DisplayName = "Strip refuses to flatten a folder that holds a file Open Forge manages and writes nothing"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task FlattenRefusesManagedFolder()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-flatten-managed");
        await workspace.InstallAsync();
        workspace.Write(Notes + "/team/note.md", "# Note\n");
        var before = workspace.SnapshotHashes();

        var result = await workspace.StripAsync(workspace.Request(StripCommandKind.Strip, [], WorkspaceLayout.DefaultMainRoot));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        var finding = Assert.Single(result.Findings);
        Assert.Equal(StripFindingCode.FlattenManaged, finding.Code);
        Assert.StartsWith(WorkspaceLayout.DefaultMainRoot + "/", finding.Path, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    // Strip never edits the managed entrypoint, so moving the documents below it would break Open Forge's own Entries.
    [Fact(DisplayName = "Strip refuses to flatten a Framework category that holds only its managed entrypoint and user documents below it"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task FlattenRefusesManagedEntrypointFolder()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-flatten-managed-entrypoint");
        await workspace.InstallAsync();
        var entrypoint = Assert.Single(Directory.GetFiles(workspace.Combine(MemoryCategory)));
        workspace.Write(MemoryCategory + "/team/note.md", "# Note\n");
        var before = workspace.SnapshotHashes();

        var result = await workspace.StripAsync(workspace.Request(StripCommandKind.Strip, [], MemoryCategory));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        var finding = Assert.Single(result.Findings);
        Assert.Equal((StripFindingCode.FlattenManaged, MemoryCategory + "/" + Path.GetFileName(entrypoint)), (finding.Code, finding.Path));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Strip flattens a user route whose entrypoint is an unowned scaffold"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task FlattenUserRouteWithScaffold()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-flatten-user-route");
        await workspace.InstallAsync();
        workspace.Write(Docs + "/rules/rules.md", "# Rules\n");
        await workspace.IncludeAsync(Docs);
        Assert.True(workspace.Exists(Docs + "/_" + Docs + ".md"));

        var result = await workspace.StripAsync(workspace.Request(StripCommandKind.Strip, [], Docs));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var moved = Assert.Single(result.Plan, entry => entry.Action == StripPlanAction.Move);
        Assert.Equal((Docs + "/rules/rules.md", Docs + "/rules.md"), (moved.Path, moved.To));
        Assert.True(workspace.Exists(Docs + "/rules.md"));
    }

    [Theory(DisplayName = "Strip refuses a document it cannot edit safely and writes nothing"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    [MemberData(nameof(Refusals))]
    public async Task DocumentUnsupportedWritesNothing(string document, string partName)
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-document-unsupported");
        workspace.Write(Note, document);
        var before = workspace.SnapshotHashes();

        var part = Enum.GetValues<StripPart>().Single(value => StrippingDefinitions.PartName(value) == partName);
        var result = await workspace.StripAsync(workspace.Request(StripCommandKind.Strip, [part]));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        var finding = Assert.Single(result.Findings);
        Assert.Equal((StripFindingCode.DocumentUnsupported, Note), (finding.Code, finding.Path));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Strip metadata handles the eval harness's frontmatter failure cases without changing their bytes"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    [MemberData(nameof(HarnessFailureCases))]
    public async Task HarnessFrontmatterFailures(byte[] document, bool refused)
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-harness-frontmatter-failures");
        workspace.WriteBytes(Note, document);
        var before = workspace.SnapshotHashes();

        var result = await workspace.StripAsync(workspace.Request(StripCommandKind.Strip, [StripPart.Metadata]));

        Assert.Equal(refused ? CliSemanticStatus.Blocked : CliSemanticStatus.Complete, result.Status);
        StripFindingCode[] expected = refused ? [StripFindingCode.DocumentUnsupported] : [];
        Assert.Equal(expected, result.Findings.Select(finding => finding.Code));
        Assert.Equal(!refused, result.NoOp);
        Assert.Equal(before, workspace.SnapshotHashes());
    }
}
