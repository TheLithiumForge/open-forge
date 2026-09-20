using System.Globalization;

namespace OpenForge.Cli.OutputText.Status;

internal static class StatusPhrases
{
    // @OpenForgeText status.phrase.open-forge-is-installed-but-attention
    internal static string FormatOpenForgeIsInstalledButAttention(string countText, string pluralText, string valueText)
        => $"Open Forge is installed, but {countText} {pluralText} {valueText} attention.";

    // @OpenForgeText status.phrase.open-forge-is-installed-but-attention-and-some-checks-could-not-finish
    internal static string FormatOpenForgeIsInstalledButAttentionAndSomeChecksCouldNotFinish(string attentionCountText, string pluralText, string valueText)
        => $"Open Forge is installed, but {attentionCountText} {pluralText} {valueText} attention and some checks could not finish.";

    // @OpenForgeText status.phrase.cannot-check-status
    internal static string FormatCannotCheckStatus(string trimEndText)
        => $"Cannot check status: {trimEndText}.";

    // @OpenForgeText status.phrase.cannot-check-this-workspace
    internal static string FormatCannotCheckThisWorkspace(string trimEndText)
        => $"Cannot check this workspace: {trimEndText}.";

    // @OpenForgeText status.phrase.status-stopped-because-of-an-unexpected-error
    internal static string FormatStatusStoppedBecauseOfAnUnexpectedError(string trimEndText)
        => $"Status stopped because of an unexpected error: {trimEndText}.";

    // @OpenForgeText status.phrase.startup-reads-of-routed-files
    internal static string FormatStartupReadsOfRoutedFiles(string filesText, string routedFilesText, string tokensText)
        => $"  Startup reads {filesText} of {routedFilesText} routed files, {tokensText}.";

    // @OpenForgeText status.phrase.startup-is
    internal static string FormatStartupIs(string allRoutedFilesLabelText, string measurementSummaryText, string percentageText)
        => $"{allRoutedFilesLabelText}: {measurementSummaryText} (startup is {percentageText})";

    // @OpenForgeText status.phrase.added-since-shipped
    internal static string FormatAddedSinceShipped(string joinText)
        => $"Added since shipped: {joinText}";

    // @OpenForgeText status.counts.current
    internal static string CountsCurrent(string countText)
        => $"{countText} current";

    // @OpenForgeText status.phrase.added
    internal static string FormatAdded(string joinText)
        => $"added: {joinText}";

    // @OpenForgeText status.phrase.removed
    internal static string FormatRemoved(string joinText)
        => $"removed: {joinText}";

    // @OpenForgeText status.framework.unavailable-summary
    internal static string FrameworkUnavailableSummary(string frameworkFilesHeadingText)
        => $"{frameworkFilesHeadingText}: unavailable";

    // @OpenForgeText status.framework.current-summary
    internal static string FrameworkCurrentSummary(string frameworkFilesHeadingText, string toStringText)
        => $"{frameworkFilesHeadingText}: {toStringText} current";

    // @OpenForgeText status.phrase.extensions-current
    internal static string FormatExtensionsCurrent(string nameText, string currentText, string pluralText)
        => $"Extensions: {nameText} ({currentText} {pluralText} current)";

    // @OpenForgeText status.phrase.extensions-files-unavailable
    internal static string FormatExtensionsFilesUnavailable(string nameText)
        => $"Extensions: {nameText} (files unavailable)";

    // @OpenForgeText status.phrase.extensions
    internal static string FormatExtensions(string nameText)
        => $"Extensions: {nameText}";

    // @OpenForgeText status.phrase.libraries-current
    internal static string FormatLibrariesCurrent(string nameText, string currentText, string pluralText)
        => $"Libraries: {nameText} ({currentText} {pluralText} current)";

    // @OpenForgeText status.phrase.libraries-links-unavailable
    internal static string FormatLibrariesLinksUnavailable(string nameText)
        => $"Libraries: {nameText} (links unavailable)";

    // @OpenForgeText status.phrase.libraries
    internal static string FormatLibraries(string nameText)
        => $"Libraries: {nameText}";

    // @OpenForgeText status.phrase.expected-observed
    internal static string FormatExpectedObserved(string stateText, string expectedText, string observedText)
        => $"{stateText} (expected {expectedText}, observed {observedText})";

    // @OpenForgeText status.phrase.expected
    internal static string FormatExpected(string stateText, string expectedText)
        => $"{stateText} (expected {expectedText})";

    // @OpenForgeText status.phrase.recovery-bundle
    internal static string FormatRecoveryBundle(string humanStateText, string humanStateText2)
        => $"{humanStateText} recovery bundle, {humanStateText2}";

    // @OpenForgeText status.phrase.startup-context-could-not-be-measured-completely-could-not-be-read-completely
    internal static string FormatStartupContextCouldNotBeMeasuredCompletelyCouldNotBeReadCompletely(string pathText)
        => $"Startup context could not be measured completely: {pathText} could not be read completely.";

