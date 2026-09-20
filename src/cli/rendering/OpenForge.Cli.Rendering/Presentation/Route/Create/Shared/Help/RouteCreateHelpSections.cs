using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Create.Shared.Help;

internal static class RouteCreateHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.HelpSyntax())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.HelpHeadingTarget(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.HelpTarget())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.HelpHeadingMetadata(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.HelpMetadata())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.HelpHeadingTemplate(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.HelpTemplate())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingWritePolicy(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.HelpWritePolicy())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingExamples(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.HelpExamples())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingNotes(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.HelpNotes())),
        ]);
}
