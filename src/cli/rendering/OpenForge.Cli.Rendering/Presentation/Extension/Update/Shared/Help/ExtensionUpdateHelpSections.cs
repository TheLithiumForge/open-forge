using OpenForge.Cli.Core.Presentation.Extension.Update.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Extension.Update.Shared.Help;

internal static class ExtensionUpdateHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(), ExtensionUpdateWording.HelpSyntax()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.HelpHeadingSelectionAndDependencies(), ExtensionUpdateWording.HelpSelection()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingAuthority(), ExtensionUpdateWording.HelpAuthority()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingPermissions(), ExtensionUpdateWording.HelpPermissions()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingResultsAndStreams(), ExtensionUpdateWording.HelpResults()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingGlobalOptions(), CliResultHelp.ResultsAndStreams()),
        ]);
}
