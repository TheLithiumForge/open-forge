using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;

internal static class RouteListHumanRenderer
{
    internal static string Render(CliPresentationRequest<RouteListResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        return presentation.Presentation.View switch
        {
            Shell.Definitions.CliView.Compact => RouteListCompactRenderer.Render(presentation.Result),
            Shell.Definitions.CliView.Expanded => RouteListExpandedRenderer.Render(presentation.Result),
            _ => throw new ArgumentOutOfRangeException(
                nameof(presentation),
                presentation.Presentation.View,
                "The route-list human view is not defined."),
        };
    }
}
