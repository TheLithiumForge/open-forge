using System.Globalization;

namespace OpenForge.Cli.OutputText.Library.Inspect;

internal static class LibraryInspectText
{
    // @OpenForgeText library.inspect.message.library-inspect-was-cancelled
    internal static string MessageLibraryInspectWasCancelled()
        => "Library inspect was cancelled.";

    // @OpenForgeText library.inspect.message.preview-the-synchronization-before-changing-files
    internal static string MessagePreviewTheSynchronizationBeforeChangingFiles()
        => "Preview the synchronization before changing files.";

    // @OpenForgeText library.inspect.message.repair-the-destination-link-manually
    internal static string MessageRepairTheDestinationLinkManually()
        => "Repair the destination link manually.";

    // @OpenForgeText library.inspect.help.syntax
    internal static string HelpSyntax()
        => "open-forge library inspect <library-id> [global options]";

    // @OpenForgeText library.inspect.help.inspection
    internal static string HelpInspection()
        => "Compare all source files and destination links for one registered Library.";

    // @OpenForgeText library.inspect.help.examples
    internal static string HelpExamples()
        => "open-forge library inspect shared\n  open-forge library inspect shared --format json";

    // @OpenForgeText library.inspect.title.the-comparison-could-not-finish
    internal static string TitleTheComparisonCouldNotFinish()
        => "The comparison could not finish";

    // @OpenForgeText library.inspect.title.the-request-is-invalid
    internal static string TitleTheRequestIsInvalid()
        => "The request is invalid";

    // @OpenForgeText library.inspect.title.the-inspection-is-blocked
    internal static string TitleTheInspectionIsBlocked()
        => "The inspection is blocked";

    // @OpenForgeText library.inspect.label.library-inspect
    internal static string LabelLibraryInspect()
        => "library inspect";

    // @OpenForgeText library.inspect.label.files-added
    internal static string LabelFilesAdded()
        => "files added";

    // @OpenForgeText library.inspect.title.library-inspect
    internal static string TitleLibraryInspect()
        => "Library inspect";

    // @OpenForgeText library.inspect.label.new-in-the-source-folder
    internal static string LabelNewInTheSourceFolder()
        => "new in the source folder";

    // @OpenForgeText library.inspect.label.source-file-gone
    internal static string LabelSourceFileGone()
        => "source file gone";

    // @OpenForgeText library.inspect.title.source-file-is-not-linked
    internal static string TitleSourceFileIsNotLinked()
        => "Source file is not linked";

    // @OpenForgeText library.inspect.title.source-file-is-gone
    internal static string TitleSourceFileIsGone()
        => "Source file is gone";

    // @OpenForgeText library.inspect.title.library-inspect-failed
    internal static string TitleLibraryInspectFailed()
        => "Library inspect failed";

    // @OpenForgeText library.inspect.title.library-inspect-was-cancelled
    internal static string TitleLibraryInspectWasCancelled()
        => "Library inspect was cancelled";
}
