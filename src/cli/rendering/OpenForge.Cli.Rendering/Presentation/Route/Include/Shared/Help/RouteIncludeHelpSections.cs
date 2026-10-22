using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Include.Shared.Help;

internal static class RouteIncludeHelpSections
{
    internal static CliHelpContent Create() => CliCommandHelp.Create(new CliCommandHelpDefinition
    {
        Usage = global::OpenForge.Cli.OutputText.Route.Include.RouteIncludeText.HelpSyntax(),
        Examples = global::OpenForge.Cli.OutputText.Route.Include.RouteIncludeText.HelpExamples(),
        Behavior = global::OpenForge.Cli.OutputText.Route.Include.RouteIncludeText.HelpBehavior(),
        Details =
        [
            new CliHelpSection("Metadata", global::OpenForge.Cli.OutputText.Route.Include.RouteIncludeText.HelpMetadata()),
            new CliHelpSection("Boundary", global::OpenForge.Cli.OutputText.Route.Include.RouteIncludeText.HelpBoundary()),
        ],
        RelatedCommands = global::OpenForge.Cli.OutputText.Route.Include.RouteIncludeText.HelpRelated(),
    });
}
