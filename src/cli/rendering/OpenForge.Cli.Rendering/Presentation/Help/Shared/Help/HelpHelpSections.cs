using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Help.Shared.Help;

internal static class HelpHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Help.HelpText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Help.HelpText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Help.HelpText.HelpBehavior(),
            Details = [],
            RelatedCommands = global::OpenForge.Cli.OutputText.Help.HelpText.HelpRelatedCommands(),
        });
}
