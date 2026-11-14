using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;

[JsonSerializable(typeof(ToolCbmNativePlan))]
[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata, PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
internal sealed partial class ToolNativePlanJsonContext : JsonSerializerContext;
