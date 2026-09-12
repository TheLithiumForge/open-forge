using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;

internal static class RouteInspectJsonRenderer
{
    internal static string Render(CliPresentationRequest<RouteInspectResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var document = RouteInspectJsonProjection.Create(presentation.Result);
        if (presentation.Presentation.View == CliView.Compact)
        {
            return JsonSerializer.Serialize(
                CliCompactJsonProjection.Create(presentation.Result, document.Result),
                RouteInspectJsonContext.Compact.CompactDocument);
        }

        return JsonSerializer.Serialize(
            document,
            RouteInspectJsonContext.Default.RouteInspectJsonDocument);
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(RouteInspectJsonDocument))]
[JsonSerializable(typeof(CliCompactJsonDocument<RouteInspectJsonResult>), TypeInfoPropertyName = "CompactDocument")]
internal sealed partial class RouteInspectJsonContext : JsonSerializerContext
{
    private static readonly Lazy<RouteInspectJsonContext> CompactContext = new(CreateCompact);

    private static RouteInspectJsonContext CreateCompact()
        => new(new System.Text.Json.JsonSerializerOptions(Default.Options) { WriteIndented = false });

    internal static RouteInspectJsonContext Compact => CompactContext.Value;
}
