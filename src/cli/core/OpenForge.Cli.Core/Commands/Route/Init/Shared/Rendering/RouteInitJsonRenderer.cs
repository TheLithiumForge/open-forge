using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Serialization;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Rendering;

internal static class RouteInitJsonRenderer
{
    internal static string Render(
        CliPresentationRequest<RouteInitResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        return JsonSerializer.Serialize(
            RouteInitJsonProjection.Create(presentation.Result),
            CliJsonContext.Default.RouteInitJsonDocument);
    }
}