    // @OpenForgeText status.phrase.the-recovery-store-at-could-not-be-read
    internal static string FormatTheRecoveryStoreAtCouldNotBeRead(string pathText)
        => $"The recovery store at {pathText} could not be read.";

    // @OpenForgeText status.phrase.the-entries-section-of-is-stale
    internal static string FormatTheEntriesSectionOfIsStale(string pathText)
        => $"The Entries section of {pathText} is stale.";

    // @OpenForgeText status.phrase.the-entries-section-of-could-not-be-read
    internal static string FormatTheEntriesSectionOfCouldNotBeRead(string pathText)
        => $"The Entries section of {pathText} could not be read.";

    // @OpenForgeText status.phrase.the-entries-section-of-could-not-be-read-safely
    internal static string FormatTheEntriesSectionOfCouldNotBeReadSafely(string pathText)
        => $"The Entries section of {pathText} could not be read safely.";

    // @OpenForgeText status.phrase.agents-open-forge-lock-json-could-not-be-read-completely-for
    internal static string FormatAgentsOpenForgeLockJsonCouldNotBeReadCompletelyFor(string subjectText)
        => $".agents/open-forge.lock.json could not be read completely for {subjectText}.";

    // @OpenForgeText status.phrase.agents-open-forge-lock-json-cannot-be-used-for
    internal static string FormatAgentsOpenForgeLockJsonCannotBeUsedFor(string subjectText, string trimSentenceText)
        => $".agents/open-forge.lock.json cannot be used for {subjectText}: {trimSentenceText}.";

    // @OpenForgeText status.phrase.agents-open-forge-lock-json-is-invalid-for
    internal static string FormatAgentsOpenForgeLockJsonIsInvalidFor(string subjectText, string trimSentenceText)
        => $".agents/open-forge.lock.json is invalid for {subjectText}: {trimSentenceText}.";

    // @OpenForgeText status.phrase.could-not-be-checked-safely
    internal static string FormatCouldNotBeCheckedSafely(string trimSentenceText)
        => $"could not be checked safely: {trimSentenceText}";

    // @OpenForgeText status.phrase.the-source-of-the-extension-cannot-be-read-so-its-files-were-not-compared
    internal static string FormatTheSourceOfTheExtensionCannotBeReadSoItsFilesWereNotCompared(string idText, string valueText)
        => $"The source of the {idText} Extension, {valueText}, cannot be read, so its files were not compared.";

    // @OpenForgeText status.phrase.changed-since-it-was-installed-by
    internal static string FormatChangedSinceItWasInstalledBy(string valueText)
        => $"changed since it was installed by {valueText}";

    // @OpenForgeText status.phrase.the-recovery-bundle-at-is-damaged-and-cannot-be-used
    internal static string FormatTheRecoveryBundleAtIsDamagedAndCannotBeUsed(string pathText)
        => $"The recovery bundle at {pathText} is damaged and cannot be used.";

    // @OpenForgeText status.phrase.the-recovery-bundle-at-was-written-by-an-unsupported-version
    internal static string FormatTheRecoveryBundleAtWasWrittenByAnUnsupportedVersion(string pathText)
        => $"The recovery bundle at {pathText} was written by an unsupported version.";

    // @OpenForgeText status.phrase.the-recovery-bundle-at-could-not-be-read
    internal static string FormatTheRecoveryBundleAtCouldNotBeRead(string pathText)
        => $"The recovery bundle at {pathText} could not be read.";

    // @OpenForgeText status.phrase.the-source-folder-of-the-library-is-not-a-folder-inside-the-workspace
    internal static string FormatTheSourceFolderOfTheLibraryIsNotAFolderInsideTheWorkspace(string idText, string valueText)
        => $"The source folder of the {idText} Library, {valueText}, is not a folder inside the workspace.";

    // @OpenForgeText status.phrase.the-source-folder-of-the-library-resolves-to-an-ambiguous-location
    internal static string FormatTheSourceFolderOfTheLibraryResolvesToAnAmbiguousLocation(string idText, string valueText)
        => $"The source folder of the {idText} Library, {valueText}, resolves to an ambiguous location.";

    // @OpenForgeText status.phrase.the-source-folder-of-the-library-cannot-be-read
    internal static string FormatTheSourceFolderOfTheLibraryCannotBeRead(string idText, string valueText)
        => $"The source folder of the {idText} Library, {valueText}, cannot be read.";

    // @OpenForgeText status.phrase.missing-it-is-a-link-of-the-library
    internal static string FormatMissingItIsALinkOfTheLibrary(string idText)
        => $"missing; it is a link of the {idText} Library";

    // @OpenForgeText status.phrase.is-no-longer-the-link-the-library-created
    internal static string FormatIsNoLongerTheLinkTheLibraryCreated(string idText)
        => $"is no longer the link the {idText} Library created";

    // @OpenForgeText status.phrase.its-location-could-not-be-verified
    internal static string FormatItsLocationCouldNotBeVerified(string trimSentenceText)
        => $"its location could not be verified ({trimSentenceText}).";

    // @OpenForgeText status.counts.unavailable
    internal static string CountsUnavailable(string pluralText)
        => $"unavailable {pluralText}";
}
