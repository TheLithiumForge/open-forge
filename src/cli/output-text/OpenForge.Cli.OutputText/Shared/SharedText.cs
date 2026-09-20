using System.Globalization;

namespace OpenForge.Cli.OutputText.Shared;

internal static class SharedText
{
    // @OpenForgeText shared.help.global-options
    internal static string HelpGlobalOptions()
        => "--workspace <path>, --format <text|json>, --detail <minimal|standard|full|debug>, --detail-filter <error|warning|info|all>, --help, and --version apply to this command.";

    // @OpenForgeText shared.help.global-options-with-detail-selection
    internal static string HelpGlobalOptionsWithDetailSelection()
        => HelpGlobalOptions() + " --detail selects detail in text and JSON.";

    // @OpenForgeText shared.label.created
    internal static string LabelCreated()
        => "created";

    // @OpenForgeText shared.label.not-started
    internal static string LabelNotStarted()
        => "not started";

    // @OpenForgeText shared.message.no-files-were-changed
    internal static string MessageNoFilesWereChanged()
        => "No files were changed.";

    // @OpenForgeText shared.label.installed
    internal static string LabelInstalled()
        => "installed";

    // @OpenForgeText shared.label.none
    internal static string LabelNone()
        => "none";

    // @OpenForgeText shared.label.unavailable
    internal static string LabelUnavailable()
        => "unavailable";

    // @OpenForgeText shared.label.after-reviewing-the-bundle
    internal static string LabelAfterReviewingTheBundle()
        => "after reviewing the bundle";

    // @OpenForgeText shared.help.permissions
    internal static string HelpPermissions()
        => "--allow-path <path> records permission for a destination outside .agents. The command verifies the target before writing.";

    // @OpenForgeText shared.title.entries-section-updated
    internal static string TitleEntriesSectionUpdated()
        => "Entries section updated";

    // @OpenForgeText shared.label.final-state-unknown
    internal static string LabelFinalStateUnknown()
        => "final state unknown";

    // @OpenForgeText shared.label.current
    internal static string LabelCurrent()
        => "current";

    // @OpenForgeText shared.message.no-recovery-bundle-was-needed
    internal static string MessageNoRecoveryBundleWasNeeded()
        => "No recovery bundle was needed.";

    // @OpenForgeText shared.message.no-recovery-bundle-was-created
    internal static string MessageNoRecoveryBundleWasCreated()
        => "No recovery bundle was created.";

    // @OpenForgeText shared.message.the-recovery-bundle-was-removed
    internal static string MessageTheRecoveryBundleWasRemoved()
        => "The recovery bundle was removed.";

    // @OpenForgeText shared.message.the-final-state-of-the-recovery-bundle-is-unknown
    internal static string MessageTheFinalStateOfTheRecoveryBundleIsUnknown()
        => "The final state of the recovery bundle is unknown.";

    // @OpenForgeText shared.title.frontmatter-is-invalid
    internal static string TitleFrontmatterIsInvalid()
        => "Frontmatter is invalid";

    // @OpenForgeText shared.label.failed
    internal static string LabelFailed()
        => "failed";

    // @OpenForgeText shared.label.the-input-is-invalid
    internal static string LabelTheInputIsInvalid()
        => "the input is invalid";

    // @OpenForgeText shared.message.review-and-remove-the-reported-recovery-bundle
    internal static string MessageReviewAndRemoveTheReportedRecoveryBundle()
        => "Review and remove the reported recovery bundle.";

    // @OpenForgeText shared.message.no-ownership-record-exists-so-libraries-cannot-be-listed-from-it
    internal static string MessageNoOwnershipRecordExistsSoLibrariesCannotBeListedFromIt()
        => "No ownership record exists, so Libraries cannot be listed from it.";

    // @OpenForgeText shared.label.fix-by-hand
    internal static string LabelFixByHand()
        => "fix by hand";

    // @OpenForgeText shared.message.another-open-forge-command-holds-the-workspace-lock-nothing-was-changed
    internal static string MessageAnotherOpenForgeCommandHoldsTheWorkspaceLockNothingWasChanged()
        => "Another Open Forge command holds the workspace lock. Nothing was changed.";

    // @OpenForgeText shared.label.another-open-forge-command-holds-the-workspace-lock
    internal static string LabelAnotherOpenForgeCommandHoldsTheWorkspaceLock()
        => "another Open Forge command holds the workspace lock";

