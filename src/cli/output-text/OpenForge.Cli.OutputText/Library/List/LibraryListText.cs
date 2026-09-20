using System.Globalization;

namespace OpenForge.Cli.OutputText.Library.List;

internal static class LibraryListText
{
    // @OpenForgeText library.list.message.no-libraries-are-registered
    internal static string MessageNoLibrariesAreRegistered()
        => "No Libraries are registered.";

    // @OpenForgeText library.list.message.library-registration-could-not-be-checked-completely
    internal static string MessageLibraryRegistrationCouldNotBeCheckedCompletely()
        => "Library registration could not be checked completely.";

    // @OpenForgeText library.list.message.library-list-was-cancelled
    internal static string MessageLibraryListWasCancelled()
        => "Library list was cancelled.";

    // @OpenForgeText library.list.message.the-library-section-of-agents-open-forge-lock-json-could-not-be-read-so-no-libraries-are-listed
    internal static string MessageTheLibrarySectionOfAgentsOpenForgeLockJsonCouldNotBeReadSoNoLibrariesAreListed()
        => "The Library section of .agents/open-forge.lock.json could not be read, so no Libraries are listed.";

    // @OpenForgeText library.list.help.syntax
    internal static string HelpSyntax()
        => "open-forge library list [global options]";

    // @OpenForgeText library.list.help.inspection
    internal static string HelpInspection()
        => "List registered Libraries and their link status without scanning all source files.";

    // @OpenForgeText library.list.help.examples
    internal static string HelpExamples()
        => "open-forge library list\n  open-forge library list --format json";

    // @OpenForgeText library.list.message.register-a-library-before-listing-it
    internal static string MessageRegisterALibraryBeforeListingIt()
        => "Register a Library before listing it.";

    // @OpenForgeText library.list.message.repair-the-missing-registered-link
    internal static string MessageRepairTheMissingRegisteredLink()
        => "Repair the missing registered link.";

    // @OpenForgeText library.list.help.next-reason
    internal static string HelpNextReason()
        => "Correct the Library list input.";

    // @OpenForgeText library.list.message.report-the-failure-and-retry-the-same-library-list-request-with-bounded-diagnostics
    internal static string MessageReportTheFailureAndRetryTheSameLibraryListRequestWithBoundedDiagnostics()
        => "Report the failure and retry the same Library list request with bounded diagnostics.";

    // @OpenForgeText library.list.message.rerun-the-same-library-list-request
    internal static string MessageRerunTheSameLibraryListRequest()
        => "Rerun the same Library list request.";

    // @OpenForgeText library.list.label.libraries
    internal static string LabelLibraries()
        => "libraries";

    // @OpenForgeText library.list.label.links-unavailable
    internal static string LabelLinksUnavailable()
        => "links unavailable";

    // @OpenForgeText library.list.message.the-library-record-did-not-provide-an-authoritative-roster
    internal static string MessageTheLibraryRecordDidNotProvideAnAuthoritativeRoster()
        => "The Library record did not provide an authoritative roster.";

    // @OpenForgeText library.list.label.library-list
    internal static string LabelLibraryList()
        => "library list";

    // @OpenForgeText library.list.label.link
    internal static string LabelLink()
        => "link";

    // @OpenForgeText library.list.label.links
    internal static string LabelLinks()
        => "links";

    // @OpenForgeText library.list.label.needs
    internal static string LabelNeeds()
        => "needs";

    // @OpenForgeText library.list.label.need
    internal static string LabelNeed()
        => "need";

    // @OpenForgeText library.list.title.library-list
    internal static string TitleLibraryList()
        => "Library list";

    // @OpenForgeText library.list.label.list-libraries
    internal static string LabelListLibraries()
        => "list Libraries";

    // @OpenForgeText library.list.title.library-list-failed
    internal static string TitleLibraryListFailed()
        => "Library list failed";

    // @OpenForgeText library.list.title.library-list-was-cancelled
    internal static string TitleLibraryListWasCancelled()
        => "Library list was cancelled";

    // @OpenForgeText library.list.message.the-library-registration-was-not-checked
    internal static string MessageTheLibraryRegistrationWasNotChecked()
        => "The Library registration was not checked.";

    // @OpenForgeText library.list.message.the-library-registration-record-is-missing
    internal static string MessageTheLibraryRegistrationRecordIsMissing()
        => "The Library registration record is missing.";

    // @OpenForgeText library.list.message.the-library-registration-was-read-completely
    internal static string MessageTheLibraryRegistrationWasReadCompletely()
        => "The Library registration was read completely.";

    // @OpenForgeText library.list.message.the-library-registration-record-is-invalid
    internal static string MessageTheLibraryRegistrationRecordIsInvalid()
        => "The Library registration record is invalid.";

    // @OpenForgeText library.list.message.the-library-registration-record-could-not-be-read
    internal static string MessageTheLibraryRegistrationRecordCouldNotBeRead()
        => "The Library registration record could not be read.";

    // @OpenForgeText library.list.message.the-library-registration-record-is-unsafe
    internal static string MessageTheLibraryRegistrationRecordIsUnsafe()
        => "The Library registration record is unsafe.";

    // @OpenForgeText library.list.message.the-library-registration-could-not-be-read-because-an-unexpected-error-occurred
    internal static string MessageTheLibraryRegistrationCouldNotBeReadBecauseAnUnexpectedErrorOccurred()
        => "The Library registration could not be read because an unexpected error occurred.";

    // @OpenForgeText library.list.label.available
    internal static string LabelAvailable()
        => "available";
}
