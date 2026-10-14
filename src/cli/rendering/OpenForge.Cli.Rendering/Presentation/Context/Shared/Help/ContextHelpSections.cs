using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Presentation.Context.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Context.Shared.Help;

internal static class ContextHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Context.ContextText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Context.ContextText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Context.ContextText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Selection", global::OpenForge.Cli.OutputText.Context.ContextText.HelpSelectionDetails()),
                new CliHelpSection("Content and links", global::OpenForge.Cli.OutputText.Context.ContextText.HelpContentAndLinksDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Context.ContextText.HelpRelatedCommands(),
        });
}
