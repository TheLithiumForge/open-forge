using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Context.Models;

namespace OpenForge.Cli.Core.Presentation.Context.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ContextData))]
internal sealed partial class ContextDataJsonContext : JsonSerializerContext;
