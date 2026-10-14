using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.OutputText.Route.Inspect;

namespace OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Help;

internal static class RouteInspectHelpSections
{
    internal static CliHelpContent CreateInspect()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Source and matching files", global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.HelpSourceAndMatchingFilesDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.HelpRelatedCommands(),
        });
}
