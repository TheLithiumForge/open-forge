using OpenForge.Cli.Core.Framework.Tools.Models.Manifest;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;

internal enum ToolSourceState { Complete, Invalid, Unavailable, Blocked }

internal sealed record ToolDefinition(ToolManifest Manifest, string Source, string Sha256, byte[] Snapshot, bool IsDefault, bool Reviewed, string? Blocker);
internal sealed record ToolCatalogueRead(ToolSourceState State, string Source, IReadOnlyList<ToolDefinition> Definitions, string? Cause);
internal sealed record ToolSelection(ToolSourceState State, ToolDefinition? Definition, string? Cause);
internal sealed record ToolReviewedPin(string Id, string Version, string Sha256, int AdapterRevision);
internal sealed record ToolAdapterRevision(string Id, int Revision);
