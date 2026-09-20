using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Extension.Create.Shared.Help;

internal static class ExtensionCreateHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(),
                ("  " + global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.HelpSyntax())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.HelpHeadingRequiredInputAndInteraction(),
                ("  " + global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.HelpRequiredInputAndInteraction())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.HelpHeadingManifest(),
                ("  " + global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.HelpManifest())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.HelpHeadingCatalogueAndScaffold(),
                ("  " + global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.HelpCatalogueAndScaffold())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.HelpHeadingModesAndGlobalOptions(),
                ("  " + global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.HelpModesAndGlobalOptions())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingExamples(),
                ("  " + global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.HelpExamples())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingResultsAndStreams(),
                CliResultHelp.ResultsAndStreams()),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.HelpHeadingWorkspaceAndRecoveryBoundary(),
                ("  " + global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.HelpWorkspaceAndRecoveryBoundary())),
        ]);
}
