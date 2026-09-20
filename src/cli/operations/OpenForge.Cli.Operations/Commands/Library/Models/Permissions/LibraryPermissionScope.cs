namespace OpenForge.Cli.Core.Commands.Library.Models.Permissions;

internal enum LibraryPermissionScopeKind
{
    File,
    Directory,
}

internal sealed record LibraryPermissionScope(LibraryPermissionScopeKind Kind, string Path);
