using System.Globalization;

namespace OpenForge.Cli.OutputText.Shared;

internal static class SharedPhrases
{
    // @OpenForgeText shared.phrase.from
    internal static string FormatFrom(string originText)
        => $" from {originText}";

    // @OpenForgeText shared.phrase.could-belong-to-more-than-one-base-file
    internal static string FormatCouldBelongToMoreThanOneBaseFile(string pathText)
        => $"{pathText} could belong to more than one base file.";

    // @OpenForgeText shared.phrase.the-of-could-not-be-produced
    internal static string FormatTheOfCouldNotBeProduced(string valueText, string pathText, string reasonText)
        => $"The {valueText} of {pathText} could not be produced: {reasonText}.";

    // @OpenForgeText shared.phrase.has-no-section-named
    internal static string FormatHasNoSectionNamed(string pathText, string valueText)
        => $"{pathText} has no section named {valueText}.";

    // @OpenForgeText shared.phrase.a-recovery-bundle-from-an-earlier-command-is-kept-at
    internal static string FormatARecoveryBundleFromAnEarlierCommandIsKeptAt(string pathText)
        => $"A recovery bundle from an earlier command is kept at {pathText}.";

    // @OpenForgeText shared.phrase.an-unfinished-recovery-draft-is-at-a-command-did-not-finish
    internal static string FormatAnUnfinishedRecoveryDraftIsAtACommandDidNotFinish(string pathText)
        => $"An unfinished recovery draft is at {pathText}. A command did not finish.";

    // @OpenForgeText shared.phrase.has-no-entries-section
    internal static string FormatHasNoEntriesSection(string pathText)
        => $"{pathText} has no Entries section.";

    // @OpenForgeText shared.phrase.not-checked
    internal static string FormatNotChecked(string externalText, string cliTextPluralText)
        => $"{externalText} {cliTextPluralText} not checked";

    // @OpenForgeText shared.phrase.checked
    internal static string FormatChecked(string routesText, string cliTextPluralText)
        => $"{routesText} {cliTextPluralText} checked";

    // @OpenForgeText shared.phrase.missing-it-was-installed-by
    internal static string FormatMissingItWasInstalledBy(string firstOrDefaultText)
        => $"missing; it was installed by {firstOrDefaultText}";

    // @OpenForgeText shared.phrase.cannot-inspect
    internal static string FormatCannotInspect(string referenceText, string trimPeriodText)
        => $"Cannot inspect {referenceText}: {trimPeriodText}.";

    // @OpenForgeText shared.phrase.cannot-install
    internal static string FormatCannotInstall(string sentenceText)
        => $"Cannot install: {sentenceText}.";

    // @OpenForgeText shared.phrase.recovery
    internal static string FormatRecovery(string nameText)
        => $"Recovery: {nameText}.";

    // @OpenForgeText shared.phrase.cannot-remove
    internal static string FormatCannotRemove(string idText, string sentenceText)
        => $"Cannot remove {idText}: {sentenceText}.";

    // @OpenForgeText shared.update.cannot-update
    internal static string UpdateCannotUpdate(string sentenceText)
        => $"Cannot update: {sentenceText}.";

    // @OpenForgeText shared.update.cannot-update-package
    internal static string UpdateCannotUpdatePackage(string idText, string sentenceText)
        => $"Cannot update {idText}: {sentenceText}.";

    // @OpenForgeText shared.phrase.unchanged
    internal static string FormatUnchanged(string countText, string pluralText)
        => $"{countText} {pluralText} unchanged";

    // @OpenForgeText shared.phrase.the-recovery-bundle-was-retained-at
    internal static string FormatTheRecoveryBundleWasRetainedAt(string pathText)
        => $"The recovery bundle was retained at {pathText}.";

    // @OpenForgeText shared.phrase.recovery-at
    internal static string FormatRecoveryAt(string stateText, string pathText)
        => $"Recovery: {stateText} at {pathText}.";

    // @OpenForgeText shared.phrase.did-not-verify-after-it-was-written
    internal static string FormatDidNotVerifyAfterItWasWritten(string pathText, string trimSentenceText)
        => $"{pathText} did not verify after it was written: {trimSentenceText}.";

    // @OpenForgeText shared.phrase.the-library-section-of-agents-open-forge-lock-json-is-invalid
    internal static string FormatTheLibrarySectionOfAgentsOpenForgeLockJsonIsInvalid(string trimSentenceText)
        => $"The Library section of .agents/open-forge.lock.json is invalid: {trimSentenceText}.";

    // @OpenForgeText shared.phrase.delete-the-listed-above-y-n
    internal static string FormatDeleteTheListedAboveYN(string fileCountText, string pluralText)
        => $"Delete the {fileCountText} {pluralText} listed above? [y/N]";

    // @OpenForgeText shared.phrase.about-k-tokens
    internal static string FormatAboutKTokens(string valueText)
        => $"about {valueText}k tokens";

    // @OpenForgeText shared.phrase.use
    internal static string FormatUse(string commandText)
        => $"use {commandText}";

    // @OpenForgeText shared.phrase.needs-to-know-which-packages-pass-their-ids-or-all-this-session-cannot-ask
    internal static string FormatNeedsToKnowWhichPackagesPassTheirIdsOrAllThisSessionCannotAsk(string commandText)
        => $"{commandText} needs to know which packages. Pass their IDs or --all. This session cannot ask.";

    // @OpenForgeText shared.phrase.needs-to-know-which-packages-pass-their-ids-or-all
    internal static string FormatNeedsToKnowWhichPackagesPassTheirIdsOrAll(string commandText)
        => $"{commandText} needs to know which packages. Pass their IDs or --all.";

    // @OpenForgeText shared.phrase.is-needed-by-remove-both
    internal static string FormatIsNeededByRemoveBoth(string labelText, string dependentsText)
        => $"{labelText} is needed by {dependentsText}. Remove both?";

    // @OpenForgeText shared.phrase.exit-and-text
    internal static string FormatExitAndText(string machineNameText, string exitCodeText, string streamText)
        => $"  {machineNameText}: exit {exitCodeText} and text {streamText}.";
}
