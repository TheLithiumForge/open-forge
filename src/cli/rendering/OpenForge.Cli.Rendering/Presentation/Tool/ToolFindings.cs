using OpenForge.Cli.Core.Commands.Tool.Models;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.OutputText.Tool;

namespace OpenForge.Cli.Core.Presentation.Tool;

/// <summary>Words each Tool finding with its plain next step. Finding codes stay owned by the command definitions.</summary>
internal static class ToolFindings
{
    internal static CliFinding Create(ToolCommandKind kind, string command, ToolFinding finding, Func<string, string?> names) => new()
    {
        Code = ToolCommandDefinitions.FindingCode(kind, finding.Code),
        Severity = finding.Status is CliSemanticStatus.Attention or CliSemanticStatus.Incomplete ? CliSeverity.Warning : CliSeverity.Error,
        Title = ToolText.FindingTitle(),
        Message = Message(command, finding, finding.ToolId is { } id ? names(id) ?? id : null),
        Subject = finding.Path is { } path ? new(CliSubjectKind.File, Path: path) : new(CliSubjectKind.Identifier, Id: finding.ToolId ?? command),
    };

    private static string Message(string command, ToolFinding finding, string? name) => finding.Code switch
    {
        ToolFindingCode.RequirementMissing when finding.Detail == ToolWire.NpmRequirement => ToolText.NpmMissing(name ?? command),
        ToolFindingCode.RequirementMissing when finding.Detail == ToolWire.PowerShellRequirement
            => ToolText.PowerShellMissing(name ?? command),
        ToolFindingCode.RequirementMissing => ToolText.ProgramMissing(name ?? command, finding.Detail ?? command),
        ToolFindingCode.CommandFailed => $"{ToolText.CommandFailed(finding.Detail ?? command)} {ToolText.CommandFailedNext()}",
        ToolFindingCode.SettingsUnavailable => ToolText.SettingsUnavailable(finding.Path ?? command, finding.Detail),
        ToolFindingCode.OpenForgeMissing => ToolText.OpenForgeMissing(),
        ToolFindingCode.IntegrationConflict => ToolText.IntegrationConflict(finding.Path ?? command, finding.Detail),
        ToolFindingCode.TargetChangedDuringApply => ToolText.TargetChanged(),
        ToolFindingCode.WriteFailed => ToolText.WriteFailed(finding.Detail),
        ToolFindingCode.UnknownTool => ToolText.UnknownTool(finding.ToolId ?? command),
        ToolFindingCode.ConfirmationRequired => CliFindingWording.ConfirmationRequired(char.ToUpperInvariant(command[0]) + command[1..]),
        ToolFindingCode.Interrupted => ToolText.Interrupted(),
        ToolFindingCode.InvalidInput => ToolText.InvalidInput(finding.Detail),
        _ => throw new ArgumentOutOfRangeException(nameof(finding), finding.Code, "The Tool finding code is not defined."),
    };
}
