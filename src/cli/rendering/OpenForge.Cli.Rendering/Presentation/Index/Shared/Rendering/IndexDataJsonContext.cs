using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Index.Models;

namespace OpenForge.Cli.Core.Presentation.Index.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(IndexData))]
internal sealed partial class IndexDataJsonContext : JsonSerializerContext;
