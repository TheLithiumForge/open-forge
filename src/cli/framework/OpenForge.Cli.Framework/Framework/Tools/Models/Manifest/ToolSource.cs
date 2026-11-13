using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Manifest;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolSource
{
    public required string Repository { get; init; }

    public required string Tag { get; init; }

    public required string? Commit { get; init; }
}
