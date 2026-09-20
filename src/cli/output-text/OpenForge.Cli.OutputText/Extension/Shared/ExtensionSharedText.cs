using System.Globalization;

namespace OpenForge.Cli.OutputText.Extension.Shared;

internal static class ExtensionSharedText
{
    // @OpenForgeText extension.shared.title.installed
    internal static string TitleInstalled()
        => "Installed";

    // @OpenForgeText extension.shared.label.the-extension
    internal static string LabelTheExtension()
        => "the Extension";

    // @OpenForgeText extension.shared.label.it-resolves-to-an-unsafe-location
    internal static string LabelItResolvesToAnUnsafeLocation()
        => "it resolves to an unsafe location";

    // @OpenForgeText extension.shared.label.no-longer-part-of-the-package
    internal static string LabelNoLongerPartOfThePackage()
        => "no longer part of the package";

    // @OpenForgeText extension.shared.label.files-changed
    internal static string LabelFilesChanged()
        => "files changed";

    // @OpenForgeText extension.shared.label.files-missing
    internal static string LabelFilesMissing()
        => "files missing";

    // @OpenForgeText extension.shared.label.unchanged
    internal static string LabelUnchanged()
        => "unchanged";

    // @OpenForgeText extension.shared.title.extension-source-is-invalid
    internal static string TitleExtensionSourceIsInvalid()
        => "Extension source is invalid";

    // @OpenForgeText extension.shared.label.the-selected-source
    internal static string LabelTheSelectedSource()
        => "the selected source";

    // @OpenForgeText extension.shared.label.the-selected
    internal static string LabelTheSelected()
        => "the selected";

    // @OpenForgeText extension.shared.title.entries-sections-unchanged-none
    internal static string TitleEntriesSectionsUnchangedNone()
        => "Entries sections unchanged: none";

    // @OpenForgeText extension.shared.title.selection-is-required
    internal static string TitleSelectionIsRequired()
        => "Selection is required";

    // @OpenForgeText extension.shared.title.interaction-ended
    internal static string TitleInteractionEnded()
        => "Interaction ended";

    // @OpenForgeText extension.shared.title.source-is-unavailable
    internal static string TitleSourceIsUnavailable()
        => "Source is unavailable";

    // @OpenForgeText extension.shared.title.source-is-invalid
    internal static string TitleSourceIsInvalid()
        => "Source is invalid";

    // @OpenForgeText extension.shared.title.framework-is-unsafe
    internal static string TitleFrameworkIsUnsafe()
        => "Framework is unsafe";

    // @OpenForgeText extension.shared.title.extension-record-is-unavailable
    internal static string TitleExtensionRecordIsUnavailable()
        => "Extension record is unavailable";

    // @OpenForgeText extension.shared.title.entries-verification-failed
    internal static string TitleEntriesVerificationFailed()
        => "Entries verification failed";

    // @OpenForgeText extension.shared.title.extension-record-could-not-be-written
    internal static string TitleExtensionRecordCouldNotBeWritten()
        => "Extension record could not be written";

    // @OpenForgeText extension.shared.message.list-the-installed-extensions-then-rerun-the-request-with-an-explicit-selection
    internal static string MessageListTheInstalledExtensionsThenRerunTheRequestWithAnExplicitSelection()
        => "List the installed Extensions, then rerun the request with an explicit selection.";

    // @OpenForgeText extension.shared.message.inspect-the-reported-extension-state
    internal static string MessageInspectTheReportedExtensionState()
        => "Inspect the reported Extension state.";

    // @OpenForgeText extension.shared.title.extension-record-is-blocked
    internal static string TitleExtensionRecordIsBlocked()
        => "Extension record is blocked";

    // @OpenForgeText extension.shared.help.heading.selection-and-dependencies
    internal static string HelpHeadingSelectionAndDependencies()
        => "Selection and dependencies";

    // @OpenForgeText extension.shared.help.command-help
    internal static string HelpCommandHelp()
        => "Use open-forge extension <command> --help for selection, write policy, and examples.";
}
