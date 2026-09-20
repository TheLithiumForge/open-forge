using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Help;

internal static class RouteInspectHelpSections
{
    internal static CliHelpContent CreateInspect()
    {
        return new CliHelpContent(
        [
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.HelpSyntax())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSourceReferences(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.HelpSourceReferences())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingInheritedGlobalOptions(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.HelpInheritedGlobalOptions())),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingResultsAndStreams(), CliResultHelp.ResultsAndStreams()),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingExamples(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.HelpExamples())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingRelatedCommands(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.HelpRelatedCommands())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingNotes(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.HelpNotes())),
        ]);
    }
}
