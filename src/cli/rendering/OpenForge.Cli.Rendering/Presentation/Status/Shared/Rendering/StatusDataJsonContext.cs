using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Shared.Models.Layout;
using OpenForge.Cli.Core.Presentation.Status.Models;

namespace OpenForge.Cli.Core.Presentation.Status.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(StatusData))]
[JsonSerializable(typeof(WorkspaceLayoutInfo))]
internal sealed partial class StatusDataJsonContext : JsonSerializerContext;
