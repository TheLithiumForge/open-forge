using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Update.Shared.Help;

internal static class RouteUpdateHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Target and metadata", global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.HelpTargetAndMetadataDetails()),
                new CliHelpSection("Globs and Templates", global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.HelpGlobsAndTemplatesDetails()),
                new CliHelpSection("Write boundary", global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.HelpWriteBoundaryDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.HelpRelatedCommands(),
        });
}
