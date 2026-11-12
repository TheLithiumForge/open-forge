using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Execution;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolIndex
{
    public required string Location { get; init; }

    public required string Path { get; init; }

    public required bool PerWorktree { get; init; }

    public required string SharedContentCache { get; init; }

    public required string Freshness { get; init; }

    public required bool RebuildOnVersionChange { get; init; }
}
