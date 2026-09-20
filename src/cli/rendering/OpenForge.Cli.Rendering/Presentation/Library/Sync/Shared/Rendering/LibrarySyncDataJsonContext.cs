using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Library.Sync.Models;

namespace OpenForge.Cli.Core.Presentation.Library.Sync.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(LibrarySyncData))]
internal sealed partial class LibrarySyncDataJsonContext : JsonSerializerContext;
