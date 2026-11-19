using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.OutputText.Export;
using OpenForge.Cli.OutputText.Strip;

namespace OpenForge.Cli.Core.Presentation.Export;

internal static class ExportHelp
{
    internal static CliHelpContent Create() => CliCommandHelp.Create(new CliCommandHelpDefinition
    {
        Usage = ExportText.HelpSyntax(),
        Examples = ExportText.HelpExamples(),
        Behavior = ExportText.HelpBehavior(),
        Details =
        [
            new CliHelpSection("Parts", StripText.HelpParts()),
            new CliHelpSection("Flatten", StripText.HelpFlatten()),
        ],
        RelatedCommands = ExportText.HelpRelated(),
    });
}
