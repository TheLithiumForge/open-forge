using System.Text.Json;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Records;

internal abstract record ToolAppliedChange
{
    public required string Id { get; init; }
}

internal sealed record ToolJsonUnionAddition : ToolAppliedChange
{
    public required string Path { get; init; }
    public required string Pointer { get; init; }
    public required IReadOnlyList<JsonElement> Items { get; init; }
}

internal sealed record ToolJsonValueAddition : ToolAppliedChange
{
    public required string Path { get; init; }
    public required string Pointer { get; init; }
    public required JsonElement Value { get; init; }
}

internal sealed record ToolMarkedBlockAddition : ToolAppliedChange
{
    public required string Path { get; init; }
    public required string StartMarker { get; init; }
    public required string EndMarker { get; init; }
    public required string ContentSha256 { get; init; }
}

internal sealed record ToolManagedFileAddition : ToolAppliedChange
{
    public required string Path { get; init; }
    public required string Sha256 { get; init; }
}

internal sealed record ToolNativeCommandAddition : ToolAppliedChange
{
    public required string AllowListId { get; init; }
}
