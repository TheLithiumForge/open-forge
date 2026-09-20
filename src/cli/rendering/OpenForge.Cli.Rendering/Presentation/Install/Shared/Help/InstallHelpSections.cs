using OpenForge.Cli.Core.Presentation.Install.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Install.Shared.Help;

internal static class InstallHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(), InstallWording.HelpSyntax()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Install.InstallText.HelpHeadingEstablishment(), InstallWording.HelpEstablishment()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingWritePolicy(), InstallWording.HelpWritePolicy()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Install.InstallText.HelpHeadingConfirmation(), InstallWording.HelpConfirmation()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingGlobalOptions(), InstallWording.HelpGlobalOptions()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingExamples(), InstallWording.HelpExamples()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingRelatedCommands(), InstallWording.HelpRelatedCommands()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingResultsAndStreams(), CliResultHelp.ResultsAndStreams()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingNotes(), InstallWording.HelpNotes()),
        ]);
}
