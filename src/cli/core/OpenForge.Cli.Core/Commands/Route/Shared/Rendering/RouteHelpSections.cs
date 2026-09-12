using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Rendering;

internal static class RouteHelpSections
{
    internal static CliHelpContent CreateGroup()
    {
        return new CliHelpContent(
        [
            new CliHelpSection(
                "Command help",
                "  Use open-forge route <command> --help for arguments, write policy, and examples."),
        ]);
    }
}
