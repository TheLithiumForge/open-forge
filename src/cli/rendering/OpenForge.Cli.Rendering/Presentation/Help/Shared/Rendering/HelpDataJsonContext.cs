using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Help.Models;

namespace OpenForge.Cli.Core.Presentation.Help.Shared.Rendering;

[JsonSerializable(typeof(HelpData))]
internal sealed partial class HelpDataJsonContext : JsonSerializerContext;
