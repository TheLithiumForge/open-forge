using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Extension.Create.Shared.Help;

internal static class ExtensionCreateHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Input and scaffold", global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.HelpInputAndScaffoldDetails()),
                new CliHelpSection("Manifest", global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.HelpManifestDetails()),
                new CliHelpSection("Boundary", global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.HelpBoundaryDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.HelpRelatedCommands(),
        });
}
