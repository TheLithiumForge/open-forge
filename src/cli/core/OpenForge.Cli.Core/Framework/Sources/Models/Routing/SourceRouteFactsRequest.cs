using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Routing;

internal sealed class SourceRouteFactsRequest
{
    internal SourceRouteFactsRequest(
        SourceCatalogue catalogue,
        SourceCatalogueSelection selection)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(selection);
        Catalogue = catalogue;
        Selection = selection;
    }

    internal SourceCatalogue Catalogue { get; }

    internal SourceCatalogueSelection Selection { get; }
}
