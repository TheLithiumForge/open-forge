using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;

internal sealed partial class RouteRemoveApplicationOperation(
    WorkspaceLockManager lockManager,
    RouteRemovePlanRevalidator revalidator,
    RouteRemoveRecoveryLifecycle recoveryLifecycle,
    RouteRemoveEffectApplication effectApplication,
    RouteRemoveApplicationCompletion completion)
{
    private readonly WorkspaceLockManager _lockManager = lockManager;
    private readonly RouteRemovePlanRevalidator _revalidator = revalidator;
    private readonly RouteRemoveRecoveryLifecycle _recoveryLifecycle = recoveryLifecycle;
    private readonly RouteRemoveEffectApplication _effectApplication = effectApplication;
    private readonly RouteRemoveApplicationCompletion _completion = completion;

    internal async ValueTask<RouteRemoveApplicationProgress> ExecuteAsync(
        RouteRemovePlan plan,
        Guid operationId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (operationId == Guid.Empty)
        {
            throw new ArgumentException(
                "Route Remove application requires a nonempty operation identity.",
                nameof(operationId));
        }

        var acquired = await AcquireAsync(plan, operationId, cancellationToken)
            .ConfigureAwait(false);
        if (acquired.Progress is { } boundary)
        {
            return boundary;
        }

        return await ExecuteAndReleaseAsync(
            new RouteRemoveHeldApplication(
                plan,
                operationId,
                acquired.Lease
                    ?? throw new InvalidOperationException(
                        "Successful Route Remove lock acquisition requires its lease.")),
            cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<LockAcquisition> AcquireAsync(
        RouteRemovePlan plan,
        Guid operationId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _lockManager.AcquireAsync(
                new WorkspaceLockRequest(
                    plan.Request.Workspace,
                    RouteRemoveDefinitions.CommandIdentity,
                    operationId),
                cancellationToken).ConfigureAwait(false);
            return result.State == WorkspaceLockState.Acquired && result.Lease is { } lease
                ? LockAcquisition.Complete(lease)
                : LockAcquisition.Stop(
                    RouteRemoveApplicationProgressProjector.LockBoundary(plan, result));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return LockAcquisition.Stop(BeforeLockFailure(plan, interrupted: true));
        }
        catch (Exception)
        {
            return LockAcquisition.Stop(BeforeLockFailure(plan, interrupted: false));
        }
    }

    private async ValueTask<RouteRemoveApplicationProgress> ExecuteAndReleaseAsync(
        RouteRemoveHeldApplication application,
        CancellationToken cancellationToken)
    {
        var progress = await ExecuteHeldSafelyAsync(application, cancellationToken)
            .ConfigureAwait(false);
        try
        {
            await application.Lease.DisposeAsync().ConfigureAwait(false);
            return progress;
        }
        catch (Exception)
        {
            return RouteRemoveApplicationProgressProjector.LockReleaseFailed(
                application.Plan,
                progress);
        }
    }

    private async ValueTask<RouteRemoveApplicationProgress> ExecuteHeldSafelyAsync(
        RouteRemoveHeldApplication application,
        CancellationToken cancellationToken)
    {
        try
        {
            return await ExecuteUnderLeaseAsync(application, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return BeforeHeldFailure(application.Plan, interrupted: true);
        }
        catch (Exception)
        {
            return BeforeHeldFailure(application.Plan, interrupted: false);
        }
    }

    private static RouteRemoveApplicationProgress BeforeLockFailure(
        RouteRemovePlan plan,
        bool interrupted)
        => RouteRemoveApplicationProgressProjector.BeforeApplication(
            plan,
            interrupted ? RouteRemoveFindingCode.Interrupted : RouteRemoveFindingCode.OperationFailed,
            interrupted ? CliSemanticStatus.Interrupted : CliSemanticStatus.Failed,
            interrupted
                ? "Route Remove workspace lock acquisition was interrupted."
                : "Route Remove workspace lock acquisition failed unexpectedly.");

    private static RouteRemoveApplicationProgress BeforeHeldFailure(
        RouteRemovePlan plan,
        bool interrupted)
        => RouteRemoveApplicationProgressProjector.BeforeApplication(
            plan,
            interrupted ? RouteRemoveFindingCode.Interrupted : RouteRemoveFindingCode.OperationFailed,
            interrupted ? CliSemanticStatus.Interrupted : CliSemanticStatus.Failed,
            interrupted
                ? "Route Remove application was interrupted before recovery preparation completed."
                : "Route Remove application failed before recovery preparation completed.");

    private async ValueTask<RouteRemoveApplicationProgress> ExecuteUnderLeaseAsync(
        RouteRemoveHeldApplication held,
        CancellationToken cancellationToken)
    {
        var revalidation = await _revalidator.RevalidateAsync(
            held.Plan,
            held.Lease,
            cancellationToken).ConfigureAwait(false);
        if (revalidation.State != RouteRemovePlanRevalidationState.Exact)
        {
            return RouteRemoveApplicationProgressProjector.RevalidationBoundary(
                held.Plan,
                revalidation);
        }

        var preparation = await _recoveryLifecycle.PrepareAsync(
            new RouteRemoveRecoveryPreparationInput
            {
                Plan = held.Plan,
                OperationId = held.OperationId,
                Lease = held.Lease,
            },
            cancellationToken).ConfigureAwait(false);
        if (preparation.State != RouteRemoveRecoveryPreparationState.Prepared)
        {
            return RouteRemoveApplicationProgressProjector.PreparationBoundary(
                held.Plan,
                preparation);
        }

        return await ExecutePreparedAsync(held, preparation, cancellationToken)
            .ConfigureAwait(false);
    }

    private async ValueTask<RouteRemoveApplicationProgress> ExecutePreparedAsync(
        RouteRemoveHeldApplication held,
        RouteRemoveRecoveryPreparationResult preparation,
        CancellationToken cancellationToken)
    {
        RouteRemoveApplicationProgress? progress = null;
        try
        {
            progress = await _effectApplication.ApplyAsync(
                new RouteRemoveEffectApplicationInput
                {
                    Plan = held.Plan,
                    Lease = held.Lease,
                    RecoveryPreparation = preparation,
                },
                cancellationToken).ConfigureAwait(false);
            if (!progress.Findings.IsEmpty)
            {
                return progress;
            }

            return await _completion.CompleteAsync(
                held,
                preparation,
                progress,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ClosePreparedFailure(held.Plan, preparation, progress, interrupted: true);
        }
        catch (Exception)
        {
            return ClosePreparedFailure(held.Plan, preparation, progress, interrupted: false);
        }
    }

    private static RouteRemoveApplicationProgress ClosePreparedFailure(
        RouteRemovePlan plan,
        RouteRemoveRecoveryPreparationResult preparation,
        RouteRemoveApplicationProgress? progress,
        bool interrupted)
    {
        if (progress is null)
        {
            return RouteRemoveApplicationProgressProjector.UnexpectedAfterPreparation(
                plan,
                preparation,
                interrupted);
        }

        var cause = interrupted
            ? "Route Remove completion was interrupted unexpectedly."
            : "Route Remove completion failed unexpectedly.";
        return RouteRemoveApplicationProgressProjector.UnexpectedAfterApplication(
            plan,
            progress,
            interrupted,
            cause);
    }

    private sealed record LockAcquisition(
        WorkspaceLockLease? Lease,
        RouteRemoveApplicationProgress? Progress)
    {
        internal static LockAcquisition Complete(WorkspaceLockLease lease)
            => new(lease, Progress: null);

        internal static LockAcquisition Stop(RouteRemoveApplicationProgress progress)
            => new(Lease: null, progress);
    }
}

internal sealed record RouteRemoveHeldApplication(
    RouteRemovePlan Plan,
    Guid OperationId,
    WorkspaceLockLease Lease);
