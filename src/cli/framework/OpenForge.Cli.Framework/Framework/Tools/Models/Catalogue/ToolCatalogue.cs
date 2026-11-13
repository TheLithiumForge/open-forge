using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolCatalogue
{
    public required int Schema { get; init; }

    public required ToolCatalogueEntry[] Tools { get; init; }
}
