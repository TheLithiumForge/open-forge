using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolCatalogueEntry
{
    public required string Id { get; init; }

    public required string DefaultVersion { get; init; }

    public required ToolCatalogueVersion[] Versions { get; init; }
}
