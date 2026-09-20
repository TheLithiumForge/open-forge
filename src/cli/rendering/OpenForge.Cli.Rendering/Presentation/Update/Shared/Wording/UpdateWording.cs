using System.Globalization;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Presentation.Update.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.Presentation.Update.Shared.Wording;

internal static class UpdateWording
{
    internal const string HelpCommand = "open-forge update --help";
    internal const string OwnershipRecordPath = ".agents/open-forge.lock.json";

    internal static string Confirmation(UpdateConfirmationFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);
        if (facts.DeletionCount > 0)
        {
            var noun = facts.DeletionCount == 1 ? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFile() : global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFiles();
            return global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatDeleteTheListedAboveYN(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{facts.DeletionCount}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{noun}"));
        }

        return CliPromptWording.Confirm();
    }

    internal static string MachineCode<T>(T value) where T : struct, Enum
        => $"update.{OpenForge.Cli.Core.Presentation.Shared.Rendering.CliReportVocabulary.Name(value)}";

    internal static string UpToDate() => global::OpenForge.Cli.OutputText.Update.UpdateText.MessageTheFrameworkIsUpToDateNothingToDo();

    internal static string Updated(int count)
        => global::OpenForge.Cli.OutputText.Update.UpdatePhrases.FrameworkUpdated(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(count, "file", "files")}"));

    internal static string WouldUpdate(int count)
        => global::OpenForge.Cli.OutputText.Update.UpdatePhrases.FormatWouldUpdateFramework(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(count, "file", "files")}"));

