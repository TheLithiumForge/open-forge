using System.CommandLine;

namespace OpenForge.Cli.Core.Commands.Route;

internal static class RouteBinding
{
    internal static Command CreateGroup()
    {
        var route = new Command(
            RouteDefinitions.RouteGroup.Name,
            RouteDefinitions.RouteGroup.Description);
        route.SetAction(static _ => 0);
        return route;
    }
}
