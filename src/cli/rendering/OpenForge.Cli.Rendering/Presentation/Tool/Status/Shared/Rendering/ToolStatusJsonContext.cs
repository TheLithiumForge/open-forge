using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Tool.Status.Models;

namespace OpenForge.Cli.Core.Presentation.Tool.Status.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ToolStatusData))]
internal sealed partial class ToolStatusJsonContext : JsonSerializerContext;
