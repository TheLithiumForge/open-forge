using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UseStringEnumConverter = true, GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(ToolMachineState))]
[JsonSerializable(typeof(ToolWorkspaceMachineState))]
[JsonSerializable(typeof(ToolNodeRuntimeRecord))]
[JsonSerializable(typeof(ToolPathState))]
internal sealed partial class ToolMachineJsonContext : JsonSerializerContext;
