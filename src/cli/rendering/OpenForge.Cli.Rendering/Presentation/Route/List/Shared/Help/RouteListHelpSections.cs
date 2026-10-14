using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Route.List.Shared.Help;

internal static class RouteListHelpSections
{
    internal static CliHelpContent CreateList()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Route.List.RouteListText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Route.List.RouteListText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Route.List.RouteListText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Sources and depth", global::OpenForge.Cli.OutputText.Route.List.RouteListText.HelpSourcesAndDepthDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Route.List.RouteListText.HelpRelatedCommands(),
        });
}
