using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.References.Models;

namespace OpenForge.Cli.Core.Presentation.References.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ReferencesData))]
internal sealed partial class ReferencesDataJsonContext : JsonSerializerContext;
