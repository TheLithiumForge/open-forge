using System.Globalization;

namespace OpenForge.Cli.OutputText.Repair;

internal static class RepairPhrases
{
    // @OpenForgeText repair.target.candidate-with-evidence
    internal static string TargetCandidateWithEvidence(string pathText, string joinText)
        => $"possible target: {pathText} ({joinText})";

    // @OpenForgeText repair.target.candidate
    internal static string TargetCandidate(string pathText)
        => $"possible target: {pathText}";

    // @OpenForgeText repair.phrase.diagnosis-coverage
    internal static string FormatDiagnosisCoverage(string coverageText)
        => $"Diagnosis coverage: {coverageText}.";

    // @OpenForgeText repair.phrase.checks-before-writing
    internal static string FormatChecksBeforeWriting(string preflightWordingText)
        => $"Checks before writing: {preflightWordingText}.";

    // @OpenForgeText repair.phrase.recovery
    internal static string FormatRecovery(string recoveryText)
        => $"Recovery: {recoveryText}";

    // @OpenForgeText repair.phrase.checks-after-writing
    internal static string FormatChecksAfterWriting(string postDiagnosisWordingText)
        => $"Checks after writing: {postDiagnosisWordingText}.";

    internal static string FormatRecoveryData(string pathText)
        => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.FormatRecoveryData(pathText);

    // @OpenForgeText repair.phrase.repaired
    internal static string FormatRepaired(string countText, string pluralText)
        => $"Repaired {countText} {pluralText}.";

    // @OpenForgeText repair.phrase.would-repair
    internal static string FormatWouldRepair(string countText, string pluralText)
        => $"Would repair {countText} {pluralText}.";

    // @OpenForgeText repair.phrase.repaired-still-a
    internal static string FormatRepairedStillA(string repairedText, string pluralText, string remainingText, string pluralText2, string pluralText3, string valueText)
        => $"Repaired {repairedText} {pluralText}. {remainingText} {pluralText2} still {pluralText3} a {valueText}.";

    // @OpenForgeText repair.phrase.would-repair-still-a
    internal static string FormatWouldRepairStillA(string repairedText, string pluralText, string remainingText, string pluralText2, string pluralText3, string valueText)
        => $"Would repair {repairedText} {pluralText}. {remainingText} {pluralText2} still {pluralText3} a {valueText}.";

    // @OpenForgeText repair.phrase.nothing-could-be-repaired-automatically-a
    internal static string FormatNothingCouldBeRepairedAutomaticallyA(string remainingText, string pluralText, string pluralText2, string valueText)
        => $"Nothing could be repaired automatically. {remainingText} {pluralText} {pluralText2} a {valueText}.";

    // @OpenForgeText repair.phrase.cannot-repair-nothing-was-changed
    internal static string FormatCannotRepairNothingWasChanged(string trimSentenceText)
        => $"Cannot repair: {trimSentenceText}. Nothing was changed.";

    // @OpenForgeText repair.phrase.cannot-repair
    internal static string FormatCannotRepair(string trimSentenceText)
        => $"Cannot repair: {trimSentenceText}.";

    // @OpenForgeText repair.phrase.broken-link-possible
    internal static string FormatBrokenLinkPossible(string candidatesText, string pluralText)
        => $"Broken link; {candidatesText} possible {pluralText}";

    // @OpenForgeText repair.phrase.remaining-link
    internal static string RemainingLink(string destination, string assessment)
        => $"\"{destination}\"; {assessment}";

    // @OpenForgeText repair.phrase.the-repair-for-cannot-be-applied
    internal static string FormatTheRepairForCannotBeApplied(string locationText, string trimSentenceText)
        => $"The repair for {locationText} cannot be applied: {trimSentenceText}.";

    // @OpenForgeText repair.phrase.the-workspace-could-not-be-checked
    internal static string FormatTheWorkspaceCouldNotBeChecked(string trimSentenceText)
        => $"The workspace could not be checked: {trimSentenceText}.";

    // @OpenForgeText repair.phrase.a-recovery-bundle-was-prepared-at
    internal static string FormatARecoveryBundleWasPreparedAt(string pathText)
        => $"A recovery bundle was prepared at {pathText}.";

    // @OpenForgeText repair.phrase.verification-targets
    internal static string FormatVerificationTargets(string verificationStateText)
        => $"Verification: targets {verificationStateText}; ";

    // @OpenForgeText repair.phrase.resulting-bytes
    internal static string FormatResultingBytes(string verificationStateText)
        => $"resulting bytes {verificationStateText}; ";

    // @OpenForgeText repair.phrase.post-conditions
    internal static string FormatPostConditions(string verificationStateText)
        => $"post-conditions {verificationStateText}.";

    // @OpenForgeText repair.phrase.choose-a-target-for
    internal static string FormatChooseATargetFor(string sourceCanonicalPathText, string lineText, string columnText, string expectedDestinationText)
        => $"Choose a new destination for the broken link \"{expectedDestinationText}\" at {sourceCanonicalPathText}:{lineText}:{columnText}.";

    // @OpenForgeText repair.phrase.recommended-for-review
    internal static string FormatRecommendedForReview(string evidenceText)
        => $"{evidenceText}, suggested";

    // @OpenForgeText repair.phrase.library-recover-at
    internal static string FormatLibraryRecoveryQuestion(string libraryId)
        => $"Library {libraryId}: include this recovery step?";

    // @OpenForgeText repair.phrase.apply-the-that-safe-y-n
    internal static string FormatIncludeLinkRepairsYN(int count)
        => count == 1
            ? "Include this 1 link repair in the plan? [y/N]"
            : string.Create(CultureInfo.InvariantCulture, $"Include these {count} link repairs in the plan? [y/N]");

    // @OpenForgeText repair.phrase.recovery-remove-created-file
    internal static string RecoveryRemoveCreatedFile(string path) => $"Remove the file this Library created at {path}.";

    // @OpenForgeText repair.phrase.recovery-restore-previous-file
    internal static string RecoveryRestorePreviousFile(string path) => $"Restore the previous file at {path} from the recovery bundle.";

    // @OpenForgeText repair.phrase.recovery-restore-previous-entries
    internal static string RecoveryRestorePreviousEntries(string path) => $"Restore the previous Entries section in {path} from the recovery bundle.";

    // @OpenForgeText repair.phrase.recovery-restore-deleted-file
    internal static string RecoveryRestoreDeletedFile(string path) => $"Restore the deleted file at {path} from the recovery bundle.";

    // @OpenForgeText repair.phrase.recovery-remove-created-link
    internal static string RecoveryRemoveCreatedLink(string path) => $"Remove the Library link created at {path}.";

    // @OpenForgeText repair.phrase.recovery-restore-deleted-link
    internal static string RecoveryRestoreDeletedLink(string path) => $"Restore the deleted Library link at {path}.";

    // @OpenForgeText repair.phrase.recovery-check-previous-state
    internal static string RecoveryCheckPreviousState(string path) => $"Check that {path} already matches its previous state.";

    // @OpenForgeText repair.phrase.could-not-be-checked
    internal static string FormatCouldNotBeChecked(string subjectText)
        => $"{subjectText} could not be checked";

    // @OpenForgeText repair.phrase.blocked
    internal static string FormatBlocked(string subjectText, string verbText)
        => $"{subjectText} {verbText} blocked";
}
