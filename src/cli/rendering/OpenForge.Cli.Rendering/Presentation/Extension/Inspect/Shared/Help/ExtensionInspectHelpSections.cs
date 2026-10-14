using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;
namespace OpenForge.Cli.Core.Presentation.Extension.Inspect.Shared.Help;

internal static class ExtensionInspectHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Source and comparison", global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.HelpSourceAndComparisonDetails()),
                new CliHelpSection("Fingerprints", global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.HelpFingerprintsDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.HelpRelatedCommands(),
        });
}
