using System.Globalization;

namespace OpenForge.Cli.OutputText.Extension.Install;

internal static class ExtensionInstallPhrases
{
    // @OpenForgeText extension.install.phrase.source
    internal static string FormatSource(string identityText, string enumNameText)
        => $"Source: {identityText} ({enumNameText})";

    // @OpenForgeText extension.install.phrase.dependency-order
    internal static string FormatDependencyOrder(string joinText)
        => $"Dependency order: {joinText}";

    // @OpenForgeText extension.install.phrase.permission-scope
    internal static string FormatPermissionScope(string joinText)
        => $"Permission scope: {joinText}";

    // @OpenForgeText extension.install.phrase.permission-evaluation-decision-required-missing-saved
    internal static string FormatPermissionEvaluationDecisionRequiredMissingSaved(string enumNameText, string lengthText, string lengthText2, string valueText)
        => $"Permission evaluation: decision {enumNameText}; required {lengthText}; missing {lengthText2}; saved {valueText}";

    internal static string FormatFrameworkFingerprint(string inventoryFingerprintText)
        => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.FrameworkFingerprint(inventoryFingerprintText);

    // @OpenForgeText extension.install.phrase.next
    internal static string FormatNext(string permissionNextText)
        => $"Next: {permissionNextText}";

    // @OpenForgeText extension.install.phrase.next-preview-replacing-it
    internal static string FormatNextPreviewReplacingIt(string forcePreviewNextText)
        => $"Next: {forcePreviewNextText}  (preview replacing it)";

    // @OpenForgeText extension.install.phrase.replace-the-existing-file-listed-above-y-n
    internal static string FormatReplaceTheExistingFileListedAboveYN(string countText, string valueText)
        => $"Replace the {countText} existing file{valueText} listed above? [y/N]";

    // @OpenForgeText extension.install.phrase.extensions
    internal static string FormatExtensions(string verbText, string countText, string joinText)
        => $"{verbText} {countText} Extensions: {joinText}.";

    // @OpenForgeText extension.install.phrase.the-extension
    internal static string FormatTheExtension(string verbText, string rootIdText)
        => $"{verbText} the {rootIdText} Extension.";

    // @OpenForgeText extension.install.phrase.the-extension-and-packages-it-requires
    internal static string FormatTheExtensionAndPackagesItRequires(string verbText, string rootIdText, string lengthText, string joinText)
        => $"{verbText} the {rootIdText} Extension and {lengthText} packages it requires: {joinText}.";

    // @OpenForgeText extension.install.phrase.the-extension-could-not-be-installed-nothing-was-changed
    internal static string FormatTheExtensionCouldNotBeInstalledNothingWasChanged(string idText, string sentenceText)
        => $"The {idText} Extension could not be installed: {sentenceText}. Nothing was changed.";

    // @OpenForgeText extension.install.phrase.cannot-install
    internal static string FormatCannotInstall(string idText, string sentenceText)
        => $"Cannot install {idText}: {sentenceText}.";

    // @OpenForgeText extension.install.phrase.ownership-conflict
    internal static string FormatOwnershipConflict(string causeText, string targetText)
        => $"{causeText} Target: {targetText}";

    // @OpenForgeText extension.install.phrase.already-where-the-package-would-write
    internal static string FormatAlreadyWhereThePackageWouldWrite(string countText, string pluralText, string valueText)
        => $"{countText} {pluralText} already {valueText} where the package would write";

    // @OpenForgeText extension.install.phrase.would-create
    internal static string FormatWouldCreate(string packageText)
        => $"Would create ({packageText})";

    // @OpenForgeText extension.install.phrase.created
    internal static string FormatCreated(string packageText)
        => $"created ({packageText})";

    // @OpenForgeText extension.install.phrase.under
    internal static string FormatUnder(string valueText, string countText, string pluralText, string directoriesText)
        => $"{valueText} {countText} {pluralText} under {directoriesText}";

    // @OpenForgeText extension.install.phrase.the-entries-section-of
    internal static string FormatTheEntriesSectionOf(string valueText, string countText, string pluralText)
        => $"{valueText} the Entries section of {countText} {pluralText}";

    internal static string FormatWouldSaveAGrantForToAgentsOpenForgeJson(string pathText)
        => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.WouldSaveGrant(pathText);

    internal static string FormatSavedAGrantForToAgentsOpenForgeJson(string pathText)
        => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.SavedGrant(pathText);

    // @OpenForgeText extension.install.phrase.verification-targets-entries-extension-record-framework-record
    internal static string FormatVerificationTargetsEntriesExtensionRecordFrameworkRecord(string humanNameText, string humanNameText2, string humanNameText3, string humanNameText4)
        => $"Verification: targets {humanNameText}; Entries {humanNameText2}; Extension record {humanNameText3}; Framework record {humanNameText4}";
}
