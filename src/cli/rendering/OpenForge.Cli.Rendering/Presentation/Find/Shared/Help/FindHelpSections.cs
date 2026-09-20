using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.Presentation.Find.Shared.Help;

internal static class FindHelpSections
{
    internal static CliHelpContent Create()
    {
        return new CliHelpContent(
        [
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(),
                ("  " + global::OpenForge.Cli.OutputText.Find.FindText.HelpSyntax())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSourceReferences(),
                ("  " + global::OpenForge.Cli.OutputText.Find.FindText.HelpSourceReferences())
                ),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Find.FindText.HelpHeadingPredicatesAndRegions(),
                ("  " + global::OpenForge.Cli.OutputText.Find.FindText.HelpPredicatesAndRegions())
                ),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Find.FindText.HelpHeadingContentAndViews(),
                ("  " + global::OpenForge.Cli.OutputText.Find.FindText.HelpContentAndViews())
                ),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingInheritedGlobalOptions(),
                ("  " + global::OpenForge.Cli.OutputText.Find.FindText.HelpInheritedGlobalOptions())
                ),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingResultsAndStreams(), CliResultHelp.ResultsAndStreams()),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingExamples(),
                ("  " + global::OpenForge.Cli.OutputText.Find.FindText.HelpExamples())
                ),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingRelatedCommands(),
                ("  " + global::OpenForge.Cli.OutputText.Find.FindText.HelpRelatedCommands())
                ),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingNotes(),
                ("  " + global::OpenForge.Cli.OutputText.Find.FindText.HelpNotes())
                ),
        ]);
    }
}
