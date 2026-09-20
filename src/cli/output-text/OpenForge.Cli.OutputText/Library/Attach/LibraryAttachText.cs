using System.Globalization;

namespace OpenForge.Cli.OutputText.Library.Attach;

internal static class LibraryAttachText
{
    // @OpenForgeText library.attach.message.library-attach-was-cancelled-nothing-was-changed
    internal static string MessageLibraryAttachWasCancelledNothingWasChanged()
        => "Library attach was cancelled. Nothing was changed.";

    // @OpenForgeText library.attach.label.agents-open-forge-lock-json-records-the-library
    internal static string LabelAgentsOpenForgeLockJsonRecordsTheLibrary()
        => ".agents/open-forge.lock.json records the Library";

    // @OpenForgeText library.attach.title.would-record-the-library-in-agents-open-forge-lock-json
    internal static string TitleWouldRecordTheLibraryInAgentsOpenForgeLockJson()
        => "Would record the Library in .agents/open-forge.lock.json";

    // @OpenForgeText library.attach.help.syntax
    internal static string HelpSyntax()
        => "open-forge library attach <library-id> <source-root> [--to <directory>] [--dry-run] [--automatic] [--allow-path <path>] [global options]";

    // @OpenForgeText library.attach.help.source-and-destination
    internal static string HelpSourceAndDestination()
        => "Register a source directory contained in the workspace and create relative file links. Both paths are workspace-relative. --to defaults to the workspace root.";

    // @OpenForgeText library.attach.help.preview-and-permissions
    internal static string HelpPreviewAndPermissions()
        => "Use --dry-run to inspect the complete plan without writing. Use --automatic for a non-interactive apply. Destinations outside .agents require the applicable workspace permission.";

    // @OpenForgeText library.attach.help.examples
    internal static string HelpExamples()
        => "open-forge library attach shared vendor/shared --dry-run\n  open-forge library attach shared vendor/shared --to docs --dry-run";

    // @OpenForgeText library.attach.message.inspect-the-registered-library-before-choosing-another-id
    internal static string MessageInspectTheRegisteredLibraryBeforeChoosingAnotherId()
        => "Inspect the registered Library before choosing another ID.";

    // @OpenForgeText library.attach.message.inspect-the-reported-library-attach-state
    internal static string MessageInspectTheReportedLibraryAttachState()
        => "Inspect the reported Library attach state.";

    // @OpenForgeText library.attach.message.allow-the-destination-folder-then-rerun-the-same-request
    internal static string MessageAllowTheDestinationFolderThenRerunTheSameRequest()
        => "Allow the destination folder, then rerun the same request.";

    // @OpenForgeText library.attach.message.choose-another-to-folder
    internal static string MessageChooseAnotherToFolder()
        => "Choose another --to folder.";

    // @OpenForgeText library.attach.message.review-and-remove-the-retained-recovery-data-after-confirming-the-result
    internal static string MessageReviewAndRemoveTheRetainedRecoveryDataAfterConfirmingTheResult()
        => "Review and remove the retained recovery data after confirming the result.";

    // @OpenForgeText library.attach.message.the-library-attach-result-did-not-contain-a-finding
    internal static string MessageTheLibraryAttachResultDidNotContainAFinding()
        => "The Library Attach result did not contain a finding.";

    // @OpenForgeText library.attach.label.the-entries-file
    internal static string LabelTheEntriesFile()
        => "the Entries file";

    // @OpenForgeText library.attach.label.the-recovery-store
    internal static string LabelTheRecoveryStore()
        => "the recovery store";

    // @OpenForgeText library.attach.label.the-recovery-bundle
    internal static string LabelTheRecoveryBundle()
        => "the recovery bundle";

    // @OpenForgeText library.attach.title.library-attach
    internal static string TitleLibraryAttach()
        => "Library attach";

    // @OpenForgeText library.attach.label.choose-another-to-folder
    internal static string LabelChooseAnotherToFolder()
        => "choose another --to folder";

    // @OpenForgeText library.attach.message.the-final-state-could-not-be-verified
    internal static string MessageTheFinalStateCouldNotBeVerified()
        => "The final state could not be verified.";

    // @OpenForgeText library.attach.label.links-created
    internal static string LabelLinksCreated()
        => "links created";

    // @OpenForgeText library.attach.label.permission-was-not-evaluated
    internal static string LabelPermissionWasNotEvaluated()
        => "permission was not evaluated";

    // @OpenForgeText library.attach.label.no-outside-permission-was-needed
    internal static string LabelNoOutsidePermissionWasNeeded()
        => "no outside permission was needed";

    // @OpenForgeText library.attach.label.the-destination-paths-were-already-allowed
    internal static string LabelTheDestinationPathsWereAlreadyAllowed()
        => "the destination paths were already allowed";

