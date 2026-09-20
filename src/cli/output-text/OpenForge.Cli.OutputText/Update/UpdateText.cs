using System.Globalization;

namespace OpenForge.Cli.OutputText.Update;

internal static class UpdateText
{
    // @OpenForgeText update.message.the-framework-is-up-to-date-nothing-to-do
    internal static string MessageTheFrameworkIsUpToDateNothingToDo()
        => "The Framework is up to date. Nothing to do.";

    // @OpenForgeText update.message.update-was-cancelled-nothing-was-changed
    internal static string MessageUpdateWasCancelledNothingWasChanged()
        => "Update was cancelled. Nothing was changed.";

    // @OpenForgeText update.message.no-ownership-record-exists-so-update-cannot-tell-which-files-it-manages-nothing-was-changed
    internal static string MessageNoOwnershipRecordExistsSoUpdateCannotTellWhichFilesItManagesNothingWasChanged()
        => "No ownership record exists, so update cannot tell which files it manages. Nothing was changed.";

    // @OpenForgeText update.message.no-ownership-record-exists-agents-open-forge-lock-json-is-missing-so-update-cannot-tell-which-files-it-manages
    internal static string MessageNoOwnershipRecordExistsAgentsOpenForgeLockJsonIsMissingSoUpdateCannotTellWhichFilesItManages()
        => "No ownership record exists (.agents/open-forge.lock.json is missing), so update cannot tell which files it manages.";

    // @OpenForgeText update.message.the-ownership-record-names-files-that-cannot-be-interpreted-so-nothing-was-changed
    internal static string MessageTheOwnershipRecordNamesFilesThatCannotBeInterpretedSoNothingWasChanged()
        => "The ownership record names files that cannot be interpreted, so nothing was changed.";

    // @OpenForgeText update.label.new-content-in-this-release
    internal static string LabelNewContentInThisRelease()
        => "new content in this release";

    // @OpenForgeText update.label.you-had-changed-it-and-this-release-changes-it
    internal static string LabelYouHadChangedItAndThisReleaseChangesIt()
        => "you had changed it and this release changes it";

    // @OpenForgeText update.label.new-in-this-release
    internal static string LabelNewInThisRelease()
        => "new in this release";

    // @OpenForgeText update.label.no-longer-part-of-this-release
    internal static string LabelNoLongerPartOfThisRelease()
        => "no longer part of this release";

    // @OpenForgeText update.label.you-have-changed-it-a-recovery-bundle-is-written-first
    internal static string LabelYouHaveChangedItARecoveryBundleIsWrittenFirst()
        => "you have changed it; a recovery bundle is written first";

    // @OpenForgeText update.label.kept-no-longer-part-of-this-release
    internal static string LabelKeptNoLongerPartOfThisRelease()
        => "kept; no longer part of this release";

    // @OpenForgeText update.title.previous-content-git-diff
    internal static string TitlePreviousContentGitDiff()
        => "Previous content: git diff";

    // @OpenForgeText update.message.correct-the-named-update-input-then-rerun-the-request
    internal static string MessageCorrectTheNamedUpdateInputThenRerunTheRequest()
        => "Correct the named Update input, then rerun the request.";

    // @OpenForgeText update.message.rerun-the-same-update-request-with-explicit-automatic-mode
    internal static string MessageRerunTheSameUpdateRequestWithExplicitAutomaticMode()
        => "Rerun the same Update request with explicit automatic mode.";

    // @OpenForgeText update.message.preview-deleting-the-retained-files-before-applying-the-prune
    internal static string MessagePreviewDeletingTheRetainedFilesBeforeApplyingThePrune()
        => "Preview deleting the retained files before applying the prune.";

    // @OpenForgeText update.message.review-and-remove-the-reported-recovery-artifact-after-confirming-the-verified-update-result
    internal static string MessageReviewAndRemoveTheReportedRecoveryArtifactAfterConfirmingTheVerifiedUpdateResult()
        => "Review and remove the reported recovery artifact after confirming the verified Update result.";

    // @OpenForgeText update.message.inspect-the-blocked-unavailable-or-incomplete-update-facts-before-rerunning
    internal static string MessageInspectTheBlockedUnavailableOrIncompleteUpdateFactsBeforeRerunning()
        => "Inspect the blocked, unavailable, or incomplete Update facts before rerunning.";

    // @OpenForgeText update.message.rerun-the-same-update-request
    internal static string MessageRerunTheSameUpdateRequest()
        => "Rerun the same Update request.";

    // @OpenForgeText update.label.required-information-was-unavailable
    internal static string LabelRequiredInformationWasUnavailable()
        => "required information was unavailable";

    // @OpenForgeText update.label.the-input-was-invalid
    internal static string LabelTheInputWasInvalid()
        => "the input was invalid";

    // @OpenForgeText update.label.the-workspace-is-blocked
    internal static string LabelTheWorkspaceIsBlocked()
        => "the workspace is blocked";

    // @OpenForgeText update.title.workspace-unavailable
    internal static string TitleWorkspaceUnavailable()
        => "Workspace unavailable";

