using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Selection;

internal sealed class SourceUniverseFilterRequest
{
    internal SourceUniverseFilterRequest(
        SourceCatalogue catalogue,
        SourceCatalogueSelectionScope? defaultSelectionScope,
        IEnumerable<SourceUniverseSelectorOccurrence> occurrences)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(occurrences);
        var materializedOccurrences = occurrences
            .Select(occurrence => occurrence ?? throw new ArgumentException(
                "Source-universe selector occurrences cannot contain null members.",
                nameof(occurrences)))
            .ToArray();
        for (var index = 0; index < materializedOccurrences.Length; index++)
        {
            if (materializedOccurrences[index].Position != index + 1)
            {
                throw new ArgumentException(
                    "Source-universe selector positions must preserve contiguous command-line order.",
                    nameof(occurrences));
            }
        }

        Catalogue = catalogue;
        DefaultSelectionScope = defaultSelectionScope;
        Occurrences = new ReadOnlyCollection<SourceUniverseSelectorOccurrence>(materializedOccurrences);
    }

    internal SourceCatalogue Catalogue { get; }

    internal SourceCatalogueSelectionScope? DefaultSelectionScope { get; }

    internal IReadOnlyList<SourceUniverseSelectorOccurrence> Occurrences { get; }
}
