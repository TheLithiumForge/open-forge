using OpenForge.Cli.Core.Shell.Presentation.Models;
using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Models.Presentation;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(ExtensionUpdateJsonDocument))]
[JsonSerializable(typeof(CliCompactJsonDocument<ExtensionUpdateJsonResult>), TypeInfoPropertyName = "CompactDocument")]
internal sealed partial class ExtensionUpdateJsonContext : JsonSerializerContext
{
    private static readonly Lazy<ExtensionUpdateJsonContext> CompactContext = new(CreateCompact);

    private static ExtensionUpdateJsonContext CreateCompact()
        => new(new System.Text.Json.JsonSerializerOptions(Default.Options) { WriteIndented = false });

    internal static ExtensionUpdateJsonContext Compact => CompactContext.Value;
}
