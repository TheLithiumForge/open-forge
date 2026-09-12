using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Rendering;

internal static class RouteInitJsonRenderer
{
    internal static string Render(
        CliPresentationRequest<RouteInitResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var document = RouteInitJsonProjection.Create(presentation.Result);
        if (presentation.Presentation.View == CliView.Compact)
        {
            return JsonSerializer.Serialize(
                CliCompactJsonProjection.Create(presentation.Result, document.Result),
                RouteInitJsonContext.Compact.CompactDocument);
        }

        return JsonSerializer.Serialize(document,
            RouteInitJsonContext.Default.RouteInitJsonDocument);
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(RouteInitJsonDocument))]
[JsonSerializable(typeof(CliCompactJsonDocument<RouteInitJsonResult>), TypeInfoPropertyName = "CompactDocument")]
internal sealed partial class RouteInitJsonContext : JsonSerializerContext
{
    private static readonly Lazy<RouteInitJsonContext> CompactContext = new(CreateCompact);

    private static RouteInitJsonContext CreateCompact()
        => new(new System.Text.Json.JsonSerializerOptions(Default.Options) { WriteIndented = false });

    internal static RouteInitJsonContext Compact => CompactContext.Value;
}
