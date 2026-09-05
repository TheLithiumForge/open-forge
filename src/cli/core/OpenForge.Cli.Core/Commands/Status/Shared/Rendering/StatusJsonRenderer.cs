using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Status.Models.Presentation;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using StatusJsonContextModel = OpenForge.Cli.Core.Commands.Status.Models.Presentation.StatusJsonContext;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Rendering;

internal static class StatusJsonRenderer
{
    internal static string Render(CliPresentationRequest<StatusResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        return JsonSerializer.Serialize(
            StatusJsonProjection.Create(presentation.Result),
            StatusJsonContext.Default.StatusJsonDocument);
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(StatusJsonDocument))]
[JsonSerializable(typeof(StatusJsonContextModel), TypeInfoPropertyName = "StatusJsonContextModel")]
internal sealed partial class StatusJsonContext : JsonSerializerContext;
