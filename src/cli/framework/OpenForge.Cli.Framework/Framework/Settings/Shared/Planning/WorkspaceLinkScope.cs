using OpenForge.Cli.Core.Framework.Settings.Models.Document;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Planning;

// Shared operation contract, Link Scope: match retained paths without filesystem discovery.
internal static class WorkspaceLinkScope
{
    internal static bool IgnoresSource(WorkspaceSettingsDocument settings, string sourcePath)
        => WorkspaceAllowList.Admits(settings.IgnoreLinksFrom, sourcePath);

    internal static bool IgnoresTarget(WorkspaceSettingsDocument settings, string? targetPath)
        => targetPath is not null && WorkspaceAllowList.Admits(settings.IgnoreLinksTo, targetPath);

    internal static bool Ignores(WorkspaceSettingsDocument settings, string sourcePath, string? targetPath)
        => IgnoresSource(settings, sourcePath) || IgnoresTarget(settings, targetPath);
}
