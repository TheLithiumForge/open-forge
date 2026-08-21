using OpenForge.Cli.Commands.Route.List;
using OpenForge.Cli.Commands.Route.List.Rendering;
using OpenForge.Cli.Definitions;
using OpenForge.Cli.Invocation;
using OpenForge.Cli.Pipeline;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List;

internal static class RouteListResultProjectionTestSupport
{
    internal static string Create(RouteListResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        var presentation = new CliPresentation(
            CliOutputFormat.Json,
            CliView.Expanded,
            CliVerbosity.Normal);
        return RouteListJsonRenderer.Render(
            new CliPresentationMessage<RouteListResult>(
                result.Status,
                result,
                presentation));
    }
}
