using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Library.Detach.Models;

namespace OpenForge.Cli.Core.Presentation.Library.Detach.Shared.Rendering;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(LibraryDetachData))]
internal sealed partial class LibraryDetachDataJsonContext : JsonSerializerContext;
