using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.Encodings.Web;
using OpenForge.Cli.Core.UnitTests.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Shell.Presentation.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(CliPipelineTestData))]
internal sealed partial class CliPipelineTestJsonContext : JsonSerializerContext
{
    internal static CliPipelineTestJsonContext Relaxed { get; } = new(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    });
}
