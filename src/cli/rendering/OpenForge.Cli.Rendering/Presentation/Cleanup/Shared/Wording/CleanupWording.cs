using System.Globalization;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Cleanup.Shared.Wording;

internal static class CleanupWording
{
    internal static string NoData() => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageNoRecoveryDataToRemove();

    internal static string RemovedSummary(int bundles, int drafts, bool preview)
    {
        var verb = preview ? global::OpenForge.Cli.OutputText.Cleanup.CleanupText.TitleWouldRemove() : global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRemoved();
        if (bundles == 0 && drafts == 0)
        {
            return NoData();
        }

        if (bundles == 0)
        {
            return string.Create(
                CultureInfo.InvariantCulture,
                $"{verb} {drafts} {Plural(drafts, "unfinished draft")}.");
        }

        if (drafts == 0)
        {
            return string.Create(
                CultureInfo.InvariantCulture,
                $"{verb} {bundles} {Plural(bundles, "recovery bundle")}.");
        }

        return global::OpenForge.Cli.OutputText.Cleanup.CleanupPhrases.FormatAnd(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{verb}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{bundles}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{Plural(bundles, "recovery bundle")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{drafts}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{Plural(drafts, "unfinished draft")}"));
    }

    internal static string StoreIncomplete()
        => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageTheRecoveryStoreCouldNotBeReadCompletelyNothingWasRemoved();

    internal static string Failed(int removed, int total)
        => global::OpenForge.Cli.OutputText.Cleanup.CleanupWording.Failed(removed, total);

    internal static string Cancelled(int removed, int total)
        => removed == 0
            ? global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageCleanupWasCancelledNothingWasRemoved()
            : global::OpenForge.Cli.OutputText.Cleanup.CleanupPhrases.FormatCleanupWasCancelledAfterRemovingOfItems(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{removed}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{total}"));

    internal static string InvalidInput(string cause)
        => CliFindingWording.InvalidInput(global::OpenForge.Cli.OutputText.Cleanup.CleanupText.LabelCleanUp(), TrimSentence(cause));

    internal static string LockHeld()
        => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageCannotCleanUpAnotherOpenForgeCommandHoldsTheWorkspaceLockNothingWasRemoved();

    internal static string ChangedDuringApply()
        => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageCannotCleanUpTheRecoveryStoreChangedWhileCleanupWasRunningNothingWasRemoved();

    internal static string BlockedCandidate(string path, Enum integrity)
        => EnumName(integrity) switch
        {
            "malformed"
                => global::OpenForge.Cli.OutputText.Cleanup.CleanupPhrases.FormatIsDamagedAndWasLeftInPlaceNothingWasRemoved($"{path}"),
            "unsupported"
                => global::OpenForge.Cli.OutputText.Cleanup.CleanupPhrases.FormatIsFromAnUnsupportedVersionAndWasLeftInPlaceNothingWasRemoved($"{path}"),
            "unavailable"
                => global::OpenForge.Cli.OutputText.Cleanup.CleanupPhrases.FormatIsUnreadableAndWasLeftInPlaceNothingWasRemoved($"{path}"),
            "incomplete"
                => global::OpenForge.Cli.OutputText.Cleanup.CleanupPhrases.FormatIsNotAnOrdinaryFileAndWasLeftInPlaceNothingWasRemoved($"{path}"),
            "verified"
                => global::OpenForge.Cli.OutputText.Cleanup.CleanupPhrases.FormatCouldNotBeRemovedSafelyNothingWasRemoved($"{path}"),
            _ => throw new ArgumentOutOfRangeException(nameof(integrity), integrity, "The recovery integrity is not defined."),
        };

    internal static string EffectWording(CleanupEffect effect, bool verificationFailed)
    {
        ArgumentNullException.ThrowIfNull(effect);
        return effect.Outcome switch
        {
            CleanupEffectOutcome.Planned => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.LabelWouldBeRemoved(),
            CleanupEffectOutcome.Verified => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelRemoved(),
            CleanupEffectOutcome.NotStarted => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
            CleanupEffectOutcome.VerificationFailed when verificationFailed
                => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.LabelStillExistsAfterRemoval(),
            CleanupEffectOutcome.VerificationFailed when effect.Cause is { } cause
                => global::OpenForge.Cli.OutputText.Cleanup.CleanupPhrases.RemovalFailureReason($"{TrimSentence(cause)}"),
            CleanupEffectOutcome.VerificationFailed => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.LabelCouldNotBeRemoved(),
            CleanupEffectOutcome.CompletionUnknown when effect.Cause is { } cause
                && !cause.StartsWith("No deletion was attempted", StringComparison.Ordinal)
                => global::OpenForge.Cli.OutputText.Cleanup.CleanupPhrases.RemovalFailureReason($"{TrimSentence(cause)}"),
            CleanupEffectOutcome.CompletionUnknown => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
            _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Outcome, "The cleanup effect outcome is not defined."),
        };
    }

    internal static string EffectMetadata(CleanupEffect effect)
    {
        ArgumentNullException.ThrowIfNull(effect);
        var kind = DataKind(effect.KindValue) == "bundle" ? "bundle" : global::OpenForge.Cli.OutputText.Cleanup.CleanupText.LabelUnfinishedDraft();
        var origin = effect.Provenance?.Command is { } command
            ? global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatFrom($"{Origin(command)}")
            : string.Empty;
        return $"{kind}{origin}, {Integrity(effect.IntegrityValue)}";
    }

    internal static string IntegrityCheck(CleanupEffect effect)
        => global::OpenForge.Cli.OutputText.Cleanup.CleanupPhrases.FormatIntegrityCheck($"{Integrity(effect.IntegrityValue)}");

    internal static string CandidateRowReason(CleanupCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        return global::OpenForge.Cli.OutputText.Cleanup.CleanupText.LabelNotRecognized();
    }

    internal static bool IsPreserved(Enum action) => EnumName(action) == "preserve";

    internal static bool IsExplicitWorkspace(Enum selection) => EnumName(selection) == "explicit-workspace";

    internal static string CandidateIntegrityCheck(CleanupCandidate candidate)
        => global::OpenForge.Cli.OutputText.Cleanup.CleanupPhrases.FormatIntegrityCheck($"{Integrity(candidate.IntegrityValue)}");

    internal static string DataKind(Enum kind)
        => EnumName(kind) switch
        {
            "final" => "bundle",
            "draft" => "draft",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The recovery candidate kind is not defined."),
        };

    internal static string DataOutcome(CleanupEffect effect)
    {
        ArgumentNullException.ThrowIfNull(effect);
        return effect.Outcome switch
        {
            CleanupEffectOutcome.Planned => "would-be-removed",
            CleanupEffectOutcome.NotStarted => "not-started",
            CleanupEffectOutcome.Verified => "removed",
            CleanupEffectOutcome.VerificationFailed => "could-not-be-removed",
            CleanupEffectOutcome.CompletionUnknown when effect.Cause is { } cause
                && cause.StartsWith("No deletion was attempted", StringComparison.Ordinal)
                => "not-started",
            CleanupEffectOutcome.CompletionUnknown => "final-state-unknown",
            _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Outcome, "The cleanup effect outcome is not defined."),
        };
    }

    internal static string? Origin(CleanupEffect effect)
        => effect.Provenance?.Command;

    internal static string Integrity(Enum integrity) => EnumName(integrity);

    internal static string Origin(string command)
        => command.StartsWith("open-forge ", StringComparison.Ordinal)
            ? command["open-forge ".Length..]
            : command;

    internal static string FindingCode(CleanupFindingCode code)
        => code switch
        {
            CleanupFindingCode.InvalidInput => "cleanup.invalid-input",
            CleanupFindingCode.WorkspaceUnavailable => "cleanup.workspace-unavailable",
            CleanupFindingCode.WorkspaceNotDirectory => "cleanup.workspace-not-directory",
            CleanupFindingCode.WorkspaceUnsafe => "cleanup.workspace-unsafe",
            CleanupFindingCode.CatalogueIncomplete => "cleanup.catalogue-incomplete",
            CleanupFindingCode.RecoveryFinalMalformed => "cleanup.recovery-final-malformed",
            CleanupFindingCode.RecoveryFinalUnsupported => "cleanup.recovery-final-unsupported",
            CleanupFindingCode.RecoveryFinalUnavailable => "cleanup.recovery-final-unavailable",
            CleanupFindingCode.RecoveryDraftUnsafe => "cleanup.recovery-draft-unsafe",
            CleanupFindingCode.WorkspaceLockUnavailable => "cleanup.workspace-lock-unavailable",
            CleanupFindingCode.CatalogueChangedDuringApply => "cleanup.catalogue-changed-during-apply",
            CleanupFindingCode.CandidateChangedDuringApply => "cleanup.candidate-changed-during-apply",
            CleanupFindingCode.DeletionFailed => "cleanup.deletion-failed",
            CleanupFindingCode.VerificationFailed => "cleanup.verification-failed",
            CleanupFindingCode.OperationFailed => "cleanup.operation-failed",
            CleanupFindingCode.Interrupted => "cleanup.interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The cleanup finding code is not defined."),
        };

