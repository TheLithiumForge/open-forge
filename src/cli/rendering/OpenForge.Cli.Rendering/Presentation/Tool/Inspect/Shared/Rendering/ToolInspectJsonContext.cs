using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Tool.Inspect.Models;

namespace OpenForge.Cli.Core.Presentation.Tool.Inspect.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ToolInspectData))]
internal sealed partial class ToolInspectJsonContext : JsonSerializerContext;
