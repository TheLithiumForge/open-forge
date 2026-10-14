using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Update.Shared.Help;

internal static class UpdateHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Update.UpdateText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Update.UpdateText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Update.UpdateText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Authority", global::OpenForge.Cli.OutputText.Update.UpdateText.HelpAuthorityDetails()),
                new CliHelpSection("Management boundary", global::OpenForge.Cli.OutputText.Update.UpdateText.HelpManagementBoundaryDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Update.UpdateText.HelpRelatedCommands(),
        });
}
