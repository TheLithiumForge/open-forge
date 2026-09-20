using OpenForge.Cli.Core.Presentation.Index.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.Presentation.Index.Shared.Help;

internal static class IndexHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(),
                IndexWording.HelpSyntax()),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSelection(),
                IndexWording.HelpSelection()),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingWritePolicy(),
                IndexWording.HelpWritePolicy()),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingGlobalOptions(),
                IndexWording.HelpGlobalOptions()),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingExamples(),
                IndexWording.HelpExamples()),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingRelatedCommands(),
                IndexWording.HelpRelatedCommands()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingResultsAndStreams(), CliResultHelp.ResultsAndStreams()),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingNotes(),
                IndexWording.HelpNotes()),
        ]);
}
