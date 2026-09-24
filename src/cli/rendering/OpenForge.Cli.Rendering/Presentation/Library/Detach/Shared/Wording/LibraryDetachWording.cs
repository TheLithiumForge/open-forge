using System.Globalization;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.Presentation.Library.Detach.Shared.Wording;

internal static class LibraryDetachWording
{
    internal static string Detached(string id, int links, string destination, string source)
        => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachPhrases.FormatDetachedRemovedUnderSourceFilesInWereKept(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{id}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{links}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(links, "link", "links")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{destination}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{source}"));

    internal static string DetachedNoLinks(string id)
        => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachWording.DetachedNoLinks(id);

    internal static string RemovalRecorded(string id)
        => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachWording.RemovalRecorded(id);

    internal static string WouldRecordRemoval(string id)
        => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachWording.WouldRecordRemoval(id);

    internal static string WouldDetach(string id, int links, string destination)
        => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachPhrases.FormatWouldDetachRemoveUnder(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{id}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{links}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(links, "link", "links")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{destination}"));

    internal static string NoOwnership(string id)
        => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachWording.NoOwnership(id);

    internal static string Incomplete(string id, string limitation)
        => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachPhrases.FormatCouldNotBeDetachedNothingWasChanged($"{id}", $"{Sentence(limitation)}");

    internal static string Cannot(string id, string problem)
        => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachPhrases.FormatCannotDetach($"{id}", $"{Sentence(problem)}");

    internal static string Blocked(string id, string reason)
        => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachPhrases.FormatCannotDetachNothingWasChanged($"{id}", $"{Sentence(reason)}");

    internal static string Failed(int completed, int total)
        => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachWording.Failed(completed, total);

    internal static string Cancelled() => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.MessageLibraryDetachWasCancelledNothingWasChanged();

    internal static string InvalidInput(string reference, string problem)
        => Cannot(string.IsNullOrWhiteSpace(reference) ? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSuppliedValue() : reference, problem);

    internal static string InvalidId(string value)
        => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachWording.InvalidId(value);

    internal static string UnknownId(string value)
        => CliFindingWording.UnknownId(global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleLibrary(), value);

    internal static string OwnershipObservation(string id)
        => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachWording.OwnershipObservation(id);

    internal static string RecordInvalid(string cause)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatTheLibrarySectionOfAgentsOpenForgeLockJsonIsInvalid($"{Sentence(cause)}");

    internal static string RecordUnavailable() => CliFindingWording.LifecycleUnavailable();

    internal static string RecordBlocked(string cause)
        => CliFindingWording.LifecycleBlocked(Sentence(cause));

    internal static string RegisteredLinkMissing(string path)
        => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachWording.RegisteredLinkMissing(path);

    internal static string MappingBlocked(string path, string kind)
        => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachWording.MappingBlocked(path, kind);

    internal static string RetainedDestination(string path)
        => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachWording.RetainedDestination(path);

    internal static string MappingBlockedCause(string cause)
        => Sentence(cause);

    internal static string DestinationProtected(string path)
        => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachWording.DestinationProtected(path);

    internal static string MappingUnavailable(string path)
        => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachWording.MappingUnavailable(path);

    internal static string LinkCapabilityUnavailable()
        => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.MessageThisSystemCannotHandleTheFileLinksLibrariesUse();

    internal static string ConsumerBlocked(string path)
        => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachWording.ConsumerBlocked(path);

    internal static string OwnershipConflict(string path)
        => CliFindingWording.OwnershipConflict(path, global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelAnotherManagedDomain(), global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.LabelDetach());

    internal static string PermissionRequired()
        => CliFindingWording.PermissionRequired(global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.LabelDetach());

    internal static string PermissionBlockedReason()
        => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.MessageItWritesOutsideAgentsAndNoGrantAllowsThat();

    internal static string PermissionDeclined() => CliFindingWording.PermissionDeclined();

    internal static string PermissionInvalid(string cause)
        => CliFindingWording.PermissionsInvalid(Sentence(cause));

    internal static string PermissionUnavailable() => CliFindingWording.PermissionsUnavailable();

    internal static string PermissionChanged() => CliFindingWording.PermissionsChanged();

    internal static string PermissionWriteFailed() => CliFindingWording.PermissionWriteFailed();

    internal static string GeneratedNavigationBlocked(string path, string cause)
        => CliFindingWording.GeneratedRegionUnsafe(path, Sentence(cause));

    internal static string GeneratedNavigationIncomplete(string path, string cause)
        => CliFindingWording.ProjectionUnavailable(path, Sentence(cause));

    internal static string LockUnavailable() => CliFindingWording.WorkspaceLockUnavailable();

    internal static string LockUnavailableReason()
        => CliFindingWording.WorkspaceLockUnavailableReason();

    internal static string RecoveryUnavailable(string path)
        => CliFindingWording.RecoveryUnavailable(path);

    internal static string RecoveryRetained(string path)
        => CliFindingWording.RecoveryRetained(path);

    internal static string ApplicationFailed(string cause) => Sentence(cause);

    internal static string VerificationFailed(string path, string? recovery, string cause)
        => recovery is { Length: > 0 }
            ? CliFindingWording.VerificationFailed(path, recovery)
            : Sentence(cause);

    internal static string OperationFailed(string cause)
        => CliFindingWording.OperationFailed(global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.TitleLibraryDetach(), Sentence(cause));

    internal static string Interrupted() => Cancelled();

    internal static string ConfirmationRequired()
        => CliFindingWording.ConfirmationRequired(global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.TitleLibraryDetach());

    internal static string LinkRow(bool dryRun) => dryRun ? global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.LabelWouldRemove() : global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.LabelLinkRemoved();

    internal static string NoFilesChanged() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoFilesWereChanged();

    internal static string SectionRow(string path, bool dryRun)
        => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachPhrases.FormatTheEntriesSectionOf($"{(dryRun ? "Would" : "Updated")}", $"{path}");

    internal static string LockRow(bool dryRun)
        => dryRun ? global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.LabelRegistrationWouldBeRemoved() : global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.LabelRegistrationRemoved();

    internal static string SettingsRow(bool dryRun)
        => dryRun ? global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.LabelSettingsWouldBeUpdated() : global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.LabelSettingsUpdated();

    internal static string ExpectedState(string path, string kind, long? length, string? sha256, string? target)
        => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachPhrases.FormatExpectedLengthSha256Target($"{path}", $"{kind}", $"{Value(length)}", $"{Value(sha256)}", $"{Value(target)}");

    internal static string Verification(
        LibraryVerificationState state,
        LibraryRecordPublicationState registration)
        => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachPhrases.FormatVerificationRegistration($"{VerificationText(state)}", $"{RegistrationText(registration)}");

    internal static string RecoveryFacts(LibraryRecoveryState state, string? path, int residuals)
    {
        var detail = residuals == 0
            ? global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.LabelNoResidualEffects()
            : global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachPhrases.FormatResidual($"{residuals}", $"{CliText.Plural(residuals, "effect", "effects")}");
        return state switch
        {
            LibraryRecoveryState.NotRequested => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachPhrases.FormatRecoveryNoRecoveryBundleWasRequired($"{detail}"),
            LibraryRecoveryState.Prepared => path is { Length: > 0 }
                ? global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachPhrases.FormatRecoveryBundlePreparedAt($"{path}", $"{detail}")
                : global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachPhrases.FormatRecoveryABundleWasPrepared($"{detail}"),
            LibraryRecoveryState.Removed => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachPhrases.FormatRecoveryBundleRemoved($"{detail}"),
            LibraryRecoveryState.Retained => path is { Length: > 0 }
                ? global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachPhrases.FormatRecoveryBundleRetainedAt($"{path}", $"{detail}")
                : global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachPhrases.FormatRecoveryBundleRetained($"{detail}"),
            LibraryRecoveryState.Unknown => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachPhrases.FormatRecoveryFinalBundleStateIsUnknown($"{detail}"),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Library recovery state is not defined."),
        };
    }

    private static string VerificationText(LibraryVerificationState state) => state switch
    {
        LibraryVerificationState.NotStarted => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.LabelWasNotRequested(),
        LibraryVerificationState.Verified => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.LabelAllChangedTargetsWereVerified(),
        LibraryVerificationState.Failed => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.LabelChangedTargetsDidNotVerify(),
        LibraryVerificationState.Unavailable => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.LabelCouldNotBeCompleted(),
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Library verification state is not defined."),
    };

    private static string RegistrationText(LibraryRecordPublicationState state) => state switch
    {
        LibraryRecordPublicationState.NotStarted => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.LabelWasNotPublished(),
        LibraryRecordPublicationState.Verified => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.LabelWasPublishedAndVerified(),
        LibraryRecordPublicationState.Failed => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.LabelPublicationFailed(),
        LibraryRecordPublicationState.Unknown => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.LabelFinalPublicationStateIsUnknown(),
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Library record publication state is not defined."),
    };

    internal static string Confirm(int links)
        => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachPhrases.FormatRemoveTheLinksListedAboveYN(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{links}"));

    internal static string Mode(LibraryMode mode) => mode switch
    {
        LibraryMode.DryRun => "dry-run",
        LibraryMode.Apply => "apply",
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "The Library mode is not defined."),
    };

    internal static string FindingCode(LibraryDetachFindingCode code) => code switch
    {
        LibraryDetachFindingCode.InvalidInput => "library-detach.invalid-input",
        LibraryDetachFindingCode.PermissionRequired => "library-detach.permission-required",
        LibraryDetachFindingCode.PermissionDeclined => "library-detach.permission-declined",
        LibraryDetachFindingCode.PermissionInvalid => "library-detach.permission-invalid",
        LibraryDetachFindingCode.PermissionUnavailable => "library-detach.permission-unavailable",
        LibraryDetachFindingCode.PermissionChanged => "library-detach.permission-changed",
        LibraryDetachFindingCode.PermissionWriteFailed => "library-detach.permission-write-failed",
        LibraryDetachFindingCode.InvalidId => "library-detach.invalid-id",
        LibraryDetachFindingCode.RecordInvalid => "library-detach.record-invalid",
        LibraryDetachFindingCode.RecordUnavailable => "library-detach.record-unavailable",
        LibraryDetachFindingCode.OwnershipObservation => "library-detach.ownership-observation",
        LibraryDetachFindingCode.RecordBlocked => "library-detach.record-blocked",
        LibraryDetachFindingCode.ConsumerBlocked => "library-detach.consumer-blocked",
        LibraryDetachFindingCode.MappingBlocked => "library-detach.mapping-blocked",
        LibraryDetachFindingCode.DestinationProtected => "library-detach.destination-protected",
        LibraryDetachFindingCode.MappingUnavailable => "library-detach.mapping-unavailable",
        LibraryDetachFindingCode.OwnershipConflict => "library-detach.ownership-conflict",
        LibraryDetachFindingCode.GeneratedNavigationIncomplete => "library-detach.generated-navigation-incomplete",
        LibraryDetachFindingCode.GeneratedNavigationBlocked => "library-detach.generated-navigation-blocked",
        LibraryDetachFindingCode.LinkCapabilityUnavailable => "library-detach.link-capability-unavailable",
        LibraryDetachFindingCode.LockUnavailable => "library-detach.lock-unavailable",
        LibraryDetachFindingCode.RecoveryUnavailable => "library-detach.recovery-unavailable",
        LibraryDetachFindingCode.ApplicationFailed => "library-detach.application-failed",
        LibraryDetachFindingCode.VerificationFailed => "library-detach.verification-failed",
        LibraryDetachFindingCode.RecoveryRetained => "library-detach.recovery-retained",
        LibraryDetachFindingCode.OperationFailed => "library-detach.operation-failed",
        LibraryDetachFindingCode.Interrupted => "library-detach.interrupted",
        LibraryDetachFindingCode.UnknownId => "library-detach.unknown-id",
        LibraryDetachFindingCode.RegisteredLinkMissing => "library-detach.registered-link-missing",
        LibraryDetachFindingCode.ConfirmationRequired => "library-detach.confirmation-required",
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Library Detach finding code is not defined."),
    };

    internal static string FindingTitle(LibraryDetachFindingCode code) => code switch
    {
        LibraryDetachFindingCode.InvalidInput => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidInput(),
        LibraryDetachFindingCode.PermissionRequired => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionIsRequired(),
        LibraryDetachFindingCode.PermissionDeclined => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionWasDeclined(),
        LibraryDetachFindingCode.PermissionInvalid => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionsAreInvalid(),
        LibraryDetachFindingCode.PermissionUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionsAreUnavailable(),
        LibraryDetachFindingCode.PermissionChanged => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionsChanged(),
        LibraryDetachFindingCode.PermissionWriteFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionCouldNotBeSaved(),
        LibraryDetachFindingCode.InvalidId => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleInvalidLibraryId(),
        LibraryDetachFindingCode.RecordInvalid => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleLibraryRecordIsInvalid(),
        LibraryDetachFindingCode.RecordUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleLibraryRecordIsUnavailable(),
        LibraryDetachFindingCode.OwnershipObservation => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleOwnershipRecordIsMissing(),
        LibraryDetachFindingCode.RecordBlocked => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleLibraryRecordIsBlocked(),
        LibraryDetachFindingCode.ConsumerBlocked => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.TitleLibraryConsumerIsBlocked(),
        LibraryDetachFindingCode.MappingBlocked => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleRegisteredLinkChanged(),
        LibraryDetachFindingCode.DestinationProtected => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.TitleLibraryDestinationIsProtected(),
        LibraryDetachFindingCode.MappingUnavailable => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleRegisteredLinkIsUnavailable(),
        LibraryDetachFindingCode.OwnershipConflict => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleOwnershipConflicts(),
        LibraryDetachFindingCode.GeneratedNavigationIncomplete => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionIsUnavailable(),
        LibraryDetachFindingCode.GeneratedNavigationBlocked => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionIsUnsafe(),
        LibraryDetachFindingCode.LinkCapabilityUnavailable => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleFileLinksAreUnavailable(),
        LibraryDetachFindingCode.LockUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceLockIsUnavailable(),
        LibraryDetachFindingCode.RecoveryUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryDataIsUnavailable(),
        LibraryDetachFindingCode.ApplicationFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWritingFailed(),
        LibraryDetachFindingCode.VerificationFailed => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.TitleDetachVerificationFailed(),
        LibraryDetachFindingCode.RecoveryRetained => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleWasRetained(),
        LibraryDetachFindingCode.OperationFailed => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.TitleLibraryDetachFailed(),
        LibraryDetachFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.TitleLibraryDetachWasCancelled(),
        LibraryDetachFindingCode.UnknownId => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleLibraryIdIsUnknown(),
        LibraryDetachFindingCode.RegisteredLinkMissing => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleRegisteredLinkIsMissing(),
        LibraryDetachFindingCode.ConfirmationRequired => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleConfirmationIsRequired(),
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Library Detach finding code is not defined."),
    };

    internal static string HelpSyntax()
        => ("  " + global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.HelpSyntax());

    internal static string HelpPolicy()
        => ("  " + global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.HelpPolicy());

    internal static string HelpPermissions()
        => ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.HelpPermissions());

    internal static string HelpGlobalOptions()
        => ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.HelpGlobalOptions());

