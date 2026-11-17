using System.Text.Json;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Records;

internal enum ToolRecordedValueKind { Json, Bytes, Native }

internal sealed record ToolRecordedValue
{
    public required bool Present { get; init; }
    public required ToolRecordedValueKind Kind { get; init; }
    public JsonElement? Json { get; init; }
    public byte[]? Bytes { get; init; }
    public string? Native { get; init; }
}