    internal static string UpToDateWithKept(int count)
        => global::OpenForge.Cli.OutputText.Update.UpdatePhrases.FormatTheFrameworkIsUpToDateBut(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{KeptCount(count)}"));

    internal static string UpdatedWithKept(int updated, int kept)
        => global::OpenForge.Cli.OutputText.Update.UpdatePhrases.FrameworkUpdatedWithKept(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{updated}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(updated, "file", "files")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{KeptCount(kept)}"));

    internal static string Incomplete(string reason)
        => global::OpenForge.Cli.OutputText.Update.UpdatePhrases.FormatUpdateCouldNotStartNothingWasChanged($"{TrimSentence(reason)}");

    internal static string Blocked(string reason)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.UpdateCannotUpdate($"{TrimSentence(reason)}");

    internal static string Failed(int completed, int total)
        => global::OpenForge.Cli.OutputText.Update.UpdateWording.Failed(completed, total);

    internal static string Cancelled() => global::OpenForge.Cli.OutputText.Update.UpdateText.MessageUpdateWasCancelledNothingWasChanged();

    internal static string CancelledAfter(int completed, int total)
        => global::OpenForge.Cli.OutputText.Update.UpdateWording.CancelledAfter(completed, total);

    internal static string NoOwnership()
        => global::OpenForge.Cli.OutputText.Update.UpdateText.MessageNoOwnershipRecordExistsSoUpdateCannotTellWhichFilesItManagesNothingWasChanged();

    internal static string LifecycleMissing()
        => global::OpenForge.Cli.OutputText.Update.UpdateText.MessageNoOwnershipRecordExistsAgentsOpenForgeLockJsonIsMissingSoUpdateCannotTellWhichFilesItManages();

    internal static string OwnershipObservation()
        => global::OpenForge.Cli.OutputText.Update.UpdateText.MessageTheOwnershipRecordNamesFilesThatCannotBeInterpretedSoNothingWasChanged();

    internal static string TargetUnavailable(string path) => global::OpenForge.Cli.OutputText.Update.UpdateWording.TargetUnavailable(path);

    internal static string SourceProvenanceInvalid(string path)
        => global::OpenForge.Cli.OutputText.Update.UpdateWording.SourceProvenanceInvalid(path);

    internal static string FingerprintUnsupported(string path)
        => global::OpenForge.Cli.OutputText.Update.UpdateWording.FingerprintUnsupported(path);

    internal static string RetiredPreserved(string path)
        => global::OpenForge.Cli.OutputText.Update.UpdateWording.RetiredPreserved(path);

    internal static string RetirementIneligible(string path, string reason)
        => global::OpenForge.Cli.OutputText.Update.UpdatePhrases.FormatIsNoLongerPartOfThisReleaseButCannotBeDeletedSafely($"{path}", $"{TrimSentence(reason)}");

    internal static string PlanBlocked(string path, string reason)
        => global::OpenForge.Cli.OutputText.Update.UpdatePhrases.FormatPreventsTheUpdate($"{path}", $"{TrimSentence(reason)}");

    internal static string WriteFailed(string path, string reason)
        => global::OpenForge.Cli.OutputText.Update.UpdatePhrases.FormatCouldNotBeWritten($"{path}", $"{TrimSentence(reason)}");

    internal static string VerificationFailed(string path, string reason)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatDidNotVerifyAfterItWasWritten($"{path}", $"{TrimSentence(reason)}");

    internal static string FindingTitle<T>(T code) where T : struct, Enum
    {
        var name = OpenForge.Cli.Core.Presentation.Shared.Rendering.CliReportVocabulary.Name(code);
        return name switch
        {
            "invalid-input" => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidInput(),
            "confirmation-required" => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleConfirmationIsRequired(),
            "workspace-unavailable" => global::OpenForge.Cli.OutputText.Update.UpdateText.TitleWorkspaceUnavailable(),
            "workspace-unsafe" => global::OpenForge.Cli.OutputText.Update.UpdateText.TitleWorkspaceUnsafe(),
            "payload-unavailable" => global::OpenForge.Cli.OutputText.Update.UpdateText.TitleFrameworkUnavailable(),
            "payload-invalid" => global::OpenForge.Cli.OutputText.Update.UpdateText.TitleFrameworkInvalid(),
            "lifecycle-missing" => global::OpenForge.Cli.OutputText.Update.UpdateText.TitleOwnershipRecordMissing(),
            "lifecycle-unavailable" => global::OpenForge.Cli.OutputText.Update.UpdateText.TitleOwnershipRecordUnavailable(),
            "lifecycle-blocked" => global::OpenForge.Cli.OutputText.Update.UpdateText.TitleOwnershipRecordBlocked(),
            "ownership-observation" => global::OpenForge.Cli.OutputText.Update.UpdateText.TitleOwnershipRecordObserved(),
            "ownership-conflict" => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleOwnershipConflict(),
            "target-unavailable" => global::OpenForge.Cli.OutputText.Update.UpdateText.TitleTargetUnavailable(),
            "target-unsafe" => global::OpenForge.Cli.OutputText.Update.UpdateText.TitleTargetUnsafe(),
            "source-provenance-invalid" => global::OpenForge.Cli.OutputText.Update.UpdateText.TitleSourceProvenanceInvalid(),
            "fingerprint-unsupported" => global::OpenForge.Cli.OutputText.Update.UpdateText.TitleFingerprintUnavailable(),
            "retired-content-preserved" => global::OpenForge.Cli.OutputText.Update.UpdateText.TitleRetiredContentKept(),
            "retirement-ineligible" => global::OpenForge.Cli.OutputText.Update.UpdateText.TitleRetirementBlocked(),
            "projection-unavailable" => global::OpenForge.Cli.OutputText.Update.UpdateText.TitleEntriesProjectionUnavailable(),
            "generated-region-unsafe" => global::OpenForge.Cli.OutputText.Update.UpdateText.TitleEntriesSectionUnsafe(),
            "plan-blocked" => global::OpenForge.Cli.OutputText.Update.UpdateText.TitlePlanBlocked(),
            "recovery-conflict" => global::OpenForge.Cli.OutputText.Update.UpdateText.TitleRecoveryConflict(),
            "recovery-unavailable" => global::OpenForge.Cli.OutputText.Update.UpdateText.TitleRecoveryUnavailable(),
            "recovery-artifact-retained" => global::OpenForge.Cli.OutputText.Update.UpdateText.TitleRecoveryArtifactRetained(),
            "write-failed" => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWriteFailed(),
            "verification-failed" => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleVerificationFailed(),
            "lifecycle-publication-failed" => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleOwnershipPublicationFailed(),
            "recovery-failed" => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryFailed(),
            "operation-failed" => global::OpenForge.Cli.OutputText.Update.UpdateText.TitleUpdateFailed(),
            "interrupted" => global::OpenForge.Cli.OutputText.Update.UpdateText.TitleUpdateCancelled(),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Update finding code is not defined."),
        };
    }

    internal static string ChangedReason() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelYouHadChangedIt();
    internal static string NewReleaseReason() => global::OpenForge.Cli.OutputText.Update.UpdateText.LabelNewContentInThisRelease();
    internal static string ChangedBothReason() => global::OpenForge.Cli.OutputText.Update.UpdateText.LabelYouHadChangedItAndThisReleaseChangesIt();
    internal static string MissingReason() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelItWasMissing();
    internal static string NewReason() => global::OpenForge.Cli.OutputText.Update.UpdateText.LabelNewInThisRelease();
    internal static string RetiredReason() => global::OpenForge.Cli.OutputText.Update.UpdateText.LabelNoLongerPartOfThisRelease();
    internal static string DryChangedReason() => global::OpenForge.Cli.OutputText.Update.UpdateText.LabelYouHaveChangedItARecoveryBundleIsWrittenFirst();
    internal static string RetainedRow() => global::OpenForge.Cli.OutputText.Update.UpdateText.LabelKeptNoLongerPartOfThisRelease();
    internal static string FormatOnlyRow() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnchangedLineEndingsDiffer();
    internal static string EntriesSectionUpdated() => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionUpdated();
    internal static string LockUpdated() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUpdated();
    internal static string PreviousContent() => global::OpenForge.Cli.OutputText.Update.UpdateText.TitlePreviousContentGitDiff();
    internal static string NoFilesChanged() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoFilesWereChanged();

    internal static string UnchangedCount(int count)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatUnchanged(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(count, "file", "files")}"));

    internal static string CurrentHash(string path, string hash) => global::OpenForge.Cli.OutputText.Update.UpdateWording.CurrentHash(path, hash);
    internal static string ShippedHash(string path, string hash) => global::OpenForge.Cli.OutputText.Update.UpdateWording.ShippedHash(path, hash);
    internal static string SourceAsset(string path, string source) => global::OpenForge.Cli.OutputText.Update.UpdateWording.SourceAsset(path, source);
    internal static string SourceFacts(string path, UpdateDataSource source)
        => global::OpenForge.Cli.OutputText.Update.UpdatePhrases.FormatBundledFrameworkIdVersionFingerprint($"{path}", $"{source.Id}", $"{source.Version ?? "unavailable"}", $"{source.Fingerprint}");

    internal static string Verification(UpdateVerificationState state)
        => state switch
        {
            UpdateVerificationState.NotRequested => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageVerificationWasNotRequested(),
            UpdateVerificationState.Verified => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageAllWrittenTargetsWereVerified(),
            UpdateVerificationState.Failed => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageWrittenTargetsDidNotVerify(),
            UpdateVerificationState.Unknown => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheFinalVerificationStateIsUnknown(),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Update verification state is not defined."),
        };

    internal static string RecoveryData(string path) => global::OpenForge.Cli.OutputText.Update.UpdateWording.RecoveryData(path);

    internal static string NextConfirmationCommand(UpdateResult result)
        => BuildCommand(result, automatic: true, mode: UpdateMode.Apply);

    internal static string NextRetiredCommand() => "open-forge update --prune --dry-run";
    internal static string NextRecoveryCommand() => "open-forge cleanup";
    internal static string NextDoctorCommand() => "open-forge doctor";
    internal static string NextRetryCommand(UpdateResult result) => BuildCommand(result, result.Automatic, result.Mode);
    internal static string NextHelpReason() => global::OpenForge.Cli.OutputText.Update.UpdateText.MessageCorrectTheNamedUpdateInputThenRerunTheRequest();
    internal static string NextConfirmationReason() => global::OpenForge.Cli.OutputText.Update.UpdateText.MessageRerunTheSameUpdateRequestWithExplicitAutomaticMode();
    internal static string NextRetiredReason() => global::OpenForge.Cli.OutputText.Update.UpdateText.MessagePreviewDeletingTheRetainedFilesBeforeApplyingThePrune();
    internal static string NextRecoveryReason() => global::OpenForge.Cli.OutputText.Update.UpdateText.MessageReviewAndRemoveTheReportedRecoveryArtifactAfterConfirmingTheVerifiedUpdateResult();
    internal static string NextDoctorReason() => global::OpenForge.Cli.OutputText.Update.UpdateText.MessageInspectTheBlockedUnavailableOrIncompleteUpdateFactsBeforeRerunning();
    internal static string NextRetryReason() => global::OpenForge.Cli.OutputText.Update.UpdateText.MessageRerunTheSameUpdateRequest();

    private static string BuildCommand(UpdateResult result, bool automatic, UpdateMode mode)
    {
        var flags = new List<string>();
        if (result.Force)
        {
            flags.Add("--force");
        }

        if (result.Prune)
        {
            flags.Add("--prune");
        }

        if (automatic)
        {
            flags.Add("--automatic");
        }

        if (mode == UpdateMode.DryRun)
        {
            flags.Add("--dry-run");
        }

        return flags.Count == 0
            ? "open-forge update"
            : $"open-forge update {string.Join(' ', flags)}";
    }

    private static string TrimSentence(string value)
        => CliFindingWording.PlainCause(value);

    private static string KeptCount(int count)
        => global::OpenForge.Cli.OutputText.Update.UpdatePhrases.FormatFromAnEarlierVersionKept(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(count, "file", "files")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{(count == 1 ? "was" : "were")}"));
}
