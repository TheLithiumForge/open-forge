using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Library.List.Models;

namespace OpenForge.Cli.Core.Presentation.Library.List.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(LibraryListData))]
internal sealed partial class LibraryListDataJsonContext : JsonSerializerContext;
