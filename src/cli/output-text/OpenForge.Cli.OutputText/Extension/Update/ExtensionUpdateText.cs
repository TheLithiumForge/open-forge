using System.Globalization;

namespace OpenForge.Cli.OutputText.Extension.Update;

internal static class ExtensionUpdateText
{
    // @OpenForgeText extension.update.message.extension-update-was-cancelled-nothing-was-changed
    internal static string MessageExtensionUpdateWasCancelledNothingWasChanged()
        => "Extension update was cancelled. Nothing was changed.";

    // @OpenForgeText extension.update.help.syntax
    internal static string HelpSyntax()
        => "open-forge extension update [<stable-id>...] [--source <package-or-catalogue-path>] [--all] [--force] [--prune] [--automatic] [--dry-run] [--allow-path <path>] [global options]";

    // @OpenForgeText extension.update.help.selection
    internal static string HelpSelection()
        => "Select exact managed stable IDs or --all from one reviewed source. Dependencies, including dependencies of dependencies, are resolved first.";

    // @OpenForgeText extension.update.help.authority
    internal static string HelpAuthority()
        => "Ordinary Update replaces changed or restores missing eligible current content. --prune deletes eligible retired content. The recovery bundle retains previous bytes.";

    // @OpenForgeText extension.update.help.results
    internal static string HelpResults()
        => "Dry-run writes nothing. Human and JSON output report each changed, restored, created, deleted, and kept path with the shared semantic status and next action.";

    // @OpenForgeText extension.update.label.the-selected-package-changes-it
    internal static string LabelTheSelectedPackageChangesIt()
        => "the selected package changes it";

    // @OpenForgeText extension.update.label.you-had-changed-it-and-this-version-changes-it
    internal static string LabelYouHadChangedItAndThisVersionChangesIt()
        => "you had changed it and this version changes it";

    // @OpenForgeText extension.update.label.new-content-in-this-version
    internal static string LabelNewContentInThisVersion()
        => "new content in this version";

    // @OpenForgeText extension.update.label.not-requested
    internal static string LabelNotRequested()
        => "not requested";

    // @OpenForgeText extension.update.label.planned-but-not-checked
    internal static string LabelPlannedButNotChecked()
        => "planned but not checked";

    // @OpenForgeText extension.update.label.not-required
    internal static string LabelNotRequired()
        => "not required";

    // @OpenForgeText extension.update.label.not-created
    internal static string LabelNotCreated()
        => "not created";

    // @OpenForgeText extension.update.label.the-selected-version
    internal static string LabelTheSelectedVersion()
        => "the selected version";

    // @OpenForgeText extension.update.message.the-extension-update-could-not-be-classified
    internal static string MessageTheExtensionUpdateCouldNotBeClassified()
        => "The Extension Update could not be classified.";

    // @OpenForgeText extension.update.message.inspect-the-installed-extension-identity-before-retrying-the-update
    internal static string MessageInspectTheInstalledExtensionIdentityBeforeRetryingTheUpdate()
        => "Inspect the installed Extension identity before retrying the update.";

    // @OpenForgeText extension.update.message.rerun-extension-update-with-automatic-or-use-an-interactive-terminal-to-supply-confirmation
    internal static string MessageRerunExtensionUpdateWithAutomaticOrUseAnInteractiveTerminalToSupplyConfirmation()
        => "Rerun extension update with --automatic or use an interactive terminal to supply confirmation.";

    // @OpenForgeText extension.update.label.preview-deleting-the-kept-file
    internal static string LabelPreviewDeletingTheKeptFile()
        => "preview deleting the kept file";

    // @OpenForgeText extension.update.message.inspect-the-partial-update-and-its-recovery-bundle
    internal static string MessageInspectThePartialUpdateAndItsRecoveryBundle()
        => "Inspect the partial update and its recovery bundle.";

    // @OpenForgeText extension.update.label.packages-updated
    internal static string LabelPackagesUpdated()
        => "packages updated";

    // @OpenForgeText extension.update.prompt.which-extensions-do-you-want-to-update
    internal static string PromptWhichExtensionsDoYouWantToUpdate()
        => "Which Extensions do you want to update?";

    // @OpenForgeText extension.update.label.restore-it-was-missing
    internal static string LabelRestoreItWasMissing()
        => "restore (it was missing)";

    // @OpenForgeText extension.update.label.restored-it-was-missing
    internal static string LabelRestoredItWasMissing()
        => "restored (it was missing)";

    // @OpenForgeText extension.update.label.create-new-in-this-version
    internal static string LabelCreateNewInThisVersion()
        => "create (new in this version)";

    // @OpenForgeText extension.update.label.created-new-in-this-version
    internal static string LabelCreatedNewInThisVersion()
        => "created (new in this version)";

    // @OpenForgeText extension.update.label.delete-no-longer-part-of-the-package
    internal static string LabelDeleteNoLongerPartOfThePackage()
        => "delete (no longer part of the package)";

    // @OpenForgeText extension.update.label.deleted-no-longer-part-of-the-package
    internal static string LabelDeletedNoLongerPartOfThePackage()
        => "deleted (no longer part of the package)";

    // @OpenForgeText extension.update.label.keep-no-longer-part-of-the-package
    internal static string LabelKeepNoLongerPartOfThePackage()
        => "keep; no longer part of the package";

    // @OpenForgeText extension.update.label.kept-no-longer-part-of-the-package
    internal static string LabelKeptNoLongerPartOfThePackage()
        => "kept; no longer part of the package";

    // @OpenForgeText extension.update.title.dependency-closure-none
    internal static string TitleDependencyClosureNone()
        => "Dependency closure: none";

    // @OpenForgeText extension.update.title.source-overlaps-the-workspace
    internal static string TitleSourceOverlapsTheWorkspace()
        => "Source overlaps the workspace";

    // @OpenForgeText extension.update.title.source-identity-conflicts
    internal static string TitleSourceIdentityConflicts()
        => "Source identity conflicts";

    // @OpenForgeText extension.update.title.retired-file-was-kept
    internal static string TitleRetiredFileWasKept()
        => "Retired file was kept";

    // @OpenForgeText extension.update.title.recovery-bundle-blocks-update
    internal static string TitleRecoveryBundleBlocksUpdate()
        => "Recovery bundle blocks update";

    // @OpenForgeText extension.update.title.extension-record-was-not-updated
    internal static string TitleExtensionRecordWasNotUpdated()
        => "Extension record was not updated";

    // @OpenForgeText extension.update.title.update-verification-failed
    internal static string TitleUpdateVerificationFailed()
        => "Update verification failed";

    // @OpenForgeText extension.update.title.extension-update-failed
    internal static string TitleExtensionUpdateFailed()
        => "Extension update failed";

    // @OpenForgeText extension.update.title.extension-update-was-cancelled
    internal static string TitleExtensionUpdateWasCancelled()
        => "Extension update was cancelled";

    // @OpenForgeText extension.update.title.extension-update
    internal static string TitleExtensionUpdate()
        => "Extension update";

    // @OpenForgeText extension.update.label.not-needed
    internal static string LabelNotNeeded()
        => "not needed";

    // @OpenForgeText extension.update.label.not-evaluated
    internal static string LabelNotEvaluated()
        => "not evaluated";

    // @OpenForgeText extension.update.label.no-action
    internal static string LabelNoAction()
        => "no action";

    // @OpenForgeText extension.update.label.no-outcome-recorded
    internal static string LabelNoOutcomeRecorded()
        => "no outcome recorded";

    // @OpenForgeText extension.update.label.retained
    internal static string LabelRetained()
        => "retained";
}
