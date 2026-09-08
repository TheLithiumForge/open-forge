using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
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
    string? Cause);
