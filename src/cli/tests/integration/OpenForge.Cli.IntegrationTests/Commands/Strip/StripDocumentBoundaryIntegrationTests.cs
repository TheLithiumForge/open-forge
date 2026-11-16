using System.Text;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
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

    // Each case names its part by the wire name the contract gives it.
    public static TheoryData<string, string> Refusals => new()
    {
        { "---\nopen-forge: [unclosed\n---\n# Note\n", StrippingDefinitions.PartName(StripPart.Metadata) },
        { "---\nopen-forge:\n  description: A\nopen-forge:\n  description: B\n---\n# Note\n", StrippingDefinitions.PartName(StripPart.Metadata) },
        { "---\n{description: Note, title: Keep}\n---\n# Note\n", StrippingDefinitions.PartName(StripPart.Metadata) },
        { "# Note\n\n## Entries\n\n- [A](a.md) - A\n\n## Entries\n\n- [B](b.md) - B\n", StrippingDefinitions.PartName(StripPart.Entries) },
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

    [Fact(DisplayName = "Strip entries alone removes the generated Entries and keeps metadata and links"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task EntriesAlone()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-entries-alone");
        await workspace.InstallAsync();
        workspace.Write(Docs + "/rules/rules.md", "---\nopen-forge:\n  description: Rules\n  tags: [Game]\n---\n# Rules\n\nBack to the [docs](../_docs.md).\n");
        await workspace.IncludeAsync(Docs);
        Assert.Equal(CliSemanticStatus.Complete, (await workspace.RunAsync([IndexDefinitions.CommandIdentity])).Status);
        var catalogue = Docs + "/_" + Docs + ".md";
        var rules = workspace.Read(Docs + "/rules/rules.md");

        var result = await workspace.StripAsync(workspace.Request(StripCommandKind.Strip, [StripPart.Entries]));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.NotEmpty(result.Plan);
        Assert.All(result.Plan, entry => Assert.Equal([StripRewritePart.Entries], entry.Parts));
        var facts = new MarkdownDocumentParser().Parse(workspace.Read(catalogue));
        Assert.Equal(MarkdownGeneratedRegionState.Absent, facts.GeneratedRegion.State);
        Assert.Equal(MarkdownFrontmatterState.Complete, facts.Frontmatter.State);
        Assert.Equal(rules, workspace.Read(Docs + "/rules/rules.md"));
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
