using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;

namespace OpenForge.Cli.Core.Framework.Ownership.Models.Observation;

internal enum WorkspaceOwnershipReadState
{
    /// <summary>No lock file. Nothing is recorded as owned, and nothing is wrong.</summary>
    Absent,

    /// <summary>The file was read and understood.</summary>
    Complete,

    /// <summary>The file exists but could not be understood. <see cref="WorkspaceOwnershipRead.Cause"/> says why.</summary>
    Invalid,

    /// <summary>The file exists but could not be read at all.</summary>
    Unavailable,
}

/// <summary>
/// One reading of the workspace lock. <see cref="Document"/> is never null: an
/// absent, unintelligible, or unreadable file yields no recorded ownership, so a
/// caller always has a document to act on and reports the state separately.
///
/// Lock trouble is a finding, never a reason to refuse a command. A command that
/// finds the lock missing or stale rebuilds what it can, does its work, and says
/// so.
///
/// <see cref="Snapshot"/> carries the exact bytes and resolved physical path the
/// mutation layer needs to plan a replacement.
/// Re-rendering the decoded document is not a substitute: writing normalizes the
/// document and drops members this release does not know, so it cannot describe
/// the bytes currently on disk.
///
/// It is null only when the bytes could not be read at all. A file that was read
/// but could not be understood still has a snapshot, so it can be safely
/// replaced by a document rebuilt from
/// <see cref="WorkspaceOwnershipDocument.Empty"/>.
/// </summary>
internal sealed record WorkspaceOwnershipRead(
    WorkspaceOwnershipReadState State,
    WorkspaceOwnershipDocument Document,
    string LogicalPath,
    FileStateSnapshot? Snapshot,
    string? Cause)
{
    internal static WorkspaceOwnershipRead Absent(string logicalPath)
        => new(
            WorkspaceOwnershipReadState.Absent,
            WorkspaceOwnershipDocument.Empty,
            logicalPath,
            FileStateSnapshot.Missing(logicalPath),
            Cause: null);

    /// <summary>
    /// Whether the recorded ownership can be acted on as written. False means the
    /// caller should rebuild best-effort and report, not stop.
    /// </summary>
    internal bool IsTrustworthy => State is WorkspaceOwnershipReadState.Complete;
}
