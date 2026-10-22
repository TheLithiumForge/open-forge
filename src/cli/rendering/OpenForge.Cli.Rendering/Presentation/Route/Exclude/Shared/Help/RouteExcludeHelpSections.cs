using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Exclude.Shared.Help;

internal static class RouteExcludeHelpSections
{
    internal static CliHelpContent Create() => CliCommandHelp.Create(new CliCommandHelpDefinition
    {
        Usage = global::OpenForge.Cli.OutputText.Route.Exclude.RouteExcludeText.HelpSyntax(),
        Examples = global::OpenForge.Cli.OutputText.Route.Exclude.RouteExcludeText.HelpExamples(),
        Behavior = global::OpenForge.Cli.OutputText.Route.Exclude.RouteExcludeText.HelpBehavior(),
        Details =
        [
            new CliHelpSection("Boundary", global::OpenForge.Cli.OutputText.Route.Exclude.RouteExcludeText.HelpBoundary()),
        ],
        RelatedCommands = global::OpenForge.Cli.OutputText.Route.Exclude.RouteExcludeText.HelpRelated(),
    });
}
