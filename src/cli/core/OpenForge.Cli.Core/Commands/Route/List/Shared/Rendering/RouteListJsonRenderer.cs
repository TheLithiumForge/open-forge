using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Commands.Route.List.Models.Presentation;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;

internal static class RouteListJsonRenderer
{
    internal static string Render(CliPresentationRequest<RouteListResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        if (presentation.Presentation.View == CliView.Compact)
        {
            return JsonSerializer.Serialize(
                CliCompactJsonProjection.Create(presentation.Result, RouteListJsonProjection.CreateCompact(presentation.Result)),
                RouteListJsonContext.Compact.CompactDocument);
        }

        var document = RouteListJsonProjection.Create(presentation.Result);
        return JsonSerializer.Serialize(
            document,
            RouteListJsonContext.Default.RouteListJsonDocument);
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(RouteListJsonDocument))]
[JsonSerializable(typeof(CliCompactJsonDocument<RouteListCompactJsonResult>), TypeInfoPropertyName = "CompactDocument")]
internal sealed partial class RouteListJsonContext : JsonSerializerContext
{
    private static readonly Lazy<RouteListJsonContext> CompactContext = new(CreateCompact);

    private static RouteListJsonContext CreateCompact()
        => new(new System.Text.Json.JsonSerializerOptions(Default.Options) { WriteIndented = false });

    internal static RouteListJsonContext Compact => CompactContext.Value;
}
