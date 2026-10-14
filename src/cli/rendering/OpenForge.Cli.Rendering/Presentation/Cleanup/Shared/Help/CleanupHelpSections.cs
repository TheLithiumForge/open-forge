using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Presentation.Cleanup.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Cleanup.Shared.Help;

internal static class CleanupHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Cleanup.CleanupText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Cleanup.CleanupText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Cleanup.CleanupText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Safety", global::OpenForge.Cli.OutputText.Cleanup.CleanupText.HelpSafetyDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Cleanup.CleanupText.HelpRelatedCommands(),
        });
}
