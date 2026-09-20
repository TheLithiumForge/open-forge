using System.Globalization;

namespace OpenForge.Cli.OutputText.Extension.List;

internal static class ExtensionListText
{
    // @OpenForgeText extension.list.title.available-bundled-with-this-cli
    internal static string TitleAvailableBundledWithThisCli()
        => "Available (bundled with this CLI)";

    // @OpenForgeText extension.list.placeholder.no-ownership-record-so-installed-packages-cannot-be-listed
    internal static string PlaceholderNoOwnershipRecordSoInstalledPackagesCannotBeListed()
        => "(no ownership record, so installed packages cannot be listed)";

    // @OpenForgeText extension.list.title.source-unavailable
    internal static string TitleSourceUnavailable()
        => "Source: unavailable";

    // @OpenForgeText extension.list.message.extension-list-was-cancelled
    internal static string MessageExtensionListWasCancelled()
        => "Extension list was cancelled.";

    // @OpenForgeText extension.list.message.install-one-of-the-available-extensions
    internal static string MessageInstallOneOfTheAvailableExtensions()
        => "Install one of the available Extensions.";

    // @OpenForgeText extension.list.message.no-ownership-record-exists-so-installed-packages-cannot-be-listed-from-it
    internal static string MessageNoOwnershipRecordExistsSoInstalledPackagesCannotBeListedFromIt()
        => "No ownership record exists, so installed packages cannot be listed from it.";

    // @OpenForgeText extension.list.message.extension-list-could-not-be-read-completely
    internal static string MessageExtensionListCouldNotBeReadCompletely()
        => "Extension list could not be read completely.";

    // @OpenForgeText extension.list.message.extension-list-completed-with-warnings
    internal static string MessageExtensionListCompletedWithWarnings()
        => "Extension list completed with warnings.";

    // @OpenForgeText extension.list.label.the-installed
    internal static string LabelTheInstalled()
        => "the installed";

    // @OpenForgeText extension.list.label.the-recorded-source
    internal static string LabelTheRecordedSource()
        => "the recorded source";

    // @OpenForgeText extension.list.label.the-installed-package
    internal static string LabelTheInstalledPackage()
        => "the installed package";

    // @OpenForgeText extension.list.label.the-installed-file
    internal static string LabelTheInstalledFile()
        => "the installed file";

    // @OpenForgeText extension.list.title.extension-list
    internal static string TitleExtensionList()
        => "Extension list";

    // @OpenForgeText extension.list.label.installed-packages
    internal static string LabelInstalledPackages()
        => "installed packages";

    // @OpenForgeText extension.list.message.installed-package-facts-are-unavailable
    internal static string MessageInstalledPackageFactsAreUnavailable()
        => "Installed package facts are unavailable.";

    // @OpenForgeText extension.list.label.available-packages
    internal static string LabelAvailablePackages()
        => "available packages";

    // @OpenForgeText extension.list.message.available-package-facts-are-unavailable
    internal static string MessageAvailablePackageFactsAreUnavailable()
        => "Available package facts are unavailable.";

    // @OpenForgeText extension.list.label.the-selected-input
    internal static string LabelTheSelectedInput()
        => "the selected input";

    // @OpenForgeText extension.list.title.available
    internal static string TitleAvailable()
        => "Available";

    // @OpenForgeText extension.list.label.source-missing
    internal static string LabelSourceMissing()
        => "source missing";

    // @OpenForgeText extension.list.label.source-invalid
    internal static string LabelSourceInvalid()
        => "source invalid";

    // @OpenForgeText extension.list.label.source-blocked
    internal static string LabelSourceBlocked()
        => "source blocked";

    // @OpenForgeText extension.list.label.source-unavailable
    internal static string LabelSourceUnavailable()
        => "source unavailable";

    // @OpenForgeText extension.list.label.files-unavailable
    internal static string LabelFilesUnavailable()
        => "files unavailable";

    // @OpenForgeText extension.list.label.files-could-not-be-checked
    internal static string LabelFilesCouldNotBeChecked()
        => "files could not be checked";

    // @OpenForgeText extension.list.label.source-package-missing
    internal static string LabelSourcePackageMissing()
        => "source package missing";

    // @OpenForgeText extension.list.label.source-version-differs
    internal static string LabelSourceVersionDiffers()
        => "source version differs";

    // @OpenForgeText extension.list.label.source-dependencies-differ
    internal static string LabelSourceDependenciesDiffer()
        => "source dependencies differ";