    // @OpenForgeText shared.label.the-filesystem-operation-failed
    internal static string LabelTheFilesystemOperationFailed()
        => "the filesystem operation failed";

    // @OpenForgeText shared.label.filesystem-access-was-denied
    internal static string LabelFilesystemAccessWasDenied()
        => "filesystem access was denied";

    // @OpenForgeText shared.message.the-changes-were-applied-but-the-final-state-of-the-recovery-bundle-is-unknown
    internal static string MessageTheChangesWereAppliedButTheFinalStateOfTheRecoveryBundleIsUnknown()
        => "The changes were applied, but the final state of the recovery bundle is unknown.";

    // @OpenForgeText shared.message.the-changes-were-applied-but-the-ownership-record-agents-open-forge-lock-json-could-not-be-written
    internal static string MessageTheChangesWereAppliedButTheOwnershipRecordAgentsOpenForgeLockJsonCouldNotBeWritten()
        => "The changes were applied, but the ownership record .agents/open-forge.lock.json could not be written.";

    // @OpenForgeText shared.message.agents-open-forge-lock-json-could-not-be-read-completely
    internal static string MessageAgentsOpenForgeLockJsonCouldNotBeReadCompletely()
        => ".agents/open-forge.lock.json could not be read completely.";

    // @OpenForgeText shared.message.you-declined-the-destinations-so-nothing-was-changed
    internal static string MessageYouDeclinedTheDestinationsSoNothingWasChanged()
        => "You declined the destinations, so nothing was changed.";

    // @OpenForgeText shared.message.agents-open-forge-json-could-not-be-read
    internal static string MessageAgentsOpenForgeJsonCouldNotBeRead()
        => ".agents/open-forge.json could not be read.";

    // @OpenForgeText shared.message.agents-open-forge-json-changed-after-the-plan-was-made-nothing-was-changed
    internal static string MessageAgentsOpenForgeJsonChangedAfterThePlanWasMadeNothingWasChanged()
        => ".agents/open-forge.json changed after the plan was made. Nothing was changed.";

    // @OpenForgeText shared.message.the-grant-could-not-be-saved-to-agents-open-forge-json
    internal static string MessageTheGrantCouldNotBeSavedToAgentsOpenForgeJson()
        => "The grant could not be saved to .agents/open-forge.json.";

    // @OpenForgeText shared.message.the-framework-bundled-in-this-cli-could-not-be-read-completely
    internal static string MessageTheFrameworkBundledInThisCliCouldNotBeReadCompletely()
        => "The Framework bundled in this CLI could not be read completely.";

    // @OpenForgeText shared.message.the-framework-bundled-in-this-cli-is-invalid
    internal static string MessageTheFrameworkBundledInThisCliIsInvalid()
        => "The Framework bundled in this CLI is invalid.";

    // @OpenForgeText shared.message.the-framework-files-this-command-needs-could-not-be-read-completely
    internal static string MessageTheFrameworkFilesThisCommandNeedsCouldNotBeReadCompletely()
        => "The Framework files this command needs could not be read completely.";

    // @OpenForgeText shared.message.the-framework-files-this-command-needs-could-not-be-verified
    internal static string MessageTheFrameworkFilesThisCommandNeedsCouldNotBeVerified()
        => "The Framework files this command needs could not be verified.";

    // @OpenForgeText shared.message.input-ended-before-a-choice-was-made-nothing-was-changed
    internal static string MessageInputEndedBeforeAChoiceWasMadeNothingWasChanged()
        => "Input ended before a choice was made. Nothing was changed.";

    // @OpenForgeText shared.label.up-down-move-enter-choose-esc-cancel
    internal static string LabelUpDownMoveEnterChooseEscCancel()
        => "up/down: move   enter: choose   esc: cancel";

    // @OpenForgeText shared.label.space-toggle-a-all-n-none-up-down-move-enter-continue-esc-cancel
    internal static string LabelSpaceToggleAAllNNoneUpDownMoveEnterContinueEscCancel()
        => "space: toggle   a: all   n: none   up/down: move   enter: continue   esc: cancel";

    // @OpenForgeText shared.heading.choose-numbers-separated-by-spaces-all-or-press-enter-to-cancel
    internal static string HeadingChooseNumbersSeparatedBySpacesAllOrPressEnterToCancel()
        => "Choose numbers separated by spaces, \"all\", or press Enter to cancel:";

