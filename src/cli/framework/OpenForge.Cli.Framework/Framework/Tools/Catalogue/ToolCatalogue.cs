using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Tools.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Catalogue;

/// <summary>The supported tools in display order. Their commands depend on the platform they run on.</summary>
internal sealed class ToolCatalogue
{
    private ToolCatalogue(ToolPlatform platform, ImmutableArray<ToolEntry> entries)
    {
        Platform = platform;
        Entries = entries;
    }

    internal ToolPlatform Platform { get; }

    internal ImmutableArray<ToolEntry> Entries { get; }

    internal static ToolCatalogue Create(ToolPlatform platform)
        => new(platform, [CodeGraphEntry.Create(), CodebaseMemoryEntry.Create(platform)]);

    internal ToolEntry? Find(string id) => Entries.FirstOrDefault(entry => string.Equals(entry.Id, id, StringComparison.Ordinal));
}
