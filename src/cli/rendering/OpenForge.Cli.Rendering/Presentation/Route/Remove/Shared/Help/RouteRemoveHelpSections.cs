using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Remove.Shared.Help;

internal static class RouteRemoveHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.HelpSyntax())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.HelpHeadingSource(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.HelpUnmanagedRouteSource())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingWritePolicy(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.HelpWritePolicy())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingExamples(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.HelpExamples())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingResultsAndStreams(),
                CliResultHelp.ResultsAndStreams()),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingNotes(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.HelpNotes())),
        ]);
}
