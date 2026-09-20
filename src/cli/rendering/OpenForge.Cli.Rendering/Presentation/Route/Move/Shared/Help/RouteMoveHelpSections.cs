using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.Presentation.Route.Move.Shared.Help;

internal static class RouteMoveHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.HelpSyntax())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.HelpHeadingSource(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.HelpUnmanagedRouteSource())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.HelpHeadingDestination(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.HelpDestination())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingWritePolicy(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.HelpWritePolicy())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingExamples(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.HelpExamples())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingNotes(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.HelpNotes() + "\n")
                + CliResultHelp.ResultsAndStreams()),
        ]);
}
