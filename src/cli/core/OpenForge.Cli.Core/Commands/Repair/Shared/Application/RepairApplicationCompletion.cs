using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Commands.Repair.Models.Application;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Application;

internal sealed class RepairApplicationCompletion(
    RepairPostVerifier postVerifier,
    RepairRecoveryLifecycle recoveryLifecycle)
{
    private readonly RepairPostVerifier _postVerifier = postVerifier;
    private readonly RepairRecoveryLifecycle _recoveryLifecycle = recoveryLifecycle;

    internal async ValueTask<RepairApplicationOutcome> CompleteAsync(
        RepairPreparedApplication application,
        CancellationToken cancellationToken)
    {
        try
        {
            return await CompleteCoreAsync(application, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return await RetainAsync(application, RepairFindingCode.Interrupted, "Repair completion was interrupted.")
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            return await RetainAsync(application, RepairFindingCode.OperationFailed,
                $"Repair completion failed: {exception.GetType().Name}.").ConfigureAwait(false);
        }
    }

    internal async ValueTask<RepairApplicationOutcome> RetainAsync(
        RepairPreparedApplication application, RepairFindingCode code, string cause)
    {
        RepairPostVerification verified;
        try
        {
            verified = await _postVerifier.VerifyRetainedAsync(application, CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            verified = RepairPostVerifier.Unavailable(application.Plan, application.Receipts);
        }

        var state = code == RepairFindingCode.Interrupted ? RepairApplicationState.Interrupted : RepairApplicationState.Failed;
        var appliedCount = application.Receipts.Count(receipt => receipt.EffectState == FilesystemEffectState.Applied);
        if (code == RepairFindingCode.TargetChanged && appliedCount == 0)
        {
            state = RepairApplicationState.NotStarted;
        }

        return new RepairApplicationOutcome(
            RepairApplicationOutcomeFactory.Ready(RepairRecoveryState.Prepared),
            new RepairApplication(state,
                application.Receipts.Count(receipt => receipt.EffectState == FilesystemEffectState.Applied),
                state == RepairApplicationState.NotStarted ? null : cause),
            verified.Verification,
            RepairApplicationOutcomeFactory.Retained(application.Preparation, application.Preparation.Attribution),
            verified.Diagnosis,
            [new RepairFinding(code, cause)]);
    }

    private async ValueTask<RepairApplicationOutcome> CompleteCoreAsync(
        RepairPreparedApplication application, CancellationToken cancellationToken)
    {
        var plan = application.Plan;
        var receipts = application.Receipts;
        var lease = application.Lease;
        var preparation = application.Preparation;
        var attribution = preparation.Attribution;
        var verified = await _postVerifier.VerifyAsync(plan, receipts, cancellationToken)
            .ConfigureAwait(false);
        if (verified.Verification.Targets != RepairVerificationState.Verified
            || verified.Verification.ResultingBytes != RepairVerificationState.Verified
            || verified.Verification.PostConditions != RepairVerificationState.Verified)
        {
            return new RepairApplicationOutcome(
                RepairApplicationOutcomeFactory.Ready(RepairRecoveryState.Prepared),
                RepairApplicationOutcomeFactory.Applied(receipts.Count),
                verified.Verification,
                RepairApplicationOutcomeFactory.Retained(preparation, attribution),
                verified.Diagnosis,
                []);
        }

        RecoveryBundleDeletionResult deletion;
        try
        {
            deletion = await _recoveryLifecycle.DeleteAsync(lease, preparation, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            return new RepairApplicationOutcome(
                RepairApplicationOutcomeFactory.Ready(RepairRecoveryState.Prepared),
                RepairApplicationOutcomeFactory.Applied(receipts.Count),
                verified.Verification,
                new RepairRecovery(RepairRecoveryState.Unknown, RepairResidualState.Unknown, preparation.BundlePath, attribution),
                verified.Diagnosis,
                [new RepairFinding(RepairFindingCode.RecoveryFailed, "Recovery deletion did not establish its final disposition.")]);
        }

        var recovery = RepairRecoveryLifecycle.ReadDeletion(deletion, attribution);
        RepairFinding[] findings = [];
        if (deletion.Disposition != RecoveryBundleDisposition.Removed)
        {
            var findingCode = deletion.Disposition == RecoveryBundleDisposition.Retained
                ? RepairFindingCode.RecoveryArtifactRetained
                : RepairFindingCode.RecoveryFailed;
            findings =
            [
                new RepairFinding(
                    findingCode,
                    deletion.Cause ?? "The Repair recovery final could not be removed."),
            ];
        }
        try
        {
            verified = await _postVerifier.VerifyAsync(plan, receipts, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            return new RepairApplicationOutcome(
                RepairApplicationOutcomeFactory.Ready(RepairRecoveryState.Prepared),
                RepairApplicationOutcomeFactory.Applied(receipts.Count),
                RepairPostVerifier.Unavailable(plan, receipts).Verification,
                recovery, RepairPostDiagnosis.NotRequested,
                [
                    .. findings,
                    new RepairFinding(RepairFindingCode.OperationFailed,
                            "Fresh diagnosis after recovery disposition did not complete."),
                ]);
        }

        return new RepairApplicationOutcome(
            RepairApplicationOutcomeFactory.Ready(RepairRecoveryState.Prepared),
            RepairApplicationOutcomeFactory.Applied(receipts.Count),
            verified.Verification,
            recovery,
            verified.Diagnosis,
            findings);
    }
}
