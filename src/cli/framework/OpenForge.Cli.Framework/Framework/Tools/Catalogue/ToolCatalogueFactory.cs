using System.Collections.Frozen;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;

namespace OpenForge.Cli.Core.Framework.Tools.Catalogue;

internal static class ToolCatalogueFactory
{
    // Private Node pins come from the supply catalogue at composition time.
    internal static ToolCatalogue Create(IReadOnlyList<ToolPrivateNodeSpec> nodeRuntimes)
    {
        ArgumentNullException.ThrowIfNull(nodeRuntimes);
        ToolCatalogueEntry[] entries = [CodeGraphCatalogueEntry.Create(), CodebaseMemoryCatalogueEntry.Create()];
        return new()
        {
            Entries = entries.ToFrozenDictionary(entry => entry.Identity.Id, StringComparer.Ordinal),
            NodeRuntimes = nodeRuntimes.ToImmutableArray(),
        };
    }
}
