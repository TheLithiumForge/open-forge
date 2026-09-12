using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Rendering;

internal static class RouteUpdateJsonRenderer
{
    internal static string Render(
        CliPresentationRequest<RouteUpdateResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var document = RouteUpdateJsonProjection.Create(presentation.Result);
        if (presentation.Presentation.View == CliView.Compact)
        {
            return JsonSerializer.Serialize(
                CliCompactJsonProjection.Create(presentation.Result, document.Result),
                RouteUpdateJsonContext.Compact.CompactDocument);
        }

        return JsonSerializer.Serialize(document,
            RouteUpdateJsonContext.Default.RouteUpdateJsonDocument);
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(RouteUpdateJsonDocument))]
[JsonSerializable(typeof(CliCompactJsonDocument<RouteUpdateJsonResult>), TypeInfoPropertyName = "CompactDocument")]
internal sealed partial class RouteUpdateJsonContext : JsonSerializerContext
{
    private static readonly Lazy<RouteUpdateJsonContext> CompactContext = new(CreateCompact);

    private static RouteUpdateJsonContext CreateCompact()
        => new(new System.Text.Json.JsonSerializerOptions(Default.Options) { WriteIndented = false });

    internal static RouteUpdateJsonContext Compact => CompactContext.Value;
}
