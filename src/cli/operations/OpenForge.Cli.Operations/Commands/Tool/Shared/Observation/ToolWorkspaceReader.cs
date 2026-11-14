using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Evidence;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Inventory;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Provider;
using OpenForge.Cli.Core.Commands.Shared;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Observation;

internal static class ToolWorkspaceReader
{
    internal static async ValueTask<ToolWorkspaceRead> ReadAsync(CliWorkspace workspace, CancellationToken token)
    {
        var paths = new PhysicalPathResolver();
        var settings = await WorkspaceSettingsReader.ReadAsync(paths, workspace, token).ConfigureAwait(false);
        var receipt = await WorkspaceOwnershipReader.ReadAsync(paths, workspace, token).ConfigureAwait(false);
        var issues = new List<ToolReadIssue>();
        var settingsComplete = settings.State is WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete;
        if (!settingsComplete) issues.Add(new(ToolReadIssueCode.SettingsUnavailable, CliSemanticStatus.Incomplete, settings.LogicalPath, settings.Cause ?? "Settings aren't available."));
        var lockComplete = receipt.State is WorkspaceOwnershipReadState.Absent or WorkspaceOwnershipReadState.Complete;
        if (!lockComplete || settings.Document.Tools.Count != 0 && receipt.State == WorkspaceOwnershipReadState.Absent)
            issues.Add(new(ToolReadIssueCode.LockUnavailable, CliSemanticStatus.Incomplete, receipt.LogicalPath, receipt.Cause ?? "Registered tools have no receipt lock."));
        foreach (var row in new ToolWorkspaceRead(settings, receipt, [], []).Installed())
            if (row.Verification.State != ToolEvidenceState.Matching) issues.Add(new(
                row.Verification.State == ToolEvidenceState.Changed ? ToolReadIssueCode.ReceiptMismatch : ToolReadIssueCode.ReceiptMissing,
                row.Verification.State == ToolEvidenceState.Changed ? CliSemanticStatus.Attention : CliSemanticStatus.Incomplete,
                row.Id, row.Verification.Cause ?? "Tool receipt verification is unavailable."));
        var coverage = new List<ToolCoverage>
        {
            new("settings", settingsComplete ? ToolEvidenceState.Complete : ToolEvidenceState.Unavailable, settings.Cause),
            new("lock", lockComplete ? ToolEvidenceState.Complete : ToolEvidenceState.Unavailable, receipt.Cause),
        };
        foreach (var backup in ToolBackupReader.Read(workspace, ToolBackupPaths.WorkspaceTargets, token))
            issues.Add(new(backup.SafePaths ? ToolReadIssueCode.BackupPresent : ToolReadIssueCode.BackupUnavailable,
                backup.SafePaths ? CliSemanticStatus.Attention : CliSemanticStatus.Incomplete,
                Path.GetRelativePath(workspace.LexicalRoot, backup.Backup).Replace(Path.DirectorySeparatorChar, '/'),
                backup.Cause ?? $"A sibling backup is present at a Tool control path. Preview restoration with {CliCommandText.InWorkspace(CommandLines.RepairAutomaticPreview, workspace.LexicalRoot)}."));
        return new(settings, receipt, [.. coverage], [.. issues]);
    }
}
