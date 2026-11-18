using System.Text;
using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Stripping;

// Covers the plain-docs Behavior contract's Required Evidence for single parts, folder links and every document refusal.
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

    // In the copy, entrypoints leaves out every entrypoint: an edited managed one uninstall would keep, a user's own with
    // text and a route scaffold. The project itself keeps every byte.
    [Fact(DisplayName = "Strip entrypoints leaves every entrypoint out of an export, managed and edited ones included, and keeps the project"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task EntrypointsInTheCopy()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-entrypoints-export");
        await workspace.InstallAsync();
        workspace.Write(Docs + "/rules/rules.md", "# Rules\n");
        workspace.Write(Docs + "/rules/_rules.md", "# Rules\n\nNotes the user wrote.\n");
        await workspace.IncludeAsync(Docs);
        var managed = MemoryCategory + "/" + Path.GetFileName(Assert.Single(Directory.GetFiles(workspace.Combine(MemoryCategory))));
        workspace.Write(managed, workspace.Read(managed) + "\nAn edit.\n");
        var before = workspace.SnapshotHashes();

        var kept = await workspace.ExportAsync(workspace.ExportRequest([], dryRun: true));
        var result = await workspace.ExportAsync(workspace.ExportRequest([StripPart.Entrypoints]));

        Assert.Contains(kept.Kept, file => file.Path == managed);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.DoesNotContain(result.Kept, file => file.Path == managed);
        Assert.Equal([Docs + "/rules/rules.md"], workspace.ExportFiles());
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Strip frontmatter removes the whole block with foreign keys, wins over metadata and keeps the Skill keys of a SKILL.md"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task FrontmatterRemovesTheWholeBlock()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-frontmatter");
        var skill = WorkspaceLayout.NativeSkillsPath + "/notes/" + SourceFormClassifier.NativeSkillFileName;
        const string SkillDocument = "---\nname: notes\ndescription: Notes\n---\n# Notes\n";
        workspace.Write(Note, "\uFEFF---\r\ntitle: \"Public\"\r\nopen-forge:\r\n  description: Note\r\n---\r\n# Note\r\n");
        workspace.Write(Notes + "/broken.md", "---\nname: [\n---\n# Broken\n");
        workspace.Write(skill, "---\nname: notes\ndescription: Notes\ntags: [Docs]\n---\n# Notes\n");

        var result = await workspace.UninstallAsync(workspace.Request(StripCommandKind.Uninstall, [StripPart.Metadata, StripPart.Frontmatter]));

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

        var result = await workspace.UninstallAsync(workspace.Request(StripCommandKind.Uninstall, [StripPart.Links]));

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

        var result = await workspace.UninstallAsync(workspace.Request(StripCommandKind.Uninstall, [StripPart.Links]));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var link = Assert.Single(new MarkdownDocumentParser().Parse(workspace.Read(Note)).Links);
        Assert.Equal(Chart, link.RawDestination);
    }

    [Theory(DisplayName = "Strip refuses a document it cannot edit safely and writes nothing"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    [MemberData(nameof(Refusals))]
    public async Task DocumentUnsupportedWritesNothing(string document, string partName)
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-document-unsupported");
        workspace.Write(Note, document);
        var before = workspace.SnapshotHashes();

        var part = Enum.GetValues<StripPart>().Single(value => StrippingDefinitions.PartName(value) == partName);
        var result = await workspace.UninstallAsync(workspace.Request(StripCommandKind.Uninstall, [part]));

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

        var result = await workspace.UninstallAsync(workspace.Request(StripCommandKind.Uninstall, [StripPart.Metadata]));

        Assert.Equal(refused ? CliSemanticStatus.Blocked : CliSemanticStatus.Complete, result.Status);
        StripFindingCode[] expected = refused ? [StripFindingCode.DocumentUnsupported] : [];
        Assert.Equal(expected, result.Findings.Select(finding => finding.Code));
        Assert.Equal(!refused, result.NoOp);
        Assert.Equal(before, workspace.SnapshotHashes());
    }
}
