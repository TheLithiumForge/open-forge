using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Presentation.Extension.Remove.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Extension.Remove.Shared.Help;

internal static class ExtensionRemoveHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Ownership and permissions", global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.HelpOwnershipAndPermissionsDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.HelpRelatedCommands(),
        });
}
