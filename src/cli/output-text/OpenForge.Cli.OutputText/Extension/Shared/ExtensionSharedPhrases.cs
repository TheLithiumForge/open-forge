using System.Globalization;

namespace OpenForge.Cli.OutputText.Extension.Shared;

internal static class ExtensionSharedPhrases
{
    // @OpenForgeText extension.shared.phrase.is-not-a-valid-extension-id-use-lowercase-letters-digits-and-hyphens
    internal static string FormatIsNotAValidExtensionIdUseLowercaseLettersDigitsAndHyphens(string valueText)
        => $"{valueText} is not a valid Extension ID. Use lowercase letters, digits and hyphens.";

    // @OpenForgeText extension.shared.phrase.entries-sections-unchanged
    internal static string FormatEntriesSectionsUnchanged(string joinText)
        => $"Entries sections unchanged: {joinText}";

    // @OpenForgeText extension.shared.phrase.recovery-at-protected
    internal static string FormatRecoveryAtProtected(string nameText, string pathText, string countText, string pluralText)
        => $"Recovery: {nameText} at {pathText}; {countText} protected {pluralText}.";

    // @OpenForgeText extension.shared.phrase.is-not-a-valid-package-or-package-folder
    internal static string FormatIsNotAValidPackageOrPackageFolder(string pathText, string sentenceText)
        => $"{pathText} is not a valid package or package folder: {sentenceText}.";

    // @OpenForgeText extension.shared.phrase.delete-the-file-listed-above-y-n
    internal static string FormatDeleteTheFileListedAboveYN(string countText, string valueText)
        => $"Delete the {countText} file{valueText} listed above? [y/N]";

    // @OpenForgeText extension.shared.phrase.verification-targets-entries-extension-record
    internal static string FormatVerificationTargetsEntriesExtensionRecord(string humanNameText, string humanNameText2, string humanNameText3)
        => $"Verification: targets {humanNameText}; Entries {humanNameText2}; Extension record {humanNameText3}";
}
