using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Manifest;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolAdapter
{
    public required string Id { get; init; }

    public required int Revision { get; init; }
}
