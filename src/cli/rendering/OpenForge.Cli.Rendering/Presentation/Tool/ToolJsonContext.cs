using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Tool.Models;

namespace OpenForge.Cli.Core.Presentation.Tool;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ToolListData))]
[JsonSerializable(typeof(ToolWriteData))]
internal sealed partial class ToolJsonContext : JsonSerializerContext;
