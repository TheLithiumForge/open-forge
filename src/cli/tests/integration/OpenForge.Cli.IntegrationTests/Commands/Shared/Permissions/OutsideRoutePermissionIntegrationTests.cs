using OpenForge.Cli.Core.Commands.Extension.Models;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Shared.Permissions;

[Trait("Feature", "workspace-permissions"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class OutsideRoutePermissionIntegrationTests
{
    private const string CustomRoot = "knowledge/main";
    private const string Outside = "docs/team";
    private const string Owner = "outside-permission-control";

    [Theory(DisplayName = "Registration never grants Extension or Library permission to exact or case-alias outside destinations")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false, false), InlineData(WorkspaceLayout.DefaultMainRoot, true, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false, true), InlineData(WorkspaceLayout.DefaultMainRoot, true, true)]
    [InlineData(CustomRoot, false, false), InlineData(CustomRoot, true, false)]
    [InlineData(CustomRoot, false, true), InlineData(CustomRoot, true, true)]
    public async Task OutsidePermissionRequiresIndependentGrant(string root, bool alias, bool grant)
    {
        await AssertAdmissionAsync(root, alias ? "DOCS/TEAM/note.md" : Outside + "/note.md",
            grant ? WorkspacePermissionDecision.Granted : WorkspacePermissionDecision.Required, grant);
    }

    [Theory(DisplayName = "Registration leaves primary and native implicit permission unchanged")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(WorkspaceLayout.DefaultMainRoot, true)]
    [InlineData(CustomRoot, false), InlineData(CustomRoot, true)]
    public async Task PrimaryAndNativeControl(string root, bool native)
    {
        var path = native ? WorkspaceLayout.NativeSkillsPath + "/own/note.md" : root + "/own/note.md";
        await AssertAdmissionAsync(root, path, WorkspacePermissionDecision.NotRequired, grant: false);
    }

    private static async Task AssertAdmissionAsync(string root, string target, WorkspacePermissionDecision expected, bool grant)
    {
        using var temporary = TemporaryWorkspace.Create(Owner);
        temporary.WriteText(WorkspaceSettingsDefinitions.RelativePath, $$"""
            {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},
             "{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{root}}",
             "{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":["{{Outside}}"]}
            """);
        temporary.WriteText(root + "/" + WorkspaceLayoutNames.LoaderFileName, "# Independent Loader\n");
        temporary.WriteText(Outside + "/_team.md", "# Independent outside catalogue\n");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var read = await new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(workspace, TestContext.Current.CancellationToken);
        Assert.Equal(Outside, Assert.Single(read.Layout!.OutsideRoutes).DirectoryPath);
        var before = temporary.SnapshotHashes(includeRuntime: true);
        var extension = await new ExtensionPermissionOperation(NoPrompt).DetermineAsync(
            new(workspace, [new(target, ExtensionPermissionEffect.Copy)], Owner, false)
            {
                LayoutRead = read,
                ExplicitGrantPaths = grant ? [target] : [],
            }, TestContext.Current.CancellationToken);
        var library = await new LibraryPermissionOperation(NoPrompt).DetermineAsync(new()
        {
            Workspace = workspace,
            LibraryId = LibraryId.Create(Owner),
            SettingsObservation = read.Settings,
            LayoutRead = read,
            Targets = [new(target, LibraryPermissionTargetUse.Live) { Effect = LibraryPermissionEffect.CreateLink }],
            AllowPrompt = false,
            ExplicitGrantPaths = grant ? [target] : [],
        }, TestContext.Current.CancellationToken);
        Assert.Equal(expected, extension.Result.Decision);
        Assert.Equal(expected, library.Result.Decision);
        Assert.Equal(expected == WorkspacePermissionDecision.NotRequired, read.Layout.IsImplicitContentDestination(target));
        Assert.Equal(before, temporary.SnapshotHashes(includeRuntime: true));
        Assert.False(File.Exists(temporary.Combine(target)));
    }

    private static ValueTask<CliPromptReply<CliPermissionChoice>> NoPrompt(CliPermissionQuestion question, CliPromptPolicy policy, CancellationToken token)
        => throw new InvalidOperationException("This permission control must not prompt.");
}
