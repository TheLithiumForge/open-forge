using OpenForge.Cli.Core.Commands.Shared;
using OpenForge.Cli.Core.Commands.Tool.Models.Evidence;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Reporting;

internal static class ToolReadNextAction
{
    internal static string? Read(string command, CliSemanticStatus status, ToolReadIssue[] issues, string? workspace)
    {
        if (issues.Any(issue => issue.Code == ToolReadIssueCode.BackupPresent))
        {
            var next = CommandLines.RepairAutomaticPreview;
            if (workspace is not null) next = CliCommandText.InWorkspace(next, workspace);
            return next;
        }
        return status == CliSemanticStatus.Complete ? null : $"{CliSyntaxDefinitions.ExecutableName} {command} --help";
    }
}
