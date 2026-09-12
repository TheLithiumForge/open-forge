using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.References.Models.Presentation;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.References.Shared.Rendering;

internal static class ReferencesJsonRenderer
{
    internal static string Render(CliPresentationRequest<ReferencesResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        if (presentation.Presentation.View == CliView.Compact)
        {
            return JsonSerializer.Serialize(
                CliCompactJsonProjection.Create(presentation.Result, ReferencesJsonProjection.CreateCompact(presentation.Result)),
                ReferencesJsonContext.Compact.CompactDocument);
        }

        var document = ReferencesJsonProjection.Create(presentation.Result);
        return JsonSerializer.Serialize(document, ReferencesJsonContext.Default.ReferencesJsonDocument);
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(ReferencesJsonDocument))]
[JsonSerializable(typeof(CliCompactJsonDocument<ReferencesCompactJsonResult>), TypeInfoPropertyName = "CompactDocument")]
internal sealed partial class ReferencesJsonContext : JsonSerializerContext
{
    private static readonly Lazy<ReferencesJsonContext> CompactContext = new(CreateCompact);

    private static ReferencesJsonContext CreateCompact()
        => new(new System.Text.Json.JsonSerializerOptions(Default.Options) { WriteIndented = false });

    internal static ReferencesJsonContext Compact => CompactContext.Value;
}
