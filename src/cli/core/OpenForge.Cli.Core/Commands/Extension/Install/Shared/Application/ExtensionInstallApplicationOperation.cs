using OpenForge.Cli.Core.Commands.Extension.Install.Models.Application;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Application;

internal sealed class ExtensionInstallApplicationOperation(
    WorkspaceLockManager lockManager,
    ExtensionInstallApplicationPreconditionValidator preconditionValidator,
    ExtensionInstallRecoveryOperation recoveryOperation,
    ExtensionInstallEffectApplication effectApplication)
{
    private readonly WorkspaceLockManager _lockManager = lockManager;
    private readonly ExtensionInstallApplicationPreconditionValidator _preconditionValidator = preconditionValidator;
    private readonly ExtensionInstallRecoveryOperation _recoveryOperation = recoveryOperation;
    private readonly ExtensionInstallEffectApplication _effectApplication = effectApplication;

    internal async ValueTask<ExtensionInstallApplicationStageResult> ExecuteAsync(
        ExtensionInstallPlan plan,
        CancellationToken cancellationToken)
    {
        var operationId = Guid.NewGuid();
        var progress = ExtensionInstallApplicationProgress.Start(plan, preparation: null);
        WorkspaceLockResult lockResult;
        try
        {
            lockResult = await _lockManager.AcquireAsync(
                new WorkspaceLockRequest(
                    plan.Request.Workspace,
                    ExtensionInstallDefinitions.CommandIdentity,
                    operationId),
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Stop(
                progress,
                ExtensionInstallFindingCode.Interrupted,
                "Extension Install lock acquisition was interrupted.");
        }
        catch (Exception)
        {
            return Stop(
                progress,
                ExtensionInstallFindingCode.OperationFailed,
                "Extension Install lock acquisition failed unexpectedly.");
        }

        if (lockResult.State != WorkspaceLockState.Acquired
            || lockResult.Lease is not { } lease)
        {
            return Stop(
                progress,
                ReadLockFindingCode(lockResult.State),
                lockResult.Cause
                    ?? "The persistent Extension Install workspace lease could not be acquired.");
        }

        await using (lease.ConfigureAwait(false))
        {
            return await ExecuteUnderLeaseAsync(
                new ExtensionInstallApplicationLease(plan, lease, operationId),
                progress,
                cancellationToken).ConfigureAwait(false);
        }
    }

    private async ValueTask<ExtensionInstallApplicationStageResult> ExecuteUnderLeaseAsync(
        ExtensionInstallApplicationLease context,
        ExtensionInstallApplicationProgress progress,
        CancellationToken cancellationToken)
    {
        ExtensionInstallApplicationPrecondition precondition;
        try
        {
            precondition = await _preconditionValidator.ValidateAsync(
                context.Plan,
                context.Lease,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Stop(
                progress,
                ExtensionInstallFindingCode.Interrupted,
                "Extension Install plan revalidation was interrupted.");
        }
        catch (Exception)
        {
            return Stop(
                progress,
                ExtensionInstallFindingCode.OperationFailed,
                "Extension Install plan revalidation failed unexpectedly.");
        }

        if (!precondition.IsValid)
        {
            return new ExtensionInstallApplicationStageResult(
                progress,
                precondition.Finding
                    ?? new ExtensionInstallFinding(
                        ExtensionInstallFindingCode.TargetChanged,
                        "The Extension Install plan changed before application."));
        }

        RecoveryBundlePreparationResult preparation;
        try
        {
            preparation = await _recoveryOperation.PrepareAsync(
                context.Plan,
                context.OperationId,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Stop(
                progress,
                ExtensionInstallFindingCode.Interrupted,
                "Extension Install recovery preparation was interrupted.");
        }
        catch (Exception)
        {
            return new ExtensionInstallApplicationStageResult(
                progress.WithRecovery(new ExtensionInstallRecovery(
                    ExtensionInstallRecoveryState.Unknown,
                    [],
                    residualPath: null)),
                new ExtensionInstallFinding(
                    ExtensionInstallFindingCode.OperationFailed,
                    "Extension Install recovery preparation failed unexpectedly."));
        }

        var preparationBoundary = ReadPreparationBoundary(context.Plan, progress, preparation);
        if (preparationBoundary is not null)
        {
            return preparationBoundary;
        }

        progress = ExtensionInstallApplicationProgress.Start(
            context.Plan,
            preparation.Preparation);
        var application = await _effectApplication.ApplyAsync(
            new ExtensionInstallEffectApplicationInput
            {
                Plan = context.Plan,
                Lease = context.Lease,
                Validation = precondition.Validation
                    ?? throw new InvalidOperationException(
                        "A valid Extension Install precondition requires mutation checks."),
                Progress = progress,
            },
            cancellationToken).ConfigureAwait(false);
        if (application.Finding is not null
            || application.Progress.RecoveryPreparation is not { } recoveryPreparation)
        {
            return application;
        }

        var cleanup = await _recoveryOperation.CleanupAsync(
            new ExtensionInstallRecoveryCleanupRequest(
                context.Plan,
                context.Lease,
                recoveryPreparation),
            cancellationToken).ConfigureAwait(false);
        return new ExtensionInstallApplicationStageResult(
            application.Progress.WithRecovery(cleanup.Recovery),
            cleanup.Finding);
    }

    private static ExtensionInstallApplicationStageResult? ReadPreparationBoundary(
        ExtensionInstallPlan plan,
        ExtensionInstallApplicationProgress progress,
        RecoveryBundlePreparationResult result)
    {
        if (result.State is RecoveryBundlePreparationState.NotNeeded
            or RecoveryBundlePreparationState.Prepared)
        {
            return null;
        }

        var recovery = new ExtensionInstallRecovery(
            ReadPreparationRecoveryState(plan, result),
            [],
            result.ResidualPath);
        var finding = result.State switch
        {
            RecoveryBundlePreparationState.Incomplete => new ExtensionInstallFinding(
                ExtensionInstallFindingCode.RecoveryUnavailable,
                result.Cause
                    ?? "Extension Install recovery storage is unavailable before effects."),
            RecoveryBundlePreparationState.Blocked => new ExtensionInstallFinding(
                ExtensionInstallFindingCode.RecoveryConflict,
                result.Cause
                    ?? "Extension Install recovery preparation is blocked before effects."),
            RecoveryBundlePreparationState.Cancelled => new ExtensionInstallFinding(
                ExtensionInstallFindingCode.Interrupted,
                "Extension Install recovery preparation was interrupted."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(result),
                result.State,
                "The recovery preparation state is not defined."),
        };
        return new ExtensionInstallApplicationStageResult(
            progress.WithRecovery(recovery),
            finding);
    }

    private static ExtensionInstallFindingCode ReadLockFindingCode(WorkspaceLockState state)
        => state switch
        {
            WorkspaceLockState.Cancelled => ExtensionInstallFindingCode.Interrupted,
            WorkspaceLockState.Failed => ExtensionInstallFindingCode.WorkspaceLockUnavailable,
            WorkspaceLockState.Acquired => throw new InvalidOperationException(
                "An acquired workspace lock has no failure finding."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The workspace lock state is not defined."),
        };

    private static ExtensionInstallRecoveryState ReadPreparationRecoveryState(
        ExtensionInstallPlan plan,
        RecoveryBundlePreparationResult result)
    {
        if (result.ResidualPath is not null)
        {
            return ExtensionInstallRecoveryState.Unknown;
        }

        if (plan.RequiresRecovery)
        {
            return ExtensionInstallRecoveryState.NotCreated;
        }

        return ExtensionInstallRecoveryState.NotRequired;
    }

    private static ExtensionInstallApplicationStageResult Stop(
        ExtensionInstallApplicationProgress progress,
        ExtensionInstallFindingCode code,
        string cause)
        => new(progress, new ExtensionInstallFinding(code, cause));
}