    internal static string FindingTitle(CleanupFindingCode code)
        => code switch
        {
            CleanupFindingCode.InvalidInput => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidInput(),
            CleanupFindingCode.WorkspaceUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnavailable(),
            CleanupFindingCode.WorkspaceNotDirectory => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsNotADirectory(),
            CleanupFindingCode.WorkspaceUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnsafe(),
            CleanupFindingCode.CatalogueIncomplete => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.TitleRecoveryStoreIsIncomplete(),
            CleanupFindingCode.RecoveryFinalMalformed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleIsDamaged(),
            CleanupFindingCode.RecoveryFinalUnsupported => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.TitleRecoveryBundleVersionIsUnsupported(),
            CleanupFindingCode.RecoveryFinalUnavailable => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.TitleRecoveryBundleIsUnreadable(),
            CleanupFindingCode.RecoveryDraftUnsafe => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.TitleRecoveryDraftIsUnsafe(),
            CleanupFindingCode.WorkspaceLockUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceLockIsUnavailable(),
            CleanupFindingCode.CatalogueChangedDuringApply => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.TitleRecoveryStoreChanged(),
            CleanupFindingCode.CandidateChangedDuringApply => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.TitleRecoveryItemChanged(),
            CleanupFindingCode.DeletionFailed => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.TitleRecoveryItemCouldNotBeRemoved(),
            CleanupFindingCode.VerificationFailed => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.TitleRecoveryItemRemains(),
            CleanupFindingCode.OperationFailed => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.TitleCleanupFailed(),
            CleanupFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.TitleCleanupWasCancelled(),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The cleanup finding code is not defined."),
        };

