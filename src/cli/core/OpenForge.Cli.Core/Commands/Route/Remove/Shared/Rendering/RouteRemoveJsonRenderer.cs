using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Rendering;

internal static class RouteRemoveJsonRenderer
{
    internal static string Render(CliPresentationRequest<RouteRemoveResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        return JsonSerializer.Serialize(
            RouteRemoveJsonProjection.Create(presentation.Result),
            RouteRemoveJsonContext.Default.RouteRemoveJsonDocument);
    }
}
