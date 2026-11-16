using System.Text;
using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Stripping;

namespace OpenForge.Cli.IntegrationTests.Commands.Uninstall;

public sealed class UninstallIntegrationTests
{
    private const string Directives = WorkspaceLayout.DefaultMainRoot + "/directives/_directives.md";
    private const string Note = WorkspaceLayout.DefaultMainRoot + "/memory/crystallized/project.md";
    private const string NoteBytes = "---\nopen-forge:\n  description: Project note\n  tags: [Project]\n---\n# Project\n";
    private const string UserText = "\n## Mine\n\nKeep this.\n";

    [Fact(DisplayName = "Uninstall removes recognized Framework files and state and keeps what the user wrote or edited"), Trait("Feature", "uninstall"), Trait("Evidence", "Integration")]
    public async Task KeepsUserFiles()
    {
        using var workspace = await InstalledAsync("uninstall-keeps-user-files");
        var request = workspace.Request(StripCommandKind.Uninstall, []);

        var result = await workspace.UninstallAsync(request);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.True(result.Verified);
        Assert.Equal(Directives, Assert.Single(result.Kept).Path);
        Assert.Contains(result.Findings, finding => finding.Code == StripFindingCode.EditedKept && finding.Path == Directives);
        Assert.True(workspace.Exists(Directives));
        Assert.Equal(NoteBytes, workspace.Read(Note));
        Assert.False(workspace.Exists(WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.LoaderFileName));
        Assert.False(workspace.Exists(WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.SettingsFileName));
        Assert.False(workspace.Exists(WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.OwnershipFileName));
        Assert.False(workspace.Exists(WorkspaceLayoutNames.ClaudeHostFileName));
        Assert.False(workspace.Exists(WorkspaceRuntimePaths.DirectoryName));
        var host = new MarkdownDocumentParser().Parse(workspace.Read(WorkspaceLayoutNames.AgentHostFileName));
        Assert.Equal(MarkdownManagedHostState.Absent, host.ManagedHost.State);
        Assert.Equal(UserText.TrimStart('\n'), workspace.Read(WorkspaceLayoutNames.AgentHostFileName));
        Assert.True((await workspace.UninstallAsync(request)).NoOp);
    }

    [Fact(DisplayName = "Uninstall removes an edited managed file only when the request names it"), Trait("Feature", "uninstall"), Trait("Evidence", "Integration")]
    public async Task RemovesNamedEditedFile()
    {
        using var workspace = await InstalledAsync("uninstall-remove-edited");

        var result = await workspace.UninstallAsync(workspace.Request(StripCommandKind.Uninstall, []) with { RemoveEdited = ["./" + Directives] });

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Kept);
        Assert.Contains(result.Plan, entry => entry.Path == Directives && entry.Action == StripPlanAction.Delete && entry.State == StripEffectState.Done);
        Assert.False(workspace.Exists(Directives));
        Assert.Equal(NoteBytes, workspace.Read(Note));
    }

    [Fact(DisplayName = "Uninstall with strip parts strips the documents that stay and nothing it deletes"), Trait("Feature", "uninstall"), Trait("Evidence", "Integration")]
    public async Task StripsRemainingDocuments()
    {
        using var workspace = await InstalledAsync("uninstall-strip");

        var result = await workspace.UninstallAsync(workspace.Request(StripCommandKind.Uninstall, [StripPart.Metadata]) with { RemoveEdited = [Directives] });

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(NoteBytes[NoteBytes.IndexOf("# ", StringComparison.Ordinal)..], workspace.Read(Note));
    }

    [Fact(DisplayName = "Uninstall stops before any write when the ownership lock cannot be read"), Trait("Feature", "uninstall"), Trait("Evidence", "Integration")]
    public async Task UnreadableOwnershipBlocks()
    {
        using var workspace = await InstalledAsync("uninstall-ownership-unavailable");
        workspace.Write(WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.OwnershipFileName, "{");
        var before = workspace.SnapshotHashes();

        var result = await workspace.UninstallAsync(workspace.Request(StripCommandKind.Uninstall, []));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(StripFindingCode.OwnershipUnavailable, Assert.Single(result.Findings).Code);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Uninstall preview lists the plan and writes nothing"), Trait("Feature", "uninstall"), Trait("Evidence", "Integration")]
    public async Task PreviewWritesNothing()
    {
        using var workspace = await InstalledAsync("uninstall-preview");
        var before = workspace.SnapshotHashes();

        var result = await workspace.UninstallAsync(workspace.Request(StripCommandKind.Uninstall, [StripPart.Metadata], dryRun: true));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.NotEmpty(result.Plan);
        Assert.All(result.Plan, entry => Assert.Equal(StripEffectState.Planned, entry.State));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    internal static async Task<StripIntegrationWorkspace> InstalledAsync(string purpose)
    {
        var workspace = StripIntegrationWorkspace.Create(purpose);
        await workspace.InstallAsync();
        workspace.Write(Note, NoteBytes);
        File.AppendAllText(workspace.Combine(Directives), UserText, new UTF8Encoding(false));
        File.AppendAllText(workspace.Combine(WorkspaceLayoutNames.AgentHostFileName), UserText, new UTF8Encoding(false));
        return workspace;
    }
}
