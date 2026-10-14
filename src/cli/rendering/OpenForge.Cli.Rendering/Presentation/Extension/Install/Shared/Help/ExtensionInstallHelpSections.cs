using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Presentation.Extension.Install.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Extension.Install.Shared.Help;

internal static class ExtensionInstallHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Selection and layout", global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.HelpSelectionAndLayoutDetails()),
                new CliHelpSection("Managed changes", global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.HelpManagedChangesDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.HelpRelatedCommands(),
        });
}
