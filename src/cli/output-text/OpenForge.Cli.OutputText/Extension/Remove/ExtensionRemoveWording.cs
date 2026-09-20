using System.Globalization;

namespace OpenForge.Cli.OutputText.Extension.Remove;

internal static class ExtensionRemoveWording
{
    // @OpenForgeText extension.remove.wording.would-remove-the-extension
    internal static string WouldRemove(string id) => $"Would remove the {id} Extension.";

    // @OpenForgeText extension.remove.wording.no-files-are-recorded-for-so-there-is-nothing-to-remove
    internal static string NoFilesRecorded(string id)
        => $"No files are recorded for {id}, so there is nothing to remove.";

    // @OpenForgeText extension.remove.wording.removed-the-extension-remains-installed-and-is-no-longer-needed-by-it
    internal static string Warning(string id, string dependency)
        => $"Removed the {id} Extension. {dependency} remains installed and is no longer needed by it.";

    // @OpenForgeText extension.remove.wording.extension-remove-stopped-after-of-changes
    internal static string Failed(int completed, int total)
        => $"Extension remove stopped after {completed} of {total} changes.";

    internal static string EntriesUpdated(string path) => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.EntriesUpdated(path);

    internal static string Recovery(string path) => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.FormatTheDeletedFilesAreKeptInARecoveryBundleAt(path);
}