    // @OpenForgeText shared.label.x-chosen-required-by-a-chosen-package-not-chosen
    internal static string LabelXChosenRequiredByAChosenPackageNotChosen()
        => "[x] chosen   [+] required by a chosen package   [ ] not chosen";

    // @OpenForgeText shared.message.choose-at-least-one-package-or-press-esc-to-cancel
    internal static string MessageChooseAtLeastOnePackageOrPressEscToCancel()
        => "Choose at least one package, or press esc to cancel.";

    // @OpenForgeText shared.title.allow-always
    internal static string TitleAllowAlways()
        => "Allow always";

    // @OpenForgeText shared.label.save-these-paths-to-agents-open-forge-json
    internal static string LabelSaveThesePathsToAgentsOpenForgeJson()
        => "save these paths to .agents/open-forge.json";

    // @OpenForgeText shared.title.allow-once
    internal static string TitleAllowOnce()
        => "Allow once";

    // @OpenForgeText shared.label.this-run-only
    internal static string LabelThisRunOnly()
        => "this run only";

    // @OpenForgeText shared.title.cancel
    internal static string TitleCancel()
        => "Cancel";

    // @OpenForgeText shared.heading.always-once-cancel
    internal static string HeadingAlwaysOnceCancel()
        => "always, once, cancel:";

    // @OpenForgeText shared.title.framework-files
    internal static string TitleFrameworkFiles()
        => "Framework files";

    // @OpenForgeText shared.title.recovery-data
    internal static string TitleRecoveryData()
        => "Recovery data";

    // @OpenForgeText shared.label.you-had-changed-it
    internal static string LabelYouHadChangedIt()
        => "you had changed it";

    // @OpenForgeText shared.label.it-was-missing
    internal static string LabelItWasMissing()
        => "it was missing";

    // @OpenForgeText shared.label.unchanged-line-endings-differ
    internal static string LabelUnchangedLineEndingsDiffer()
        => "unchanged (line endings differ)";

    // @OpenForgeText shared.label.updated
    internal static string LabelUpdated()
        => "updated";

    // @OpenForgeText shared.title.removed
    internal static string TitleRemoved()
        => "Removed";

    // @OpenForgeText shared.label.removed
    internal static string LabelRemoved()
        => "removed";

    // @OpenForgeText shared.title.invalid-input
    internal static string TitleInvalidInput()
        => "Invalid input";

    // @OpenForgeText shared.title.workspace-is-unavailable
    internal static string TitleWorkspaceIsUnavailable()
        => "Workspace is unavailable";

    // @OpenForgeText shared.title.workspace-is-not-a-directory
    internal static string TitleWorkspaceIsNotADirectory()
        => "Workspace is not a directory";

    // @OpenForgeText shared.title.workspace-is-unsafe
    internal static string TitleWorkspaceIsUnsafe()
        => "Workspace is unsafe";

    // @OpenForgeText shared.title.recovery-bundle-is-damaged
    internal static string TitleRecoveryBundleIsDamaged()
        => "Recovery bundle is damaged";

    // @OpenForgeText shared.title.workspace-lock-is-unavailable
    internal static string TitleWorkspaceLockIsUnavailable()
        => "Workspace lock is unavailable";

    // @OpenForgeText shared.label.recovery-bundles
    internal static string LabelRecoveryBundles()
        => "recovery bundles";

    // @OpenForgeText shared.label.the-operation-failed
    internal static string LabelTheOperationFailed()
        => "the operation failed";

    // @OpenForgeText shared.label.sources
    internal static string LabelSources()
        => "sources";

    // @OpenForgeText shared.title.invalid-source
    internal static string TitleInvalidSource()
        => "Invalid source";

    // @OpenForgeText shared.title.source-is-ambiguous
    internal static string TitleSourceIsAmbiguous()
        => "Source is ambiguous";

    // @OpenForgeText shared.title.source-is-unsafe
    internal static string TitleSourceIsUnsafe()
        => "Source is unsafe";

    // @OpenForgeText shared.title.overwrite-is-ambiguous
    internal static string TitleOverwriteIsAmbiguous()
        => "Overwrite is ambiguous";

    // @OpenForgeText shared.title.startup-context-is-unavailable
    internal static string TitleStartupContextIsUnavailable()
        => "Startup context is unavailable";

    // @OpenForgeText shared.title.source-could-not-be-read
    internal static string TitleSourceCouldNotBeRead()
        => "Source could not be read";

    // @OpenForgeText shared.title.source-encoding-is-invalid
    internal static string TitleSourceEncodingIsInvalid()
        => "Source encoding is invalid";

