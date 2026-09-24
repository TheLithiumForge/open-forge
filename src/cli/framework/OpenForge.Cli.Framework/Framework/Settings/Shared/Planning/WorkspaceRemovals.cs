using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Planning;

/// <summary>
/// Applies the persistent exclusion policy defined in
/// <c>.agents/memory/working/cli-development/tasks/task50-unified-remove.md</c>.
/// </summary>
internal static class WorkspaceRemovals
{
    internal static bool IsPathRemoved(string path, WorkspaceSettingsDocument settings)
    {
        if (!settings.RemovedCategories.IsDefaultOrEmpty
            && settings.RemovedCategories.Any(category =>
                IsSameOrDescendant(path, WorkspaceSettingsDefinitions.ImplicitPathPrefix + category)))
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
