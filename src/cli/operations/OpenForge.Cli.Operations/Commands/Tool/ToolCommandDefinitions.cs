using OpenForge.Cli.Core.Commands.Tool.Models;
using OpenForge.Cli.Core.Shell.Definitions.Models;

namespace OpenForge.Cli.Core.Commands.Tool;

/// <summary>The one owner of the Tool group's names, help purposes, command identities and finding codes.</summary>
internal static class ToolCommandDefinitions
{
    internal static readonly CliSyntaxDefinition Group = new("tool", "Install supported tools and set them up for this project.");

    internal const string ListName = "list";
    internal const string ListPurpose = "List supported tools, their install method and their state.";
    internal const string InstallName = "install";
    internal const string InstallPurpose = "Install a tool and set it up for this project, or install this project's missing tools.";
    internal const string RemoveName = "remove";
    internal const string RemovePurpose = "Remove Open Forge's changes for a tool and offer its own uninstall.";
    internal const string IdArgument = "id";
    internal const string YesOption = "--yes";
    internal const string DryRunOption = "--dry-run";

    internal static string Identity(ToolCommandKind kind) => $"{Group.Name} {Name(kind)}";

    internal static string Name(ToolCommandKind kind) => kind switch
    {
        ToolCommandKind.List => ListName,
        ToolCommandKind.Install => InstallName,
        ToolCommandKind.Remove => RemoveName,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Tool command is not defined."),
    };

    internal static string FindingCode(ToolCommandKind kind, ToolFindingCode code) => $"{Group.Name}-{Name(kind)}.{code switch
    {
        ToolFindingCode.InvalidInput => "invalid-input",
        ToolFindingCode.UnknownTool => "unknown-tool",
        ToolFindingCode.SettingsUnavailable => "settings-unavailable",
        ToolFindingCode.OpenForgeMissing => "open-forge-missing",
        ToolFindingCode.IntegrationConflict => "integration-conflict",
        ToolFindingCode.RequirementMissing => "requirement-missing",
        ToolFindingCode.ConfirmationRequired => "confirmation-required",
        ToolFindingCode.CommandFailed => "command-failed",
        ToolFindingCode.TargetChangedDuringApply => "target-changed-during-apply",
        ToolFindingCode.WriteFailed => "write-failed",
        ToolFindingCode.Interrupted => "interrupted",
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Tool finding code is not defined."),
    }}";
}
