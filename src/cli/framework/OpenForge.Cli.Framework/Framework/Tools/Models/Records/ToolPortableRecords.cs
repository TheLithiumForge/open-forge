using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Records;

internal sealed record ToolSetting(ToolVersionRange Version);
internal sealed record ToolLockRecord(string Version, string ProfileDigest);

internal sealed record ToolPortableRecords
{
    public required IReadOnlyDictionary<string, ToolSetting> Settings { get; init; }
    public required IReadOnlyDictionary<string, ToolLockRecord> Lock { get; init; }
}
