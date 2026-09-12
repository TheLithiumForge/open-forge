using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Rendering;

internal static class RouteMoveJsonRenderer
{
    internal static string Render(CliPresentationRequest<RouteMoveResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var document = RouteMoveJsonProjection.Create(presentation.Result);
        if (presentation.Presentation.View == CliView.Compact)
        {
            return JsonSerializer.Serialize(
                CliCompactJsonProjection.Create(presentation.Result, document.Result),
                RouteMoveJsonContext.Compact.CompactDocument);
        }

        return JsonSerializer.Serialize(document,
            RouteMoveJsonContext.Default.RouteMoveJsonDocument);
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(RouteMoveJsonDocument))]
[JsonSerializable(typeof(CliCompactJsonDocument<RouteMoveJsonResult>), TypeInfoPropertyName = "CompactDocument")]
internal sealed partial class RouteMoveJsonContext : JsonSerializerContext
{
    private static readonly Lazy<RouteMoveJsonContext> CompactContext = new(CreateCompact);

    private static RouteMoveJsonContext CreateCompact()
        => new(new System.Text.Json.JsonSerializerOptions(Default.Options) { WriteIndented = false });

    internal static RouteMoveJsonContext Compact => CompactContext.Value;
}
