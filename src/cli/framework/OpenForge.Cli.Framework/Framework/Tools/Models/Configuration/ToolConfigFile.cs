using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Configuration;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolConfigFile
{
    public required string Path { get; init; }

    public required string Format { get; init; }

    public required string[] Members { get; init; }
}
