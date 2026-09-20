using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Library.Inspect.Models;

namespace OpenForge.Cli.Core.Presentation.Library.Inspect.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(LibraryInspectData))]
internal sealed partial class LibraryInspectDataJsonContext : JsonSerializerContext;
