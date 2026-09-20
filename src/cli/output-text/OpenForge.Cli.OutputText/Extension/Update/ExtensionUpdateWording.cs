using System.Globalization;

namespace OpenForge.Cli.OutputText.Extension.Update;

internal static class ExtensionUpdateWording
{
    // @OpenForgeText extension.update.wording.the-extension-is-up-to-date-nothing-to-do
    internal static string UpToDate(string id)
        => $"The {id} Extension is up to date. Nothing to do.";

    // @OpenForgeText extension.update.wording.all-extensions-are-up-to-date-nothing-to-do
    internal static string AllUpToDate(int count)
        => $"All {count} Extensions are up to date. Nothing to do.";

    // @OpenForgeText extension.update.wording.updated-the-extension-to
    internal static string Updated(string id, string version)
        => $"Updated the {id} Extension to {version}.";

    // @OpenForgeText extension.update.wording.updated-extensions
    internal static string Updated(int count) => $"Updated {count} Extensions.";

    // @OpenForgeText extension.update.wording.would-update-the-extension-to
    internal static string WouldUpdate(string id, string version)
        => $"Would update the {id} Extension to {version}.";

    // @OpenForgeText extension.update.wording.the-extension-was-not-updated-because-its-ownership-could-not-be-established
    internal static string OwnershipUnknown(string id)
        => $"The {id} Extension was not updated because its ownership could not be established.";

    // @OpenForgeText extension.update.wording.extension-update-stopped-after-of-changes
    internal static string Failed(int completed, int total)
        => $"Extension update stopped after {completed} of {total} changes.";

    internal static string Source(string path) => global::OpenForge.Cli.OutputText.Extension.Shared.CanonicalPhrases.Source(path);

    // @OpenForgeText extension.update.wording.previous-content
    internal static string PreviousContent(string value) => $"Previous content: {value}";

    internal static string SourceUnreadable(string path) => global::OpenForge.Cli.OutputText.Extension.Shared.CanonicalPhrases.SourceUnreadable(path);

    internal static string SourceOverlap(string path) => global::OpenForge.Cli.OutputText.Extension.Shared.CanonicalPhrases.FormatIsInsideTheWorkspaceAndCannotBeUsedAsASource(path);

    // @OpenForgeText extension.update.wording.the-source-at-does-not-contain-which-is-what-is-installed
    internal static string SourceIdentityConflict(string path, string id) => $"The source at {path} does not contain {id}, which is what is installed.";
}
