using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Tool.Models.Read;

namespace OpenForge.Cli.Core.Commands.Tool.Models.Read;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ToolReadPayload))]
internal sealed partial class ToolReadJsonContext : JsonSerializerContext;
