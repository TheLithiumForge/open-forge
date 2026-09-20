using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Find.Models;

namespace OpenForge.Cli.Core.Presentation.Find.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(FindData))]
internal sealed partial class FindDataJsonContext : JsonSerializerContext;
