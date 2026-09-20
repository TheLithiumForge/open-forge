using System.Globalization;

namespace OpenForge.Cli.OutputText.Cleanup;

internal static class CleanupPhrases
{
    // @OpenForgeText cleanup.phrase.left-in-place
    internal static string FormatLeftInPlace(string candidateRowReasonText)
        => $"left in place: {candidateRowReasonText}";

    // @OpenForgeText cleanup.phrase.and
    internal static string FormatAnd(string verbText, string bundlesText, string pluralText, string draftsText, string pluralText2)
        => $"{verbText} {bundlesText} {pluralText} and {draftsText} {pluralText2}.";

    // @OpenForgeText cleanup.phrase.cleanup-was-cancelled-after-removing-of-items
    internal static string FormatCleanupWasCancelledAfterRemovingOfItems(string removedText, string totalText)
        => $"Cleanup was cancelled after removing {removedText} of {totalText} items.";

    // @OpenForgeText cleanup.phrase.is-damaged-and-was-left-in-place-nothing-was-removed
    internal static string FormatIsDamagedAndWasLeftInPlaceNothingWasRemoved(string pathText)
        => $"{pathText} is damaged and was left in place. Nothing was removed.";

    // @OpenForgeText cleanup.phrase.is-from-an-unsupported-version-and-was-left-in-place-nothing-was-removed
    internal static string FormatIsFromAnUnsupportedVersionAndWasLeftInPlaceNothingWasRemoved(string pathText)
        => $"{pathText} is from an unsupported version and was left in place. Nothing was removed.";

    // @OpenForgeText cleanup.phrase.is-unreadable-and-was-left-in-place-nothing-was-removed
    internal static string FormatIsUnreadableAndWasLeftInPlaceNothingWasRemoved(string pathText)
        => $"{pathText} is unreadable and was left in place. Nothing was removed.";

    // @OpenForgeText cleanup.phrase.is-not-an-ordinary-file-and-was-left-in-place-nothing-was-removed
    internal static string FormatIsNotAnOrdinaryFileAndWasLeftInPlaceNothingWasRemoved(string pathText)
        => $"{pathText} is not an ordinary file and was left in place. Nothing was removed.";

    // @OpenForgeText cleanup.phrase.could-not-be-removed-safely-nothing-was-removed
    internal static string FormatCouldNotBeRemovedSafelyNothingWasRemoved(string pathText)
        => $"{pathText} could not be removed safely. Nothing was removed.";

    // @OpenForgeText cleanup.removal.failure-reason
    internal static string RemovalFailureReason(string trimSentenceText)
        => $"could not be removed: {trimSentenceText}";

    // @OpenForgeText cleanup.phrase.integrity-check
    internal static string FormatIntegrityCheck(string integrityText)
        => $"integrity check: {integrityText}";

    // @OpenForgeText cleanup.phrase.the-recovery-store-at-could-not-be-read-completely
    internal static string FormatTheRecoveryStoreAtCouldNotBeReadCompletely(string trimSentenceText)
        => $"The recovery store at {trimSentenceText} could not be read completely.";

    // @OpenForgeText cleanup.phrase.the-recovery-store-could-not-be-read-completely
    internal static string FormatTheRecoveryStoreCouldNotBeReadCompletely(string trimSentenceText)
        => $"The recovery store could not be read completely: {trimSentenceText}.";

    // @OpenForgeText cleanup.phrase.is-damaged-and-was-left-in-place
    internal static string FormatIsDamagedAndWasLeftInPlace(string pathText)
        => $"{pathText} is damaged and was left in place.";

    // @OpenForgeText cleanup.phrase.was-written-by-an-unsupported-version-and-was-left-in-place
    internal static string FormatWasWrittenByAnUnsupportedVersionAndWasLeftInPlace(string pathText)
        => $"{pathText} was written by an unsupported version and was left in place.";

    // @OpenForgeText cleanup.phrase.could-not-be-read-and-was-left-in-place
    internal static string FormatCouldNotBeReadAndWasLeftInPlace(string pathText)
        => $"{pathText} could not be read and was left in place.";

    // @OpenForgeText cleanup.phrase.is-not-an-ordinary-file-and-was-left-in-place
    internal static string FormatIsNotAnOrdinaryFileAndWasLeftInPlace(string pathText)
        => $"{pathText} is not an ordinary file and was left in place.";

    // @OpenForgeText cleanup.phrase.changed-while-cleanup-was-running-and-was-left-in-place
    internal static string FormatChangedWhileCleanupWasRunningAndWasLeftInPlace(string pathText)
        => $"{pathText} changed while cleanup was running and was left in place.";

    // @OpenForgeText cleanup.removal.failed-path
    internal static string RemovalFailedPath(string pathText, string trimSentenceText)
        => $"{pathText} could not be removed: {trimSentenceText}";

    // @OpenForgeText cleanup.phrase.still-exists-after-removal
    internal static string FormatStillExistsAfterRemoval(string pathText)
        => $"{pathText} still exists after removal";
}
