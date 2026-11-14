using System.Text.Json;
using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Status;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Evidence;
using OpenForge.Cli.Core.Commands.Tool.Status;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.GitIgnore;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.IntegrationTests.Commands.Status;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool;

[Trait("Feature", "tool-backups"), Trait("Evidence", "Integration")]
public sealed class ToolBackupReportingTests
{
    private const string PrivateSavedBytes = "Private fixture bytes. These bytes aren't report data.\n";

    [Theory(DisplayName = "Tool Status, root Status and Doctor report sibling backups independently of current control records")]
    [InlineData(null), InlineData(WorkspaceSettingsDefinitions.RelativePath), InlineData(WorkspaceOwnershipDefinitions.RelativePath)]
    public async Task ReportsWithoutReadingBackupContents(string? brokenRecord)
    {
        using var workspace = await StatusIntegrationWorkspace.CreateInstalledAsync("tool-backup-reports");
        workspace.WriteText(ToolBackupPaths.Beside(ManagedGitIgnoreSection.Path), PrivateSavedBytes);
        workspace.WritePostInstallAgentText(ToolBackupPaths.Beside(WorkspaceSettingsDefinitions.RelativePath), PrivateSavedBytes);
        workspace.WritePostInstallAgentText(ToolBackupPaths.Beside(WorkspaceOwnershipDefinitions.RelativePath), PrivateSavedBytes);
        if (brokenRecord is not null) workspace.OverwriteInstalledText(brokenRecord, "{");
        var before = workspace.SnapshotHashes();
        var tools = await new ToolStatusOperation().ExecuteAsync(new(workspace.Workspace, null), TestContext.Current.CancellationToken);
        Assert.Equal(brokenRecord is not null ? CliSemanticStatus.Incomplete : CliSemanticStatus.Attention, tools.Status);
        Assert.Equal(3, tools.Issues.Count(issue => issue.Code == ToolReadIssueCode.BackupPresent));
        using var caller = TemporaryWorkspace.Create("tool-backup-caller");
        foreach (var (command, code) in new[]
        {
            (StatusDefinitions.CommandIdentity, OpenForge.Cli.Core.Presentation.Status.Shared.Wording.StatusWording.MachineCode(StatusFindingCode.ToolBackupPresent)),
            (DoctorDefinitions.CommandIdentity, DoctorDefinitions.ReadFindingKind(DoctorFindingKind.ToolBackupPresent)),
        })
        {
            var output = await CliHostCapture.RunAsync([command, "--workspace", workspace.Path, "--format", "json", "--detail", CliPresentationDefinitions.Standard], caller.Path);
            using var json = JsonDocument.Parse(output.Output);
            var matches = json.RootElement.GetProperty(CliJsonFields.Findings).EnumerateArray()
                .Count(finding => finding.GetProperty(CliJsonFields.Code).GetString() == code);
            Assert.True(matches == 3, $"{command}: {output.Output}");
        }
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Empty(caller.SnapshotHashes());
    }

    [Fact(DisplayName = "Tool backup warnings have reviewed text and JSON output at every detail")]
    public async Task ReportsBackupOutput()
    {
        using var workspace = await StatusIntegrationWorkspace.CreateInstalledAsync("tool-backup-output");
        workspace.WriteText(ToolBackupPaths.Beside(ManagedGitIgnoreSection.Path), PrivateSavedBytes);
        var before = workspace.SnapshotHashes();
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var command in new[] { new[] { "tool", "status" }, new[] { "status" }, new[] { "doctor" } })
            await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
            {
                Situation = string.Join('-', command),
                Arguments = command,
                ExitCode = 2,
            }, snapshotCollector: snapshots);
        CommandOutputSnapshot.MatchDetailSnapshot(snapshots, testName: nameof(ReportsBackupOutput));
        Assert.Equal(before, workspace.SnapshotHashes());
    }
}
