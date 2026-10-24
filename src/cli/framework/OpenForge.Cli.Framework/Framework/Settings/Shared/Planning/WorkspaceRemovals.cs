using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Planning;

/// <summary>
/// Applies the persistent exclusion policy defined in
/// <c>.agents/memory/archived/cli-development/tasks/task50-unified-remove.md</c>.
/// </summary>
internal static class WorkspaceRemovals
{
    private static readonly WorkspaceLayout DefaultLayout = WorkspaceLayout.Create(
        new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory),
        WorkspaceLayout.DefaultMainRoot);

    internal static bool IsPathRemoved(string path, WorkspaceSettingsDocument settings)
        => IsPathRemoved(DefaultLayout, path, settings);

    internal static bool IsPathRemoved(WorkspaceLayout layout, string path, WorkspaceSettingsDocument settings)
    {
        if (!settings.RemovedCategories.IsDefaultOrEmpty
            && settings.RemovedCategories.Any(category =>
                IsSameOrDescendant(path, layout.CategoryDirectory(category))))
        {
            return true;
        }

        if (!settings.RemovedFiles.IsDefaultOrEmpty
            && settings.RemovedFiles.Contains(path, StringComparer.Ordinal))
        {
            return true;
        }

        return !settings.RemovedDirectories.IsDefaultOrEmpty
            && settings.RemovedDirectories.Any(directory => IsSameOrDescendant(path, directory));
    }

    internal static bool IsExtensionRemoved(string extensionId, WorkspaceSettingsDocument settings)
        => !settings.RemovedExtensions.IsDefaultOrEmpty
            && settings.RemovedExtensions.Contains(extensionId, StringComparer.Ordinal);

    internal static bool IsLibraryRemoved(string libraryId, WorkspaceSettingsDocument settings)
        => !settings.RemovedLibraries.IsDefaultOrEmpty
            && settings.RemovedLibraries.Contains(libraryId, StringComparer.Ordinal);

    private static bool IsSameOrDescendant(string path, string directory)
        => string.Equals(path, directory, StringComparison.Ordinal)
            || path.StartsWith(directory + "/", StringComparison.Ordinal);
}
