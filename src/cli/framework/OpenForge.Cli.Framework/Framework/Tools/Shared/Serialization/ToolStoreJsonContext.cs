using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Models.Storage;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true, RespectNullableAnnotations = true)]
[JsonSerializable(typeof(ToolStoreRegistryDocument))]
internal sealed partial class ToolStoreJsonContext : JsonSerializerContext;
