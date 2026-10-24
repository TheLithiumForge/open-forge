using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Reading;

internal sealed record SourceReadSession
{
    internal SourceReadSession(
        SourceCatalogue catalogue,
        SourceDocumentReader documentReader,
        IReadOnlyList<SourceCatalogueSelectionScope> defaultSelectionScopes)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(defaultSelectionScopes);
        ArgumentNullException.ThrowIfNull(documentReader);
        Catalogue = catalogue;
        DocumentReader = documentReader;
        DefaultSelectionScopes = Array.AsReadOnly(defaultSelectionScopes.ToArray());
    }

    internal SourceCatalogue Catalogue { get; }

    internal WorkspaceLayout Layout => Catalogue.Layout;

    internal SourceDocumentReader DocumentReader { get; }

    internal IReadOnlyList<SourceCatalogueSelectionScope> DefaultSelectionScopes { get; }
}
