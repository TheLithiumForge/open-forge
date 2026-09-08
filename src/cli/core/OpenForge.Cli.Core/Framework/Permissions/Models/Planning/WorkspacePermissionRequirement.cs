namespace OpenForge.Cli.Core.Framework.Permissions.Models.Planning;

internal abstract record WorkspacePermissionSubject(string Id);

internal sealed record ExtensionPermissionSubject : WorkspacePermissionSubject
{
    internal ExtensionPermissionSubject(string id) : base(id) { }
}

internal sealed record LibraryPermissionSubject : WorkspacePermissionSubject
{
    internal LibraryPermissionSubject(string id, string sourceRoot) : base(id)
    {
        SourceRoot = sourceRoot;
    }

    internal string SourceRoot { get; }
}

internal sealed record WorkspacePermissionRequirement(
    WorkspacePermissionSubject Subject,
    string Path);