    // @OpenForgeText shared.title.link-target-is-missing
    internal static string TitleLinkTargetIsMissing()
        => "Link target is missing";

    // @OpenForgeText shared.title.link-encoding-is-invalid
    internal static string TitleLinkEncodingIsInvalid()
        => "Link encoding is invalid";

    // @OpenForgeText shared.title.link-target-could-not-be-read
    internal static string TitleLinkTargetCouldNotBeRead()
        => "Link target could not be read";

    // @OpenForgeText shared.title.section-is-ambiguous
    internal static string TitleSectionIsAmbiguous()
        => "Section is ambiguous";

    // @OpenForgeText shared.title.content-part-is-unavailable
    internal static string TitleContentPartIsUnavailable()
        => "Content part is unavailable";

    // @OpenForgeText shared.title.source-identity-collides
    internal static string TitleSourceIdentityCollides()
        => "Source identity collides";

    // @OpenForgeText shared.title.section-is-missing
    internal static string TitleSectionIsMissing()
        => "Section is missing";

    // @OpenForgeText shared.label.the-requested-id
    internal static string LabelTheRequestedId()
        => "the requested ID";

    // @OpenForgeText shared.label.not-followed
    internal static string LabelNotFollowed()
        => "not followed";

    // @OpenForgeText shared.label.the-source
    internal static string LabelTheSource()
        => "the source";

    // @OpenForgeText shared.label.route
    internal static string LabelRoute()
        => "route";

    // @OpenForgeText shared.label.errors
    internal static string LabelErrors()
        => "errors";

    // @OpenForgeText shared.label.warnings
    internal static string LabelWarnings()
        => "warnings";

    // @OpenForgeText shared.label.infos
    internal static string LabelInfos()
        => "infos";

    // @OpenForgeText shared.title.workspace
    internal static string TitleWorkspace()
        => "Workspace";

    // @OpenForgeText shared.help.heading.links
    internal static string HelpHeadingLinks()
        => "Links";

    // @OpenForgeText shared.title.extensions
    internal static string TitleExtensions()
        => "Extensions";

    // @OpenForgeText shared.title.library-source-folder-is-invalid
    internal static string TitleLibrarySourceFolderIsInvalid()
        => "Library source folder is invalid";

    // @OpenForgeText shared.title.recovery-bundle-is-kept
    internal static string TitleRecoveryBundleIsKept()
        => "Recovery bundle is kept";

    // @OpenForgeText shared.title.entries-section-is-stale
    internal static string TitleEntriesSectionIsStale()
        => "Entries section is stale";

    // @OpenForgeText shared.title.entries-section-is-missing
    internal static string TitleEntriesSectionIsMissing()
        => "Entries section is missing";

    // @OpenForgeText shared.title.overwrite-has-no-base-file
    internal static string TitleOverwriteHasNoBaseFile()
        => "Overwrite has no base file";

    // @OpenForgeText shared.title.broken-link
    internal static string TitleBrokenLink()
        => "Broken link";

    // @OpenForgeText shared.title.libraries
    internal static string TitleLibraries()
        => "Libraries";

    // @OpenForgeText shared.label.installed-extensions
    internal static string LabelInstalledExtensions()
        => "installed Extensions";

    // @OpenForgeText shared.label.filename-match
    internal static string LabelFilenameMatch()
        => "filename match";

    // @OpenForgeText shared.label.title-match
    internal static string LabelTitleMatch()
        => "title match";

    // @OpenForgeText shared.label.content-match
    internal static string LabelContentMatch()
        => "content match";

    // @OpenForgeText shared.label.nearby-route
    internal static string LabelNearbyRoute()
        => "nearby route";

    // @OpenForgeText shared.label.error
    internal static string LabelError()
        => "error";

    // @OpenForgeText shared.label.warning
    internal static string LabelWarning()
        => "warning";

    // @OpenForgeText shared.label.can-be-fixed-automatically
    internal static string LabelCanBeFixedAutomatically()
        => "can be fixed automatically";

    // @OpenForgeText shared.label.needs-a-choice
    internal static string LabelNeedsAChoice()
        => "needs a choice";

    // @OpenForgeText shared.label.the-destination
    internal static string LabelTheDestination()
        => "the destination";

    // @OpenForgeText shared.label.the-workspace
    internal static string LabelTheWorkspace()
        => "the workspace";

