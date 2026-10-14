using System.Globalization;

namespace OpenForge.Cli.OutputText.Extension.Install;

internal static class ExtensionInstallText
{
    // @OpenForgeText extension.install.message.extension-install-was-cancelled-nothing-was-changed
    internal static string MessageExtensionInstallWasCancelledNothingWasChanged()
        => "Extension install was cancelled. Nothing was changed.";

    // @OpenForgeText extension.install.help.syntax
    internal static string HelpSyntax()
        => "open-forge extension install [<stable-id>...] [options]";

    // @OpenForgeText extension.install.help.selection
    internal static string HelpSelection()
        => "Select exact Extension IDs, --all, or the sole package in a one-package source. Dependencies are mandatory and installed first.";

    // @OpenForgeText extension.install.help.layout
    internal static string HelpLayout()
        => "A package directory holds extension.json beside a content/ directory. Package files live under content/, for example content/.agents/guidance/example.md. The manifest accepts exactly id, name, description, version and dependencies. A catalogue is a directory of such packages.";

    // @OpenForgeText extension.install.help.interaction
    internal static string HelpInteraction()
        => "Interactive text requests can offer a list of packages to choose from. --automatic and non-interactive requests do not choose packages or enable --force.";

    // @OpenForgeText extension.install.help.force
    internal static string HelpForce()
        => "--force replaces only eligible existing content during initial installation. Use extension update for changes to managed content.";

    // @OpenForgeText extension.install.help.results
    internal static string HelpResults()
        => "Dry-run writes nothing. Human and JSON output preserve the shared semantic status, stream, and exit mapping.";

    // @OpenForgeText extension.install.message.list-the-available-extensions-then-rerun-the-request-with-an-explicit-selection
    internal static string MessageListTheAvailableExtensionsThenRerunTheRequestWithAnExplicitSelection()
        => "List the available Extensions, then rerun the request with an explicit selection.";

    // @OpenForgeText extension.install.message.use-extension-update-to-reconcile-the-managed-package
    internal static string MessageUseExtensionUpdateToReconcileTheManagedPackage()
        => "Use Extension Update to reconcile the managed package.";

    // @OpenForgeText extension.install.message.use-extension-update-to-bring-the-recorded-package-back-in-step-with-the-source
    internal static string MessageUseExtensionUpdateToBringTheRecordedPackageBackInStepWithTheSource()
        => "Use Extension Update to bring the recorded package back in step with the source.";

    // @OpenForgeText extension.install.message.preview-replacing-the-existing-file-before-rerunning-the-installation
    internal static string MessagePreviewReplacingTheExistingFileBeforeRerunningTheInstallation()
        => "Preview replacing the existing file before rerunning the installation.";

    // @OpenForgeText extension.install.message.repair-workspace-settings-then-rerun-install
    internal static string MessageRepairWorkspaceSettingsThenRerunInstall()
        => "Repair .agents/open-forge.json, then rerun extension install.";

    // @OpenForgeText extension.install.message.remove-extension-from-removed-extensions-then-rerun-install
    internal static string MessageRemoveExtensionFromRemovedExtensionsThenRerunInstall()
        => "Remove the Extension ID from removedExtensions in .agents/open-forge.json, then rerun extension install.";

    // @OpenForgeText extension.install.message.remove-matching-path-from-workspace-removal-settings-then-rerun-install
    internal static string MessageRemoveMatchingPathFromWorkspaceRemovalSettingsThenRerunInstall()
        => "Remove the matching path from removedCategories, removedFiles, or removedDirectories in .agents/open-forge.json, then rerun extension install.";

    // @OpenForgeText extension.install.label.the-required-extension-install-facts-are-unavailable
    internal static string LabelTheRequiredExtensionInstallFactsAreUnavailable()
        => "the required Extension install facts are unavailable";

    // @OpenForgeText extension.install.title.extension-install
    internal static string TitleExtensionInstall()
        => "Extension install";

    // @OpenForgeText extension.install.message.the-package-has-no-content-directory
    internal static string MessageThePackageHasNoContentDirectory()
        => "The package has no content directory.";

    // @OpenForgeText extension.install.heading.the-entries-sections-did-not-match-the-installed-files-after-writing-recovery-data
    internal static string HeadingTheEntriesSectionsDidNotMatchTheInstalledFilesAfterWritingRecoveryData()
        => "The Entries sections did not match the installed files after writing. Recovery data:";

    // @OpenForgeText extension.install.label.packages-installed
    internal static string LabelPackagesInstalled()
        => "packages installed";

    // @OpenForgeText extension.install.label.your-previous-file-is-in-the-recovery-bundle
    internal static string LabelYourPreviousFileIsInTheRecoveryBundle()
        => "your previous file is in the recovery bundle";

    // @OpenForgeText extension.install.label.the-final-state-is-unknown
    internal static string LabelTheFinalStateIsUnknown()
        => "the final state is unknown";

