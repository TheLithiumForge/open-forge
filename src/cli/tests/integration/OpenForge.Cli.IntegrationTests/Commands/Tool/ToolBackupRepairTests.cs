using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Interaction;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Application;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.GitIgnore;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Snapshots;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool;

[Trait("Feature", "tool-backups"), Trait("Evidence", "Integration")]
public sealed class ToolBackupRepairTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;
    private const string Saved = "{\n}\n";
    private const string Changed = "{\"fixture\":true}\n";

    [Fact(DisplayName = "Repair help includes the reviewed control-file backup flow")]
    public async Task ReportsRepairHelp()
        => await new OpenForge.Cli.IntegrationTests.Shell.Presentation.Shared.Help.CommandHelpIntegrationTests().HelpPagesAt80Columns(RepairDefinitions.CommandIdentity);

    [Theory(DisplayName = "Root Repair previews without writes and restores invalid settings from an exact sibling backup")]
    [InlineData(true), InlineData(false)]
    public async Task RestoresSettings(bool preview)
    {
        using var temporary = TemporaryWorkspace.Create("tool-backup-repair");
        using var locks = WorkspaceLockTestStore.Create("tool-backup-repair-locks");
        var workspace = Workspace(temporary);
        _ = locks.Track(workspace);
        temporary.WriteText(WorkspaceSettingsDefinitions.RelativePath, "{");
        temporary.WriteText(ToolBackupPaths.Beside(WorkspaceSettingsDefinitions.RelativePath), Saved);
        var before = temporary.SnapshotHashes();
        var components = RepairOperationFactory.CreateDefaultComponents() with { ToolBackups = new(locks.Manager, Disabled()) };
        var result = await new RepairOperation(components).ExecuteAsync(new(workspace, preview ? RepairMode.DryRun : RepairMode.Apply, true, [], false), Token);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var facts = Assert.IsType<RepairToolBackupResult>(result.ToolBackups);
        Assert.Single(facts.Files);
        Assert.Equal(preview ? RepairToolBackupNextStep.Apply : RepairToolBackupNextStep.Continue, facts.Next.Step);
        if (preview)
        {
            Assert.Equal(before, temporary.SnapshotHashes());
            Assert.False(locks.InfrastructureExists);
        }
        else
        {
            Assert.Equal(Saved, await File.ReadAllTextAsync(temporary.Combine(WorkspaceSettingsDefinitions.RelativePath), Token));
            Assert.False(File.Exists(temporary.Combine(ToolBackupPaths.Beside(WorkspaceSettingsDefinitions.RelativePath))));
            Assert.True(facts.Files[0].Verified);
            Assert.True(facts.Files[0].BackupRemoved);
        }
    }

    [Theory(DisplayName = "Root Repair keeps saved bytes when a target or backup changes during confirmation")]
    [InlineData(false), InlineData(true)]
    public async Task RechecksConfirmationState(bool changeBackup)
    {
        using var temporary = TemporaryWorkspace.Create("tool-backup-stale");
        using var locks = WorkspaceLockTestStore.Create("tool-backup-stale-locks");
        var workspace = Workspace(temporary);
        _ = locks.Track(workspace);
        temporary.WriteText(WorkspaceSettingsDefinitions.RelativePath, "{}");
        temporary.WriteText(ToolBackupPaths.Beside(WorkspaceSettingsDefinitions.RelativePath), Saved);
        var interaction = Disabled() with
        {
            Confirm = (_, _, _, _) =>
        {
            File.WriteAllText(temporary.Combine(changeBackup ? ToolBackupPaths.Beside(WorkspaceSettingsDefinitions.RelativePath) : WorkspaceSettingsDefinitions.RelativePath), Changed);
            return ValueTask.FromResult(CliPromptReply<bool>.Answered(true));
        }
        };
        var result = await new RepairToolBackupOperation(locks.Manager, interaction).ExecuteAsync(new(workspace, RepairMode.Apply, false, [], true), Token);
        Assert.NotNull(result);
        Assert.NotEqual(CliSemanticStatus.Complete, result.Status);
        Assert.True(File.Exists(temporary.Combine(ToolBackupPaths.Beside(WorkspaceSettingsDefinitions.RelativePath))));
        Assert.Equal(changeBackup ? "{}" : Changed, await File.ReadAllTextAsync(temporary.Combine(WorkspaceSettingsDefinitions.RelativePath), Token));
    }

    [Theory(DisplayName = "Root Repair requires current permission and honors removal exclusions for the ignore file")]
    [InlineData(false, false, false), InlineData(true, true, false), InlineData(true, false, true)]
    public async Task RequiresIgnoreAuthority(bool allowed, bool excluded, bool succeeds)
    {
        using var temporary = TemporaryWorkspace.Create("tool-backup-ignore");
        using var locks = WorkspaceLockTestStore.Create("tool-backup-ignore-locks");
        var workspace = Workspace(temporary);
        _ = locks.Track(workspace);
        var grants = allowed ? $"\"{ManagedGitIgnoreSection.Path}\"" : string.Empty;
        var removals = excluded ? $"\"{ManagedGitIgnoreSection.Path}\"" : string.Empty;
        temporary.WriteText(WorkspaceSettingsDefinitions.RelativePath,
            $"{{\"{WorkspaceSettingsDefinitions.AllowInstallPathsProperty}\":[{grants}],\"{WorkspaceSettingsDefinitions.RemovedFilesProperty}\":[{removals}]}}");
        temporary.WriteText(ManagedGitIgnoreSection.Path, "current\n");
        temporary.WriteText(ToolBackupPaths.Beside(ManagedGitIgnoreSection.Path), "prior\n");
        var result = await new RepairToolBackupOperation(locks.Manager, Disabled()).ExecuteAsync(new(workspace, RepairMode.Apply, true, [], false), Token);
        Assert.NotNull(result);
        Assert.Equal(succeeds ? CliSemanticStatus.Complete : CliSemanticStatus.Blocked, result.Status);
        var facts = Assert.IsType<RepairToolBackupResult>(result.ToolBackups);
        Assert.Equal(succeeds ? RepairToolBackupNextStep.Continue : RepairToolBackupNextStep.Preview, facts.Next.Step);
        Assert.Equal(!succeeds, File.Exists(temporary.Combine(ToolBackupPaths.Beside(ManagedGitIgnoreSection.Path))));
        Assert.Equal(succeeds ? "prior\n" : "current\n", await File.ReadAllTextAsync(temporary.Combine(ManagedGitIgnoreSection.Path), Token));
    }

    [Fact(DisplayName = "Root Repair rejects malformed saved control records before any write")]
    public async Task RejectsInvalidBackup()
    {
        using var temporary = TemporaryWorkspace.Create("tool-backup-invalid");
        using var locks = WorkspaceLockTestStore.Create("tool-backup-invalid-locks");
        temporary.WriteText(WorkspaceOwnershipDefinitions.RelativePath, "{}");
        temporary.WriteText(ToolBackupPaths.Beside(WorkspaceOwnershipDefinitions.RelativePath), "{");
        var before = temporary.SnapshotHashes();
        var result = await new RepairToolBackupOperation(locks.Manager, Disabled()).ExecuteAsync(new(Workspace(temporary), RepairMode.Apply, true, [], false), Token);
        Assert.NotNull(result);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(before, temporary.SnapshotHashes());
        Assert.False(locks.InfrastructureExists);
    }

    [Fact(DisplayName = "Root Repair blocks a saved tool pin without a matching resulting lock")]
    public async Task RequiresMatchingToolPins()
    {
        using var temporary = TemporaryWorkspace.Create("tool-backup-pin-mismatch");
        using var locks = WorkspaceLockTestStore.Create("tool-backup-pin-mismatch-locks");
        temporary.WriteText(WorkspaceSettingsDefinitions.RelativePath, "{}");
        temporary.WriteText(ToolBackupPaths.Beside(WorkspaceSettingsDefinitions.RelativePath),
            $"{{\"{WorkspaceSettingsDefinitions.ToolsProperty}\":{{\"{ToolFixtureSource.Id}\":{{\"version\":\"{ToolFixtureSource.Version}\",\"roles\":[]}}}}}}");
        var before = temporary.SnapshotHashes();
        var result = await new RepairToolBackupOperation(locks.Manager, Disabled()).ExecuteAsync(new(Workspace(temporary), RepairMode.Apply, true, [], false), Token);
        Assert.NotNull(result);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(before, temporary.SnapshotHashes());
        Assert.False(locks.InfrastructureExists);
    }

    [Fact(DisplayName = "Root Repair rejects invalid saved ignore encoding without writing")]
    public async Task RejectsInvalidIgnoreEncoding()
    {
        using var temporary = TemporaryWorkspace.Create("tool-backup-invalid-encoding");
        using var locks = WorkspaceLockTestStore.Create("tool-backup-invalid-encoding-locks");
        temporary.WriteText(WorkspaceSettingsDefinitions.RelativePath,
            $"{{\"{WorkspaceSettingsDefinitions.AllowInstallPathsProperty}\":[\"{ManagedGitIgnoreSection.Path}\"]}}");
        temporary.WriteBytes(ToolBackupPaths.Beside(ManagedGitIgnoreSection.Path), [255, 254, 195]);
        var before = temporary.SnapshotHashes();
        var result = await new RepairToolBackupOperation(locks.Manager, Disabled()).ExecuteAsync(new(Workspace(temporary), RepairMode.Apply, true, [], false), Token);
        Assert.NotNull(result);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(before, temporary.SnapshotHashes());
        Assert.False(locks.InfrastructureExists);
    }

    [Fact(DisplayName = "Root Repair backup preview has reviewed text and JSON at every detail")]
    public async Task ReportsRestorationPreview()
    {
        using var temporary = TemporaryWorkspace.Create("tool-backup-preview-output");
        temporary.WriteText(WorkspaceSettingsDefinitions.RelativePath, "{");
        temporary.WriteText(ToolBackupPaths.Beside(WorkspaceSettingsDefinitions.RelativePath), Saved);
        var before = temporary.SnapshotHashes();
        await new ReadCommandOutputCapture(temporary.Path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = "tool-backup-preview",
            Arguments = [RepairDefinitions.CommandIdentity, RepairDefinitions.Automatic.Name, RepairDefinitions.DryRun.Name],
            ExitCode = 0,
        });
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    private static CliWorkspace Workspace(TemporaryWorkspace temporary)
        => new(temporary.Path, new PhysicalPathResolver().ResolveRoot(temporary.Path).GetContainedPhysicalPath(), CliWorkspaceSelectionMethod.ExplicitWorkspace);

    private static RepairInteraction Disabled() => new(
        static (_, _, _) => ValueTask.FromResult(CliPromptReply<RepairReferencePromptAnswer>.Unavailable()),
        static (_, _, _) => ValueTask.FromResult(CliPromptReply<RepairLibraryPromptAnswer>.Unavailable()),
        static (_, _, _, _) => ValueTask.FromResult(CliPromptReply<bool>.Unavailable()));
}
