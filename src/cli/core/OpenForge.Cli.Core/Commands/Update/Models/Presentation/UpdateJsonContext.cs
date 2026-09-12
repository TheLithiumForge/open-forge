using OpenForge.Cli.Core.Shell.Presentation.Models;
using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Commands.Update.Models.Presentation;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(UpdateJsonDocument))]
[JsonSerializable(typeof(CliCompactJsonDocument<UpdateJsonResult>), TypeInfoPropertyName = "CompactDocument")]
internal sealed partial class UpdateJsonContext : JsonSerializerContext
{
    private static readonly Lazy<UpdateJsonContext> CompactContext = new(CreateCompact);

    private static UpdateJsonContext CreateCompact()
        => new(new System.Text.Json.JsonSerializerOptions(Default.Options) { WriteIndented = false });

    internal static UpdateJsonContext Compact => CompactContext.Value;
}
