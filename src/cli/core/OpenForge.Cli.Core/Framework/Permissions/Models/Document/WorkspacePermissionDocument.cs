using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Framework.Permissions.Models.Document;

internal sealed record WorkspacePermissionDocument(
    ImmutableArray<ExtensionPermissionGrant> Extensions,
    ImmutableArray<LibraryPermissionGrant> Libraries)
{
    internal static readonly WorkspacePermissionDocument Empty = new([], []);
}

internal sealed record ExtensionPermissionGrant(string Id, ImmutableArray<string> Paths);

internal sealed record LibraryPermissionGrant(
    string Id,
    string SourceRoot,
    ImmutableArray<string> Paths,
    ImmutableArray<string> Directories);

internal sealed record WorkspacePermissionDecode(
    WorkspacePermissionDocument? Document,
    string? Cause);