    // @OpenForgeText update.title.workspace-unsafe
    internal static string TitleWorkspaceUnsafe()
        => "Workspace unsafe";

    // @OpenForgeText update.title.framework-unavailable
    internal static string TitleFrameworkUnavailable()
        => "Framework unavailable";

    // @OpenForgeText update.title.framework-invalid
    internal static string TitleFrameworkInvalid()
        => "Framework invalid";

    // @OpenForgeText update.title.ownership-record-missing
    internal static string TitleOwnershipRecordMissing()
        => "Ownership record missing";

    // @OpenForgeText update.title.ownership-record-unavailable
    internal static string TitleOwnershipRecordUnavailable()
        => "Ownership record unavailable";

    // @OpenForgeText update.title.ownership-record-blocked
    internal static string TitleOwnershipRecordBlocked()
        => "Ownership record blocked";

    // @OpenForgeText update.title.ownership-record-observed
    internal static string TitleOwnershipRecordObserved()
        => "Ownership record observed";

    // @OpenForgeText update.title.target-unavailable
    internal static string TitleTargetUnavailable()
        => "Target unavailable";

    // @OpenForgeText update.title.target-unsafe
    internal static string TitleTargetUnsafe()
        => "Target unsafe";

    // @OpenForgeText update.title.source-provenance-invalid
    internal static string TitleSourceProvenanceInvalid()
        => "Source provenance invalid";

    // @OpenForgeText update.title.fingerprint-unavailable
    internal static string TitleFingerprintUnavailable()
        => "Fingerprint unavailable";

    // @OpenForgeText update.title.retired-content-kept
    internal static string TitleRetiredContentKept()
        => "Retired content kept";

    // @OpenForgeText update.title.retirement-blocked
    internal static string TitleRetirementBlocked()
        => "Retirement blocked";

    // @OpenForgeText update.title.entries-projection-unavailable
    internal static string TitleEntriesProjectionUnavailable()
        => "Entries projection unavailable";

    // @OpenForgeText update.title.entries-section-unsafe
    internal static string TitleEntriesSectionUnsafe()
        => "Entries section unsafe";

    // @OpenForgeText update.title.plan-blocked
    internal static string TitlePlanBlocked()
        => "Plan blocked";

    // @OpenForgeText update.title.recovery-conflict
    internal static string TitleRecoveryConflict()
        => "Recovery conflict";

    // @OpenForgeText update.title.recovery-unavailable
    internal static string TitleRecoveryUnavailable()
        => "Recovery unavailable";

    // @OpenForgeText update.title.recovery-artifact-retained
    internal static string TitleRecoveryArtifactRetained()
        => "Recovery artifact retained";

    // @OpenForgeText update.title.update-failed
    internal static string TitleUpdateFailed()
        => "Update failed";

    // @OpenForgeText update.title.update-cancelled
    internal static string TitleUpdateCancelled()
        => "Update cancelled";

    // @OpenForgeText update.label.replace
    internal static string LabelReplace()
        => "replace";

    // @OpenForgeText update.label.restored
    internal static string LabelRestored()
        => "restored";

    // @OpenForgeText update.title.update
    internal static string TitleUpdate()
        => "Update";

    // @OpenForgeText update.help.syntax
    internal static string HelpSyntax()
        => "open-forge update [--force] [--prune] [--automatic] [--dry-run] [global options]";

    // @OpenForgeText update.help.heading.reconciliation
    internal static string HelpHeadingReconciliation()
        => "Reconciliation";

    // @OpenForgeText update.help.reconciliation
    internal static string HelpReconciliation()
        => "Update compares the managed Framework installation with the complete version bundled in this CLI. Current content that already matches needs no writes.";

    // @OpenForgeText update.help.authority
    internal static string HelpAuthority()
        => "Ordinary Update replaces changed or restores missing eligible current managed content. --prune may delete eligible retired managed content. The recovery bundle retains previous bytes; --automatic skips confirmation.";

    // @OpenForgeText update.help.heading.execution
    internal static string HelpHeadingExecution()
        => "Execution";

    // @OpenForgeText update.help.execution
    internal static string HelpExecution()
        => "--automatic suppresses confirmation without adding force or prune. --dry-run previews the same complete checked plan and writes nothing. An interactive text request asks once after checks if it would write files. JSON and redirected execution never prompt.";

    // @OpenForgeText update.help.examples
    internal static string HelpExamples()
        => "open-forge update\n  open-forge update --automatic --dry-run --format json\n  open-forge update --force\n  open-forge update --force --prune --automatic";

    // @OpenForgeText update.help.related-commands
    internal static string HelpRelatedCommands()
        => "open-forge doctor — inspect blocked or unavailable lifecycle and safety facts.\n  open-forge cleanup — remove a reported retained recovery artifact after review.";

    // @OpenForgeText update.help.notes
    internal static string HelpNotes()
        => "Update uses local ownership observations and the Framework payload embedded in the running CLI. It does not fetch content, adopt unmanaged files, manipulate Git, or remove recovery artifacts owned by Cleanup.";
}
