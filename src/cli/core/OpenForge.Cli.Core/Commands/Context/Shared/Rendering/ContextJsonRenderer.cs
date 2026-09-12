using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Context.Models.Presentation;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Rendering;

internal static class ContextJsonRenderer
{
    internal static string Render(CliPresentationRequest<ContextResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        if (presentation.Presentation.View == CliView.Compact)
        {
            return JsonSerializer.Serialize(
                CliCompactJsonProjection.Create(presentation.Result, ContextJsonProjector.CreateCompact(presentation.Result)),
                ContextJsonContext.Compact.CompactDocument);
        }

        return JsonSerializer.Serialize(
            ContextJsonProjector.Create(presentation.Result),
            ContextJsonContext.Default.ContextJsonDocument);
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(ContextJsonDocument))]
[JsonSerializable(typeof(CliCompactJsonDocument<ContextCompactJsonResult>), TypeInfoPropertyName = "CompactDocument")]
internal sealed partial class ContextJsonContext : JsonSerializerContext
{
    private static readonly Lazy<ContextJsonContext> CompactContext = new(CreateCompact);

    private static ContextJsonContext CreateCompact()
        => new(new System.Text.Json.JsonSerializerOptions(Default.Options) { WriteIndented = false });

    internal static ContextJsonContext Compact => CompactContext.Value;
}