    // @OpenForgeText extension.list.label.source-package-is-ambiguous
    internal static string LabelSourcePackageIsAmbiguous()
        => "source package is ambiguous";

    // @OpenForgeText extension.list.message.the-installed-files-match-their-recorded-sources
    internal static string MessageTheInstalledFilesMatchTheirRecordedSources()
        => "The installed files match their recorded sources.";

    // @OpenForgeText extension.list.title.extension-source-is-blocked
    internal static string TitleExtensionSourceIsBlocked()
        => "Extension source is blocked";

    // @OpenForgeText extension.list.title.installed-extension-source-is-missing
    internal static string TitleInstalledExtensionSourceIsMissing()
        => "Installed Extension source is missing";

    // @OpenForgeText extension.list.title.installed-extension-source-is-unavailable
    internal static string TitleInstalledExtensionSourceIsUnavailable()
        => "Installed Extension source is unavailable";

    // @OpenForgeText extension.list.title.installed-extension-source-is-invalid
    internal static string TitleInstalledExtensionSourceIsInvalid()
        => "Installed Extension source is invalid";

    // @OpenForgeText extension.list.title.installed-extension-source-is-blocked
    internal static string TitleInstalledExtensionSourceIsBlocked()
        => "Installed Extension source is blocked";

    // @OpenForgeText extension.list.title.installed-files-changed
    internal static string TitleInstalledFilesChanged()
        => "Installed files changed";

    // @OpenForgeText extension.list.title.installed-files-are-missing
    internal static string TitleInstalledFilesAreMissing()
        => "Installed files are missing";

    // @OpenForgeText extension.list.title.installed-file-is-unavailable
    internal static string TitleInstalledFileIsUnavailable()
        => "Installed file is unavailable";

    // @OpenForgeText extension.list.title.installed-file-could-not-be-checked
    internal static string TitleInstalledFileCouldNotBeChecked()
        => "Installed file could not be checked";

    // @OpenForgeText extension.list.title.installed-files-could-not-be-compared
    internal static string TitleInstalledFilesCouldNotBeCompared()
        => "Installed files could not be compared";

    // @OpenForgeText extension.list.title.extension-list-failed
    internal static string TitleExtensionListFailed()
        => "Extension list failed";

    // @OpenForgeText extension.list.title.extension-list-was-cancelled
    internal static string TitleExtensionListWasCancelled()
        => "Extension list was cancelled";

    // @OpenForgeText extension.list.message.inspect-the-installed-extension-facts
    internal static string MessageInspectTheInstalledExtensionFacts()
        => "Inspect the installed Extension facts.";

    // @OpenForgeText extension.list.label.it-is-inside-the-workspace
    internal static string LabelItIsInsideTheWorkspace()
        => "it is inside the workspace";

    // @OpenForgeText extension.list.help.syntax
    internal static string HelpSyntax()
        => "open-forge extension list [--installed] [--available] [--source <package-or-catalogue-path>] [global options]";

    // @OpenForgeText extension.list.help.heading.sections
    internal static string HelpHeadingSections()
        => "Sections";

    // @OpenForgeText extension.list.help.sections
    internal static string HelpSections()
        => "With neither filter, show Installed then Available. --installed or --available alone selects one section; together they select both. Repeating a section flag has no additional effect.";

    // @OpenForgeText extension.list.help.heading.source-and-workspace
    internal static string HelpHeadingSourceAndWorkspace()
        => "Source and workspace";

    // @OpenForgeText extension.list.help.source-and-workspace
    internal static string HelpSourceAndWorkspace()
        => "Installed facts use .agents/open-forge.lock.json ownership in the exact selected workspace. Unavailable ownership is reported as unknown coverage. Available facts use the embedded catalogue or one exact disjoint local package/catalogue supplied by --source. No network, registry, cache, or fallback is used.";

    // @OpenForgeText extension.list.help.examples
    internal static string HelpExamples()
        => "open-forge extension list\n  open-forge extension list --installed --format json\n  open-forge extension list --available --source D:/packages/open-forge";

    // @OpenForgeText extension.list.help.notes
    internal static string HelpNotes()
        => "Extension List is deterministic and read-only. An available package is not necessarily installed. Installed packages are still listed when their source is unavailable. The command writes no payload, lifecycle, generated navigation, backup, cache, temporary file, or diagnostic artifact.";
}
