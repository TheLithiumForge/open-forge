using System.Globalization;

namespace OpenForge.Cli.OutputText.Extension.Inspect;

internal static class ExtensionInspectPhrases
{
    // @OpenForgeText extension.inspect.phrase.installed-sha-256
    internal static string FormatInstalledSha256(string valueText)
        => $"Installed SHA-256: {valueText}";

    // @OpenForgeText extension.inspect.phrase.package-sha-256
    internal static string FormatPackageSha256(string valueText)
        => $"Package SHA-256: {valueText}";

    internal static string FormatSource(string sourceTextText)
        => global::OpenForge.Cli.OutputText.Extension.Shared.CanonicalPhrases.Source(sourceTextText);

    // @OpenForgeText extension.inspect.phrase.manifest
    internal static string FormatManifest(string valueText)
        => $"Manifest: {valueText}";

    // @OpenForgeText extension.inspect.phrase.resolution-order
    internal static string FormatResolutionOrder(string joinText)
        => $"Resolution order: {joinText}";

    // @OpenForgeText extension.inspect.phrase.record-coverage
    internal static string FormatRecordCoverage(string nameText)
        => $"Record coverage: {nameText}";

    // @OpenForgeText extension.inspect.phrase.under
    internal static string FormatUnder(string lengthText, string pluralText, string directoryText)
        => $"{lengthText} {pluralText} under {directoryText}";

    // @OpenForgeText extension.inspect.phrase.no-extension-has-the-id-in
    internal static string FormatNoExtensionHasTheIdIn(string idText, string sourceText)
        => $"No Extension has the ID {idText} in {sourceText}.";

    // @OpenForgeText extension.inspect.phrase.the-package-at-is-invalid
    internal static string FormatThePackageAtIsInvalid(string idText, string pathText, string trimPeriodText)
        => $"The package {idText} at {pathText} is invalid: {trimPeriodText}.";

    // @OpenForgeText extension.inspect.phrase.is-not-an-extension-package-or-package-folder
    internal static string FormatIsNotAnExtensionPackageOrPackageFolder(string pathText, string trimPeriodText)
        => $"{pathText} is not an Extension package or package folder: {trimPeriodText}.";

    // @OpenForgeText extension.inspect.phrase.the-source-could-not-be-read-so-the-comparison-could-not-finish
    internal static string FormatTheSourceCouldNotBeReadSoTheComparisonCouldNotFinish(string pathText)
        => $"The source {pathText} could not be read, so the comparison could not finish.";

    // @OpenForgeText extension.inspect.phrase.is-found-more-than-once-in
    internal static string FormatIsFoundMoreThanOnceIn(string idText, string sourceText)
        => $"{idText} is found more than once in {sourceText}.";

    internal static string FormatIsInsideTheWorkspaceAndCannotBeUsedAsASource(string pathText)
        => global::OpenForge.Cli.OutputText.Extension.Shared.CanonicalPhrases.FormatIsInsideTheWorkspaceAndCannotBeUsedAsASource(pathText);

    // @OpenForgeText extension.inspect.phrase.the-ownership-record-names-in-a-way-that-cannot-be-matched-to-one-package
    internal static string FormatTheOwnershipRecordNamesInAWayThatCannotBeMatchedToOnePackage(string valueText)
        => $"The ownership record names {valueText} in a way that cannot be matched to one package.";

    // @OpenForgeText extension.inspect.phrase.changed-since-it-was-installed
    internal static string FormatChangedSinceItWasInstalled(string pathText)
        => $"{pathText}  changed since it was installed";

    // @OpenForgeText extension.inspect.phrase.missing-it-was-installed-by
    internal static string FormatMissingItWasInstalledBy(string pathText, string idText)
        => $"{pathText}  missing; it was installed by {idText}";

    // @OpenForgeText extension.inspect.phrase.new-in-the-package-not-installed-yet
    internal static string FormatNewInThePackageNotInstalledYet(string pathText)
        => $"{pathText}  new in the package; not installed yet";

