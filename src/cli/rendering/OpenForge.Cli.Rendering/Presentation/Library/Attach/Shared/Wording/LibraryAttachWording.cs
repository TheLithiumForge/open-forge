using System.Globalization;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.Presentation.Library.Attach.Shared.Wording;

internal static class LibraryAttachWording
{
    internal static string Registered(string id, string source)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachWording.Registered(id, source);

    internal static string RegisteredEmpty(string id, string source)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachWording.RegisteredEmpty(id, source);

    internal static string WouldRegister(string id, string source)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachWording.WouldRegister(id, source);

    internal static string RemoveExcludedLibraryNext(string id)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachWording.RemoveExcludedLibraryNext(id);

    internal static string WorkspaceSettingsNextReason()
        => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.MessageUpdateSettingsBeforeRerunningLibraryCommand();

    internal static string Incomplete(string id, string limitation)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachPhrases.FormatTheLibraryCouldNotBeAttachedNothingWasChanged($"{id}", $"{TrimSentence(limitation)}");

    internal static string CannotAttach(string id, string problem)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachPhrases.FormatCannotAttach($"{id}", $"{TrimSentence(problem)}");

    internal static string Failed(int created, int total)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachWording.Failed(created, total);

    internal static string Cancelled() => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.MessageLibraryAttachWasCancelledNothingWasChanged();

    internal static string CancelledAfter(int created, int total)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachWording.CancelledAfter(created, total);