    // @OpenForgeText library.attach.label.the-destination-paths-were-approved
    internal static string LabelTheDestinationPathsWereApproved()
        => "the destination paths were approved";

    // @OpenForgeText library.attach.label.permission-is-required
    internal static string LabelPermissionIsRequired()
        => "permission is required";

    // @OpenForgeText library.attach.label.the-destination-paths-were-declined
    internal static string LabelTheDestinationPathsWereDeclined()
        => "the destination paths were declined";

    // @OpenForgeText library.attach.label.the-permission-settings-could-not-be-used
    internal static string LabelThePermissionSettingsCouldNotBeUsed()
        => "the permission settings could not be used";

    // @OpenForgeText library.attach.label.permission-could-not-be-checked
    internal static string LabelPermissionCouldNotBeChecked()
        => "permission could not be checked";

    // @OpenForgeText library.attach.label.the-permission-settings-changed-before-writing
    internal static string LabelThePermissionSettingsChangedBeforeWriting()
        => "the permission settings changed before writing";

    // @OpenForgeText library.attach.label.will-be-absent
    internal static string LabelWillBeAbsent()
        => "will be absent";

    // @OpenForgeText library.attach.label.will-be-a-file
    internal static string LabelWillBeAFile()
        => "will be a file";

    // @OpenForgeText library.attach.label.will-be-a-relative-file-link
    internal static string LabelWillBeARelativeFileLink()
        => "will be a relative file link";

    // @OpenForgeText library.attach.message.no-recovery-data-was-requested
    internal static string MessageNoRecoveryDataWasRequested()
        => "No recovery data was requested.";

    // @OpenForgeText library.attach.message.recovery-data-was-prepared
    internal static string MessageRecoveryDataWasPrepared()
        => "Recovery data was prepared.";

    // @OpenForgeText library.attach.message.recovery-data-was-removed
    internal static string MessageRecoveryDataWasRemoved()
        => "Recovery data was removed.";

    // @OpenForgeText library.attach.message.recovery-data-was-retained
    internal static string MessageRecoveryDataWasRetained()
        => "Recovery data was retained.";

    // @OpenForgeText library.attach.message.the-final-state-of-recovery-data-is-unknown
    internal static string MessageTheFinalStateOfRecoveryDataIsUnknown()
        => "The final state of recovery data is unknown.";

    // @OpenForgeText library.attach.label.verification-has-not-started
    internal static string LabelVerificationHasNotStarted()
        => "verification has not started";

    // @OpenForgeText library.attach.label.all-targets-were-verified
    internal static string LabelAllTargetsWereVerified()
        => "all targets were verified";

    // @OpenForgeText library.attach.label.verification-could-not-be-completed
    internal static string LabelVerificationCouldNotBeCompleted()
        => "verification could not be completed";

    // @OpenForgeText library.attach.title.library-id-is-already-registered
    internal static string TitleLibraryIdIsAlreadyRegistered()
        => "Library ID is already registered";

    // @OpenForgeText library.attach.title.destination-folder-is-invalid
    internal static string TitleDestinationFolderIsInvalid()
        => "Destination folder is invalid";

    // @OpenForgeText library.attach.title.destination-collision
    internal static string TitleDestinationCollision()
        => "Destination collision";

    // @OpenForgeText library.attach.title.link-plan-is-unavailable
    internal static string TitleLinkPlanIsUnavailable()
        => "Link plan is unavailable";

    // @OpenForgeText library.attach.title.link-is-blocked
    internal static string TitleLinkIsBlocked()
        => "Link is blocked";

    // @OpenForgeText library.attach.title.destination-is-managed-by-open-forge
    internal static string TitleDestinationIsManagedByOpenForge()
        => "Destination is managed by Open Forge";

    // @OpenForgeText library.attach.title.destination-ownership-conflicts
    internal static string TitleDestinationOwnershipConflicts()
        => "Destination ownership conflicts";

    // @OpenForgeText library.attach.title.recovery-data-was-retained
    internal static string TitleRecoveryDataWasRetained()
        => "Recovery data was retained";

    // @OpenForgeText library.attach.title.link-creation-failed
    internal static string TitleLinkCreationFailed()
        => "Link creation failed";

    // @OpenForgeText library.attach.title.library-attach-failed
    internal static string TitleLibraryAttachFailed()
        => "Library attach failed";

    // @OpenForgeText library.attach.title.library-attach-was-cancelled
    internal static string TitleLibraryAttachWasCancelled()
        => "Library attach was cancelled";

    // @OpenForgeText library.attach.help.heading.source-and-destination
    internal static string HelpHeadingSourceAndDestination()
        => "Source and destination";

    // @OpenForgeText library.attach.help.heading.preview-and-permissions
    internal static string HelpHeadingPreviewAndPermissions()
        => "Preview and permissions";
}
