using System.Globalization;

namespace OpenForge.Cli.OutputText.Extension.Remove;

internal static class ExtensionRemoveText
{
    // @OpenForgeText extension.remove.message.extension-remove-was-cancelled-nothing-was-changed
    internal static string MessageExtensionRemoveWasCancelledNothingWasChanged()
        => "Extension remove was cancelled. Nothing was changed.";

    // @OpenForgeText extension.remove.label.was-already-gone-its-ownership-was-released
    internal static string LabelWasAlreadyGoneItsOwnershipWasReleased()
        => "was already gone; its ownership was released";

    // @OpenForgeText extension.remove.label.would-release-ownership
    internal static string LabelWouldReleaseOwnership()
        => "would release ownership of the already-gone file";

    // @OpenForgeText extension.remove.label.workspace-settings-removal-recorded
    internal static string SettingsEffect(bool planned)
        => planned
            ? "would record the removal in workspace settings"
            : "recorded the removal in workspace settings";

    // @OpenForgeText extension.remove.label.workspace-settings-directory
    internal static string DirectoryEffect(bool planned)
        => planned
            ? "would create the workspace settings directory"
            : "created the workspace settings directory";

    // @OpenForgeText extension.remove.help.syntax
    internal static string HelpSyntax()
        => "open-forge extension remove [<stable-id>...] [--automatic] [--dry-run] [--allow-path <path>] [global options]";

    // @OpenForgeText extension.remove.help.selection
    internal static string HelpSelection()
        => "Select exact managed stable IDs, or choose from the interactive package list. A dependency cannot be removed while a retained package needs it.";

    // @OpenForgeText extension.remove.help.ownership
    internal static string HelpOwnership()
        => "Shared paths remain owned by retained packages. When the last owner is removed, eligible existing files are deleted, including edited files. A recovery bundle preserves their bytes before deletion and remains after success.";

    // @OpenForgeText extension.remove.help.results
    internal static string HelpResults()
        => "Dry-run writes nothing. Human and JSON output report each removed, retained, released, and updated path with the shared semantic status and next action.";

    // @OpenForgeText extension.remove.message.remove-the-unused-dependency-when-it-is-no-longer-needed
    internal static string MessageRemoveTheUnusedDependencyWhenItIsNoLongerNeeded()
        => "Remove the unused dependency when it is no longer needed.";

    // @OpenForgeText extension.remove.message.correct-the-extension-remove-input-then-rerun-the-request
    internal static string MessageCorrectTheExtensionRemoveInputThenRerunTheRequest()
        => "Correct the Extension Remove input, then rerun the request.";

    // @OpenForgeText extension.remove.label.packages-removed
    internal static string LabelPackagesRemoved()
        => "packages removed";

    // @OpenForgeText extension.remove.label.files-released
    internal static string LabelFilesReleased()
        => "files released";

    // @OpenForgeText extension.remove.prompt.which-extensions-do-you-want-to-remove
    internal static string PromptWhichExtensionsDoYouWantToRemove()
        => "Which Extensions do you want to remove?";

    // @OpenForgeText extension.remove.label.would-delete
    internal static string LabelWouldDelete()
        => "would delete";

    // @OpenForgeText extension.remove.label.kept-still-owned-by-none
    internal static string LabelKeptStillOwnedByNone()
        => "kept; still owned by none";

    // @OpenForgeText extension.remove.label.would-update
    internal static string LabelWouldUpdate()
        => "would update";

    // @OpenForgeText extension.remove.title.dependency-remains-installed
    internal static string TitleDependencyRemainsInstalled()
        => "Dependency remains installed";

    // @OpenForgeText extension.remove.title.dependency-blocks-removal
    internal static string TitleDependencyBlocksRemoval()
        => "Dependency blocks removal";

    // @OpenForgeText extension.remove.title.settings-are-invalid
    internal static string TitleSettingsAreInvalid()
        => "Workspace settings are invalid";

    // @OpenForgeText extension.remove.title.settings-are-unavailable
    internal static string TitleSettingsAreUnavailable()
        => "Workspace settings are unavailable";

    // @OpenForgeText extension.remove.title.generated-navigation-path-excluded
    internal static string TitleGeneratedNavigationPathExcluded()
        => "Generated navigation was left unchanged";

    // @OpenForgeText extension.remove.message.clear-covering-exclusions-then-run-index
    internal static string MessageClearCoveringExclusionsThenRunIndex()
        => "To regenerate this navigation, remove all exclusions covering this path from removedCategories, removedFiles, and removedDirectories in .agents/open-forge.json, then run open-forge index.";

    // @OpenForgeText extension.remove.message.generated-navigation-path-excluded
    internal static string MessageGeneratedNavigationPathExcluded(string path)
        => ExtensionRemovePhrases.FormatGeneratedNavigationPathExcluded(path);

    // @OpenForgeText extension.remove.message.fix-invalid-settings
    internal static string MessageFixInvalidSettings()
        => "Correct .agents/open-forge.json, then rerun the removal.";

    // @OpenForgeText extension.remove.message.settings-unavailable
    internal static string MessageSettingsUnavailable()
        => "Restore access to .agents/open-forge.json, then rerun the removal.";

    // @OpenForgeText extension.remove.title.removal-verification-failed
    internal static string TitleRemovalVerificationFailed()
        => "Removal verification failed";

    // @OpenForgeText extension.remove.title.extension-remove-failed
    internal static string TitleExtensionRemoveFailed()
        => "Extension remove failed";

    // @OpenForgeText extension.remove.title.extension-remove-was-cancelled
    internal static string TitleExtensionRemoveWasCancelled()
        => "Extension remove was cancelled";

    // @OpenForgeText extension.remove.title.extension-remove
    internal static string TitleExtensionRemove()
        => "Extension remove";

    // @OpenForgeText extension.remove.help.heading.ownership-and-recovery
    internal static string HelpHeadingOwnershipAndRecovery()
        => "Ownership and recovery";
}
