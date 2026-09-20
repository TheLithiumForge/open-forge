using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Status.Models;

namespace OpenForge.Cli.Core.Presentation.Status.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(StatusData))]
internal sealed partial class StatusDataJsonContext : JsonSerializerContext;
