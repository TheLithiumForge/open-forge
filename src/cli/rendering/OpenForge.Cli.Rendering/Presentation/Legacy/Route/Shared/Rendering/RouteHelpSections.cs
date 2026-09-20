using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Legacy.Route.Shared.Rendering;

internal static class RouteHelpSections
{
    internal static CliHelpContent CreateGroup()
    {
        return new CliHelpContent(
        [
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingCommandHelp(),
                ("  " + global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.HelpCommandHelp())),
        ]);
    }
}