    // @OpenForgeText shared.message.fix-it-by-hand
    internal static string MessageFixItByHand()
        => "Fix it by hand.";

    // @OpenForgeText shared.label.info
    internal static string LabelInfo()
        => "info";

    // @OpenForgeText shared.title.frontmatter-could-not-be-read
    internal static string TitleFrontmatterCouldNotBeRead()
        => "Frontmatter could not be read";

    // @OpenForgeText shared.title.routes-are-ambiguous
    internal static string TitleRoutesAreAmbiguous()
        => "Routes are ambiguous";

    // @OpenForgeText shared.title.target-is-unsafe
    internal static string TitleTargetIsUnsafe()
        => "Target is unsafe";

    // @OpenForgeText shared.title.entries-section-is-unsafe
    internal static string TitleEntriesSectionIsUnsafe()
        => "Entries section is unsafe";

    // @OpenForgeText shared.title.workspace-lock-is-held
    internal static string TitleWorkspaceLockIsHeld()
        => "Workspace lock is held";

    // @OpenForgeText shared.title.target-changed
    internal static string TitleTargetChanged()
        => "Target changed";

    // @OpenForgeText shared.title.recovery-data-blocks-the-change
    internal static string TitleRecoveryDataBlocksTheChange()
        => "Recovery data blocks the change";

    // @OpenForgeText shared.title.entries-content-is-unavailable
    internal static string TitleEntriesContentIsUnavailable()
        => "Entries content is unavailable";

    // @OpenForgeText shared.title.recovery-data-is-unavailable
    internal static string TitleRecoveryDataIsUnavailable()
        => "Recovery data is unavailable";

    // @OpenForgeText shared.title.recovery-bundle-was-retained
    internal static string TitleRecoveryBundleWasRetained()
        => "Recovery bundle was retained";

    // @OpenForgeText shared.title.target-changed-during-the-write
    internal static string TitleTargetChangedDuringTheWrite()
        => "Target changed during the write";

    // @OpenForgeText shared.title.write-failed
    internal static string TitleWriteFailed()
        => "Write failed";

    // @OpenForgeText shared.title.verification-failed
    internal static string TitleVerificationFailed()
        => "Verification failed";

    // @OpenForgeText shared.title.recovery-state-is-unknown
    internal static string TitleRecoveryStateIsUnknown()
        => "Recovery state is unknown";

    // @OpenForgeText shared.label.files-created
    internal static string LabelFilesCreated()
        => "files created";

    // @OpenForgeText shared.label.directories-created
    internal static string LabelDirectoriesCreated()
        => "directories created";

    // @OpenForgeText shared.label.files-replaced
    internal static string LabelFilesReplaced()
        => "files replaced";

    // @OpenForgeText shared.label.file
    internal static string LabelFile()
        => "file";

    // @OpenForgeText shared.label.files
    internal static string LabelFiles()
        => "files";

    // @OpenForgeText shared.label.replaced
    internal static string LabelReplaced()
        => "replaced";

    // @OpenForgeText shared.label.directory
    internal static string LabelDirectory()
        => "directory";

    // @OpenForgeText shared.message.no-recovery-bundle-was-required
    internal static string MessageNoRecoveryBundleWasRequired()
        => "No recovery bundle was required.";

    // @OpenForgeText shared.message.verification-was-not-requested
    internal static string MessageVerificationWasNotRequested()
        => "Verification was not requested.";

    // @OpenForgeText shared.message.all-written-targets-were-verified
    internal static string MessageAllWrittenTargetsWereVerified()
        => "All written targets were verified.";

    // @OpenForgeText shared.message.written-targets-did-not-verify
    internal static string MessageWrittenTargetsDidNotVerify()
        => "Written targets did not verify.";

    // @OpenForgeText shared.message.the-final-verification-state-is-unknown
    internal static string MessageTheFinalVerificationStateIsUnknown()
        => "The final verification state is unknown.";

    // @OpenForgeText shared.title.confirmation-is-required
    internal static string TitleConfirmationIsRequired()
        => "Confirmation is required";

    // @OpenForgeText shared.title.target-is-occupied
    internal static string TitleTargetIsOccupied()
        => "Target is occupied";

    // @OpenForgeText shared.title.ownership-conflict
    internal static string TitleOwnershipConflict()
        => "Ownership conflict";

    // @OpenForgeText shared.title.generated-region-is-unsafe
    internal static string TitleGeneratedRegionIsUnsafe()
        => "Generated region is unsafe";