    internal static string CreatedLinks(int count, string folder)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachPhrases.FormatCreatedUnder(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(count, "link", "links")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{folder}"));

    internal static string WouldCreateLinks(int count, string folder)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachPhrases.FormatWouldCreateUnder(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(count, "link", "links")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{folder}"));

    internal static string LinkTarget(string target)
        => $"-> {target}";

    internal static string LinkCreated() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelCreated();

    internal static string LinkNotStarted() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted();

    internal static string LinkUnknown() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFinalStateUnknown();

    internal static string UpdatedEntries(string path) => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachWording.UpdatedEntries(path);

    internal static string WouldUpdateEntries(string path) => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachWording.WouldUpdateEntries(path);

    internal static string SavedGrant(string path) => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachWording.SavedGrant(path);

    internal static string WouldSaveGrant(string path) => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachWording.WouldSaveGrant(path);

    internal static string LockRecorded() => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.LabelAgentsOpenForgeLockJsonRecordsTheLibrary();

    internal static string WouldRecord() => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.TitleWouldRecordTheLibraryInAgentsOpenForgeLockJson();

    internal static string NotRecordedRecovery(string path)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachWording.NotRecordedRecovery(path);

    internal static string Inventory(int eligible, int excluded)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachPhrases.FormatSourceInventory(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{eligible}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(eligible, "eligible file", "eligible files")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{excluded}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(excluded, "excluded item", "excluded items")}"));

    internal static string PermissionEvaluation(
        string decision,
        IReadOnlyList<string> required,
        IReadOnlyList<string> missing,
        bool saved)
    {
        var requiredText = required.Count == 0 ? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNone() : string.Join(", ", required);
        var missingText = missing.Count == 0 ? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNone() : string.Join(", ", missing);
        var decisionText = decision switch
        {
            "not-evaluated" => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.LabelPermissionWasNotEvaluated(),
            "not-required" => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.LabelNoOutsidePermissionWasNeeded(),
            "granted" => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.LabelTheDestinationPathsWereAlreadyAllowed(),
            "approved" => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.LabelTheDestinationPathsWereApproved(),
            "required" => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.LabelPermissionIsRequired(),
            "declined" => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.LabelTheDestinationPathsWereDeclined(),
            "invalid" => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.LabelThePermissionSettingsCouldNotBeUsed(),
            "unavailable" => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.LabelPermissionCouldNotBeChecked(),
            "changed" => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.LabelThePermissionSettingsChangedBeforeWriting(),
            _ => throw new ArgumentOutOfRangeException(nameof(decision), decision, "The Library permission decision is not defined."),
        };
        return global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachPhrases.FormatPermissionEvaluationRequiredPathsMissingPaths($"{decisionText}", $"{requiredText}", $"{missingText}", $"{(saved ? "A grant was saved." : "No grant was saved.")}");
    }

    internal static string ExpectedState(string path, string kind, long? length, string? hash, string? target)
    {
        var details = new List<string>
        {
            kind switch
            {
                "missing" => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.LabelWillBeAbsent(),
                "ordinary-file" => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.LabelWillBeAFile(),
                "relative-file-link" => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.LabelWillBeARelativeFileLink(),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Library expected state is not defined."),
            },
        };
        if (length is { } value)
        {
            details.Add(global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachPhrases.FormatBytes(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{value}")));
        }

        if (hash is { } sha256)
        {
            details.Add(global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachPhrases.FormatSha256($"{sha256}"));
        }

        if (target is { } relativeTarget)
        {
            details.Add(global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachPhrases.FormatTarget($"{relativeTarget}"));
        }

        return global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachPhrases.FormatExpectedState($"{path}", $"{string.Join(", ", details)}");
    }

    internal static string Verification(string state, int created, int total, bool recorded)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachPhrases.FormatVerificationOfLinksCreatedTheLibrary(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{VerificationState(state)}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{created}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{total}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{(recorded ? "was recorded" : "was not recorded")}"));

    internal static string Recovery(string state, string? path, int remaining)
    {
        var sentence = state switch
        {
            "not-requested" => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.MessageNoRecoveryDataWasRequested(),
            "prepared" => path is null ? global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.MessageRecoveryDataWasPrepared() : global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachPhrases.FormatRecoveryDataWasPreparedAt($"{path}"),
            "removed" => path is null ? global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.MessageRecoveryDataWasRemoved() : global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachPhrases.FormatRecoveryDataWasRemovedAt($"{path}"),
            "retained" => path is null ? global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.MessageRecoveryDataWasRetained() : global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachPhrases.FormatRecoveryDataWasRetainedAt($"{path}"),
            "unknown" => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.MessageTheFinalStateOfRecoveryDataIsUnknown(),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Library recovery state is not defined."),
        };
        return remaining == 0
            ? sentence
            : $"{sentence.TrimEnd('.')}; {remaining} {CliText.Plural(remaining, "item remains", "items remain")}.";
    }

    private static string VerificationState(string state)
        => state switch
        {
            "not-started" => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.LabelVerificationHasNotStarted(),
            "verified" => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.LabelAllTargetsWereVerified(),
            "failed" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelVerificationFailed(),
            "unavailable" => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.LabelVerificationCouldNotBeCompleted(),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Library verification state is not defined."),
        };

    internal static string InvalidId(string? value)
    {
        var supplied = string.IsNullOrWhiteSpace(value) ? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleTheSuppliedValue() : value;
        return global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedPhrases.FormatIsNotAValidLibraryIdUseLowercaseLettersDigitsAndHyphens($"{supplied}");
    }

    internal static string DuplicateId(string id)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachWording.DuplicateId(id);

    internal static string SourceRootInvalid(string source)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachWording.SourceRootInvalid(source);

    internal static string SourceRootUnavailable(string source)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachWording.SourceRootUnavailable(source);

    internal static string SourceRootBlocked(string source)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachWording.SourceRootBlocked(source);

    internal static string DestinationRootInvalid(string value)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachWording.DestinationRootInvalid(value);

    internal static string DestinationCollision(string path)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachWording.DestinationCollision(path);

    internal static string InventoryIncomplete(string source)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachWording.InventoryIncomplete(source);

    internal static string MappingUnavailable(string source, string reason)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachPhrases.FormatTheLinkPlanForCouldNotBeCompleted($"{source}", $"{TrimSentence(reason)}");

    internal static string MappingBlocked(string path, string reason)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachPhrases.FormatCannotBeLinked($"{path}", $"{TrimSentence(reason)}");

    internal static string ConsumerBlocked(string path)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachWording.ConsumerBlocked(path);

    internal static string OwnershipObservation(string id)
        => CliFindingWording.OwnershipObservation(global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachPhrases.FormatTheLibrary($"{id}"));

    internal static string OwnershipConflict(string path, string id)
        => CliFindingWording.OwnershipConflict(path, global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelAnotherManagedDomain(), global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachPhrases.FormatAttach($"{id}"));

    internal static string RecordInvalid(string reason)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatTheLibrarySectionOfAgentsOpenForgeLockJsonIsInvalid($"{TrimSentence(reason)}");

    internal static string PermissionRequired(string folder)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachWording.PermissionRequired(folder);

    internal static string ApplicationFailed(string path, int created, int total)
        => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachWording.ApplicationFailed(path, created, total);

    internal static string FindingTitle(LibraryAttachFindingCode code) => code switch
    {
        LibraryAttachFindingCode.InvalidInput => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidInput(),
        LibraryAttachFindingCode.InvalidId => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleInvalidLibraryId(),
        LibraryAttachFindingCode.DuplicateId => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.TitleLibraryIdIsAlreadyRegistered(),
        LibraryAttachFindingCode.LibraryRemoved => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleLibraryIdIsExcluded(),
        LibraryAttachFindingCode.PathExcluded => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleDestinationIsExcluded(),
        LibraryAttachFindingCode.SourceRootInvalid => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleSourceFolderIsInvalid(),
        LibraryAttachFindingCode.SourceRootUnavailable => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleSourceFolderIsUnavailable(),
        LibraryAttachFindingCode.SourceRootBlocked => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleSourceFolderIsUnsafe(),
        LibraryAttachFindingCode.DestinationRootInvalid => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.TitleDestinationFolderIsInvalid(),
        LibraryAttachFindingCode.DestinationCollision => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.TitleDestinationCollision(),
        LibraryAttachFindingCode.InventoryIncomplete => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleSourceInventoryIsIncomplete(),
        LibraryAttachFindingCode.MappingUnavailable => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.TitleLinkPlanIsUnavailable(),
        LibraryAttachFindingCode.MappingBlocked => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.TitleLinkIsBlocked(),
        LibraryAttachFindingCode.LinkCapabilityUnavailable => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleFileLinksAreUnavailable(),
        LibraryAttachFindingCode.ConsumerBlocked => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.TitleDestinationIsManagedByOpenForge(),
        LibraryAttachFindingCode.OwnershipConflict => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.TitleDestinationOwnershipConflicts(),
        LibraryAttachFindingCode.OwnershipObservation => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleOwnershipRecordIsMissing(),
        LibraryAttachFindingCode.RecordInvalid => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleLibraryRecordIsInvalid(),
        LibraryAttachFindingCode.RecordUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleLibraryRecordIsUnavailable(),
        LibraryAttachFindingCode.RecordBlocked => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleLibraryRecordIsBlocked(),
        LibraryAttachFindingCode.PermissionRequired => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionIsRequired(),
        LibraryAttachFindingCode.PermissionDeclined => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionWasDeclined(),
        LibraryAttachFindingCode.PermissionInvalid => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionsAreInvalid(),
        LibraryAttachFindingCode.PermissionUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionsAreUnavailable(),
        LibraryAttachFindingCode.PermissionChanged => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionsChanged(),
        LibraryAttachFindingCode.PermissionWriteFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionCouldNotBeSaved(),
        LibraryAttachFindingCode.GeneratedNavigationBlocked => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionIsUnsafe(),
        LibraryAttachFindingCode.GeneratedNavigationIncomplete => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionIsUnavailable(),
        LibraryAttachFindingCode.LockUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceLockIsUnavailable(),
        LibraryAttachFindingCode.RecoveryUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryDataIsUnavailable(),
        LibraryAttachFindingCode.RecoveryRetained => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.TitleRecoveryDataWasRetained(),
        LibraryAttachFindingCode.ApplicationFailed => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.TitleLinkCreationFailed(),
        LibraryAttachFindingCode.VerificationFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleVerificationFailed(),
        LibraryAttachFindingCode.OperationFailed => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.TitleLibraryAttachFailed(),
        LibraryAttachFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.TitleLibraryAttachWasCancelled(),
        LibraryAttachFindingCode.ConfirmationRequired => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleConfirmationIsRequired(),
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Library Attach finding code is not defined."),
    };

    internal static string MachineCode(LibraryAttachFindingCode code) => code switch
    {
        LibraryAttachFindingCode.DestinationRootInvalid => "library-attach.destination-root-invalid",
        LibraryAttachFindingCode.PermissionRequired => "library-attach.permission-required",
        LibraryAttachFindingCode.PermissionDeclined => "library-attach.permission-declined",
        LibraryAttachFindingCode.PermissionInvalid => "library-attach.permission-invalid",
        LibraryAttachFindingCode.PermissionUnavailable => "library-attach.permission-unavailable",
        LibraryAttachFindingCode.PermissionChanged => "library-attach.permission-changed",
        LibraryAttachFindingCode.PermissionWriteFailed => "library-attach.permission-write-failed",
        LibraryAttachFindingCode.InvalidInput => "library-attach.invalid-input",
        LibraryAttachFindingCode.InvalidId => "library-attach.invalid-id",
        LibraryAttachFindingCode.RecordInvalid => "library-attach.record-invalid",
        LibraryAttachFindingCode.OwnershipObservation => "library-attach.ownership-observation",
        LibraryAttachFindingCode.RecordUnavailable => "library-attach.record-unavailable",
        LibraryAttachFindingCode.RecordBlocked => "library-attach.record-blocked",
        LibraryAttachFindingCode.ConsumerBlocked => "library-attach.consumer-blocked",
        LibraryAttachFindingCode.MappingBlocked => "library-attach.mapping-blocked",
        LibraryAttachFindingCode.MappingUnavailable => "library-attach.mapping-unavailable",
        LibraryAttachFindingCode.OwnershipConflict => "library-attach.ownership-conflict",
        LibraryAttachFindingCode.GeneratedNavigationIncomplete => "library-attach.generated-navigation-incomplete",
        LibraryAttachFindingCode.GeneratedNavigationBlocked => "library-attach.generated-navigation-blocked",
        LibraryAttachFindingCode.LinkCapabilityUnavailable => "library-attach.link-capability-unavailable",
        LibraryAttachFindingCode.LockUnavailable => "library-attach.lock-unavailable",
        LibraryAttachFindingCode.RecoveryUnavailable => "library-attach.recovery-unavailable",
        LibraryAttachFindingCode.ApplicationFailed => "library-attach.application-failed",
        LibraryAttachFindingCode.VerificationFailed => "library-attach.verification-failed",
        LibraryAttachFindingCode.RecoveryRetained => "library-attach.recovery-retained",
        LibraryAttachFindingCode.OperationFailed => "library-attach.operation-failed",
        LibraryAttachFindingCode.Interrupted => "library-attach.interrupted",
        LibraryAttachFindingCode.DuplicateId => "library-attach.duplicate-id",
        LibraryAttachFindingCode.LibraryRemoved => "library-attach.library-removed",
        LibraryAttachFindingCode.PathExcluded => "library-attach.path-excluded",
        LibraryAttachFindingCode.SourceRootInvalid => "library-attach.source-root-invalid",
        LibraryAttachFindingCode.SourceRootUnavailable => "library-attach.source-root-unavailable",
        LibraryAttachFindingCode.SourceRootBlocked => "library-attach.source-root-blocked",
        LibraryAttachFindingCode.InventoryIncomplete => "library-attach.inventory-incomplete",
        LibraryAttachFindingCode.DestinationCollision => "library-attach.destination-collision",
        LibraryAttachFindingCode.ConfirmationRequired => "library-attach.confirmation-required",
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Library Attach finding code is not defined."),
    };

    internal static string HelpSyntax()
        => ("  " + global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.HelpSyntax());

    internal static string HelpSourceAndDestination()
        => ("  " + global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.HelpSourceAndDestination());

    internal static string HelpPreviewAndPermissions()
        => ("  " + global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.HelpPreviewAndPermissions());

    internal static string HelpExamples()
        => ("  " + global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.HelpExamples());

    internal static string DuplicateNextReason() => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.MessageInspectTheRegisteredLibraryBeforeChoosingAnotherId();
    internal static string DoctorNextReason() => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.MessageInspectTheReportedLibraryAttachState();
    internal static string PermissionNextReason() => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.MessageAllowTheDestinationFolderThenRerunTheSameRequest();
    internal static string CollisionNextReason() => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.MessageChooseAnotherToFolder();
    internal static string CleanupNextReason() => global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.MessageReviewAndRemoveTheRetainedRecoveryDataAfterConfirmingTheResult();

    internal static string GlobalOptions()
        => ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.HelpGlobalOptions());

    private static string TrimSentence(string value) => CliFindingWording.PlainCause(value);
}
