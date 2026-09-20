using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Selection;

internal sealed class SourceUniverseFilterResolution
{
    internal SourceUniverseFilterResolution(
        IEnumerable<SourceUniverseSelectorResolution> selectors,
        SourceCatalogueSelection selection)
    {
        ArgumentNullException.ThrowIfNull(selectors);
        ArgumentNullException.ThrowIfNull(selection);
        var materializedSelectors = selectors
            .Select(selector => selector ?? throw new ArgumentException(
                "Source-universe selector resolutions cannot contain null members.",
                nameof(selectors)))
            .ToArray();
        for (var index = 0; index < materializedSelectors.Length; index++)
        {
            if (materializedSelectors[index].Occurrence.Position != index + 1)
            {
                throw new ArgumentException(
                    "Source-universe selector resolutions must preserve command-line order.",
                    nameof(selectors));
            }
        }

        Selectors = new ReadOnlyCollection<SourceUniverseSelectorResolution>(materializedSelectors);
        Selection = selection;
    }

    internal IReadOnlyList<SourceUniverseSelectorResolution> Selectors { get; }

    internal SourceCatalogueSelection Selection { get; }

    internal bool IsResolved => Selectors.All(
        selector => selector.Reference.State == SourceReferenceResolutionState.Resolved);
}