    // @OpenForgeText extension.install.prompt.which-extensions-do-you-want-to-install
    internal static string PromptWhichExtensionsDoYouWantToInstall()
        => "Choose Extensions to install";

    // @OpenForgeText extension.install.title.would-install
    internal static string TitleWouldInstall()
        => "Would install";

    // @OpenForgeText extension.install.title.extension-record-is-invalid
    internal static string TitleExtensionRecordIsInvalid()
        => "Extension record is invalid";

    // @OpenForgeText extension.install.title.ownership-observation-is-incomplete
    internal static string TitleOwnershipObservationIsIncomplete()
        => "Ownership observation is incomplete";

    // @OpenForgeText extension.install.title.managed-content-changed
    internal static string TitleManagedContentChanged()
        => "Managed content changed";

    // @OpenForgeText extension.install.title.recorded-package-differs
    internal static string TitleRecordedPackageDiffers()
        => "Recorded package differs";

    // @OpenForgeText extension.install.title.recovery-bundle-blocks-installation
    internal static string TitleRecoveryBundleBlocksInstallation()
        => "Recovery bundle blocks installation";

    // @OpenForgeText extension.install.title.package-content-is-missing
    internal static string TitlePackageContentIsMissing()
        => "Package content is missing";

    // @OpenForgeText extension.install.title.installation-verification-failed
    internal static string TitleInstallationVerificationFailed()
        => "Installation verification failed";

    // @OpenForgeText extension.install.title.extension-install-failed
    internal static string TitleExtensionInstallFailed()
        => "Extension install failed";

    // @OpenForgeText extension.install.title.extension-install-was-cancelled
    internal static string TitleExtensionInstallWasCancelled()
        => "Extension install was cancelled";

    // @OpenForgeText extension.install.title.workspace-settings-are-invalid
    internal static string TitleWorkspaceSettingsAreInvalid()
        => "Workspace settings are invalid";

    // @OpenForgeText extension.install.title.workspace-settings-are-unavailable
    internal static string TitleWorkspaceSettingsAreUnavailable()
        => "Workspace settings are unavailable";

    // @OpenForgeText extension.install.title.extension-is-excluded-by-workspace-settings
    internal static string TitleExtensionIsExcludedByWorkspaceSettings()
        => "Extension is excluded by workspace settings";

    // @OpenForgeText extension.install.title.extension-path-is-excluded
    internal static string TitleExtensionPathIsExcluded()
        => "Extension path is excluded";

    // @OpenForgeText extension.install.title.required-parent-is-excluded
    internal static string TitleRequiredParentIsExcluded()
        => "Required parent is excluded";

    // @OpenForgeText extension.install.label.selected
    internal static string LabelSelected()
        => "selected";

    // @OpenForgeText extension.install.label.package
    internal static string LabelPackage()
        => "package";

    // @OpenForgeText extension.install.help.heading.external-package-layout
    internal static string HelpHeadingExternalPackageLayout()
        => "External package layout";

    // @OpenForgeText extension.install.help.heading.interaction-and-automatic-mode
    internal static string HelpHeadingInteractionAndAutomaticMode()
        => "Interaction and automatic mode";

    // @OpenForgeText extension.install.help.heading.initial-force
    internal static string HelpHeadingInitialForce()
        => "Initial force";

    // @OpenForgeText extension.install.extensioninstall.help.examples
    internal static string HelpExamples()
        => "open-forge extension install planning --dry-run\nopen-forge extension install planning\nopen-forge extension install planning --automatic";

    // @OpenForgeText extension.install.extensioninstall.help.behavior
    internal static string HelpBehavior()
        => "Use the bundled catalogue unless --source names a local source. Required packages are installed first.\nInteractive text can offer a package picker and asks for confirmation of a writing plan.\n--dry-run shows the checked plan and writes nothing.\nRedirected or JSON requests that would write need --automatic and a complete selection.\n--automatic adds no force. --force applies only during first installation. Interrupted writes report recovery data.";

    // @OpenForgeText extension.install.extensioninstall.help.relatedcommands
    internal static IReadOnlyList<(string Label, string? Description)> HelpRelatedCommands()
        =>
        [
            ("extension list", "List installed and available packages."),
            ("extension inspect --help", "Compare a package with its installation."),
            ("extension update --help", "Update managed Extension files."),
        ];
    // @OpenForgeText extension.install.help.details-1
    internal static string HelpSelectionAndLayoutDetails()
        => "Select exact IDs, --all, or the sole package in a one-package source. Automatic and non-interactive requests never choose packages. A package has extension.json beside content/, with files such as content/.agents/guidance/example.md. The manifest accepts exactly id, name, description, version, and dependencies. A catalogue contains such packages.";

    // @OpenForgeText extension.install.help.details-2
    internal static string HelpManagedChangesDetails()
        => "Use extension update to change already managed content.";
}
