using System.Globalization;

namespace OpenForge.Cli.OutputText.Library.Attach;

internal static class LibraryAttachPhrases
{
    // @OpenForgeText library.attach.phrase.the-library-could-not-be-attached-nothing-was-changed
    internal static string FormatTheLibraryCouldNotBeAttachedNothingWasChanged(string idText, string trimSentenceText)
        => $"The {idText} Library could not be attached: {trimSentenceText}. Nothing was changed.";

    // @OpenForgeText library.attach.phrase.cannot-attach
    internal static string FormatCannotAttach(string idText, string trimSentenceText)
        => $"Cannot attach {idText}: {trimSentenceText}.";

    // @OpenForgeText library.attach.phrase.created-under
    internal static string FormatCreatedUnder(string countText, string pluralText, string folderText)
        => $"Created {countText} {pluralText} under {folderText}";

    // @OpenForgeText library.attach.phrase.would-create-under
    internal static string FormatWouldCreateUnder(string countText, string pluralText, string folderText)
        => $"Would create {countText} {pluralText} under {folderText}";

    // @OpenForgeText library.attach.phrase.source-inventory
    internal static string FormatSourceInventory(string eligibleText, string pluralText, string excludedText, string pluralText2)
        => $"Source inventory: {eligibleText} {pluralText}, {excludedText} {pluralText2}";

    // @OpenForgeText library.attach.phrase.permission-evaluation-required-paths-missing-paths
    internal static string FormatPermissionEvaluationRequiredPathsMissingPaths(string decisionTextText, string requiredTextText, string missingTextText, string valueText)
        => $"Permission evaluation: {decisionTextText}. Required paths: {requiredTextText}. Missing paths: {missingTextText}. {valueText}";

    // @OpenForgeText library.attach.phrase.bytes
    internal static string FormatBytes(string valueText)
        => $"{valueText} bytes";

    // @OpenForgeText library.attach.phrase.sha256
    internal static string FormatSha256(string sha256Text)
        => $"sha256 {sha256Text}";

    // @OpenForgeText library.attach.phrase.target
    internal static string FormatTarget(string relativeTargetText)
        => $"target {relativeTargetText}";

    // @OpenForgeText library.attach.phrase.expected-state
    internal static string FormatExpectedState(string pathText, string joinText)
        => $"Expected state: {pathText} ({joinText})";

    // @OpenForgeText library.attach.phrase.verification-of-links-created-the-library
    internal static string FormatVerificationOfLinksCreatedTheLibrary(string verificationStateText, string createdText, string totalText, string valueText)
        => $"Verification: {verificationStateText}; {createdText} of {totalText} links created; the Library {valueText}";

    // @OpenForgeText library.attach.phrase.recovery-data-was-prepared-at
    internal static string FormatRecoveryDataWasPreparedAt(string pathText)
        => $"Recovery data was prepared at {pathText}.";

    // @OpenForgeText library.attach.phrase.recovery-data-was-removed-at
    internal static string FormatRecoveryDataWasRemovedAt(string pathText)
        => $"Recovery data was removed at {pathText}.";

    // @OpenForgeText library.attach.phrase.recovery-data-was-retained-at
    internal static string FormatRecoveryDataWasRetainedAt(string pathText)
        => $"Recovery data was retained at {pathText}.";

    // @OpenForgeText library.attach.phrase.the-link-plan-for-could-not-be-completed
    internal static string FormatTheLinkPlanForCouldNotBeCompleted(string sourceText, string trimSentenceText)
        => $"The link plan for {sourceText} could not be completed: {trimSentenceText}.";

    // @OpenForgeText library.attach.phrase.cannot-be-linked
    internal static string FormatCannotBeLinked(string pathText, string trimSentenceText)
        => $"{pathText} cannot be linked: {trimSentenceText}.";

    // @OpenForgeText library.attach.phrase.the-library
    internal static string FormatTheLibrary(string idText)
        => $"the {idText} Library";

    // @OpenForgeText library.attach.phrase.attach
    internal static string FormatAttach(string idText)
        => $"attach {idText}";
}
