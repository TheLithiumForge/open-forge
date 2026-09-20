using OpenForge.Cli.Core.Presentation.Extension.Install.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Extension.Install.Shared.Help;

internal static class ExtensionInstallHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(), ExtensionInstallWording.HelpSyntax()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.HelpHeadingSelectionAndDependencies(), ExtensionInstallWording.HelpSelection()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.HelpHeadingExternalPackageLayout(), ExtensionInstallWording.HelpLayout()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.HelpHeadingInteractionAndAutomaticMode(), ExtensionInstallWording.HelpInteraction()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.HelpHeadingInitialForce(), ExtensionInstallWording.HelpForce()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingResultsAndStreams(), ExtensionInstallWording.HelpResults()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingGlobalOptions(), CliResultHelp.ResultsAndStreams()),
        ]);
}
