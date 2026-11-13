using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.OutputText.Tool.Run;

namespace OpenForge.Cli.Core.Presentation.Tool.Run;

internal static class ToolRunHelp
{
    internal static CliHelpContent Create() => CliCommandHelp.Create(new CliCommandHelpDefinition
    {
        Usage = ToolRunText.Usage,
        Examples = ToolRunText.Examples,
        Behavior = ToolRunText.Behavior,
        Details = [],
        RelatedCommands = [("open-forge tool install --help", null), ("open-forge tool status --help", null)],
    });
}
