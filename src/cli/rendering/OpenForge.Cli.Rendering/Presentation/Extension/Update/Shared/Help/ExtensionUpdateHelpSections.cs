using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Presentation.Extension.Update.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Extension.Update.Shared.Help;

internal static class ExtensionUpdateHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Source and permissions", global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.HelpSourceAndPermissionsDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.HelpRelatedCommands(),
        });
}
