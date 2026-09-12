using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Index.Models.Presentation;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Index.Shared.Rendering;

internal static class IndexJsonRenderer
{
    internal static string Render(CliPresentationRequest<IndexResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var document = IndexJsonProjection.Create(presentation.Result);
        if (presentation.Presentation.View == CliView.Compact)
        {
            return JsonSerializer.Serialize(
                CliCompactJsonProjection.Create(presentation.Result, document.Result),
                IndexJsonContext.Compact.CompactDocument);
        }

        return JsonSerializer.Serialize(document,
            IndexJsonContext.Default.IndexJsonDocument);
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(IndexJsonDocument))]
[JsonSerializable(typeof(CliCompactJsonDocument<IndexJsonResult>), TypeInfoPropertyName = "CompactDocument")]
internal sealed partial class IndexJsonContext : JsonSerializerContext
{
    private static readonly Lazy<IndexJsonContext> CompactContext = new(CreateCompact);

    private static IndexJsonContext CreateCompact()
        => new(new System.Text.Json.JsonSerializerOptions(Default.Options) { WriteIndented = false });

    internal static IndexJsonContext Compact => CompactContext.Value;
}
