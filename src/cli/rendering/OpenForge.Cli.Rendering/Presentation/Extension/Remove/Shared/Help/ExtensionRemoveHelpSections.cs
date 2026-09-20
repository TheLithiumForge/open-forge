using OpenForge.Cli.Core.Presentation.Extension.Remove.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Extension.Remove.Shared.Help;

internal static class ExtensionRemoveHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(), ExtensionRemoveWording.HelpSyntax()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.HelpHeadingSelectionAndDependencies(), ExtensionRemoveWording.HelpSelection()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.HelpHeadingOwnershipAndRecovery(), ExtensionRemoveWording.HelpOwnership()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingPermissions(), ExtensionRemoveWording.HelpPermissions()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingResultsAndStreams(), ExtensionRemoveWording.HelpResults()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingGlobalOptions(), CliResultHelp.ResultsAndStreams()),
        ]);
}
