using OpenForge.Cli.Core.Commands.Tool.Models.Evidence;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.OutputText.Tool;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Tool.Shared;

internal static class ToolReadPresentation
{
    internal static CliNextAction? Next(string? command, ToolReadIssue[] issues)
    {
        if (command is null) return null;
        var reason = issues.Any(issue => issue.Code == ToolReadIssueCode.BackupPresent) ? ToolText.BackupNext() : ToolText.ReadNext();
        return new(command, reason);
    }

    internal static CliHeadlineKind Headline(CliSemanticStatus status) => status switch
    {
        CliSemanticStatus.Complete => CliHeadlineKind.Done,
        CliSemanticStatus.Attention => CliHeadlineKind.Warnings,
        CliSemanticStatus.Incomplete => CliHeadlineKind.Incomplete,
        CliSemanticStatus.Invalid => CliHeadlineKind.CannotStart,
        CliSemanticStatus.Blocked => CliHeadlineKind.Blocked,
        CliSemanticStatus.Interrupted => CliHeadlineKind.Cancelled,
        _ => CliHeadlineKind.Failed,
    };

    internal static CliFinding[] Findings(string command, ToolReadIssue[] issues)
        => issues.Select(issue => new CliFinding
        {
            Code = command.Replace(' ', '-') + "." + issue.MachineCode,
            Severity = Severity(issue.Status),
            Title = ToolText.FindingTitle(),
            Message = issue.Cause,
            Subject = new(CliSubjectKind.Identifier, Id: issue.Subject),
        }).OrderBy(finding => finding.Severity).ToArray();

    private static CliSeverity Severity(CliSemanticStatus status)
        => status switch
        {
            CliSemanticStatus.Attention or CliSemanticStatus.Incomplete => CliSeverity.Warning,
            CliSemanticStatus.Invalid or CliSemanticStatus.Blocked or CliSemanticStatus.Failed or CliSemanticStatus.Interrupted => CliSeverity.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(status)),
        };
}
