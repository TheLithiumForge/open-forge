using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.OutputText.Strip;

namespace OpenForge.Cli.Core.Presentation.Strip;

internal static class StripHelp
{
    internal static CliHelpContent Create() => CliCommandHelp.Create(new CliCommandHelpDefinition
    {
        Usage = StripText.HelpSyntax(),
        Examples = StripText.HelpExamples(),
        Behavior = StripText.HelpBehavior(),
        Details =
        [
            new CliHelpSection("Parts", StripText.HelpParts()),
            new CliHelpSection("Flatten", StripText.HelpFlatten()),
        ],
        RelatedCommands = StripText.HelpRelated(),
    });
}
