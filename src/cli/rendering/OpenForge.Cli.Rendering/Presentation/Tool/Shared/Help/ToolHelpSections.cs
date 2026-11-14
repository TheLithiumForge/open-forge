using OpenForge.Cli.Core.Commands.Tool;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Tool.Shared.Help;

internal static class ToolHelpSections
{
    internal static CliHelpContent CreateGroup() => new([])
    {
        Usage = $"{CliSyntaxDefinitions.ExecutableName} {ToolDefinitions.Group.Name} <command> [options]",
        Footer = global::OpenForge.Cli.OutputText.Shared.SharedText.HelpCommandHelpDescription(),
    };
}
