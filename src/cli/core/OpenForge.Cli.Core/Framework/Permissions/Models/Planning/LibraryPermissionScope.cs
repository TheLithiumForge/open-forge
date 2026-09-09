using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Permissions.Models.Observation;

namespace OpenForge.Cli.Core.Framework.Permissions.Models.Planning;

internal enum LibraryPermissionScopeKind
{
    File,
    Directory,
}

internal sealed record LibraryPermissionScope(
    LibraryPermissionSubject Subject,
    LibraryPermissionScopeKind Kind,
    string Path);

internal sealed record LibraryPermissionGrantChange
{
    public required LibraryPermissionSubject Subject { get; init; }
    public required ImmutableArray<LibraryPermissionScope> ApprovedScopes { get; init; }
    public required string? PreviousSourceRoot { get; init; }
}

internal sealed record LibraryPermissionChangeRequest(
    WorkspacePermissionRead Observation,
    LibraryPermissionGrantChange Approval);
