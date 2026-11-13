using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Shared;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Result;

internal static class RepairToolBackupNextAction
{
    internal static RepairToolBackupNext Read(RepairResult result, RepairToolBackupFile[] files, bool applied)
    {
        var nextCommand = CommandLines.RepairAutomaticPreview;
        var step = RepairToolBackupNextStep.Preview;
        if (result.Status == CliSemanticStatus.Complete && result.Mode == RepairMode.DryRun)
        {
            nextCommand = CommandLines.RepairAutomatic;
            step = RepairToolBackupNextStep.Apply;
        }
        if (applied && files.All(file => file.Verified && file.BackupRemoved))
        {
            nextCommand = CommandLines.Repair;
            step = RepairToolBackupNextStep.Continue;
        }
        if (result.WorkspacePath is { } root) nextCommand = CliCommandText.InWorkspace(nextCommand, root);
        return new(nextCommand, step);
    }
}
