using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Reading;

internal sealed record SourceReadSession
{
    internal SourceReadSession(
        SourceCatalogue catalogue,
        SourceDocumentReader documentReader,
        SourceCatalogueSelectionScope? defaultSelectionScope)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(documentReader);
        Catalogue = catalogue;
        DocumentReader = documentReader;
        DefaultSelectionScope = defaultSelectionScope;
    }

    internal SourceCatalogue Catalogue { get; }

    internal SourceDocumentReader DocumentReader { get; }

    internal SourceCatalogueSelectionScope? DefaultSelectionScope { get; }
}
