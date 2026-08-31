using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Rendering;

internal static class RouteHelpSections
{
    internal static CliHelpContent CreateGroup()
    {
        return new CliHelpContent(
        [
            new CliHelpSection(
                "Notes",
                "  The route group performs no operation.\n"
                + "  Planned but unavailable operations: update, move, and remove."),
        ]);
    }
}