    internal static string HelpExamples()
        => ("  " + global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.HelpExamples());

    internal static string DetachedNextReason() => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.MessageReviewTheDetachedLibraryStateIfNeeded();
    internal static string ListNextReason() => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.MessageChooseARegisteredLibraryId();
    internal static string InspectNextReason() => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.MessageInspectTheLibraryBeforeChangingIt();
    internal static string DoctorNextReason() => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.MessageInspectTheReportedLibraryState();
    internal static string FailedNextReason() => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.MessageRetryTheSameLibraryDetachRequestWithBoundedDiagnostics();
    internal static string RetryNextReason() => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.MessageRerunTheSameLibraryDetachRequest();
    internal static string RecoveryNextReason() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageReviewAndRemoveTheReportedRecoveryBundle();

    internal static string WireState<T>(T value) where T : struct, Enum
        => JsonNamingPolicy.KebabCaseLower.ConvertName(value.ToString());

    internal static string ExpectedKind(LibraryExpectedStateKind kind) => kind switch
    {
        LibraryExpectedStateKind.Missing => "missing",
        LibraryExpectedStateKind.OrdinaryFile => "ordinary-file",
        LibraryExpectedStateKind.RelativeFileLink => "relative-file-link",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Library expected state kind is not defined."),
    };

    internal static string PlanState(LibraryPlanState state) => WireState(state);

    internal static string ApplicationState(LibraryApplicationState state) => WireState(state);

    internal static string VerificationState(LibraryVerificationState state) => WireState(state);

    internal static string RecoveryState(LibraryRecoveryState state) => WireState(state);

    internal static string ResidualKind(LibraryResidualKind kind) => WireState(kind);

    internal static string ResidualState(LibraryResidualState state) => WireState(state);

    internal static string Value(long? value)
        => value?.ToString(CultureInfo.InvariantCulture) ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnavailable();

    internal static string Value(string? value) => value ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnavailable();

    private static string Sentence(string value) => CliFindingWording.PlainCause(value);
}
