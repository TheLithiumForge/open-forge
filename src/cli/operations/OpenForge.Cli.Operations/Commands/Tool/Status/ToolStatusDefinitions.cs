using OpenForge.Cli.Core.Commands.Tool.Models.Help;

namespace OpenForge.Cli.Core.Commands.Tool.Status;

internal static class ToolStatusDefinitions
{
    internal const string CommandName = ToolLeafDefinitions.StatusName;
    internal const string Purpose = ToolLeafDefinitions.StatusPurpose;
    internal const string CommandIdentity = "tool status";
    internal const string InvalidInputCode = "tool-status.invalid-input";
    internal const string UnavailableCode = "tool-status.unavailable";
}
