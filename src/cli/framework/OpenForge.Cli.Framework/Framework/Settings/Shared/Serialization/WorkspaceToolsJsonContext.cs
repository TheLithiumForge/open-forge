using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, RespectNullableAnnotations = true)]
[JsonSerializable(typeof(Dictionary<string, WorkspaceToolSetting>), TypeInfoPropertyName = "ToolSettings")]
internal sealed partial class WorkspaceToolsJsonContext : JsonSerializerContext;
