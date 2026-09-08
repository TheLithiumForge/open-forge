namespace OpenForge.Cli.Core.Framework.Permissions;

internal static class WorkspacePermissionDefinitions
{
    internal const int SchemaVersion = 1;
    internal const string ImplicitDirectoryPath = ".agents";
    internal const string ImplicitPathPrefix = ImplicitDirectoryPath + "/";
    internal const string RelativePath = ImplicitPathPrefix + "open-forge.permissions.json";
}
