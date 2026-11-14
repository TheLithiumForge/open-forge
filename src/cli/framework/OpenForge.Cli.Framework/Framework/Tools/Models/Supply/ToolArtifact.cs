using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Supply;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolArtifact
{
    public required string File { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; init; }

    public required long? Bytes { get; init; }

    public required string Integrity { get; init; }

    public required string Archive { get; init; }

    public required int StripComponents { get; init; }
}
