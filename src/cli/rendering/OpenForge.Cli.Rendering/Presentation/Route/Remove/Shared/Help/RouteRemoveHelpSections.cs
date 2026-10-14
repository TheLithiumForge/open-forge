using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Remove.Shared.Help;

internal static class RouteRemoveHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Safety", global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.HelpSafetyDetails()),
                new CliHelpSection("References and restoration", global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.HelpReferencesAndRestorationDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.HelpRelatedCommands(),
        });
}
