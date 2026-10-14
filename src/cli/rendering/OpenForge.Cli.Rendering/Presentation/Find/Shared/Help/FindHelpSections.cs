using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;
namespace OpenForge.Cli.Core.Presentation.Find.Shared.Help;

internal static class FindHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Find.FindText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Find.FindText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Find.FindText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Filters", global::OpenForge.Cli.OutputText.Find.FindText.HelpFiltersDetails()),
                new CliHelpSection("Content and scope", global::OpenForge.Cli.OutputText.Find.FindText.HelpContentAndScopeDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Find.FindText.HelpRelatedCommands(),
        });
}
