using OpenForge.Cli.Core.Shell.Definitions.Models;

namespace OpenForge.Cli.Core.Commands.Route;

internal static class RouteDefinitions
{
    internal static readonly CliSyntaxDefinition RouteGroup = new(
        "route",
        "Inspect and maintain routed sources.");
}
