using System.Globalization;

namespace OpenForge.Cli.OutputText.Cleanup;

internal static class CleanupText
{
    // @OpenForgeText cleanup.message.no-recovery-data-to-remove
    internal static string MessageNoRecoveryDataToRemove()
        => "No recovery data to remove.";

    // @OpenForgeText cleanup.message.the-recovery-store-could-not-be-read-completely-nothing-was-removed
    internal static string MessageTheRecoveryStoreCouldNotBeReadCompletelyNothingWasRemoved()
        => "The recovery store could not be read completely. Nothing was removed.";

    // @OpenForgeText cleanup.message.cannot-clean-up-another-open-forge-command-holds-the-workspace-lock-nothing-was-removed
    internal static string MessageCannotCleanUpAnotherOpenForgeCommandHoldsTheWorkspaceLockNothingWasRemoved()
        => "Cannot clean up: another Open Forge command holds the workspace lock. Nothing was removed.";

    // @OpenForgeText cleanup.message.cannot-clean-up-the-recovery-store-changed-while-cleanup-was-running-nothing-was-removed
    internal static string MessageCannotCleanUpTheRecoveryStoreChangedWhileCleanupWasRunningNothingWasRemoved()
        => "Cannot clean up: the recovery store changed while cleanup was running. Nothing was removed.";

    // @OpenForgeText cleanup.help.syntax
    internal static string HelpSyntax()
        => "open-forge cleanup [--dry-run] [global options]";

    // @OpenForgeText cleanup.help.catalogue
    internal static string HelpCatalogue()
        => "Remove every recognized completed recovery bundle and draft with an exact expected name for the selected workspace.\n  Unknown, malformed, unsupported, unavailable, and unsafe items remain preserved.";

    // @OpenForgeText cleanup.help.write-policy
    internal static string HelpWritePolicy()
        => "Omit --dry-run to apply the complete deletion plan while holding the workspace lock.\n  --dry-run previews the same plan without locking the workspace or writing files.";

    // @OpenForgeText cleanup.help.notes
    internal static string HelpNotes()
        => "Cleanup accepts no operands, selectors, prompts, confirmations, force mode, age filters, glob filters, or arbitrary recursive deletion.\n  Each run rechecks the exact catalogue before making changes and verifies each deletion.";

    // @OpenForgeText cleanup.label.recovery-bundles-removed
    internal static string LabelRecoveryBundlesRemoved()
        => "recovery bundles removed";

    // @OpenForgeText cleanup.label.unfinished-drafts-removed
    internal static string LabelUnfinishedDraftsRemoved()
        => "unfinished drafts removed";

    // @OpenForgeText cleanup.label.items-left-in-place
    internal static string LabelItemsLeftInPlace()
        => "items left in place";

    // @OpenForgeText cleanup.heading.cannot-clean-up
    internal static string HeadingCannotCleanUp()
        => "Cannot clean up:";

    // @OpenForgeText cleanup.message.cannot-clean-up-the-workspace-could-not-be-verified-nothing-was-removed
    internal static string MessageCannotCleanUpTheWorkspaceCouldNotBeVerifiedNothingWasRemoved()
        => "Cannot clean up: the workspace could not be verified. Nothing was removed.";

    // @OpenForgeText cleanup.message.inspect-the-recovery-store-before-relying-on-this-cleanup-result
    internal static string MessageInspectTheRecoveryStoreBeforeRelyingOnThisCleanupResult()
        => "Inspect the recovery store before relying on this Cleanup result.";

    // @OpenForgeText cleanup.message.remove-it-by-hand-after-review
    internal static string MessageRemoveItByHandAfterReview()
        => "Remove it by hand after review.";

    // @OpenForgeText cleanup.message.review-the-recovery-file-before-removing-it
    internal static string MessageReviewTheRecoveryFileBeforeRemovingIt()
        => "Review the recovery file before removing it.";

    // @OpenForgeText cleanup.message.rerun-cleanup-from-a-fresh-catalogue
    internal static string MessageRerunCleanupFromAFreshCatalogue()
        => "Rerun Cleanup from a fresh catalogue.";

    // @OpenForgeText cleanup.message.correct-the-cleanup-input-then-rerun-the-request
    internal static string MessageCorrectTheCleanupInputThenRerunTheRequest()
        => "Correct the Cleanup input, then rerun the request.";

    // @OpenForgeText cleanup.title.would-remove
    internal static string TitleWouldRemove()
        => "Would remove";

    // @OpenForgeText cleanup.message.cleanup-was-cancelled-nothing-was-removed
    internal static string MessageCleanupWasCancelledNothingWasRemoved()
        => "Cleanup was cancelled. Nothing was removed.";

    // @OpenForgeText cleanup.label.clean-up
    internal static string LabelCleanUp()
        => "clean up";

    // @OpenForgeText cleanup.label.would-be-removed
    internal static string LabelWouldBeRemoved()
        => "would be removed";

    // @OpenForgeText cleanup.label.still-exists-after-removal
    internal static string LabelStillExistsAfterRemoval()
        => "still exists after removal";

