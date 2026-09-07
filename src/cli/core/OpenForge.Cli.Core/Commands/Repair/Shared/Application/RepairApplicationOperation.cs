using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Application;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Application;

internal sealed class RepairApplicationOperation(
    RepairMutationServices mutation,
    RepairRecoveryLifecycle recoveryLifecycle,
    RepairPostVerifier postVerifier,
    RepairPlanRevalidator planRevalidator)
{
    private readonly MutationPreflight _preflight = mutation.Preflight;
    private readonly WorkspaceLockManager _lockManager = mutation.LockManager;
    private readonly MutationRevalidator _revalidator = mutation.Revalidator;
    private readonly FileChangeApplier _fileChangeApplier = mutation.Applier;
    private readonly RepairRecoveryLifecycle _recoveryLifecycle = recoveryLifecycle;
    private readonly RepairPostVerifier _postVerifier = postVerifier;
    private readonly RepairPlanRevalidator _planRevalidator = planRevalidator;

    internal async ValueTask<RepairPreflightOutcome> PreflightAsync(RepairPlan plan, CancellationToken cancellationToken)
    {
        var changes = plan.Effects.Select(effect => effect.FileChange).ToArray();
        var validation = await _preflight.ValidateAsync(plan.Request.Workspace, changes, cancellationToken).ConfigureAwait(false);
        if (validation.State != MutationValidationState.Valid)
        {
            return new RepairPreflightOutcome(RepairApplicationOutcomeFactory.ValidationBoundary(validation), []);
        }

        var targets = await new RepairTargetReader().ReadAsync(plan, cancellationToken).ConfigureAwait(false);
        return targets is null
            ? new RepairPreflightOutcome(RepairApplicationOutcomeFactory.BeforeApplication(RepairPreflightState.Blocked,
                "A selected Repair target or required fragment could not be proven.", RepairFindingCode.TargetUnsafe), [])
            : new RepairPreflightOutcome(RepairApplicationOutcomeFactory.Preview(changes.Length != 0), targets);
    }

    internal async ValueTask<RepairApplicationOutcome> ExecuteAsync(
        RepairPlan plan,
        IReadOnlyList<FileExpectation> targets,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var changes = plan.Effects.Select(effect => effect.FileChange).ToArray();
        if (changes.Length == 0)
        {
            return plan.Request.Mode == RepairMode.DryRun
                ? RepairApplicationOutcomeFactory.Preview(hasEffects: false)
                : await VerifyNoOpAsync(plan, cancellationToken).ConfigureAwait(false);
        }

        var validation = await _preflight.ValidateAsync(
            plan.Request.Workspace,
            changes,
            cancellationToken).ConfigureAwait(false);
        if (validation.State != MutationValidationState.Valid)
        {
            return RepairApplicationOutcomeFactory.ValidationBoundary(validation);
        }

        if (plan.Request.Mode == RepairMode.DryRun)
        {
            return RepairApplicationOutcomeFactory.Preview(hasEffects: true);
        }

        var operationId = Guid.NewGuid();
        var acquired = await _lockManager.AcquireAsync(
            new WorkspaceLockRequest(
                plan.Request.Workspace,
                RepairDefinitions.CommandIdentity,
                operationId),
            cancellationToken).ConfigureAwait(false);
        if (acquired.State != WorkspaceLockState.Acquired || acquired.Lease is null)
        {
            return RepairApplicationOutcomeFactory.LockBoundary(acquired);
        }

        await using var lease = acquired.Lease.ConfigureAwait(false);
        var revalidation = await _revalidator.ValidateAsync(
            acquired.Lease,
            changes,
            cancellationToken).ConfigureAwait(false);
        if (revalidation.State != MutationValidationState.Valid)
        {
            return RepairApplicationOutcomeFactory.ValidationBoundary(revalidation);
        }

        if (!await new RepairTargetReader().ValidateAsync(plan, targets, cancellationToken).ConfigureAwait(false)
            || !await _planRevalidator.ValidateAsync(plan, cancellationToken).ConfigureAwait(false))
        {
            return RepairApplicationOutcomeFactory.BeforeApplication(RepairPreflightState.Blocked,
                "The selected Repair source, target, candidate, or diagnosis changed after planning.", RepairFindingCode.TargetChanged);
        }

        var attribution = plan.Effects[0].RecoveryAttribution;
        var preparation = await _recoveryLifecycle.PrepareAsync(
            plan,
            operationId,
            cancellationToken).ConfigureAwait(false);
        if (preparation.State != RecoveryBundlePreparationState.Prepared
            || preparation.Preparation is null)
        {
            return RepairApplicationOutcomeFactory.Preparation(preparation, attribution);
        }

        return await ApplyPreparedAsync(
            new RepairPreparedApplication(plan, acquired.Lease, preparation.Preparation, []),
            revalidation, targets, cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<RepairApplicationOutcome> ApplyPreparedAsync(
        RepairPreparedApplication prepared,
        MutationValidationResult validation,
        IReadOnlyList<FileExpectation> targets,
        CancellationToken cancellationToken)
    {
        var plan = prepared.Plan;
        var lease = prepared.Lease;
        var preparation = prepared.Preparation;
        var attribution = preparation.Attribution;
        var receipts = new List<FileChangeReceipt>();
        var completion = new RepairApplicationCompletion(_postVerifier, _recoveryLifecycle);
        try
        {
            if (!await new RepairTargetReader().ValidateAsync(plan, targets, cancellationToken).ConfigureAwait(false)
                || !await _planRevalidator.ValidateAsync(plan, cancellationToken).ConfigureAwait(false))
            {
                return await completion.RetainAsync(prepared, RepairFindingCode.TargetChanged,
                    "The complete Repair plan changed during recovery preparation.").ConfigureAwait(false);
            }

            for (var index = 0; index < plan.Effects.Count; index++)
            {
                var receipt = await _fileChangeApplier.ApplyAsync(
                    lease, plan.Effects[index].FileChange, validation.Checks[index], preparation, cancellationToken)
                    .ConfigureAwait(false);
                receipts.Add(receipt);
                if (receipt.EffectState != FilesystemEffectState.Applied
                    || receipt.VerificationState != FilesystemVerificationState.Verified)
                {
                    var boundary = RepairApplicationOutcomeFactory.AppliedBoundary(receipts, preparation, attribution, receipt);
                    var retained = await completion.RetainAsync(new RepairPreparedApplication(plan, lease, preparation, receipts),
                        boundary.Findings[0].Code, boundary.Findings[0].Cause).ConfigureAwait(false);
                    return retained with { Application = boundary.Application };
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return await completion.RetainAsync(new RepairPreparedApplication(plan, lease, preparation, receipts),
                RepairFindingCode.Interrupted, "Repair application was interrupted.").ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            return await completion.RetainAsync(new RepairPreparedApplication(plan, lease, preparation, receipts),
                RepairFindingCode.WriteFailed, $"Repair application failed: {exception.GetType().Name}.").ConfigureAwait(false);
        }

        return await completion.CompleteAsync(new RepairPreparedApplication(plan, lease, preparation, receipts), cancellationToken)
            .ConfigureAwait(false);
    }

    private async ValueTask<RepairApplicationOutcome> VerifyNoOpAsync(
        RepairPlan plan,
        CancellationToken cancellationToken)
    {
        var verified = await _postVerifier.VerifyAsync(plan, [], cancellationToken)
            .ConfigureAwait(false);
        return new RepairApplicationOutcome(
            RepairApplicationOutcomeFactory.Ready(RepairRecoveryState.NotRequired),
            RepairApplication.NotRequested,
            verified.Verification,
            RepairRecovery.NotRequired,
            verified.Diagnosis,
            []);
    }

}
