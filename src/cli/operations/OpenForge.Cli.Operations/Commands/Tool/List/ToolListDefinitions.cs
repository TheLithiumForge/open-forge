using OpenForge.Cli.Core.Commands.Tool.Models.Help;

namespace OpenForge.Cli.Core.Commands.Tool.List;

internal static class ToolListDefinitions
{
    internal const string CommandName = ToolLeafDefinitions.ListName;
    internal const string Purpose = ToolLeafDefinitions.ListPurpose;
    internal const string CommandIdentity = "tool list";
    internal const string InvalidInputCode = "tool-list.invalid-input";
    internal const string UnavailableCode = "tool-list.unavailable";
}