    // @OpenForgeText shared.title.framework-payload-is-unavailable
    internal static string TitleFrameworkPayloadIsUnavailable()
        => "Framework payload is unavailable";

    // @OpenForgeText shared.title.framework-payload-is-invalid
    internal static string TitleFrameworkPayloadIsInvalid()
        => "Framework payload is invalid";

    // @OpenForgeText shared.title.ownership-record-is-unavailable
    internal static string TitleOwnershipRecordIsUnavailable()
        => "Ownership record is unavailable";

    // @OpenForgeText shared.title.ownership-publication-failed
    internal static string TitleOwnershipPublicationFailed()
        => "Ownership publication failed";

    // @OpenForgeText shared.title.the-result-did-not-contain-a-finding
    internal static string TitleTheResultDidNotContainAFinding()
        => "The result did not contain a finding";

    // @OpenForgeText shared.label.missing
    internal static string LabelMissing()
        => "missing";

    // @OpenForgeText shared.label.not-checked
    internal static string LabelNotChecked()
        => "not checked";

    // @OpenForgeText shared.title.source-is-unknown
    internal static string TitleSourceIsUnknown()
        => "Source is unknown";

    // @OpenForgeText shared.message.no-verification-was-needed
    internal static string MessageNoVerificationWasNeeded()
        => "No verification was needed.";

    // @OpenForgeText shared.label.not-run
    internal static string LabelNotRun()
        => "not run";

    // @OpenForgeText shared.message.the-recovery-bundle-was-retained
    internal static string MessageTheRecoveryBundleWasRetained()
        => "The recovery bundle was retained.";

    // @OpenForgeText shared.label.unknown
    internal static string LabelUnknown()
        => "unknown";

    // @OpenForgeText shared.label.changed-since-it-was-installed
    internal static string LabelChangedSinceItWasInstalled()
        => "changed since it was installed";

    // @OpenForgeText shared.label.could-not-be-read
    internal static string LabelCouldNotBeRead()
        => "could not be read";

    // @OpenForgeText shared.label.could-not-be-checked
    internal static string LabelCouldNotBeChecked()
        => "could not be checked";

    // @OpenForgeText shared.title.framework-is-unavailable
    internal static string TitleFrameworkIsUnavailable()
        => "Framework is unavailable";

    // @OpenForgeText shared.title.entries-section-is-unavailable
    internal static string TitleEntriesSectionIsUnavailable()
        => "Entries section is unavailable";

    // @OpenForgeText shared.title.extension-source-is-unavailable
    internal static string TitleExtensionSourceIsUnavailable()
        => "Extension source is unavailable";

    // @OpenForgeText shared.title.recovery-bundle-is-unavailable
    internal static string TitleRecoveryBundleIsUnavailable()
        => "Recovery bundle is unavailable";

    // @OpenForgeText shared.title.library-record-is-invalid
    internal static string TitleLibraryRecordIsInvalid()
        => "Library record is invalid";

    // @OpenForgeText shared.title.library-record-is-unavailable
    internal static string TitleLibraryRecordIsUnavailable()
        => "Library record is unavailable";

    // @OpenForgeText shared.label.changed
    internal static string LabelChanged()
        => "changed";

    // @OpenForgeText shared.label.blocked
    internal static string LabelBlocked()
        => "blocked";

    // @OpenForgeText shared.label.not-applicable
    internal static string LabelNotApplicable()
        => "not applicable";

    // @OpenForgeText shared.label.verified
    internal static string LabelVerified()
        => "verified";

    // @OpenForgeText shared.label.files-restored
    internal static string LabelFilesRestored()
        => "files restored";

    // @OpenForgeText shared.label.files-deleted
    internal static string LabelFilesDeleted()
        => "files deleted";

    // @OpenForgeText shared.label.files-kept
    internal static string LabelFilesKept()
        => "files kept";

    // @OpenForgeText shared.label.files-unchanged
    internal static string LabelFilesUnchanged()
        => "files unchanged";

    // @OpenForgeText shared.label.sections-updated
    internal static string LabelSectionsUpdated()
        => "sections updated";

    // @OpenForgeText shared.title.recovery-failed
    internal static string TitleRecoveryFailed()
        => "Recovery failed";

    // @OpenForgeText shared.label.create
    internal static string LabelCreate()
        => "create";

    // @OpenForgeText shared.label.delete
    internal static string LabelDelete()
        => "delete";

