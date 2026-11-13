using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, RespectNullableAnnotations = true)]
[JsonSerializable(typeof(Dictionary<string, WorkspaceToolSetting>), TypeInfoPropertyName = "ToolSettings")]
[JsonSerializable(typeof(Dictionary<string, ToolRunSummary>), TypeInfoPropertyName = "ToolRunSummaries")]
internal sealed partial class WorkspaceToolsJsonContext : JsonSerializerContext;
