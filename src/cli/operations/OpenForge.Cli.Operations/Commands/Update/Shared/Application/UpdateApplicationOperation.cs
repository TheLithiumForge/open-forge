using OpenForge.Cli.Core.Commands.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Update.Shared.Recovery;
using OpenForge.Cli.Core.Commands.Update.Shared.Result;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Application;

internal sealed class UpdateApplicationOperation(
    WorkspaceLockManager lockManager,
    UpdatePlanRevalidator revalidator,
    UpdateEffectApplication effectApplication,
    UpdateAppliedVerifier verifier)
{
    private readonly WorkspaceLockManager _lockManager = lockManager;
    private readonly UpdatePlanRevalidator _revalidator = revalidator;
    private readonly UpdateEffectApplication _effectApplication = effectApplication;
    private readonly UpdateAppliedVerifier _verifier = verifier;

    internal async ValueTask<UpdateResult> ExecuteAsync(
        UpdatePlanExecution execution,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(execution);
        var operationId = Guid.NewGuid();
        WorkspaceLockResult lockResult;
        try
        {
            lockResult = await _lockManager
                .AcquireAsync(
                    new WorkspaceLockRequest(
                        execution.Request.Workspace,
                        UpdateDefinitions.CommandIdentity,
                        operationId),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return UpdateResultBuilder.BeforeEffects(
                execution,
                Finding(UpdateFindingCode.Interrupted, "Update lock acquisition was interrupted."));
        }
        catch (Exception)
        {
            return UpdateResultBuilder.BeforeEffects(
                execution,
                Finding(UpdateFindingCode.OperationFailed, "Update lock acquisition failed unexpectedly."));
        }

        if (lockResult.State != WorkspaceLockState.Acquired
            || lockResult.Lease is not { } lease)
        {
            return UpdateResultBuilder.BeforeEffects(
                execution,
                Finding(
                    lockResult.State == WorkspaceLockState.Cancelled
                        ? UpdateFindingCode.Interrupted
                        : UpdateFindingCode.WorkspaceUnsafe,
                    lockResult.Cause
                        ?? "The persistent Update workspace lease could not be acquired."));
        }

        await using (lease.ConfigureAwait(false))
        {
            return await ExecuteUnderLeaseAsync(
                    execution,
                    lease,
                    operationId,
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async ValueTask<UpdateResult> ExecuteUnderLeaseAsync(
        UpdatePlanExecution execution,
        WorkspaceLockLease lease,
        Guid operationId,
        CancellationToken cancellationToken)
    {
        var preflight = await _revalidator
            .RevalidateAsync(execution, lease, cancellationToken)
            .ConfigureAwait(false);
        if (preflight.State != UpdatePlanRevalidationState.Exact)
        {
            if (preflight.State == UpdatePlanRevalidationState.Changed)
            {
                return UpdateResultBuilder.ChangedPlan(
                    execution,
                    preflight.Cause
                        ?? "The complete Update plan changed before application.");
            }

            var code = preflight.State switch
            {
                UpdatePlanRevalidationState.Interrupted => UpdateFindingCode.Interrupted,
                UpdatePlanRevalidationState.Failed => UpdateFindingCode.OperationFailed,
                UpdatePlanRevalidationState.Exact => throw new InvalidOperationException(
                    "An exact Update preflight is not a boundary."),
                UpdatePlanRevalidationState.Changed => throw new InvalidOperationException(
                    "A changed Update preflight is handled before finite boundary mapping."),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(execution),
                    preflight.State,
                    "The Update revalidation state is not defined."),
            };
            return UpdateResultBuilder.BeforeEffects(
                execution,
                Finding(code, preflight.Cause ?? "The complete Update plan could not be revalidated."));
        }

        RecoveryBundlePreparationResult preparation;
        try
        {
            preparation = await UpdateRecoveryOperation
                .PrepareAsync(execution, operationId, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return UpdateResultBuilder.BeforeEffects(
                execution,
                Finding(UpdateFindingCode.Interrupted, "Update recovery preparation was interrupted."));
        }
        catch (Exception)
        {
            return UpdateResultBuilder.BeforeEffects(
                execution,
                Finding(UpdateFindingCode.OperationFailed, "Update recovery preparation failed unexpectedly."));
        }

        if (preparation.State is not (
            RecoveryBundlePreparationState.Prepared
            or RecoveryBundlePreparationState.NotNeeded))
        {
            return PreparationBoundary(execution, preparation);
        }

        var attempt = await _effectApplication
            .ApplyAsync(
                execution,
                lease,
                preflight.Checks,
                preparation.Preparation,
                cancellationToken)
            .ConfigureAwait(false);
        if (attempt.Finding is { } applicationFinding)
        {
            return UpdateResultBuilder.Create(
                execution,
                UpdateApplicationResultProjector.Effects(execution, attempt),
                UpdateApplicationResultProjector.Lifecycle(execution, attempt),
                RetainedRecovery(execution, preparation),
                applicationFinding.Code == UpdateFindingCode.VerificationFailed
                    || applicationFinding.Code == UpdateFindingCode.LifecyclePublicationFailed
                    ? UpdateVerificationState.Failed
                    : UpdateVerificationState.Unknown,
                [applicationFinding]);
        }

        var verification = await _verifier
            .VerifyAsync(execution, lease, cancellationToken)
            .ConfigureAwait(false);
        if (verification.Finding is { } verificationFinding)
        {
            return UpdateResultBuilder.Create(
                execution,
                UpdateApplicationResultProjector.Effects(execution, attempt),
                UpdateApplicationResultProjector.Lifecycle(execution, attempt),
                RetainedRecovery(execution, preparation),
                verification.State,
                [verificationFinding]);
        }

        var cleanup = preparation.Preparation is { } prepared
            ? await UpdateRecoveryOperation
                .VerifyRetainedAsync(
                    lease,
                    prepared,
                    UpdateResultBuilder.ProtectedPaths(execution),
                    cancellationToken)
                .ConfigureAwait(false)
            : new UpdateRecoveryCleanup(
                new UpdateRecovery
                {
                    State = UpdateRecoveryState.NotRequired,
                    ProtectedPaths = [],
                    ResidualPath = null,
                },
                Finding: null);
        return UpdateResultBuilder.Create(
            execution,
            UpdateApplicationResultProjector.Effects(execution, attempt, fullyVerified: true),
            UpdateApplicationResultProjector.Lifecycle(execution, attempt),
            cleanup.Recovery,
            UpdateVerificationState.Verified,
            cleanup.Finding is { } cleanupFinding ? [cleanupFinding] : []);
    }

    private static UpdateResult PreparationBoundary(
        UpdatePlanExecution execution,
        RecoveryBundlePreparationResult preparation)
    {
        var code = preparation.State switch
        {
            RecoveryBundlePreparationState.Cancelled => UpdateFindingCode.Interrupted,
            RecoveryBundlePreparationState.Blocked => UpdateFindingCode.RecoveryConflict,
            RecoveryBundlePreparationState.Incomplete => UpdateFindingCode.RecoveryUnavailable,
            RecoveryBundlePreparationState.Prepared
                or RecoveryBundlePreparationState.NotNeeded => throw new ArgumentException(
                    "A complete recovery preparation is not a boundary.",
                    nameof(preparation)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(preparation),
                preparation.State,
                "The recovery preparation state is not defined."),
        };
        return UpdateResultBuilder.Create(
            execution,
            effects: [],
            LifecycleBeforeEffects(execution),
            new UpdateRecovery
            {
                State = preparation.ResidualPath is null
                    ? UpdateRecoveryState.Unknown
                    : UpdateRecoveryState.Retained,
                ProtectedPaths = UpdateResultBuilder.ProtectedPaths(execution),
                ResidualPath = preparation.ResidualPath,
            },
            UpdateVerificationState.NotRequested,
            [Finding(
                code,
                preparation.Cause ?? "Update recovery preparation did not complete safely.")]);
    }

    private static UpdateRecovery RetainedRecovery(
        UpdatePlanExecution execution,
        RecoveryBundlePreparationResult preparation)
        => preparation.Preparation is { } prepared
            ? new UpdateRecovery
            {
                State = UpdateRecoveryState.Retained,
                ProtectedPaths = UpdateResultBuilder.ProtectedPaths(execution),
                ResidualPath = prepared.BundlePath,
            }
            : new UpdateRecovery
            {
                State = UpdateRecoveryState.NotRequired,
                ProtectedPaths = [],
                ResidualPath = null,
            };

    private static UpdateLifecycle LifecycleBeforeEffects(UpdatePlanExecution execution)
        => new()
        {
            Trust = UpdateLifecycleTrust.Trusted,
            Coverage = UpdateLifecycleCoverage.Complete,
            Action = execution.OwnershipChange is null
                ? execution.Build.Preview.Lifecycle.Action
                : UpdateLifecycleAction.Publish,
            Outcome = execution.OwnershipChange is null
                ? execution.Build.Preview.Lifecycle.Outcome
                : UpdateLifecycleOutcome.NotStarted,
        };

    private static UpdateFinding Finding(UpdateFindingCode code, string cause)
        => new(code, target: null, cause);
}
