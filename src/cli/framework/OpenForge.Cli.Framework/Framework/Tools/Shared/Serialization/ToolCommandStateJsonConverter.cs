using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;

internal sealed class ToolCommandStateJsonConverter() : JsonStringEnumConverter<ToolCommandState>(allowIntegerValues: false);
