using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Update.Shared.Help;

internal static class RouteUpdateHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.HelpSyntax())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.HelpHeadingTarget(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.HelpTarget())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.HelpHeadingMetadata(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.HelpMetadata())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.HelpHeadingTemplate(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.HelpTemplate())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingWritePolicy(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.HelpWritePolicy())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingExamples(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.HelpExamples())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingNotes(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.HelpNotes() + "\n")
                + CliResultHelp.ResultsAndStreams()),
        ]);
}
