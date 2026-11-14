using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing;

namespace OpenForge.Cli.Core.Presentation.Tool.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ToolWriteReportData))]
internal sealed partial class ToolWriteReportJsonContext : JsonSerializerContext;
