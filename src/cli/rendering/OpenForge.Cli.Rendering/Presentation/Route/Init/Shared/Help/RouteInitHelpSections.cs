using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Init.Shared.Help;

internal static class RouteInitHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Target and restoration", global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.HelpTargetAndRestorationDetails()),
                new CliHelpSection("Metadata", global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.HelpMetadataDetails()),
                new CliHelpSection("Boundary", global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.HelpBoundaryDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.HelpRelatedCommands(),
        });
}
