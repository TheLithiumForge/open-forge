using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Status.Models.Presentation;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using StatusJsonContextModel = OpenForge.Cli.Core.Commands.Status.Models.Presentation.StatusJsonContext;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Rendering;

internal static class StatusJsonRenderer
{
    internal static string Render(CliPresentationRequest<StatusResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        if (presentation.Presentation.View == CliView.Compact)
        {
            return JsonSerializer.Serialize(
                CliCompactJsonProjection.Create(presentation.Result, StatusJsonProjection.CreateCompact(presentation.Result)),
                StatusJsonContext.Compact.CompactDocument);
        }

        return JsonSerializer.Serialize(
            StatusJsonProjection.Create(presentation.Result),
            StatusJsonContext.Default.StatusJsonDocument);
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(StatusJsonDocument))]
[JsonSerializable(typeof(CliCompactJsonDocument<StatusCompactJsonResult>), TypeInfoPropertyName = "CompactDocument")]
[JsonSerializable(typeof(StatusJsonContextModel), TypeInfoPropertyName = "StatusJsonContextModel")]
internal sealed partial class StatusJsonContext : JsonSerializerContext
{
    private static readonly Lazy<StatusJsonContext> CompactContext = new(CreateCompact);

    private static StatusJsonContext CreateCompact()
        => new(new System.Text.Json.JsonSerializerOptions(Default.Options) { WriteIndented = false });

    internal static StatusJsonContext Compact => CompactContext.Value;
}
