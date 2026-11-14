using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Records;

internal sealed record ToolSetting(ToolVersionRange Version);

internal sealed record ToolLockRecord
{
    public required string Version { get; init; }
    public required string Channel { get; init; }
    public string? ProfileDigest { get; init; }
    public required IReadOnlyList<ToolAppliedChange> Changes { get; init; }
}

internal sealed record ToolPortableRecords
{
    public required IReadOnlyDictionary<string, ToolSetting> Settings { get; init; }
    public required IReadOnlyDictionary<string, ToolLockRecord> Lock { get; init; }
}
