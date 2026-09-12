using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Doctor.Models.Presentation;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;

internal static class DoctorJsonRenderer
{
    internal static string Render(CliPresentationRequest<DoctorResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        if (presentation.Presentation.View == CliView.Compact)
        {
            return JsonSerializer.Serialize(
                CliCompactJsonProjection.Create(presentation.Result, DoctorCompactJsonProjection.Create(presentation.Result)),
                DoctorJsonContext.Compact.CompactDocument);
        }

        return JsonSerializer.Serialize(
            DoctorJsonProjection.Create(presentation.Result),
            DoctorJsonContext.Default.DoctorJsonDocument);
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(DoctorJsonDocument))]
[JsonSerializable(typeof(CliCompactJsonDocument<DoctorCompactJsonResult>), TypeInfoPropertyName = "CompactDocument")]
internal sealed partial class DoctorJsonContext : JsonSerializerContext
{
    private static readonly Lazy<DoctorJsonContext> CompactContext = new(CreateCompact);

    private static DoctorJsonContext CreateCompact()
        => new(new System.Text.Json.JsonSerializerOptions(Default.Options) { WriteIndented = false });

    internal static DoctorJsonContext Compact => CompactContext.Value;
}
