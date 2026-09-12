using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;

internal static class RouteInspectHumanNext
{
    internal static string? Line(RouteInspectResult result)
        => result.Next is { } next ? $"Next: {CliHumanText.Text(next.Command)}" : null;
}
