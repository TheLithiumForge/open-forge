using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;

internal static class RouteInspectHumanRenderer
{
    internal static string Render(CliPresentationRequest<RouteInspectResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        return presentation.Presentation.View switch
        {
            CliView.Compact => RouteInspectCompactRenderer.Render(presentation.Result),
            CliView.Expanded => RouteInspectExpandedRenderer.Render(presentation.Result),
            _ => throw new ArgumentOutOfRangeException(
                nameof(presentation),
                presentation.Presentation.View,
                "The route-inspect human view is not defined."),
        };
    }
}
