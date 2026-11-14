namespace OpenForge.Cli.Core.Framework.Tools.Shared.Supply.Models;

internal sealed record ToolArchiveMember(string Name, string Path, long Bytes, bool Directory, UnixFileMode Mode);
internal sealed record ToolArchivePlan(IReadOnlyList<ToolArchiveMember> Members, long ExpandedBytes);
