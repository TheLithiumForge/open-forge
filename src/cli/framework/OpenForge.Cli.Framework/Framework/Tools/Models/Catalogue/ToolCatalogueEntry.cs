using OpenForge.Cli.Core.Framework.Tools.Models.Channels;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;

// Shared facts for channels, config changes, records and lifecycle operations.
// Contract: .agents/memory/crystallized/documents/cli/technical-designs/tool-setup.md
internal sealed record ToolCatalogueEntry
{
    public required ToolIdentity Identity { get; init; }
    public required IReadOnlyList<ToolChannel> Channels { get; init; }
    public required ToolDetectionSpec Detection { get; init; }
    public required IReadOnlyList<ToolVersionProfile> Profiles { get; init; }
}

internal sealed record ToolCatalogue
{
    public required IReadOnlyDictionary<string, ToolCatalogueEntry> Entries { get; init; }
    public required IReadOnlyList<ToolPrivateNodeSpec> NodeRuntimes { get; init; }
}
