using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Presentation;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Rendering;

internal static class ExtensionInspectJsonRenderer
{
    internal static string Render(CliPresentationRequest<ExtensionInspectResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        if (presentation.Presentation.View == CliView.Compact)
        {
            return JsonSerializer.Serialize(
                CliCompactJsonProjection.Create(presentation.Result, ExtensionInspectJsonDocumentProjector.CreateCompact(presentation.Result)),
                ExtensionInspectJsonContext.Compact.CompactDocument);
        }

        return JsonSerializer.Serialize(
            ExtensionInspectJsonProjection.Create(presentation.Result),
            ExtensionInspectJsonContext.Default.ExtensionInspectJsonDocument);
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(ExtensionInspectJsonDocument))]
[JsonSerializable(typeof(CliCompactJsonDocument<ExtensionInspectCompactJsonResult>), TypeInfoPropertyName = "CompactDocument")]
internal sealed partial class ExtensionInspectJsonContext : JsonSerializerContext
{
    private static readonly Lazy<ExtensionInspectJsonContext> CompactContext = new(CreateCompact);

    private static ExtensionInspectJsonContext CreateCompact()
        => new(new System.Text.Json.JsonSerializerOptions(Default.Options) { WriteIndented = false });

    internal static ExtensionInspectJsonContext Compact => CompactContext.Value;
}