    // @OpenForgeText shared.label.deleted
    internal static string LabelDeleted()
        => "deleted";

    // @OpenForgeText shared.label.update
    internal static string LabelUpdate()
        => "update";

    // @OpenForgeText shared.title.writing-failed
    internal static string TitleWritingFailed()
        => "Writing failed";

    // @OpenForgeText shared.label.files-retired
    internal static string LabelFilesRetired()
        => "files retired";

    // @OpenForgeText shared.label.invalid
    internal static string LabelInvalid()
        => "invalid";

    // @OpenForgeText shared.title.entries-sections-updated
    internal static string TitleEntriesSectionsUpdated()
        => "Entries sections updated";

    // @OpenForgeText shared.label.grants-saved
    internal static string LabelGrantsSaved()
        => "grants saved";

    // @OpenForgeText shared.title.would-update
    internal static string TitleWouldUpdate()
        => "Would update";

    // @OpenForgeText shared.title.ownership-conflicts
    internal static string TitleOwnershipConflicts()
        => "Ownership conflicts";

    // @OpenForgeText shared.title.permission-is-required
    internal static string TitlePermissionIsRequired()
        => "Permission is required";

    // @OpenForgeText shared.title.permission-was-declined
    internal static string TitlePermissionWasDeclined()
        => "Permission was declined";

    // @OpenForgeText shared.title.permissions-are-invalid
    internal static string TitlePermissionsAreInvalid()
        => "Permissions are invalid";

    // @OpenForgeText shared.title.permissions-are-unavailable
    internal static string TitlePermissionsAreUnavailable()
        => "Permissions are unavailable";

    // @OpenForgeText shared.title.permissions-changed
    internal static string TitlePermissionsChanged()
        => "Permissions changed";

    // @OpenForgeText shared.title.permission-could-not-be-saved
    internal static string TitlePermissionCouldNotBeSaved()
        => "Permission could not be saved";

    // @OpenForgeText shared.label.the-removal-could-not-be-checked
    internal static string LabelTheRemovalCouldNotBeChecked()
        => "the removal could not be checked";

    // @OpenForgeText shared.label.the-removal-is-blocked
    internal static string LabelTheRemovalIsBlocked()
        => "the removal is blocked";

    // @OpenForgeText shared.title.recovery-bundle-blocks-removal
    internal static string TitleRecoveryBundleBlocksRemoval()
        => "Recovery bundle blocks removal";

    // @OpenForgeText shared.label.verification-failed
    internal static string LabelVerificationFailed()
        => "verification failed";

    // @OpenForgeText shared.label.the-request-is-invalid
    internal static string LabelTheRequestIsInvalid()
        => "the request is invalid";

    // @OpenForgeText shared.message.the-result-did-not-contain-a-finding
    internal static string MessageTheResultDidNotContainAFinding()
        => "The result did not contain a finding.";

    // @OpenForgeText shared.help.heading.selection
    internal static string HelpHeadingSelection()
        => "Selection";

    // @OpenForgeText shared.label.cannot-be-fixed-automatically
    internal static string LabelCannotBeFixedAutomatically()
        => "cannot be fixed automatically";

    // @OpenForgeText shared.label.the-content-is-not-valid-json
    internal static string LabelTheContentIsNotValidJson()
        => "the content is not valid JSON";

    // @OpenForgeText shared.label.the-file-is-in-use-by-another-process
    internal static string LabelTheFileIsInUseByAnotherProcess()
        => "the file is in use by another process";

    // @OpenForgeText shared.title.apply-these-changes-y-n
    internal static string TitleApplyTheseChangesYN()
        => "Apply these changes? [y/N]";

    // @OpenForgeText shared.message.text-completed-completed-with-warnings-and-incomplete-results-use-stdout-invalid-input-blocked-failed-and-cancelled-results-use-stderr
    internal static string MessageTextCompletedCompletedWithWarningsAndIncompleteResultsUseStdoutInvalidInputBlockedFailedAndCancelledResultsUseStderr()
        => "Text completed, completed-with-warnings, and incomplete results use stdout; invalid-input, blocked, failed, and cancelled results use stderr.";

    // @OpenForgeText shared.message.json-writes-one-minified-schema-version-3-envelope-to-stdout-for-every-semantic-status-debug-diagnostics-use-bounded-stderr
    internal static string MessageJsonWritesOneMinifiedSchemaVersion3EnvelopeToStdoutForEverySemanticStatusDebugDiagnosticsUseBoundedStderr()
        => "JSON writes one minified schema-version-3 envelope to stdout for every semantic status. Debug diagnostics use bounded stderr.";

