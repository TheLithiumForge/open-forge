using System.Globalization;

namespace OpenForge.Cli.OutputText.Extension.Update;

internal static class ExtensionUpdatePhrases
{
    // @OpenForgeText extension.update.phrase.status
    internal static string FormatStatus(string readText)
        => $"status={readText}";

    // @OpenForgeText extension.update.phrase.findings
    internal static string FormatFindings(string countText)
        => $"findings={countText}";

    // @OpenForgeText extension.update.phrase.state
    internal static string FormatState(string nameText)
        => $"state: {nameText}";

    // @OpenForgeText extension.update.phrase.before-sha-256
    internal static string FormatBeforeSha256(string valueText)
        => $"Before SHA-256: {valueText}";

    // @OpenForgeText extension.update.phrase.after-sha-256
    internal static string FormatAfterSha256(string valueText)
        => $"After SHA-256: {valueText}";

    // @OpenForgeText extension.update.phrase.region
    internal static string FormatRegion(string regionText)
        => $"region: {regionText}";

    // @OpenForgeText extension.update.phrase.verification
    internal static string FormatVerification(string verificationTextText)
        => $"verification: {verificationTextText}";

    // @OpenForgeText extension.update.phrase.recovery-bundle-at
    internal static string FormatRecoveryBundleAt(string recoveryPathText)
        => $"recovery bundle at {recoveryPathText}";

    // @OpenForgeText extension.update.phrase.dependency
    internal static string FormatDependency(string idText)
        => $"{idText} (dependency)";

    // @OpenForgeText extension.update.phrase.updated-the-extension-to-from-an-earlier-version-kept
    internal static string FormatUpdatedTheExtensionToFromAnEarlierVersionKept(string idText, string versionText, string keptText, string pluralText, string verbText)
        => $"Updated the {idText} Extension to {versionText}. {keptText} {pluralText} from an earlier version {verbText} kept.";

    // @OpenForgeText extension.update.phrase.would-update-with-kept
    internal static string FormatWouldUpdateTheExtensionToFromAnEarlierVersionWouldBeKept(
        string idText,
        string versionText,
        string keptText,
        string pluralText)
        => $"Would update the {idText} Extension to {versionText}; {keptText} {pluralText} from an earlier version would be kept.";

    // @OpenForgeText extension.update.phrase.the-extension-is-up-to-date-but-from-an-earlier-version-kept
    internal static string FormatTheExtensionIsUpToDateButFromAnEarlierVersionKept(string idText, string keptText, string pluralText, string verbText)
        => $"The {idText} Extension is up to date, but {keptText} {pluralText} from an earlier version {verbText} kept.";

    // @OpenForgeText extension.update.phrase.the-extension-could-not-be-updated-nothing-was-changed
    internal static string FormatTheExtensionCouldNotBeUpdatedNothingWasChanged(string idText, string sentenceText)
        => $"The {idText} Extension could not be updated: {sentenceText}. Nothing was changed.";

    // @OpenForgeText extension.update.phrase.updated
    internal static string FormatUpdated(string countText, string pluralText)
        => $"{countText} {pluralText} updated";

    // @OpenForgeText extension.update.phrase.dependency-closure
    internal static string FormatDependencyClosure(string joinText)
        => $"Dependency closure: {joinText}";

    // @OpenForgeText extension.update.phrase.grant-required-missing-decision-action-outcome
    internal static string FormatGrantRequiredMissingDecisionActionOutcome(string valuesText, string valuesText2, string permissionDecisionText, string permissionActionText, string permissionOutcomeText)
        => $"Grant: required {valuesText}; missing {valuesText2}; decision {permissionDecisionText}; action {permissionActionText}; outcome {permissionOutcomeText}";

    // @OpenForgeText extension.update.phrase.would-save-a-grant-for-to-agents-open-forge-json
    internal static string FormatWouldSaveAGrantForToAgentsOpenForgeJson(string pathText)
        => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.WouldSaveGrant(pathText);

    // @OpenForgeText extension.update.phrase.saved-a-grant-for-to-agents-open-forge-json
    internal static string FormatSavedAGrantForToAgentsOpenForgeJson(string pathText)
        => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.SavedGrant(pathText);

    // @OpenForgeText extension.update.phrase.is-not-recorded-as-installed-so-it-was-not-updated
    internal static string FormatIsNotRecordedAsInstalledSoItWasNotUpdated(string observedIdText)
        => $"{observedIdText} is not recorded as installed, so it was not updated.";

    // @OpenForgeText extension.update.phrase.the-entries-sections-did-not-match-the-installed-files-after-writing-recovery-data
    internal static string FormatTheEntriesSectionsDidNotMatchTheInstalledFilesAfterWritingRecoveryData(string valueText)
        => $"The Entries sections did not match the installed files after writing. Recovery data: {valueText}.";
}
