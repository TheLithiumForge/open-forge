using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Tool.List.Models;

namespace OpenForge.Cli.Core.Presentation.Tool.List.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ToolListData))]
internal sealed partial class ToolListJsonContext : JsonSerializerContext;
