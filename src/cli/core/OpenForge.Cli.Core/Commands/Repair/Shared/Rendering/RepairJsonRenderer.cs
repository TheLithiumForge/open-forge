using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Repair.Models.Presentation;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Rendering;

internal static class RepairJsonRenderer
{
    internal static string Render(CliPresentationRequest<RepairResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var document = RepairJsonProjection.Create(presentation.Result);
        if (presentation.Presentation.View == CliView.Compact)
        {
            return JsonSerializer.Serialize(
                CliCompactJsonProjection.Create(presentation.Result, document.Result),
                RepairJsonContext.Compact.CompactDocument);
        }

        return JsonSerializer.Serialize(document,
            RepairJsonContext.Default.RepairJsonDocument);
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(RepairJsonDocument))]
[JsonSerializable(typeof(CliCompactJsonDocument<RepairJsonResult>), TypeInfoPropertyName = "CompactDocument")]
internal sealed partial class RepairJsonContext : JsonSerializerContext
{
    private static readonly Lazy<RepairJsonContext> CompactContext = new(CreateCompact);

    private static RepairJsonContext CreateCompact()
        => new(new System.Text.Json.JsonSerializerOptions(Default.Options) { WriteIndented = false });

    internal static RepairJsonContext Compact => CompactContext.Value;
}
