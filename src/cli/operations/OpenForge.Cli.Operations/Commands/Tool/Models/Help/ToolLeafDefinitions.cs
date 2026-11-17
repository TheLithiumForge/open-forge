using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;

namespace OpenForge.Cli.Core.Commands.Tool.Models.Help;

/// <summary>
/// The one owner of each Tool leaf's name and help purpose. The command bindings and the related-command
/// help both read these values, so Presentation needs no reference to a command's binding.
/// </summary>
internal static class ToolLeafDefinitions
{
    internal const string ListName = "list";
    internal const string ListPurpose = "List supported tools and their setup profiles.";
    internal const string InspectName = "inspect";
    internal const string InspectPurpose = "Show a tool's channels, native effects and machine facts.";
    internal const string StatusName = "status";
    internal const string StatusPurpose = "Check tool versions, project additions and PATH readiness.";

    internal static string WriteName(ToolWriteKind kind) => kind switch
    {
        ToolWriteKind.Install => "install",
        ToolWriteKind.Update => "update",
        ToolWriteKind.Remove => "remove",
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    internal static string WritePurpose(ToolWriteKind kind) => kind switch
    {
        ToolWriteKind.Install => "Install tools, run native setup and add project configuration.",
        ToolWriteKind.Update => "Update declared tools within supported versions.",
        ToolWriteKind.Remove => "Remove project additions and offer native uninstall.",
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };
}
