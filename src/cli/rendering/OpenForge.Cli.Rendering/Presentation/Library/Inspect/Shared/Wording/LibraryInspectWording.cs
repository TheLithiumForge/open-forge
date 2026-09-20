using System.Globalization;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.Presentation.Library.Inspect.Shared.Wording;

internal static class LibraryInspectWording
{
    internal static string Current(string id, int files, string source, string destination)
        => global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectPhrases.FormatIsCurrentFromLinkedUnder(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{id}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{files}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(files, "file", "files")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{source}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{(files == 1 ? "is" : "are")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{destination}"));

    internal static string Empty(string id, string source)
        => global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectWording.Empty(id, source);

    internal static string NoOwnership(string id)
        => global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectWording.NoOwnership(id);

    internal static string NeedsSync(string id, int files, string source, string destination)
        => global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectPhrases.FormatNeedsASyncBetweenAnd(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{id}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{files}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(files, "file", "files")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{(files == 1 ? "differs" : "differ")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{source}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{destination}"));

    internal static string Incomplete(string id, string limitation)
        => global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectPhrases.FormatCouldNotBeInspectedCompletely($"{id}", $"{Reason(limitation)}");

    internal static string CannotInspect(string id, string problem)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatCannotInspect($"{id}", $"{Reason(problem)}");

    internal static string Failed(string reason)
        => CliFindingWording.OperationFailed(global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectText.TitleLibraryInspect(), Reason(reason));

    internal static string Cancelled() => global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectText.MessageLibraryInspectWasCancelled();

    internal static string InvalidId(string? value)
    {
        var supplied = string.IsNullOrWhiteSpace(value) ? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleTheSuppliedValue() : value;
        return global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedPhrases.FormatIsNotAValidLibraryIdUseLowercaseLettersDigitsAndHyphens($"{supplied}");
    }

    internal static string UnknownId(string value)
        => CliFindingWording.UnknownId(global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleLibrary(), value);

    internal static string RecordInvalid(string cause)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatTheLibrarySectionOfAgentsOpenForgeLockJsonIsInvalid($"{Reason(cause)}");

    internal static string RecordUnavailable()
        => CliFindingWording.LifecycleUnavailable();

    internal static string RecordBlocked(string cause)
        => CliFindingWording.LifecycleBlocked(Reason(cause));

    internal static string SourceRootInvalid(string path)
        => global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectWording.SourceRootInvalid(path);

    internal static string SourceRootUnavailable(string path)
        => global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectWording.SourceRootUnavailable(path);

    internal static string SourceRootBlocked(string path)
        => global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectWording.SourceRootBlocked(path);

    internal static string InventoryIncomplete(string source)
        => global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectWording.InventoryIncomplete(source);

    internal static string PathAdded(string path)
        => global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectWording.PathAdded(path);

    internal static string PathRetired(string path)
        => global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectWording.PathRetired(path);

    internal static string LinkMissing(string path)
        => global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectWording.LinkMissing(path);

    internal static string LinkChanged(string path, string id)
        => global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectWording.LinkChanged(path, id);

    internal static string LinkBlocked(string path, string cause)
        => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedPhrases.FormatCouldNotBeCheckedSafely($"{path}", $"{Reason(cause)}");

    internal static string Relation(string relation) => relation switch
    {
        "current" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelCurrent(),
        "added" => global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectText.LabelNewInTheSourceFolder(),
        "retired" => global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectText.LabelSourceFileGone(),
        "missing" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelMissing(),
        "changed" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelChanged(),
        "unavailable" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelCouldNotBeChecked(),
        "blocked" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelBlocked(),
        "not-started" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotChecked(),
        _ => throw new ArgumentOutOfRangeException(nameof(relation), relation, "The Library Inspect relation is not defined."),
    };

    internal static string RelationWire(LibraryComparisonRelation relation) => relation switch
    {
        LibraryComparisonRelation.NotStarted => "not-started",
        LibraryComparisonRelation.Current => "current",
        LibraryComparisonRelation.Added => "added",
        LibraryComparisonRelation.Retired => "retired",
        LibraryComparisonRelation.Missing => "missing",
        LibraryComparisonRelation.Changed => "changed",
        LibraryComparisonRelation.Unavailable => "unavailable",
        LibraryComparisonRelation.Blocked => "blocked",
        _ => throw new ArgumentOutOfRangeException(nameof(relation), relation, "The Library Inspect relation is not defined."),
    };

    internal static string FindingTitle(LibraryInspectFindingCode code) => code switch
    {
        LibraryInspectFindingCode.InvalidId => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleInvalidLibraryId(),
        LibraryInspectFindingCode.UnknownId => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleLibraryIdIsUnknown(),
        LibraryInspectFindingCode.RecordInvalid => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleLibraryRecordIsInvalid(),
        LibraryInspectFindingCode.RecordUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleLibraryRecordIsUnavailable(),
        LibraryInspectFindingCode.RecordBlocked => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleLibraryRecordIsBlocked(),
        LibraryInspectFindingCode.OwnershipObservation => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleOwnershipRecordIsMissing(),
        LibraryInspectFindingCode.SourceRootInvalid => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleSourceFolderIsInvalid(),
        LibraryInspectFindingCode.SourceRootUnavailable => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleSourceFolderIsUnavailable(),
        LibraryInspectFindingCode.SourceRootBlocked => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleSourceFolderIsUnsafe(),
        LibraryInspectFindingCode.InventoryIncomplete => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleSourceInventoryIsIncomplete(),
        LibraryInspectFindingCode.PathAdded => global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectText.TitleSourceFileIsNotLinked(),
        LibraryInspectFindingCode.PathRetired => global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectText.TitleSourceFileIsGone(),
        LibraryInspectFindingCode.LinkMissing => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleRegisteredLinkIsMissing(),
        LibraryInspectFindingCode.LinkChanged => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleRegisteredLinkChanged(),
        LibraryInspectFindingCode.LinkBlocked => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleRegisteredLinkIsBlocked(),
        LibraryInspectFindingCode.OperationFailed => global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectText.TitleLibraryInspectFailed(),
        LibraryInspectFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectText.TitleLibraryInspectWasCancelled(),
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Library Inspect finding code is not defined."),
    };

    internal static string ExpectedTarget(string? value)
        => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedPhrases.FormatExpectedTarget($"{Value(value)}");

    internal static string ObservedTarget(string? value)
        => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedPhrases.FormatObservedTarget($"{Value(value)}");

    internal static string Inventory(int eligible, int excluded)
        => global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectPhrases.FormatSourceInventory(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{eligible}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(eligible, "eligible file", "eligible files")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{excluded}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(excluded, "excluded item", "excluded items")}"));

    internal static string MachineCode(LibraryInspectFindingCode code) => code switch
    {
        LibraryInspectFindingCode.InvalidId => "library-inspect.invalid-id",
        LibraryInspectFindingCode.UnknownId => "library-inspect.unknown-id",
        LibraryInspectFindingCode.RecordInvalid => "library-inspect.record-invalid",
        LibraryInspectFindingCode.RecordUnavailable => "library-inspect.record-unavailable",
        LibraryInspectFindingCode.RecordBlocked => "library-inspect.record-blocked",
        LibraryInspectFindingCode.OwnershipObservation => "library-inspect.ownership-observation",
        LibraryInspectFindingCode.SourceRootInvalid => "library-inspect.source-root-invalid",
        LibraryInspectFindingCode.SourceRootUnavailable => "library-inspect.source-root-unavailable",
        LibraryInspectFindingCode.SourceRootBlocked => "library-inspect.source-root-blocked",
        LibraryInspectFindingCode.InventoryIncomplete => "library-inspect.inventory-incomplete",
        LibraryInspectFindingCode.PathAdded => "library-inspect.path-added",
        LibraryInspectFindingCode.PathRetired => "library-inspect.path-retired",
        LibraryInspectFindingCode.LinkMissing => "library-inspect.link-missing",
        LibraryInspectFindingCode.LinkChanged => "library-inspect.link-changed",
        LibraryInspectFindingCode.LinkBlocked => "library-inspect.link-blocked",
        LibraryInspectFindingCode.OperationFailed => "library-inspect.operation-failed",
        LibraryInspectFindingCode.Interrupted => "library-inspect.interrupted",
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Library Inspect finding code is not defined."),
    };

    internal static string ListNextReason() => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.MessageChooseARegisteredLibraryId();
    internal static string DoctorNextReason() => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.MessageInspectTheReportedLibraryState();
    internal static string SyncNextReason() => global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectText.MessagePreviewTheSynchronizationBeforeChangingFiles();
    internal static string FixByHandNextReason() => global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectText.MessageRepairTheDestinationLinkManually();
    internal static string HelpSyntax() => ("  " + global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectText.HelpSyntax());
    internal static string HelpInspection() => ("  " + global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectText.HelpInspection());
    internal static string HelpGlobalOptions() => ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.HelpGlobalOptions());
    internal static string HelpExamples() => ("  " + global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectText.HelpExamples());

    internal static string Value(string? value) => value ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnavailable();

    private static string Reason(string value) => CliFindingWording.PlainCause(value);
}
