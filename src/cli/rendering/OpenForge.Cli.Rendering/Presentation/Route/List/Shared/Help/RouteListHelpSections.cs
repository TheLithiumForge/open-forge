using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Route.List.Shared.Help;

internal static class RouteListHelpSections
{
    internal static CliHelpContent CreateList()
    {
        return new CliHelpContent(
        [
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(),
                ("  " + global::OpenForge.Cli.OutputText.Route.List.RouteListText.HelpSyntax())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSourceReferences(),
                ("  " + global::OpenForge.Cli.OutputText.Route.List.RouteListText.HelpSourceReferences())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Route.List.RouteListText.HelpHeadingDepth(),
                ("  " + global::OpenForge.Cli.OutputText.Route.List.RouteListText.HelpDepth())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingInheritedGlobalOptions(),
                ("  " + global::OpenForge.Cli.OutputText.Route.List.RouteListText.HelpInheritedGlobalOptions())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingResultsAndStreams(),
                CliResultHelp.ResultsAndStreams()),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingExamples(),
                ("  " + global::OpenForge.Cli.OutputText.Route.List.RouteListText.HelpExamples())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingRelatedCommands(),
                ("  " + global::OpenForge.Cli.OutputText.Route.List.RouteListText.HelpRelatedCommands())),
        ]);
    }
}
