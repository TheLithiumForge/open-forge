using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Serialization;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(LifecycleDocumentV1))]
internal sealed partial class LifecycleJsonContext : JsonSerializerContext;