    // @OpenForgeText shared.label.stdout
    internal static string LabelStdout()
        => "stdout";

    // @OpenForgeText shared.label.stderr
    internal static string LabelStderr()
        => "stderr";

    // @OpenForgeText shared.help.heading.syntax
    internal static string HelpHeadingSyntax()
        => "Syntax";

    // @OpenForgeText shared.help.heading.catalogue
    internal static string HelpHeadingCatalogue()
        => "Catalogue";

    // @OpenForgeText shared.help.heading.write-policy
    internal static string HelpHeadingWritePolicy()
        => "Write policy";

    // @OpenForgeText shared.help.heading.global-options
    internal static string HelpHeadingGlobalOptions()
        => "Global options";

    // @OpenForgeText shared.help.heading.notes
    internal static string HelpHeadingNotes()
        => "Notes";

    // @OpenForgeText shared.help.heading.examples
    internal static string HelpHeadingExamples()
        => "Examples";

    // @OpenForgeText shared.help.heading.related-commands
    internal static string HelpHeadingRelatedCommands()
        => "Related commands";

    // @OpenForgeText shared.help.heading.results-and-streams
    internal static string HelpHeadingResultsAndStreams()
        => "Results and streams";

    // @OpenForgeText shared.help.heading.inspection
    internal static string HelpHeadingInspection()
        => "Inspection";

    // @OpenForgeText shared.help.heading.permissions
    internal static string HelpHeadingPermissions()
        => "Permissions";

    // @OpenForgeText shared.help.heading.authority
    internal static string HelpHeadingAuthority()
        => "Authority";

    // @OpenForgeText shared.help.heading.source-references
    internal static string HelpHeadingSourceReferences()
        => "Source references";

    // @OpenForgeText shared.help.heading.inherited-global-options
    internal static string HelpHeadingInheritedGlobalOptions()
        => "Inherited global options";

    // @OpenForgeText shared.help.heading.command-help
    internal static string HelpHeadingCommandHelp()
        => "Command help";

    // @OpenForgeText shared.help.root-description
    internal static string HelpRootDescription()
        => "Inspect and maintain an Open Forge workspace.";

    // @OpenForgeText shared.help.workspace-description
    internal static string HelpWorkspaceDescription()
        => "Select the workspace explicitly.";

    // @OpenForgeText shared.help.format-description
    internal static string HelpFormatDescription()
        => "Output format: text or json.";

    // @OpenForgeText shared.help.detail-description
    internal static string HelpDetailDescription()
        => "Result detail: minimal, standard, full or debug.";

    // @OpenForgeText shared.help.severity-description
    internal static string HelpSeverityDescription()
        => "List error, warning, info or all severities; repeat to combine.";

    // @OpenForgeText shared.help.help-description
    internal static string HelpHelpDescription()
        => "Show help and exit.";

    // @OpenForgeText shared.help.version-description
    internal static string HelpVersionDescription()
        => "Show the executable version and exit.";

    // @OpenForgeText shared.message.help-and-version-are-mutually-exclusive
    internal static string MessageHelpAndVersionAreMutuallyExclusive()
        => "--help and --version are mutually exclusive.";

    // @OpenForgeText shared.message.terminal-options-cannot-be-combined-with-command-input
    internal static string MessageTerminalOptionsCannotBeCombinedWithCommandInput()
        => "Terminal options cannot be combined with command input.";

    // @OpenForgeText shared.help.heading.getting-started
    internal static string HelpHeadingGettingStarted()
        => "Getting started";

    // @OpenForgeText shared.help.getting-started-examples
    internal static string HelpGettingStartedExamples()
        => "open-forge install --dry-run\n  open-forge context\n  open-forge route list\n  open-forge doctor";

    // @OpenForgeText shared.help.command-help-description
    internal static string HelpCommandHelpDescription()
        => "Use open-forge <command> --help for options and examples.\n  Use open-forge route --help, open-forge extension --help, or\n  open-forge library --help to list their subcommands.";

    // @OpenForgeText shared.help.heading.managed-content
    internal static string HelpHeadingManagedContent()
        => "Managed content";

    // @OpenForgeText shared.help.managed-content-description
    internal static string HelpManagedContentDescription()
        => "Install establishes or verifies Framework management. Update reconciles managed files; --force and --prune permit only their documented changes.";
}
