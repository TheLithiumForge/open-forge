using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Presentation;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Extension.Create.Shared.Rendering;

internal static class ExtensionCreateJsonRenderer
{
    internal static string Render(CliPresentationRequest<ExtensionCreateResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var document = ExtensionCreateJsonProjection.Create(presentation.Result);
        if (presentation.Presentation.View == CliView.Compact)
        {
            return JsonSerializer.Serialize(
                CliCompactJsonProjection.Create(presentation.Result, document.Result),
                ExtensionCreateJsonContext.Compact.CompactDocument);
        }

        return JsonSerializer.Serialize(document,
            ExtensionCreateJsonContext.Default.ExtensionCreateJsonDocument);
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(ExtensionCreateJsonDocument))]
[JsonSerializable(typeof(CliCompactJsonDocument<ExtensionCreateJsonResult>), TypeInfoPropertyName = "CompactDocument")]
internal sealed partial class ExtensionCreateJsonContext : JsonSerializerContext
{
    private static readonly Lazy<ExtensionCreateJsonContext> CompactContext = new(CreateCompact);

    private static ExtensionCreateJsonContext CreateCompact()
        => new(new System.Text.Json.JsonSerializerOptions(Default.Options) { WriteIndented = false });

    internal static ExtensionCreateJsonContext Compact => CompactContext.Value;
}
