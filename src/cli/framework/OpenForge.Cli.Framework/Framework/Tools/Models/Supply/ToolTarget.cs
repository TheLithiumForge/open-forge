using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Supply;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolTarget
{
    public required string State { get; init; }

    public required string[] Artifacts { get; init; }

    public required string? Entry { get; init; }

    public required ToolDependencyLock? Lock { get; init; }

    public required string? Reason { get; init; }
}
