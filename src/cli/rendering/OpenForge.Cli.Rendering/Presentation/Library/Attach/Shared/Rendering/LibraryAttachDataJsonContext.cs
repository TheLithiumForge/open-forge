using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Library.Attach.Models;

namespace OpenForge.Cli.Core.Presentation.Library.Attach.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(LibraryAttachData))]
internal sealed partial class LibraryAttachDataJsonContext : JsonSerializerContext;
