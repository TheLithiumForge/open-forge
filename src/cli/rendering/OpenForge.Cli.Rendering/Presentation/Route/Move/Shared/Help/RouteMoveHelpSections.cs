using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;
namespace OpenForge.Cli.Core.Presentation.Route.Move.Shared.Help;

internal static class RouteMoveHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Source and destination", global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.HelpSourceAndDestinationDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.HelpRelatedCommands(),
        });
}
