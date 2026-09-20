using System.Globalization;

namespace OpenForge.Cli.OutputText.Library.Sync;

internal static class LibrarySyncPhrases
{
    // @OpenForgeText library.sync.phrase.target
    internal static string FormatTarget(string targetText)
        => $"Target: {targetText}";

    // @OpenForgeText library.sync.phrase.the-source-folder-is-not-a-folder-inside-the-workspace
    internal static string FormatTheSourceFolderIsNotAFolderInsideTheWorkspace(string pathText)
        => $"the source folder {pathText} is not a folder inside the workspace";

    // @OpenForgeText library.sync.phrase.the-source-folder-cannot-be-read
    internal static string FormatTheSourceFolderCannotBeRead(string pathText)
        => $"the source folder {pathText} cannot be read";

    // @OpenForgeText library.sync.phrase.the-source-folder-resolves-to-an-unsafe-location
    internal static string FormatTheSourceFolderResolvesToAnUnsafeLocation(string pathText)
        => $"the source folder {pathText} resolves to an unsafe location";

    // @OpenForgeText library.sync.phrase.some-files-under-could-not-be-listed
    internal static string FormatSomeFilesUnderCouldNotBeListed(string pathText)
        => $"some source files under {pathText} could not be listed";

    // @OpenForgeText library.sync.plan.failure-reason
    internal static string PlanFailureReason(string trimSentenceText)
        => $"the link plan could not be completed: {trimSentenceText}";

    // @OpenForgeText library.sync.phrase.added
    internal static string FormatAdded(string addedText, string pluralText)
        => $"{addedText} {pluralText} added";

    // @OpenForgeText library.sync.phrase.removed
    internal static string FormatRemoved(string removedText)
        => $"{removedText} removed";

    // @OpenForgeText library.sync.phrase.unchanged
    internal static string FormatUnchanged(string unchangedText)
        => $"{unchangedText} unchanged";

    // @OpenForgeText library.sync.phrase.synchronized
    internal static string FormatSynchronized(string idText, string joinText)
        => $"Synchronized {idText}: {joinText}.";

    // @OpenForgeText library.sync.phrase.to-add
    internal static string FormatToAdd(string addedText, string pluralText)
        => $"{addedText} {pluralText} to add";

    // @OpenForgeText library.sync.phrase.to-remove
    internal static string FormatToRemove(string removedText)
        => $"{removedText} to remove";

    // @OpenForgeText library.sync.phrase.would-synchronize
    internal static string FormatWouldSynchronize(string idText, string joinText)
        => $"Would synchronize {idText}: {joinText}.";

    // @OpenForgeText library.sync.phrase.could-not-be-synchronized-nothing-was-changed
    internal static string FormatCouldNotBeSynchronizedNothingWasChanged(string idText, string trimSentenceText)
        => $"{idText} could not be synchronized: {trimSentenceText}. Nothing was changed.";

    // @OpenForgeText library.sync.phrase.could-not-be-fully-synchronized
    internal static string FormatCouldNotBeFullySynchronized(string idText, string limitationText)
        => $"{idText} could not be fully synchronized: {limitationText}.";

    // @OpenForgeText library.sync.phrase.cannot-synchronize
    internal static string FormatCannotSynchronize(string idText, string trimSentenceText)
        => $"Cannot synchronize {idText}: {trimSentenceText}.";

    // @OpenForgeText library.sync.phrase.cannot-synchronize-nothing-was-changed
    internal static string FormatCannotSynchronizeNothingWasChanged(string idText, string trimSentenceText)
        => $"Cannot synchronize {idText}: {trimSentenceText}. Nothing was changed.";

    internal static string FormatIsNotAValidLibraryId(string isNullOrWhiteSpaceText)
        => global::OpenForge.Cli.OutputText.Library.Shared.CanonicalPhrases.FormatIsNotAValidLibraryId(isNullOrWhiteSpaceText);

    // @OpenForgeText library.sync.plan.failed
    internal static string PlanFailed(string trimSentenceText)
        => $"The link plan could not be completed: {trimSentenceText}.";

    // @OpenForgeText library.sync.phrase.inventory
    internal static string FormatInventory(string eligibleText, string pluralText)
        => $"Inventory: {eligibleText} {pluralText}, ";

    // @OpenForgeText library.sync.phrase.expected-state-is
    internal static string FormatExpectedStateIs(string pathText, string stateText)
        => $"Expected state: {pathText} is {stateText}.";

    // @OpenForgeText library.sync.phrase.expected-state-is-target
    internal static string FormatExpectedStateIsTarget(string pathText, string stateText, string targetText)
        => $"Expected state: {pathText} is {stateText}; target {targetText}.";

    // @OpenForgeText library.sync.phrase.verification-record-publication
    internal static string FormatVerificationRecordPublication(string humanStateText, string humanStateText2)
        => $"Verification: {humanStateText}; record publication: {humanStateText2}.";
}
