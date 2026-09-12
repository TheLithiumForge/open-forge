using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Presentation;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Extension.List.Shared.Rendering;

internal static class ExtensionListJsonRenderer
{
    internal static string Render(CliPresentationRequest<ExtensionListResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var document = ExtensionListJsonProjection.Create(presentation.Result);
        if (presentation.Presentation.View == CliView.Compact)
        {
            return JsonSerializer.Serialize(
                CliCompactJsonProjection.Create(presentation.Result, document.Result),
                ExtensionListJsonContext.Compact.CompactDocument);
        }

        return JsonSerializer.Serialize(document,
            ExtensionListJsonContext.Default.ExtensionListJsonDocument);
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(ExtensionListJsonDocument))]
[JsonSerializable(typeof(CliCompactJsonDocument<ExtensionListJsonResult>), TypeInfoPropertyName = "CompactDocument")]
internal sealed partial class ExtensionListJsonContext : JsonSerializerContext
{
    private static readonly Lazy<ExtensionListJsonContext> CompactContext = new(CreateCompact);

    private static ExtensionListJsonContext CreateCompact()
        => new(new System.Text.Json.JsonSerializerOptions(Default.Options) { WriteIndented = false });

    internal static ExtensionListJsonContext Compact => CompactContext.Value;
}
