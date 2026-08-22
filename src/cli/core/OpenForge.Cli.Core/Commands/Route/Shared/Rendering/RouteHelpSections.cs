using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Rendering;

internal static class RouteHelpSections
{
    internal static CliHelpContent CreateGroup()
    {
        return new CliHelpContent(
        [
            new CliHelpSection(
                "Operations",
                "  list     available — list routed sources and descendants.\n"
                + "  inspect  available — inspect one source's route behavior.\n"
                + "  init     unavailable — planned route initialization.\n"
                + "  create   unavailable — planned route creation.\n"
                + "  update   unavailable — planned route update.\n"
                + "  move     unavailable — planned route move.\n"
                + "  remove   unavailable — planned route removal."),
            new CliHelpSection(
                "Notes",
                "  The route group performs no operation; select an available child command."),
        ]);
    }
}
