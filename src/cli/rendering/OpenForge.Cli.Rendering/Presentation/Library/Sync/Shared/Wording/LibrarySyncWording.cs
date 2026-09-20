using System.Globalization;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.Presentation.Library.Sync.Shared.Wording;

internal static class LibrarySyncWording
{
    internal static string UpToDate(string id)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncWording.UpToDate(id);

    internal static string Synchronized(string id, int added, int removed, int unchanged)
    {
        var parts = new List<string>();
        if (added > 0)
        {
            parts.Add(global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncPhrases.FormatAdded(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{added}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{Plural(added, "link", "links")}")));
        }

        if (removed > 0)
        {
            parts.Add(global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncPhrases.FormatRemoved(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{removed}")));
        }

        if (unchanged > 0)
        {
            parts.Add(global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncPhrases.FormatUnchanged(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{unchanged}")));
        }

        return global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncPhrases.FormatSynchronized($"{id}", $"{string.Join(", ", parts)}");
    }

    internal static string WouldSynchronize(string id, int added, int removed)
    {
        var parts = new List<string>();
        if (added > 0)
        {
            parts.Add(global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncPhrases.FormatToAdd(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{added}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{Plural(added, "link", "links")}")));
        }

        if (removed > 0)
        {
            parts.Add(global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncPhrases.FormatToRemove(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{removed}")));
        }

        return global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncPhrases.FormatWouldSynchronize($"{id}", $"{string.Join(", ", parts)}");
    }

    internal static string NoOwnership(string id)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncWording.NoOwnership(id);

    internal static string LockUnavailable()
        => CliFindingWording.WorkspaceLockUnavailable();

    internal static string LockUnavailableReason()
        => CliFindingWording.WorkspaceLockUnavailableReason();

    internal static string Incomplete(string id, string limitation)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncPhrases.FormatCouldNotBeSynchronizedNothingWasChanged($"{id}", $"{TrimSentence(limitation)}");

    internal static string IncompleteWithEffects(string id, string limitation)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncPhrases.FormatCouldNotBeFullySynchronized(id, TrimSentence(limitation));

    internal static string CannotSynchronize(string id, string problem)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncPhrases.FormatCannotSynchronize($"{id}", $"{TrimSentence(problem)}");

    internal static string Blocked(string id, string reason)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncPhrases.FormatCannotSynchronizeNothingWasChanged($"{id}", $"{TrimSentence(reason)}");

    internal static string Failed(int completed, int total)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncWording.Failed(completed, total);

    internal static string Cancelled() => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.MessageLibrarySyncWasCancelledNothingWasChanged();

    internal static string CancelledAfter(int completed, int total)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncWording.CancelledAfter(completed, total);

    internal static string InvalidId(string? value)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncPhrases.FormatIsNotAValidLibraryId($"{(string.IsNullOrWhiteSpace(value) ? "The supplied value" : value)}");

    internal static string RecordInvalid(string reason)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatTheLibrarySectionOfAgentsOpenForgeLockJsonIsInvalid($"{TrimSentence(reason)}");

    internal static string SourceRootInvalid(string path)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncWording.SourceRootInvalid(path);

    internal static string SourceRootUnavailable(string path)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncWording.SourceRootUnavailable(path);

    internal static string SourceRootBlocked(string path)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncWording.SourceRootBlocked(path);

    internal static string InventoryIncomplete(string source)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncWording.InventoryIncomplete(source);

    internal static string MappingUnavailable(string reason)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncPhrases.PlanFailed($"{TrimSentence(reason)}");

    internal static string MappingBlocked(string path, string occupant)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncWording.MappingBlocked(path, occupant);

    internal static string DestinationCollision(string path)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncWording.DestinationCollision(path);

    internal static string RetiredLinkMissing(string path)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncWording.RetiredLinkMissing(path);

    internal static string RegisteredLinkRestored(string path)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncWording.RegisteredLinkRestored(path);

    internal static string LinkCapabilityUnavailable()
        => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.MessageThisSystemCannotCreateTheFileLinksLibrariesNeed();

    internal static string ConsumerBlocked(string path)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncWording.ConsumerBlocked(path);

    internal static string ChangedDestination(string path)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncWording.ChangedDestination(path);

    internal static string ChangedDestinationResolution(string occupant)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncWording.ChangedDestinationResolution(occupant);

    internal static string Unchanged(int count)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatUnchanged(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{Plural(count, "link", "links")}"));

    internal static string Target(string value) => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncWording.Target(value);

    internal static string Inventory(int eligible, int excluded, int unavailable)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncPhrases.FormatInventory(
            string.Create(CultureInfo.InvariantCulture, $"{eligible}"),
            string.Create(CultureInfo.InvariantCulture, $"{Plural(eligible, "eligible file", "eligible files")}"))
            + string.Create(CultureInfo.InvariantCulture, $"{excluded} {Plural(excluded, "excluded item", "excluded items")}, ")
            + string.Create(CultureInfo.InvariantCulture, $"{unavailable} {Plural(unavailable, "unavailable path", "unavailable paths")}.");

    internal static string ExpectedState(string path, string state, string? target)
        => target is null
            ? global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncPhrases.FormatExpectedStateIs($"{path}", $"{state}")
            : global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncPhrases.FormatExpectedStateIsTarget($"{path}", $"{state}", $"{target}");

    internal static string Verification(string state, string publication)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncPhrases.FormatVerificationRecordPublication($"{HumanState(state)}", $"{HumanState(publication)}");

    internal static string Recovery(string state, string? path)
        => path is null ? global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatRecovery($"{HumanRecoveryState(state)}") : global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatRecoveryAt($"{HumanRecoveryState(state)}", $"{path}");

    internal static string Lock(string action)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncWording.Lock(action);

    internal static string EntriesUpdated(string path)
        => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncWording.EntriesUpdated(path);

    internal static string EffectLabel(
        LibraryLinkEffectKind kind,
        string outcome,
        bool preview)
    {
        if (outcome == "not-started") return global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted();
        if (outcome == "unknown") return global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFinalStateUnknown();
        if (outcome == "failed") return global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFailed();
        if (preview) return kind == LibraryLinkEffectKind.Create ? global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.TitleAdd() : global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.TitleRemove();
        return kind == LibraryLinkEffectKind.Create ? global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.TitleAdded() : global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRemoved();
    }

    internal static string SectionLabel(string outcome, bool preview)
        => outcome switch
        {
            "not-started" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
            "unknown" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFinalStateUnknown(),
            "failed" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFailed(),
            _ when preview => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWouldUpdate(),
            _ => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.TitleUpdated(),
        };

    internal static string RecordLabel(string outcome, bool preview)
        => outcome switch
        {
            "not-started" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
            "unknown" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFinalStateUnknown(),
            "failed" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFailed(),
            _ when preview => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.LabelWouldBeUpdated(),
            _ => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUpdated(),
        };

    internal static string FindingTitle(LibrarySyncFindingCode code) => code switch
    {
        LibrarySyncFindingCode.InvalidInput => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidInput(),
        LibrarySyncFindingCode.PermissionRequired => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionIsRequired(),
        LibrarySyncFindingCode.PermissionDeclined => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionWasDeclined(),
        LibrarySyncFindingCode.PermissionInvalid => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.TitlePermissionDataIsInvalid(),
        LibrarySyncFindingCode.PermissionUnavailable => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.TitlePermissionDataIsUnavailable(),
        LibrarySyncFindingCode.PermissionChanged => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.TitlePermissionDataChanged(),
        LibrarySyncFindingCode.PermissionWriteFailed => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.TitlePermissionWriteFailed(),
        LibrarySyncFindingCode.InvalidId => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleInvalidLibraryId(),
        LibrarySyncFindingCode.RecordInvalid => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleLibraryRecordIsInvalid(),
        LibrarySyncFindingCode.RecordUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleLibraryRecordIsUnavailable(),
        LibrarySyncFindingCode.OwnershipObservation => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleOwnershipRecordIsMissing(),
        LibrarySyncFindingCode.RecordBlocked => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleLibraryRecordIsBlocked(),
        LibrarySyncFindingCode.ConsumerBlocked => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.TitleConsumerPathIsManaged(),
        LibrarySyncFindingCode.MappingBlocked => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.TitleLibraryMappingIsBlocked(),
        LibrarySyncFindingCode.MappingUnavailable => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.TitleLibraryMappingIsUnavailable(),
        LibrarySyncFindingCode.OwnershipConflict => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.TitleLibraryOwnershipConflicts(),
        LibrarySyncFindingCode.GeneratedNavigationIncomplete => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.TitleEntriesNavigationIsUnavailable(),
        LibrarySyncFindingCode.GeneratedNavigationBlocked => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.TitleEntriesNavigationIsUnsafe(),
        LibrarySyncFindingCode.LinkCapabilityUnavailable => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleFileLinksAreUnavailable(),
        LibrarySyncFindingCode.LockUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceLockIsUnavailable(),
        LibrarySyncFindingCode.RecoveryUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryDataIsUnavailable(),
        LibrarySyncFindingCode.ApplicationFailed => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.TitleLibrarySyncCouldNotWrite(),
        LibrarySyncFindingCode.VerificationFailed => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.TitleLibrarySyncVerificationFailed(),
        LibrarySyncFindingCode.RecoveryRetained => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleWasRetained(),
        LibrarySyncFindingCode.OperationFailed => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.TitleLibrarySyncFailed(),
        LibrarySyncFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.TitleLibrarySyncWasCancelled(),
        LibrarySyncFindingCode.UnknownId => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleLibraryIdIsUnknown(),
        LibrarySyncFindingCode.SourceRootInvalid => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleSourceFolderIsInvalid(),
        LibrarySyncFindingCode.SourceRootUnavailable => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleSourceFolderIsUnavailable(),
        LibrarySyncFindingCode.SourceRootBlocked => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleSourceFolderIsUnsafe(),
        LibrarySyncFindingCode.InventoryIncomplete => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleSourceInventoryIsIncomplete(),
        LibrarySyncFindingCode.DestinationCollision => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.TitleDestinationIsOccupied(),
        LibrarySyncFindingCode.RetiredLinkMissing => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.TitleRegisteredLinkIsMissing(),
        LibrarySyncFindingCode.RegisteredLinkRestored => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.TitleRegisteredLinkWasRestored(),
        LibrarySyncFindingCode.ConfirmationRequired => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleConfirmationIsRequired(),
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Library Sync finding code is not defined."),
    };

    internal static string MachineCode(LibrarySyncFindingCode code)
        => $"library-sync.{JsonNamingPolicy.KebabCaseLower.ConvertName(code.ToString())}";

    internal static string Family(LibrarySyncFindingCode code) => code switch
    {
        LibrarySyncFindingCode.InvalidInput => "invalid-input",
        LibrarySyncFindingCode.InvalidId or LibrarySyncFindingCode.RecordInvalid
            or LibrarySyncFindingCode.SourceRootInvalid or LibrarySyncFindingCode.SourceRootUnavailable
            or LibrarySyncFindingCode.SourceRootBlocked or LibrarySyncFindingCode.InventoryIncomplete
            or LibrarySyncFindingCode.MappingUnavailable or LibrarySyncFindingCode.MappingBlocked
            or LibrarySyncFindingCode.DestinationCollision or LibrarySyncFindingCode.RetiredLinkMissing
            or LibrarySyncFindingCode.RegisteredLinkRestored
            or LibrarySyncFindingCode.ConsumerBlocked => "local",
        LibrarySyncFindingCode.UnknownId => "unknown-id",
        LibrarySyncFindingCode.OwnershipObservation => "ownership-observation",
        LibrarySyncFindingCode.RecordUnavailable => "lifecycle-unavailable",
        LibrarySyncFindingCode.RecordBlocked => "lifecycle-blocked",
        LibrarySyncFindingCode.OwnershipConflict => "ownership-conflict",
        LibrarySyncFindingCode.PermissionRequired => "permission-required",
        LibrarySyncFindingCode.PermissionDeclined => "permission-declined",
        LibrarySyncFindingCode.PermissionInvalid => "permissions-invalid",
        LibrarySyncFindingCode.PermissionUnavailable => "permissions-unavailable",
        LibrarySyncFindingCode.PermissionChanged => "permissions-changed",
        LibrarySyncFindingCode.PermissionWriteFailed => "permission-write-failed",
        LibrarySyncFindingCode.GeneratedNavigationBlocked => "generated-region-unsafe",
        LibrarySyncFindingCode.GeneratedNavigationIncomplete => "projection-unavailable",
        LibrarySyncFindingCode.LinkCapabilityUnavailable => "link-capability-unavailable",
        LibrarySyncFindingCode.LockUnavailable => "workspace-lock-unavailable",
        LibrarySyncFindingCode.RecoveryUnavailable => "recovery-unavailable",
        LibrarySyncFindingCode.RecoveryRetained => "recovery-artifact-retained",
        LibrarySyncFindingCode.ApplicationFailed => "write-failed",
        LibrarySyncFindingCode.VerificationFailed => "verification-failed",
        LibrarySyncFindingCode.OperationFailed => "operation-failed",
        LibrarySyncFindingCode.Interrupted => "interrupted",
        LibrarySyncFindingCode.ConfirmationRequired => "confirmation-required",
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Library Sync finding family is not defined."),
    };

    internal static string HelpSyntax()
        => ("  " + global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.HelpSyntax());

    internal static string HelpWritePolicy()
        => ("  " + global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.HelpWritePolicy());

    internal static string HelpPermissions()
        => ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.HelpPermissions());

    internal static string HelpGlobalOptions()
        => ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.HelpGlobalOptions());

    internal static string HelpExamples()
        => ("  " + global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.HelpExamples());

    internal static string NextListReason() => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.MessageChooseARegisteredLibraryId();
    internal static string NextInspectReason() => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.MessageInspectTheDestinationLinkBeforeSynchronizingIt();
    internal static string NextDoctorReason() => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.MessageInspectTheReportedLibraryState();

    internal static string Wire(LibraryMode value)
        => EnumName(value);

    internal static string Wire(LibraryPlanState value)
        => EnumName(value);

    internal static string Wire(LibraryRecordEffect value)
        => EnumName(value);

    internal static string Wire(LibraryExpectedStateKind value)
        => EnumName(value);

    internal static string Wire(LibrarySourceRootViewState value)
        => EnumName(value);

    internal static string Wire(LibraryMutationInventoryState value)
        => EnumName(value);

    internal static string Wire(LibraryApplicationState value)
        => EnumName(value);

    internal static string Wire(LibraryVerificationState value)
        => EnumName(value);

    internal static string Wire(LibraryRecoveryState value)
        => EnumName(value);

    internal static string Wire(LibraryResidualKind value)
        => EnumName(value);

    internal static string Wire(LibraryResidualState value)
        => EnumName(value);

    internal static string Wire(LibraryRecordPublicationState value)
        => EnumName(value);

    private static string HumanRecoveryState(string value)
        => value switch
        {
            "not-requested" => "no recovery data was requested",
            "not-created" => "no recovery data was created",
            _ => HumanState(value),
        };

    private static string HumanState(string value)
        => value switch
        {
            "not-requested" => "not checked",
            "not-started" => "not started",
            _ => value.Replace('-', ' '),
        };

    internal static string Wire(LibraryExclusionKind value)
        => EnumName(value);

    private static string EnumName<T>(T value) where T : struct, Enum
        => JsonNamingPolicy.KebabCaseLower.ConvertName(value.ToString());

    private static string Plural(int count, string singular, string plural)
        => count == 1 ? singular : plural;

    private static string TrimSentence(string value)
        => CliFindingWording.PlainCause(value);
}
