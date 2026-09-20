using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Update.Shared.Help;

internal static class UpdateHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                heading: global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(),
                body: ("  " + global::OpenForge.Cli.OutputText.Update.UpdateText.HelpSyntax())),
            new CliHelpSection(
                heading: global::OpenForge.Cli.OutputText.Update.UpdateText.HelpHeadingReconciliation(),
                body: ("  " + global::OpenForge.Cli.OutputText.Update.UpdateText.HelpReconciliation())),
            new CliHelpSection(
                heading: global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingAuthority(),
                body: ("  " + global::OpenForge.Cli.OutputText.Update.UpdateText.HelpAuthority())),
            new CliHelpSection(
                heading: global::OpenForge.Cli.OutputText.Update.UpdateText.HelpHeadingExecution(),
                body: ("  " + global::OpenForge.Cli.OutputText.Update.UpdateText.HelpExecution())),
            new CliHelpSection(
                heading: global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingGlobalOptions(),
                body: ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.HelpGlobalOptionsWithDetailSelection())),
            new CliHelpSection(
                heading: global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingExamples(),
                body: ("  " + global::OpenForge.Cli.OutputText.Update.UpdateText.HelpExamples())),
            new CliHelpSection(
                heading: global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingRelatedCommands(),
                body: ("  " + global::OpenForge.Cli.OutputText.Update.UpdateText.HelpRelatedCommands())),
            new CliHelpSection(heading: global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingResultsAndStreams(), body: CliResultHelp.ResultsAndStreams()),
            new CliHelpSection(
                heading: global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingNotes(),
                body: ("  " + global::OpenForge.Cli.OutputText.Update.UpdateText.HelpNotes())),
        ]);
}
