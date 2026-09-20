using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.Presentation.References.Shared.Help;

internal static class ReferencesHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(),
                ("  " + global::OpenForge.Cli.OutputText.References.ReferencesText.HelpSyntax())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.References.ReferencesText.HelpHeadingSourceAndDirection(),
                ("  " + global::OpenForge.Cli.OutputText.References.ReferencesText.HelpSourceAndDirection())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.References.ReferencesText.HelpHeadingIncomingFilters(),
                ("  " + global::OpenForge.Cli.OutputText.References.ReferencesText.HelpIncomingFilters())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingGlobalOptions(),
                ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.HelpGlobalOptionsWithDetailSelection())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingExamples(),
                ("  " + global::OpenForge.Cli.OutputText.References.ReferencesText.HelpExamples())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingRelatedCommands(),
                ("  " + global::OpenForge.Cli.OutputText.References.ReferencesText.HelpRelatedCommands())),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingResultsAndStreams(), CliResultHelp.ResultsAndStreams()),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingNotes(),
                ("  " + global::OpenForge.Cli.OutputText.References.ReferencesText.HelpNotes())),
        ]);
}
