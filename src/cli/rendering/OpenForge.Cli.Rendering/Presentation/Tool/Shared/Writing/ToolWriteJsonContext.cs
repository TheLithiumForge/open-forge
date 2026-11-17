using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;

namespace OpenForge.Cli.Core.Presentation.Tool.Shared.Writing;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(ToolWriteData))]
internal sealed partial class ToolWriteJsonContext : JsonSerializerContext;
