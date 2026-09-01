using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Context.Models.Presentation;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Rendering;

internal static class ContextJsonRenderer
{
    internal static string Render(CliPresentationRequest<ContextResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        return JsonSerializer.Serialize(
            ContextJsonProjector.Create(presentation.Result),
            ContextJsonContext.Default.ContextJsonDocument);
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(ContextJsonDocument))]
internal sealed partial class ContextJsonContext : JsonSerializerContext;
