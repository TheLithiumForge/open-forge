using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Shared;
using OpenForge.Cli.Core.Presentation.Repair.Models;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.OutputText.Tool;

namespace OpenForge.Cli.Core.Presentation.Repair.Shared.Selection;

internal static partial class RepairReportSelector
{
    private static CliReport<RepairData> SelectToolBackups(RepairResult result)
    {
        var backups = result.ToolBackups ?? throw new InvalidOperationException("Backup reporting requires backup facts.");
        var preview = result.Mode == RepairMode.DryRun;
        var headline = ToolText.BackupRestoreHeadline(preview);
        var rows = backups.Files.Select(file => new RepairDataTextRow(file.Target, ToolText.BackupRestoreRow(file.Backup, file.Verified))).ToArray();
        var nextCommand = CommandLines.RepairAutomaticPreview;
        var nextReason = ToolText.BackupBlockedNext();
        if (result.Status == CliSemanticStatus.Complete && preview)
        {
            nextCommand = CommandLines.RepairAutomatic;
            nextReason = ToolText.BackupApplyNext();
        }
        if (backups.Applied && backups.Files.All(file => file.Verified && file.BackupRemoved))
        {
            nextCommand = CommandLines.Repair;
            nextReason = ToolText.BackupRestoreNext();
        }
        if (result.WorkspacePath is { } root) nextCommand = CliCommandText.InWorkspace(nextCommand, root);
        return new()
        {
            Command = result.Command,
            Status = result.Status,
            Headline = new(headline, BackupHeadline(result.Status, preview)),
            Workspace = result.WorkspacePath is { } path ? new(path, result.WorkspaceExplicit) : null,
            Findings = result.Findings.Select(finding => Finding(result, finding)).ToArray(),
            Effects = backups.Files.SelectMany(file => BackupEffects(file, preview && result.Status == CliSemanticStatus.Complete)).ToArray(),
            Data = new()
            {
                Mode = CliReportVocabulary.Name(result.Mode),
                Selection = Selection(result),
                Repairs = [],
                Remaining = [],
                ToolBackups = backups.Files,
                TextRows = rows,
                TextHeadline = headline,
                ShowNoChanges = preview,
            },
            Next = new CliNextAction(nextCommand, nextReason),
        };
    }

    private static IEnumerable<CliEffect> BackupEffects(RepairToolBackupFile file, bool preview)
    {
        if (file.Written || preview && file.Before != file.After)
            yield return new()
            {
                Path = file.Target,
                Kind = CliEffectKind.File,
                Action = CliEffectAction.Restored,
                Outcome = Outcome(file, preview),
                Before = file.Before,
                After = file.After,
            };
        if (file.BackupRemoved || preview)
            yield return new()
            {
                Path = file.Backup,
                Kind = CliEffectKind.File,
                Action = CliEffectAction.Deleted,
                Outcome = preview ? CliEffectOutcome.Planned : CliEffectOutcome.Done,
                Before = file.After,
            };
    }

    private static CliEffectOutcome Outcome(RepairToolBackupFile file, bool preview)
    {
        if (file.Verified) return CliEffectOutcome.Done;
        if (file.Written) return CliEffectOutcome.Unknown;
        return preview ? CliEffectOutcome.Planned : CliEffectOutcome.NotStarted;
    }

    private static CliHeadlineKind BackupHeadline(CliSemanticStatus status, bool preview) => status switch
    {
        CliSemanticStatus.Complete => preview ? CliHeadlineKind.Preview : CliHeadlineKind.Done,
        CliSemanticStatus.Blocked => CliHeadlineKind.Blocked,
        CliSemanticStatus.Incomplete => CliHeadlineKind.Incomplete,
        CliSemanticStatus.Interrupted => CliHeadlineKind.Cancelled,
        CliSemanticStatus.Failed => CliHeadlineKind.Failed,
        CliSemanticStatus.Invalid => CliHeadlineKind.CannotStart,
        CliSemanticStatus.Attention => CliHeadlineKind.Warnings,
        _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };
}
