using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;
namespace OpenForge.Cli.Core.Presentation.Status.Shared.Help;

internal static class StatusHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Status.StatusText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Status.StatusText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Status.StatusText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Coverage", global::OpenForge.Cli.OutputText.Status.StatusText.HelpCoverageDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Status.StatusText.HelpRelatedCommands(),
        });
}
