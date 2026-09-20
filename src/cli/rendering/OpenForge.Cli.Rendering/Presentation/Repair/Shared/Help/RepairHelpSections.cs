using OpenForge.Cli.Core.Presentation.Repair.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Repair.Shared.Help;

internal static class RepairHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(), RepairWording.HelpSyntax()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSelection(), RepairWording.HelpSelection()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingCatalogue(), RepairWording.HelpCatalogue()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Repair.RepairText.HelpHeadingLibraryRecovery(), RepairWording.HelpLibraryRecovery()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Repair.RepairText.HelpHeadingPreviewAndSafety(), RepairWording.HelpPreviewAndSafety()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingGlobalOptions(), RepairWording.HelpGlobalOptions()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingResultsAndStreams(), CliResultHelp.ResultsAndStreams()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingNotes(), RepairWording.HelpNotes()),
        ]);
}
