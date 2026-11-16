using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.OutputText.Strip;
using OpenForge.Cli.OutputText.Uninstall;

namespace OpenForge.Cli.Core.Presentation.Uninstall;

internal static class UninstallHelp
{
    internal static CliHelpContent Create() => CliCommandHelp.Create(new CliCommandHelpDefinition
    {
        Usage = UninstallText.HelpSyntax(),
        Examples = UninstallText.HelpExamples(),
        Behavior = UninstallText.HelpBehavior(),
        Details =
        [
            new CliHelpSection("Kept", UninstallText.HelpKept()),
            new CliHelpSection("Parts", StripText.HelpParts()),
        ],
        RelatedCommands = UninstallText.HelpRelated(),
    });
}
