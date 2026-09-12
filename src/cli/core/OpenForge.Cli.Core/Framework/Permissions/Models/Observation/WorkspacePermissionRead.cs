using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Permissions.Models.Document;

namespace OpenForge.Cli.Core.Framework.Permissions.Models.Observation;

internal enum WorkspacePermissionReadState
{
    Missing,
    Complete,
    Invalid,
    Unavailable,
    Blocked,
}

internal sealed record WorkspacePermissionRead(
    WorkspacePermissionReadState State,
    WorkspacePermissionDocument? Document,
    FileStateSnapshot? Snapshot,
    string? Cause)
{
    internal bool MatchesObservation(WorkspacePermissionRead current)
        => State == current.State
            && Snapshot is { } expected && current.Snapshot is { } actual
            && expected.Expectation == actual.Expectation
            && expected.Bytes.AsSpan().SequenceEqual(actual.Bytes.AsSpan());
}