    internal static string CatalogueIncomplete(string cause)
    {
        const string marker = "non-directory: ";
        var markerIndex = cause.IndexOf(marker, StringComparison.Ordinal);
        return markerIndex >= 0
            ? global::OpenForge.Cli.OutputText.Cleanup.CleanupPhrases.FormatTheRecoveryStoreAtCouldNotBeReadCompletely($"{TrimSentence(cause[(markerIndex + marker.Length)..])}")
            : global::OpenForge.Cli.OutputText.Cleanup.CleanupPhrases.FormatTheRecoveryStoreCouldNotBeReadCompletely($"{TrimSentence(cause)}");
    }

    internal static string FindingMessage(
        CleanupFinding finding,
        string path,
        int removed,
        int total)
        => finding.Code switch
        {
            CleanupFindingCode.InvalidInput => InvalidInput(finding.Cause),
            CleanupFindingCode.WorkspaceUnavailable => CliFindingWording.WorkspaceUnavailable(path),
            CleanupFindingCode.WorkspaceNotDirectory => CliFindingWording.WorkspaceNotDirectory(path),
            CleanupFindingCode.WorkspaceUnsafe => CliFindingWording.WorkspaceUnsafe(path, TrimSentence(finding.Cause)),
            CleanupFindingCode.CatalogueIncomplete => CatalogueIncomplete(finding.Cause),
            CleanupFindingCode.RecoveryFinalMalformed => global::OpenForge.Cli.OutputText.Cleanup.CleanupPhrases.FormatIsDamagedAndWasLeftInPlace($"{path}"),
            CleanupFindingCode.RecoveryFinalUnsupported => global::OpenForge.Cli.OutputText.Cleanup.CleanupPhrases.FormatWasWrittenByAnUnsupportedVersionAndWasLeftInPlace($"{path}"),
            CleanupFindingCode.RecoveryFinalUnavailable => global::OpenForge.Cli.OutputText.Cleanup.CleanupPhrases.FormatCouldNotBeReadAndWasLeftInPlace($"{path}"),
            CleanupFindingCode.RecoveryDraftUnsafe => global::OpenForge.Cli.OutputText.Cleanup.CleanupPhrases.FormatIsNotAnOrdinaryFileAndWasLeftInPlace($"{path}"),
            CleanupFindingCode.WorkspaceLockUnavailable => LockHeld(),
            CleanupFindingCode.CatalogueChangedDuringApply => ChangedDuringApply(),
            CleanupFindingCode.CandidateChangedDuringApply => global::OpenForge.Cli.OutputText.Cleanup.CleanupPhrases.FormatChangedWhileCleanupWasRunningAndWasLeftInPlace($"{path}"),
            CleanupFindingCode.DeletionFailed => global::OpenForge.Cli.OutputText.Cleanup.CleanupPhrases.RemovalFailedPath($"{path}", $"{TrimSentence(finding.Cause)}"),
            CleanupFindingCode.VerificationFailed => global::OpenForge.Cli.OutputText.Cleanup.CleanupPhrases.FormatStillExistsAfterRemoval($"{path}"),
            CleanupFindingCode.OperationFailed => CliFindingWording.OperationFailed(global::OpenForge.Cli.OutputText.Cleanup.CleanupText.TitleCleanup(), TrimSentence(finding.Cause)),
            CleanupFindingCode.Interrupted => Cancelled(removed, total),
            _ => throw new ArgumentOutOfRangeException(nameof(finding), finding.Code, "The cleanup finding code is not defined."),
        };

