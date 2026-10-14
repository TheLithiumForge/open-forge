namespace OpenForge.Cli.Core.Commands.Repair.Models.Result;

internal sealed record RepairRenameMetadata(string Description, IReadOnlyList<string> Tags, IReadOnlyList<string> ApplyTo);
internal sealed record RepairRenameProof(string? Head, string? OldBlob, string? OldHash, string NewHash, RepairRenameMetadata? Metadata);
internal sealed record RepairRename(string OldPath, string NewPath, string Evidence, int? Similarity, RepairRenameProof Proof, bool SameContent, bool Moved);
internal sealed record RepairNavigation(string Path, int Before, int After, string Outcome);
internal sealed record RepairCompletedMetadata(string Path, IReadOnlyList<string> Fields, string? Description, IReadOnlyList<string>? Tags, string Outcome);
internal sealed record RepairCompletedEntrypoint(string Path, string Outcome);
internal sealed record RepairMaintenanceEffect(string Path, string Kind, string Action, string Outcome);
internal sealed record RepairNavigationRename(string Path, string OldName, string Outcome);
internal sealed record RepairMaintenanceResults(
    IReadOnlyList<RepairRename> Renames,
    IReadOnlyList<RepairNavigation> Navigation,
    IReadOnlyList<RepairCompletedMetadata> Metadata,
    IReadOnlyList<RepairCompletedEntrypoint> Entrypoints)
{
    internal static RepairMaintenanceResults Empty { get; } = new([], [], [], []);
    internal int FilesSkipped { get; init; }
    internal IReadOnlyList<RepairMaintenanceEffect> Effects { get; init; } = [];
    internal IReadOnlyDictionary<string, string> FinalHashes { get; init; } = new Dictionary<string, string>();
    internal string? FailurePath { get; init; }
    internal IReadOnlyList<RepairNavigationRename> NavigationRenames { get; init; } = [];
    internal IReadOnlyDictionary<(string Path, int Line, int Column), IReadOnlyList<RepairRename>> LinkRenames { get; init; }
        = new Dictionary<(string, int, int), IReadOnlyList<RepairRename>>();
}
