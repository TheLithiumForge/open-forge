using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolCatalogueVersion
{
    public required string Version { get; init; }

    public required string Path { get; init; }

    public required string Sha256 { get; init; }
}