    internal static string LockFacts(CleanupLeaseState state)
        => state switch
        {
            CleanupLeaseState.NotRequested => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageNoWorkspaceLockWasNeeded(),
            CleanupLeaseState.Acquired => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageTheWorkspaceLockWasAcquiredBeforeRemoval(),
            CleanupLeaseState.Failed => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageTheWorkspaceLockCouldNotBeAcquiredSoNothingWasRemoved(),
            CleanupLeaseState.Cancelled => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageCleanupWasCancelledBeforeTheWorkspaceLockWasAcquired(),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The cleanup lock state is not defined."),
        };

    internal static string FinalCheckFacts(CleanupCatalogueComparisonState state)
        => state switch
        {
            CleanupCatalogueComparisonState.NotRequested => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageNoFinalRecoveryStoreCheckWasNeeded(),
            CleanupCatalogueComparisonState.Matched => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageTheRecoveryStoreMatchedThePlannedItemsUnderTheWorkspaceLock(),
            CleanupCatalogueComparisonState.Changed => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageTheRecoveryStoreDidNotMatchThePlannedItemsUnderTheWorkspaceLock(),
            CleanupCatalogueComparisonState.Incomplete => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageTheRecoveryStoreCouldNotBeCheckedCompletelyUnderTheWorkspaceLock(),
            CleanupCatalogueComparisonState.Blocked => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageTheRecoveryStoreCouldNotBeCheckedSafelyUnderTheWorkspaceLock(),
            CleanupCatalogueComparisonState.Cancelled => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageCleanupWasCancelledWhileCheckingTheRecoveryStoreUnderTheWorkspaceLock(),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The cleanup catalogue comparison state is not defined."),
        };

    internal static string HelpSyntax() => ("  " + global::OpenForge.Cli.OutputText.Cleanup.CleanupText.HelpSyntax());

    internal static string HelpCatalogue() => ("  " + global::OpenForge.Cli.OutputText.Cleanup.CleanupText.HelpCatalogue());

    internal static string HelpWritePolicy() => ("  " + global::OpenForge.Cli.OutputText.Cleanup.CleanupText.HelpWritePolicy());

    internal static string HelpGlobalOptions() => ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.HelpGlobalOptions());

    internal static string HelpNotes() => ("  " + global::OpenForge.Cli.OutputText.Cleanup.CleanupText.HelpNotes());

    private static string Plural(int count, string singular)
        => count == 1 ? singular : singular switch
        {
            "recovery bundle" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelRecoveryBundles(),
            "unfinished draft" => global::OpenForge.Cli.OutputText.Cleanup.CleanupText.LabelUnfinishedDrafts(),
            _ => singular + "s",
        };

    private static string TrimSentence(string value) => CliFindingWording.PlainCause(value);

    private static string EnumName(Enum value)
    {
        if (!Enum.IsDefined(value.GetType(), value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "The Cleanup enum value is not defined.");
        }

        return JsonNamingPolicy.KebabCaseLower.ConvertName(value.ToString());
    }
}
