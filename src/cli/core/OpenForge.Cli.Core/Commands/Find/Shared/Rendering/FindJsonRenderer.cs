using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Rendering;

internal static class FindJsonRenderer
{
    internal static string Render(CliPresentationRequest<FindResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        if (presentation.Presentation.View == CliView.Compact)
        {
            return JsonSerializer.Serialize(
                CliCompactJsonProjection.Create(presentation.Result, FindJsonProjection.CreateCompact(presentation.Result)),
                FindJsonContext.Compact.CompactDocument);
        }

        var document = FindJsonProjection.Create(presentation.Result);
        return JsonSerializer.Serialize(
            document,
            FindJsonContext.Default.FindJsonDocument);
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(FindJsonDocument))]
[JsonSerializable(typeof(CliCompactJsonDocument<FindCompactJsonResult>), TypeInfoPropertyName = "CompactDocument")]
internal sealed partial class FindJsonContext : JsonSerializerContext
{
    private static readonly Lazy<FindJsonContext> CompactContext = new(CreateCompact);

    private static FindJsonContext CreateCompact()
        => new(new System.Text.Json.JsonSerializerOptions(Default.Options) { WriteIndented = false });

    internal static FindJsonContext Compact => CompactContext.Value;
}
