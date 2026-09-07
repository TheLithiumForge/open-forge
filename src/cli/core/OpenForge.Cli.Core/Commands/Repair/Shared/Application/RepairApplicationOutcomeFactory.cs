using OpenForge.Cli.Core.Commands.Repair.Models.Application;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Application;

internal static class RepairApplicationOutcomeFactory
{
    internal static RepairApplicationOutcome Preview(bool hasEffects)
        => new(
            Ready(hasEffects ? RepairRecoveryState.NotCreated : RepairRecoveryState.NotRequired),
            RepairApplication.NotRequested,
            RepairVerification.NotRequested,
            hasEffects ? NotCreated() : RepairRecovery.NotRequired,
            RepairPostDiagnosis.NotRequested,
            []);

    internal static RepairApplicationOutcome ValidationBoundary(MutationValidationResult result)
    {
        if (result.State == MutationValidationState.Cancelled)
        {
            return InterruptedBeforeApplication(result.Cause ?? "Repair validation was interrupted.");
        }

        return BeforeApplication(
            result.State == MutationValidationState.Failed
                ? RepairPreflightState.Incomplete
                : RepairPreflightState.Blocked,
            result.Cause ?? "The Repair expected file state could not be validated.",
            RepairFindingCode.TargetChanged);
    }

    internal static RepairApplicationOutcome LockBoundary(WorkspaceLockResult result)
        => result.State == WorkspaceLockState.Cancelled
            ? InterruptedBeforeApplication(result.Cause ?? "Repair lock acquisition was interrupted.")
            : BeforeApplication(
            RepairPreflightState.Blocked,
            result.Cause ?? "The Repair workspace lock could not be acquired.",
            RepairFindingCode.WorkspaceLockUnavailable);

    private static RepairApplicationOutcome InterruptedBeforeApplication(string cause)
        => new(
            new RepairPreflight(RepairPreflightState.NotRequested, cause, RepairRecoveryState.NotCreated),
            new RepairApplication(RepairApplicationState.Interrupted, appliedEffects: 0, cause),
            RepairVerification.NotRequested,
            NotCreated(),
            RepairPostDiagnosis.NotRequested,
            [new RepairFinding(RepairFindingCode.Interrupted, cause)]);

    internal static RepairApplicationOutcome Preparation(
        RecoveryBundlePreparationResult result,
        RecoveryBundleAttribution attribution)
    {
        var findingCode = result.State switch
        {
            RecoveryBundlePreparationState.Blocked => RepairFindingCode.RecoveryConflict,
            RecoveryBundlePreparationState.Cancelled => RepairFindingCode.Interrupted,
            RecoveryBundlePreparationState.Incomplete or RecoveryBundlePreparationState.NotNeeded
                or RecoveryBundlePreparationState.Prepared => RepairFindingCode.RecoveryUnavailable,
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.State, "The preparation state is not defined."),
        };
        var state = result.State == RecoveryBundlePreparationState.Blocked
            ? RepairPreflightState.Blocked
            : RepairPreflightState.Incomplete;
        return new RepairApplicationOutcome(
            new RepairPreflight(
                state,
                result.Cause ?? "Repair recovery preparation did not complete.",
                RepairRecoveryLifecycle.ReadPreparationFailure(result, attribution).State),
            result.State == RecoveryBundlePreparationState.Cancelled
                ? new RepairApplication(
                    RepairApplicationState.Interrupted,
                    appliedEffects: 0,
                    result.Cause ?? "Repair recovery preparation was interrupted.")
                : RepairApplication.NotRequested,
            RepairVerification.NotRequested,
            RepairRecoveryLifecycle.ReadPreparationFailure(result, attribution),
            RepairPostDiagnosis.NotRequested,
            [new RepairFinding(findingCode, result.Cause ?? "Repair recovery preparation did not complete.")]);
    }

    internal static RepairApplicationOutcome AppliedBoundary(
        IReadOnlyList<FileChangeReceipt> receipts,
        RecoveryBundlePreparation preparation,
        RecoveryBundleAttribution attribution,
        FileChangeReceipt receipt)
    {
        var applied = receipts.Count(value => value.EffectState == FilesystemEffectState.Applied);
        var cancelled = receipt.NotStartedReason == FilesystemNotStartedReason.Cancelled;
        var changed = receipt.NotStartedReason == FilesystemNotStartedReason.TargetChanged;
        var code = RepairFindingCode.WriteFailed;
        var state = RepairApplicationState.Failed;
        if (cancelled)
        {
            code = RepairFindingCode.Interrupted;
            state = RepairApplicationState.Interrupted;
        }
        else if (changed)
        {
            code = RepairFindingCode.TargetChanged;
            state = RepairApplicationState.NotStarted;
        }
        else if (receipt.EffectState == FilesystemEffectState.Applied)
        {
            code = RepairFindingCode.VerificationFailed;
        }
        return new RepairApplicationOutcome(
            Ready(RepairRecoveryState.Prepared),
            new RepairApplication(
                state,
                applied,
                state is RepairApplicationState.Failed or RepairApplicationState.Interrupted
                    ? receipt.Cause ?? "Repair application did not complete."
                    : null),
            new RepairVerification(
                RepairVerificationState.Unknown,
                receipt.VerificationState == FilesystemVerificationState.Failed
                    ? RepairVerificationState.Failed
                    : RepairVerificationState.Unknown,
                RepairVerificationState.Unknown),
            Retained(preparation, attribution),
            RepairPostDiagnosis.NotRequested,
            [new RepairFinding(code, receipt.Cause ?? "Repair application did not complete.")]);
    }

    internal static RepairApplicationOutcome BeforeApplication(
        RepairPreflightState state,
        string cause,
        RepairFindingCode code)
        => new(
            new RepairPreflight(state, cause, RepairRecoveryState.NotCreated),
            RepairApplication.NotRequested,
            RepairVerification.NotRequested,
            NotCreated(),
            RepairPostDiagnosis.NotRequested,
            [new RepairFinding(code, cause)]);

    internal static RepairPreflight Ready(RepairRecoveryState recovery)
        => new(RepairPreflightState.Ready, cause: null, recovery);

    internal static RepairApplication Applied(int count)
        => new(RepairApplicationState.Applied, count, cause: null);

    internal static RepairRecovery Retained(
        RecoveryBundlePreparation preparation,
        RecoveryBundleAttribution attribution)
        => new(
            RepairRecoveryState.Retained,
            RepairResidualState.Retained,
            preparation.BundlePath,
            attribution);

    private static RepairRecovery NotCreated()
        => new(
            RepairRecoveryState.NotCreated,
            RepairResidualState.None,
            residualPath: null,
            attribution: null);
}
