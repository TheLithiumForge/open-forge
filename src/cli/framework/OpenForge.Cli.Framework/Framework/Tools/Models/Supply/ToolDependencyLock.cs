using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Supply;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolDependencyLock
{
    public required string[] Artifacts { get; init; }

    public required bool ClosureComplete { get; init; }
}