    // @OpenForgeText cleanup.label.could-not-be-removed
    internal static string LabelCouldNotBeRemoved()
        => "could not be removed";

    // @OpenForgeText cleanup.label.unfinished-draft
    internal static string LabelUnfinishedDraft()
        => "unfinished draft";

    // @OpenForgeText cleanup.label.not-recognized
    internal static string LabelNotRecognized()
        => "not recognized";

    // @OpenForgeText cleanup.title.recovery-store-is-incomplete
    internal static string TitleRecoveryStoreIsIncomplete()
        => "Recovery store is incomplete";

    // @OpenForgeText cleanup.title.recovery-bundle-version-is-unsupported
    internal static string TitleRecoveryBundleVersionIsUnsupported()
        => "Recovery bundle version is unsupported";

    // @OpenForgeText cleanup.title.recovery-bundle-is-unreadable
    internal static string TitleRecoveryBundleIsUnreadable()
        => "Recovery bundle is unreadable";

    // @OpenForgeText cleanup.title.recovery-draft-is-unsafe
    internal static string TitleRecoveryDraftIsUnsafe()
        => "Recovery draft is unsafe";

    // @OpenForgeText cleanup.title.recovery-store-changed
    internal static string TitleRecoveryStoreChanged()
        => "Recovery store changed";

    // @OpenForgeText cleanup.title.recovery-item-changed
    internal static string TitleRecoveryItemChanged()
        => "Recovery item changed";

    // @OpenForgeText cleanup.title.recovery-item-could-not-be-removed
    internal static string TitleRecoveryItemCouldNotBeRemoved()
        => "Recovery item could not be removed";

    // @OpenForgeText cleanup.title.recovery-item-remains
    internal static string TitleRecoveryItemRemains()
        => "Recovery item remains";

    // @OpenForgeText cleanup.title.cleanup-failed
    internal static string TitleCleanupFailed()
        => "Cleanup failed";

    // @OpenForgeText cleanup.title.cleanup-was-cancelled
    internal static string TitleCleanupWasCancelled()
        => "Cleanup was cancelled";

    // @OpenForgeText cleanup.title.cleanup
    internal static string TitleCleanup()
        => "Cleanup";

    // @OpenForgeText cleanup.message.no-workspace-lock-was-needed
    internal static string MessageNoWorkspaceLockWasNeeded()
        => "No workspace lock was needed.";

    // @OpenForgeText cleanup.message.the-workspace-lock-was-acquired-before-removal
    internal static string MessageTheWorkspaceLockWasAcquiredBeforeRemoval()
        => "The workspace lock was acquired before removal.";

    // @OpenForgeText cleanup.message.the-workspace-lock-could-not-be-acquired-so-nothing-was-removed
    internal static string MessageTheWorkspaceLockCouldNotBeAcquiredSoNothingWasRemoved()
        => "The workspace lock could not be acquired, so nothing was removed.";

    // @OpenForgeText cleanup.message.cleanup-was-cancelled-before-the-workspace-lock-was-acquired
    internal static string MessageCleanupWasCancelledBeforeTheWorkspaceLockWasAcquired()
        => "Cleanup was cancelled before the workspace lock was acquired.";

    // @OpenForgeText cleanup.message.no-final-recovery-store-check-was-needed
    internal static string MessageNoFinalRecoveryStoreCheckWasNeeded()
        => "No final recovery-store check was needed.";

    // @OpenForgeText cleanup.message.the-recovery-store-matched-the-planned-items-under-the-workspace-lock
    internal static string MessageTheRecoveryStoreMatchedThePlannedItemsUnderTheWorkspaceLock()
        => "The recovery store matched the planned items under the workspace lock.";

    // @OpenForgeText cleanup.message.the-recovery-store-did-not-match-the-planned-items-under-the-workspace-lock
    internal static string MessageTheRecoveryStoreDidNotMatchThePlannedItemsUnderTheWorkspaceLock()
        => "The recovery store did not match the planned items under the workspace lock.";

    // @OpenForgeText cleanup.message.the-recovery-store-could-not-be-checked-completely-under-the-workspace-lock
    internal static string MessageTheRecoveryStoreCouldNotBeCheckedCompletelyUnderTheWorkspaceLock()
        => "The recovery store could not be checked completely under the workspace lock.";

    // @OpenForgeText cleanup.message.the-recovery-store-could-not-be-checked-safely-under-the-workspace-lock
    internal static string MessageTheRecoveryStoreCouldNotBeCheckedSafelyUnderTheWorkspaceLock()
        => "The recovery store could not be checked safely under the workspace lock.";

    // @OpenForgeText cleanup.message.cleanup-was-cancelled-while-checking-the-recovery-store-under-the-workspace-lock
    internal static string MessageCleanupWasCancelledWhileCheckingTheRecoveryStoreUnderTheWorkspaceLock()
        => "Cleanup was cancelled while checking the recovery store under the workspace lock.";

    // @OpenForgeText cleanup.label.unfinished-drafts
    internal static string LabelUnfinishedDrafts()
        => "unfinished drafts";
}
