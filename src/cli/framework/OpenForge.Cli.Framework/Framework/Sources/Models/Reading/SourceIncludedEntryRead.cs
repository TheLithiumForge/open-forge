using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Reading;

internal sealed record SourceIncludedEntryRead(
    IReadOnlyList<SourceLogicalSource> Sources,
    IReadOnlyList<SourceIncludedEntryRow> Rows);

internal sealed record SourceIncludedEntryRow(
    string Path,
    string ParentPath,
    SourceGeneratedEntry Entry,
    PhysicalPathState PhysicalState)
{
    internal string? AliasOf { get; init; }
}