    // @OpenForgeText extension.inspect.phrase.no-longer-part-of-the-package
    internal static string FormatNoLongerPartOfThePackage(string pathText)
        => $"{pathText}  no longer part of the package";

    // @OpenForgeText extension.inspect.phrase.in-the-package-is-not-a-valid-workspace-path
    internal static string FormatInThePackageIsNotAValidWorkspacePath(string pathText)
        => $"{pathText} in the package is not a valid workspace path.";

    // @OpenForgeText extension.inspect.phrase.could-not-be-read
    internal static string FormatCouldNotBeRead(string pathText)
        => $"{pathText}  could not be read";

    // @OpenForgeText extension.inspect.phrase.could-not-be-compared
    internal static string FormatCouldNotBeCompared(string pathText)
        => $"{pathText} could not be compared.";

    // @OpenForgeText extension.inspect.phrase.was-compared-byte-for-byte-because-it-is-not-markdown
    internal static string FormatWasComparedByteForByteBecauseItIsNotMarkdown(string pathText)
        => $"{pathText} was compared byte for byte because it is not Markdown.";

    // @OpenForgeText extension.inspect.phrase.the-dependency-of-could-not-be-resolved-it-was-not-compared
    internal static string FormatTheDependencyOfCouldNotBeResolvedItWasNotCompared(string valueText, string idText, string trimPeriodText)
        => $"The dependency {valueText} of {idText} could not be resolved: {trimPeriodText}. It was not compared.";

    // @OpenForgeText extension.inspect.phrase.is-available-no-ownership-record-exists-so-installation-cannot-be-checked
    internal static string FormatIsAvailableNoOwnershipRecordExistsSoInstallationCannotBeChecked(string idText, string versionText)
        => $"{idText} {versionText} is available. No ownership record exists, so installation cannot be checked.";

    // @OpenForgeText extension.inspect.phrase.is-available-and-not-installed
    internal static string FormatIsAvailableAndNotInstalled(string idText, string versionText)
        => $"{idText} {versionText} is available and not installed.";

    // @OpenForgeText extension.inspect.phrase.is-installed-and-matches-the-package
    internal static string FormatIsInstalledAndMatchesThePackage(string idText, string versionText)
        => $"{idText} {versionText} is installed and matches the package.";

    // @OpenForgeText extension.inspect.phrase.is-available
    internal static string FormatIsAvailable(string idText)
        => $"{idText} is available.";

    // @OpenForgeText extension.inspect.phrase.is-installed-need-attention
    internal static string FormatIsInstalledNeedAttention(string idText, string versionText, string countText, string pluralText)
        => $"{idText} {versionText} is installed. {countText} {pluralText} need attention.";

    // @OpenForgeText extension.inspect.phrase.version-is-available
    internal static string FormatVersionIsAvailable(string versionText)
        => $" Version {versionText} is available.";

    // @OpenForgeText extension.inspect.phrase.is-installed-but-the-comparison-could-not-finish
    internal static string FormatIsInstalledButTheComparisonCouldNotFinish(string idText, string trimPeriodText)
        => $"{idText} is installed, but the comparison could not finish: {trimPeriodText}.";

    // @OpenForgeText extension.inspect.phrase.extension-inspect-stopped-because-of-an-unexpected-error
    internal static string FormatExtensionInspectStoppedBecauseOfAnUnexpectedError(string trimPeriodText)
        => $"Extension inspect stopped because of an unexpected error: {trimPeriodText}.";

    // @OpenForgeText extension.inspect.phrase.now-requires-which-the-installed-version-did-not
    internal static string FormatNowRequiresWhichTheInstalledVersionDidNot(string idText, string dependencyText)
        => $"{idText} now requires {dependencyText}, which the installed version did not.";

    // @OpenForgeText extension.inspect.phrase.requires-which-requires
    internal static string FormatRequiresWhichRequires(string fromText, string toText, string fromText2)
        => $"{fromText} requires {toText}, which requires {fromText2}.";

    // @OpenForgeText extension.inspect.phrase.has-a-dependency-cycle
    internal static string FormatHasADependencyCycle(string idText)
        => $"{idText} has a dependency cycle.";
}
