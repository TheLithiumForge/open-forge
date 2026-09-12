using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Rendering;

internal static class RouteCreateJsonRenderer
{
    internal static string Render(
        CliPresentationRequest<RouteCreateResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var document = RouteCreateJsonProjection.Create(presentation.Result);
        if (presentation.Presentation.View == CliView.Compact)
        {
            return JsonSerializer.Serialize(
                CliCompactJsonProjection.Create(presentation.Result, document.Result),
                RouteCreateJsonContext.Compact.CompactDocument);
        }

        return JsonSerializer.Serialize(document,
            RouteCreateJsonContext.Default.RouteCreateJsonDocument);
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(RouteCreateJsonDocument))]
[JsonSerializable(typeof(CliCompactJsonDocument<RouteCreateJsonResult>), TypeInfoPropertyName = "CompactDocument")]
internal sealed partial class RouteCreateJsonContext : JsonSerializerContext
{
    private static readonly Lazy<RouteCreateJsonContext> CompactContext = new(CreateCompact);

    private static RouteCreateJsonContext CreateCompact()
        => new(new System.Text.Json.JsonSerializerOptions(Default.Options) { WriteIndented = false });

    internal static RouteCreateJsonContext Compact => CompactContext.Value;
}
