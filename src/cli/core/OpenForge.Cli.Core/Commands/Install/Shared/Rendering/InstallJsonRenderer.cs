using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Install.Models.Presentation;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Rendering;

internal static class InstallJsonRenderer
{
    internal static string Render(CliPresentationRequest<InstallResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var document = InstallJsonProjection.Create(presentation.Result);
        if (presentation.Presentation.View == CliView.Compact)
        {
            return JsonSerializer.Serialize(
                CliCompactJsonProjection.Create(presentation.Result, document.Result),
                InstallJsonContext.Compact.CompactDocument);
        }

        return JsonSerializer.Serialize(document,
            InstallJsonContext.Default.InstallJsonDocument);
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(InstallJsonDocument))]
[JsonSerializable(typeof(CliCompactJsonDocument<InstallJsonResult>), TypeInfoPropertyName = "CompactDocument")]
internal sealed partial class InstallJsonContext : JsonSerializerContext
{
    private static readonly Lazy<InstallJsonContext> CompactContext = new(CreateCompact);

    private static InstallJsonContext CreateCompact()
        => new(new System.Text.Json.JsonSerializerOptions(Default.Options) { WriteIndented = false });

    internal static InstallJsonContext Compact => CompactContext.Value;
}
