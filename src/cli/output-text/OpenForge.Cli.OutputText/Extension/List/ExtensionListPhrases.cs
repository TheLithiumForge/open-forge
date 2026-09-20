using System.Globalization;

namespace OpenForge.Cli.OutputText.Extension.List;

internal static class ExtensionListPhrases
{
    // @OpenForgeText extension.list.phrase.the-result-contains-installed-and-available
    internal static string FormatTheResultContainsInstalledAndAvailable(string installedText, string pluralText, string availableText, string pluralText2)
        => $"The result contains {installedText} installed {pluralText} and {availableText} available {pluralText2}.";

    // @OpenForgeText extension.list.phrase.installed-file-coverage
    internal static string FormatInstalledFileCoverage(string coverageText)
        => $"Installed file coverage: {coverageText}.";

    internal static string FormatNeeds(string joinText)
        => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.Needs(joinText);

    // @OpenForgeText extension.list.phrase.cannot-list-extensions
    internal static string FormatCannotListExtensions(string trimPeriodText)
        => $"Cannot list Extensions: {trimPeriodText}.";

    // @OpenForgeText extension.list.phrase.is-not-an-extension-package-or-package-folder
    internal static string FormatIsNotAnExtensionPackageOrPackageFolder(string pathText, string trimPeriodText, string expectedManifestText)
        => $"{pathText} is not an Extension package or package folder: {trimPeriodText}. {expectedManifestText}";

    // @OpenForgeText extension.list.phrase.cannot-be-used-as-a-source
    internal static string FormatCannotBeUsedAsASource(string pathText, string blockedReasonText)
        => $"{pathText} cannot be used as a source: {blockedReasonText}.";

    // @OpenForgeText extension.list.phrase.installed-by-changed-since-installation
    internal static string FormatInstalledByChangedSinceInstallation(string countText, string pluralText, string idText, string valueText)
        => $"{countText} {pluralText} installed by {idText} {valueText} changed since installation.";

    // @OpenForgeText extension.list.phrase.installed-by-missing
    internal static string FormatInstalledByMissing(string countText, string pluralText, string idText, string valueText)
        => $"{countText} {pluralText} installed by {idText} {valueText} missing.";

    // @OpenForgeText extension.list.phrase.extension-list-stopped-because-of-an-unexpected-error
    internal static string FormatExtensionListStoppedBecauseOfAnUnexpectedError(string trimPeriodText)
        => $"Extension list stopped because of an unexpected error: {trimPeriodText}.";
}
