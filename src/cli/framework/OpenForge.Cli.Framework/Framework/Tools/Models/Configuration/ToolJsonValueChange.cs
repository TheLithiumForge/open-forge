using System.Text.Json;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Configuration;

internal enum ToolJsonValueOperation { SetIfAbsent, Union }

internal sealed record ToolJsonValueChange : ToolConfigChange
{
    public required string PathTemplate { get; init; }
    public required string Pointer { get; init; }
    public required ToolJsonValueOperation Operation { get; init; }
    public JsonElement? Value { get; init; }
}
