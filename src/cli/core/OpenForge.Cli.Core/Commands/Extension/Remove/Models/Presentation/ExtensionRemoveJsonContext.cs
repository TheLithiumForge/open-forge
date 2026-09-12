using OpenForge.Cli.Core.Shell.Presentation.Models;
using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Presentation;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(ExtensionRemoveJsonDocument))]
[JsonSerializable(typeof(CliCompactJsonDocument<ExtensionRemoveJsonResult>), TypeInfoPropertyName = "CompactDocument")]
internal sealed partial class ExtensionRemoveJsonContext : JsonSerializerContext
{
    private static readonly Lazy<ExtensionRemoveJsonContext> CompactContext = new(CreateCompact);

    private static ExtensionRemoveJsonContext CreateCompact()
        => new(new System.Text.Json.JsonSerializerOptions(Default.Options) { WriteIndented = false });

    internal static ExtensionRemoveJsonContext Compact => CompactContext.Value;
}
