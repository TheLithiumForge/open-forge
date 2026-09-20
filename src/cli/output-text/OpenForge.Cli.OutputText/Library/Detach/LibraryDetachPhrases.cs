using System.Globalization;

namespace OpenForge.Cli.OutputText.Library.Detach;

internal static class LibraryDetachPhrases
{
    // @OpenForgeText library.detach.phrase.open-forge-library-inspect
    internal static string FormatOpenForgeLibraryInspect(string idText)
        => $"open-forge library inspect {idText}";

    // @OpenForgeText library.detach.phrase.open-forge-library-detach-detail-debug
    internal static string FormatOpenForgeLibraryDetachDetailDebug(string idText)
        => $"open-forge library detach {idText} --detail debug";

    // @OpenForgeText library.detach.phrase.detached-removed-under-source-files-in-were-kept
    internal static string FormatDetachedRemovedUnderSourceFilesInWereKept(string idText, string linksText, string pluralText, string destinationText, string sourceText)
        => $"Detached {idText}: removed {linksText} {pluralText} under {destinationText}. Source files in {sourceText} were kept.";

    // @OpenForgeText library.detach.phrase.would-detach-remove-under
    internal static string FormatWouldDetachRemoveUnder(string idText, string linksText, string pluralText, string destinationText)
        => $"Would detach {idText}: remove {linksText} {pluralText} under {destinationText}.";

    // @OpenForgeText library.detach.phrase.could-not-be-detached-nothing-was-changed
    internal static string FormatCouldNotBeDetachedNothingWasChanged(string idText, string sentenceText)
        => $"{idText} could not be detached: {sentenceText}. Nothing was changed.";

    // @OpenForgeText library.detach.phrase.cannot-detach
    internal static string FormatCannotDetach(string idText, string sentenceText)
        => $"Cannot detach {idText}: {sentenceText}.";

    // @OpenForgeText library.detach.phrase.cannot-detach-nothing-was-changed
    internal static string FormatCannotDetachNothingWasChanged(string idText, string sentenceText)
        => $"Cannot detach {idText}: {sentenceText}. Nothing was changed.";

    // @OpenForgeText library.detach.phrase.the-entries-section-of
    internal static string FormatTheEntriesSectionOf(string valueText, string pathText)
        => $"{valueText} the Entries section of {pathText}";

    // @OpenForgeText library.detach.phrase.expected-length-sha-256-target
    internal static string FormatExpectedLengthSha256Target(string pathText, string kindText, string valueText, string valueText2, string valueText3)
        => $"Expected {pathText}: {kindText}; length {valueText}; SHA-256 {valueText2}; target {valueText3}";

    // @OpenForgeText library.detach.phrase.verification-registration
    internal static string FormatVerificationRegistration(string verificationTextText, string registrationTextText)
        => $"Verification: {verificationTextText}; registration {registrationTextText}.";

    // @OpenForgeText library.detach.phrase.residual
    internal static string FormatResidual(string residualsText, string pluralText)
        => $"{residualsText} residual {pluralText}";

    // @OpenForgeText library.detach.phrase.recovery-no-recovery-bundle-was-required
    internal static string FormatRecoveryNoRecoveryBundleWasRequired(string detailText)
        => $"Recovery: no recovery bundle was required; {detailText}.";

    // @OpenForgeText library.detach.phrase.recovery-bundle-prepared-at
    internal static string FormatRecoveryBundlePreparedAt(string pathText, string detailText)
        => $"Recovery: bundle prepared at {pathText}; {detailText}.";

    // @OpenForgeText library.detach.phrase.recovery-a-bundle-was-prepared
    internal static string FormatRecoveryABundleWasPrepared(string detailText)
        => $"Recovery: a bundle was prepared; {detailText}.";

    // @OpenForgeText library.detach.phrase.recovery-bundle-removed
    internal static string FormatRecoveryBundleRemoved(string detailText)
        => $"Recovery: bundle removed; {detailText}.";

    // @OpenForgeText library.detach.phrase.recovery-bundle-retained-at
    internal static string FormatRecoveryBundleRetainedAt(string pathText, string detailText)
        => $"Recovery: bundle retained at {pathText}; {detailText}.";

    // @OpenForgeText library.detach.phrase.recovery-bundle-retained
    internal static string FormatRecoveryBundleRetained(string detailText)
        => $"Recovery: bundle retained; {detailText}.";

    // @OpenForgeText library.detach.phrase.recovery-final-bundle-state-is-unknown
    internal static string FormatRecoveryFinalBundleStateIsUnknown(string detailText)
        => $"Recovery: final bundle state is unknown; {detailText}.";

    // @OpenForgeText library.detach.phrase.remove-the-links-listed-above-y-n
    internal static string FormatRemoveTheLinksListedAboveYN(string linksText)
        => $"Remove the {linksText} links listed above? [y/N]";
}
