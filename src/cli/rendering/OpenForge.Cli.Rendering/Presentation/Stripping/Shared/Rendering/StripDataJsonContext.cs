using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Stripping.Models;

namespace OpenForge.Cli.Core.Presentation.Stripping.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(StripData))]
internal sealed partial class StripDataJsonContext : JsonSerializerContext;
