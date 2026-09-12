using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Framework.Recovery.Serialization.Models;

namespace OpenForge.Cli.Core.Framework.Recovery.Serialization;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Metadata,
    WriteIndented = false,
    PropertyNameCaseInsensitive = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
[JsonSerializable(typeof(RecoveryBundleManifestV1))]
internal sealed partial class RecoveryBundleJsonContext : JsonSerializerContext;
