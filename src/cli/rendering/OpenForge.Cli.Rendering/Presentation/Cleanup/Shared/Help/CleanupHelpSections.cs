using OpenForge.Cli.Core.Presentation.Cleanup.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Cleanup.Shared.Help;

internal static class CleanupHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(), CleanupWording.HelpSyntax()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingCatalogue(), CleanupWording.HelpCatalogue()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingWritePolicy(), CleanupWording.HelpWritePolicy()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingGlobalOptions(), CleanupWording.HelpGlobalOptions()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingNotes(), CleanupWording.HelpNotes()),
        ]);
}
