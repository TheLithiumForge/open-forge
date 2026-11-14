using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Models.Supply;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Execution;
using OpenForge.Cli.Core.Framework.Tools.Models.Manifest;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true, RespectNullableAnnotations = true, AllowOutOfOrderMetadataProperties = true)]
[JsonSerializable(typeof(ToolManifest))]
[JsonSerializable(typeof(ToolCatalogue))]
internal sealed partial class ToolJsonContext : JsonSerializerContext;
