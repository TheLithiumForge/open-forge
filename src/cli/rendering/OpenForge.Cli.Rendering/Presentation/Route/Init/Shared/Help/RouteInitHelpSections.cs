using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Init.Shared.Help;

internal static class RouteInitHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.HelpSyntax())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.HelpHeadingTarget(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.HelpTarget())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.HelpHeadingScaffoldMode(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.HelpScaffoldMode())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.HelpHeadingMetadata(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.HelpMetadata())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingWritePolicy(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.HelpWritePolicy())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingGlobalOptions(),
                ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.HelpGlobalOptionsWithDetailSelection())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingExamples(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.HelpExamples())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingRelatedCommands(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.HelpRelatedCommands())),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingResultsAndStreams(), CliResultHelp.ResultsAndStreams()),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingNotes(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.HelpNotes())),
        ]);
}
