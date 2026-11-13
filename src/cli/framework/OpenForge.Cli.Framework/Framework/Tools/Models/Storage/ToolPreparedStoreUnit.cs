using OpenForge.Cli.Core.Framework.Tools.Models.Receipts;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Storage;

internal sealed record ToolPreparedStoreUnit
{
    internal required string Kind { get; init; }
    internal required string Path { get; init; }
    internal required string RecipeSha256 { get; init; }
    internal required string StagingPath { get; init; }
    internal required ToolFileReceipt[] Files { get; init; }
    internal required string[] Directories { get; init; }
    internal required string[] Dependencies { get; init; }
}

internal enum ToolStoreMemberState { Matching, Missing, Changed, Unavailable }
internal sealed record ToolStoreMemberRead(string Path, bool Directory, ToolStoreMemberState State, string? Cause);
internal sealed record ToolStoreUnitRead(bool RegisteredVerified, ToolStoreMemberRead[] Members)
{
    internal bool Matching => Members.Length > 0 && Members.All(member => member.State == ToolStoreMemberState.Matching);
    internal bool Verified => RegisteredVerified && Matching;
}

internal sealed record ToolStorePublication(ToolStoredUnit Unit, string? StagingCleanupCause);
internal enum ToolStorePublicationEventKind { PendingRecorded, FileMoved, Verified }
internal sealed record ToolStorePublicationEvent(ToolStorePublicationEventKind Kind, string Path);
