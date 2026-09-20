using System.Globalization;
using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.Presentation.Library.List.Shared.Wording;

internal static class LibraryListWording
{
    internal static string NoLibraries() => global::OpenForge.Cli.OutputText.Library.List.LibraryListText.MessageNoLibrariesAreRegistered();

    internal static string Registered(int count)
        => global::OpenForge.Cli.OutputText.Library.List.LibraryListPhrases.FormatRegistered(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(count, "Library", "Libraries")}"));

    internal static string Attention(int libraries, int links)
    {
        var libraryWord = CliText.Plural(libraries, global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleLibrary(), global::OpenForge.Cli.OutputText.Shared.SharedText.TitleLibraries());
        var linkWord = CliText.Plural(links, global::OpenForge.Cli.OutputText.Library.List.LibraryListText.LabelLink(), global::OpenForge.Cli.OutputText.Library.List.LibraryListText.LabelLinks());
        var verb = links == 1 ? global::OpenForge.Cli.OutputText.Library.List.LibraryListText.LabelNeeds() : global::OpenForge.Cli.OutputText.Library.List.LibraryListText.LabelNeed();
        return global::OpenForge.Cli.OutputText.Library.List.LibraryListPhrases.FormatRegisteredAttention(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{libraries}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{libraryWord}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{links}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{linkWord}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{verb}"));
    }

    internal static string NoOwnership() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoOwnershipRecordExistsSoLibrariesCannotBeListedFromIt();
    internal static string Incomplete() => global::OpenForge.Cli.OutputText.Library.List.LibraryListText.MessageLibraryRegistrationCouldNotBeCheckedCompletely();
    internal static string Cancelled() => global::OpenForge.Cli.OutputText.Library.List.LibraryListText.MessageLibraryListWasCancelled();

    internal static string InvalidRecord(string cause)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatTheLibrarySectionOfAgentsOpenForgeLockJsonIsInvalid($"{Reason(cause)}");

    internal static string RecordUnavailable()
        => global::OpenForge.Cli.OutputText.Library.List.LibraryListText.MessageTheLibrarySectionOfAgentsOpenForgeLockJsonCouldNotBeReadSoNoLibrariesAreListed();

    internal static string RecordBlocked(string cause)
        => global::OpenForge.Cli.OutputText.Library.List.LibraryListPhrases.FormatAgentsOpenForgeLockJsonIsInvalid($"{Reason(cause)}");

    internal static string SourceRootInvalid(string id, string path)
        => global::OpenForge.Cli.OutputText.Library.List.LibraryListWording.SourceRootInvalid(id, path);

    internal static string SourceRootUnavailable(string id, string path)
        => global::OpenForge.Cli.OutputText.Library.List.LibraryListWording.SourceRootUnavailable(id, path);

    internal static string SourceRootBlocked(string id, string path)
        => global::OpenForge.Cli.OutputText.Library.List.LibraryListWording.SourceRootBlocked(id, path);

    internal static string LinkMissing(string path) => global::OpenForge.Cli.OutputText.Library.List.LibraryListWording.LinkMissing(path);
    internal static string LinkChanged(string path, string id) => global::OpenForge.Cli.OutputText.Library.List.LibraryListWording.LinkChanged(path, id);
    internal static string LinkUnavailable(string path) => global::OpenForge.Cli.OutputText.Library.List.LibraryListWording.LinkUnavailable(path);
    internal static string LinkBlocked(string path, string cause) => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedPhrases.FormatCouldNotBeCheckedSafely($"{path}", $"{Reason(cause)}");

    internal static string Failed(string cause)
        => CliFindingWording.OperationFailed(global::OpenForge.Cli.OutputText.Library.List.LibraryListText.TitleLibraryList(), Reason(cause));

    internal static string CannotList(string cause)
        => global::OpenForge.Cli.OutputText.Library.List.LibraryListPhrases.FormatCannotListLibraries($"{Reason(cause)}");

    internal static string InvalidInput(string cause)
        => CliFindingWording.InvalidInput(global::OpenForge.Cli.OutputText.Library.List.LibraryListText.LabelListLibraries(), Reason(cause));

    internal static string Interrupted() => CliFindingWording.Interrupted(global::OpenForge.Cli.OutputText.Library.List.LibraryListText.TitleLibraryList());

    internal static string MachineCode(LibraryListFindingCode code) => code switch
    {
        LibraryListFindingCode.InvalidInput => "library-list.invalid-input",
        LibraryListFindingCode.OwnershipObservation => "library-list.ownership-observation",
        LibraryListFindingCode.InvalidRecord => "library-list.invalid-record",
        LibraryListFindingCode.RecordUnavailable => "library-list.record-unavailable",
        LibraryListFindingCode.RecordBlocked => "library-list.record-blocked",
        LibraryListFindingCode.SourceRootInvalid => "library-list.source-root-invalid",
        LibraryListFindingCode.SourceRootUnavailable => "library-list.source-root-unavailable",
        LibraryListFindingCode.SourceRootBlocked => "library-list.source-root-blocked",
        LibraryListFindingCode.LinkMissing => "library-list.link-missing",
        LibraryListFindingCode.LinkChanged => "library-list.link-changed",
        LibraryListFindingCode.LinkUnavailable => "library-list.link-unavailable",
        LibraryListFindingCode.LinkBlocked => "library-list.link-blocked",
        LibraryListFindingCode.OperationFailed => "library-list.operation-failed",
        LibraryListFindingCode.Interrupted => "library-list.interrupted",
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Library List finding code is not defined."),
    };

    internal static string RecordState(LibraryRecordViewState state) => state switch
    {
        LibraryRecordViewState.NotStarted => "not-started",
        LibraryRecordViewState.Missing => "missing",
        LibraryRecordViewState.Complete => "complete",
        LibraryRecordViewState.Invalid => "invalid",
        LibraryRecordViewState.Unavailable => "unavailable",
        LibraryRecordViewState.Blocked => "blocked",
        LibraryRecordViewState.Failed => "failed",
        LibraryRecordViewState.Interrupted => "interrupted",
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Library record state is not defined."),
    };
    internal static string FindingTitle(LibraryListFindingCode code) => code switch
    {
        LibraryListFindingCode.InvalidInput => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidInput(),
        LibraryListFindingCode.OwnershipObservation => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleOwnershipRecordIsMissing(),
        LibraryListFindingCode.InvalidRecord => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleLibraryRecordIsInvalid(),
        LibraryListFindingCode.RecordUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleLibraryRecordIsUnavailable(),
        LibraryListFindingCode.RecordBlocked => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleLibraryRecordIsBlocked(),
        LibraryListFindingCode.SourceRootInvalid => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleSourceFolderIsInvalid(),
        LibraryListFindingCode.SourceRootUnavailable => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleSourceFolderIsUnavailable(),
        LibraryListFindingCode.SourceRootBlocked => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleSourceFolderIsUnsafe(),
        LibraryListFindingCode.LinkMissing => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleRegisteredLinkIsMissing(),
        LibraryListFindingCode.LinkChanged => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleRegisteredLinkChanged(),
        LibraryListFindingCode.LinkUnavailable => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleRegisteredLinkIsUnavailable(),
        LibraryListFindingCode.LinkBlocked => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleRegisteredLinkIsBlocked(),
        LibraryListFindingCode.OperationFailed => global::OpenForge.Cli.OutputText.Library.List.LibraryListText.TitleLibraryListFailed(),
        LibraryListFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Library.List.LibraryListText.TitleLibraryListWasCancelled(),
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Library List finding code is not defined."),
    };

    internal static string LinkState(LibraryLinkViewState state) => state switch
    {
        LibraryLinkViewState.NotStarted => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotChecked(),
        LibraryLinkViewState.Current => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelCurrent(),
        LibraryLinkViewState.Missing => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelMissing(),
        LibraryLinkViewState.Changed => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelChanged(),
        LibraryLinkViewState.Unavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnavailable(),
        LibraryLinkViewState.Blocked => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelBlocked(),
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Library link state is not defined."),
    };

    internal static string LinkWireState(LibraryLinkViewState state) => state switch
    {
        LibraryLinkViewState.NotStarted => "not-started",
        LibraryLinkViewState.Current => "current",
        LibraryLinkViewState.Missing => "missing",
        LibraryLinkViewState.Changed => "changed",
        LibraryLinkViewState.Unavailable => "unavailable",
        LibraryLinkViewState.Blocked => "blocked",
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Library link state is not defined."),
    };

    internal static string LibraryRow(string sourceFolder, string destinationFolder)
        => $"{sourceFolder} -> {destinationFolder}";

    internal static string LinkSummary(int current, int missing, int changed, int unavailable)
    {
        var parts = new List<string>
        {
            global::OpenForge.Cli.OutputText.Library.List.LibraryListPhrases.FormatCurrent(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{current}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(current, "link", "links")}")),
        };
        if (missing > 0)
            parts.Add(global::OpenForge.Cli.OutputText.Library.List.LibraryListPhrases.FormatMissing(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{missing}")));
        if (changed > 0)
            parts.Add(global::OpenForge.Cli.OutputText.Library.List.LibraryListPhrases.FormatChanged(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{changed}")));
        if (unavailable > 0)
            parts.Add(global::OpenForge.Cli.OutputText.Library.List.LibraryListPhrases.FormatUnavailable(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{unavailable}")));
        return string.Join(", ", parts);
    }

    internal static string ExpectedTarget(string? value)
        => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedPhrases.FormatExpectedTarget($"{Value(value)}");

    internal static string ObservedTarget(string? value)
        => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedPhrases.FormatObservedTarget($"{Value(value)}");

    internal static string SourceId(string? value)
        => global::OpenForge.Cli.OutputText.Library.List.LibraryListPhrases.FormatSourceId($"{Value(value)}");

    internal static string RecordCoverage(LibraryListRecordView record)
        => RecordCoverage(record.State);

    internal static string RecordCoverage(LibraryRecordViewState state)
        => state switch
        {
            LibraryRecordViewState.NotStarted => global::OpenForge.Cli.OutputText.Library.List.LibraryListText.MessageTheLibraryRegistrationWasNotChecked(),
            LibraryRecordViewState.Missing => global::OpenForge.Cli.OutputText.Library.List.LibraryListText.MessageTheLibraryRegistrationRecordIsMissing(),
            LibraryRecordViewState.Complete => global::OpenForge.Cli.OutputText.Library.List.LibraryListText.MessageTheLibraryRegistrationWasReadCompletely(),
            LibraryRecordViewState.Invalid => global::OpenForge.Cli.OutputText.Library.List.LibraryListText.MessageTheLibraryRegistrationRecordIsInvalid(),
            LibraryRecordViewState.Unavailable => global::OpenForge.Cli.OutputText.Library.List.LibraryListText.MessageTheLibraryRegistrationRecordCouldNotBeRead(),
            LibraryRecordViewState.Blocked => global::OpenForge.Cli.OutputText.Library.List.LibraryListText.MessageTheLibraryRegistrationRecordIsUnsafe(),
            LibraryRecordViewState.Failed => global::OpenForge.Cli.OutputText.Library.List.LibraryListText.MessageTheLibraryRegistrationCouldNotBeReadBecauseAnUnexpectedErrorOccurred(),
            LibraryRecordViewState.Interrupted => Cancelled(),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Library record state is not defined."),
        };

    internal static string HelpSyntax() => ("  " + global::OpenForge.Cli.OutputText.Library.List.LibraryListText.HelpSyntax());
    internal static string HelpInspection() => ("  " + global::OpenForge.Cli.OutputText.Library.List.LibraryListText.HelpInspection());
    internal static string HelpExamples() => ("  " + global::OpenForge.Cli.OutputText.Library.List.LibraryListText.HelpExamples());
    internal static string HelpGlobalOptions() => ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.HelpGlobalOptions());

    internal static string AttachNextReason() => global::OpenForge.Cli.OutputText.Library.List.LibraryListText.MessageRegisterALibraryBeforeListingIt();
    internal static string SyncNextReason() => global::OpenForge.Cli.OutputText.Library.List.LibraryListText.MessageRepairTheMissingRegisteredLink();
    internal static string InspectNextReason() => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.MessageInspectTheLibraryBeforeChangingIt();
    internal static string DoctorNextReason() => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.MessageInspectTheReportedLibraryState();
    internal static string HelpNextReason() => global::OpenForge.Cli.OutputText.Library.List.LibraryListText.HelpNextReason();
    internal static string FailedNextReason() => global::OpenForge.Cli.OutputText.Library.List.LibraryListText.MessageReportTheFailureAndRetryTheSameLibraryListRequestWithBoundedDiagnostics();
    internal static string RetryNextReason() => global::OpenForge.Cli.OutputText.Library.List.LibraryListText.MessageRerunTheSameLibraryListRequest();

    private static string Reason(string cause) => CliFindingWording.PlainCause(cause);

    internal static string Value(string? value) => value is null ? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnavailable() : value;

    internal static string SourceRootState(LibrarySourceRootViewState state) => state switch
    {
        LibrarySourceRootViewState.NotStarted => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotChecked(),
        LibrarySourceRootViewState.Available => global::OpenForge.Cli.OutputText.Library.List.LibraryListText.LabelAvailable(),
        LibrarySourceRootViewState.Missing => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelMissing(),
        LibrarySourceRootViewState.Unavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnavailable(),
        LibrarySourceRootViewState.Invalid => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelInvalid(),
        LibrarySourceRootViewState.Blocked => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelBlocked(),
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Library source-root state is not defined."),
    };
}

