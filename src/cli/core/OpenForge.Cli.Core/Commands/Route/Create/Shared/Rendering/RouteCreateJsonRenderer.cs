using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Rendering;

internal static class RouteCreateJsonRenderer
{
    internal static string Render(
        CliPresentationRequest<RouteCreateResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        return JsonSerializer.Serialize(
            RouteCreateJsonProjection.Create(presentation.Result),
            RouteCreateJsonContext.Default.RouteCreateJsonDocument);
    }
}
