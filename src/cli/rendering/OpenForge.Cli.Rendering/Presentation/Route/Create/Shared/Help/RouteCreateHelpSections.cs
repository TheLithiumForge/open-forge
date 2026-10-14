using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Create.Shared.Help;

internal static class RouteCreateHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Target and metadata", global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.HelpTargetAndMetadataDetails()),
                new CliHelpSection("Globs and Templates", global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.HelpGlobsAndTemplatesDetails()),
                new CliHelpSection("Boundary", global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.HelpBoundaryDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.HelpRelatedCommands(),
        });
}
