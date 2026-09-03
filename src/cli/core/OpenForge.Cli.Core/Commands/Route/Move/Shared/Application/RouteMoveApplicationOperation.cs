using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;

internal sealed partial class RouteMoveApplicationOperation(
    WorkspaceLockManager lockManager,
    RouteMovePlanRevalidator revalidator,
    RouteMoveRecoveryLifecycle recoveryLifecycle,
    RouteMoveEffectApplication effectApplication,
    RouteMoveApplicationCompletion completion)
{
    private readonly WorkspaceLockManager _lockManager = lockManager;
    private readonly RouteMovePlanRevalidator _revalidator = revalidator;
    private readonly RouteMoveRecoveryLifecycle _recoveryLifecycle = recoveryLifecycle;
    private readonly RouteMoveEffectApplication _effectApplication = effectApplication;
    private readonly RouteMoveApplicationCompletion _completion = completion;

    private async ValueTask<RouteMoveApplicationProgress> ExecuteUnderLeaseAsync(
        RouteMoveHeldApplication held,
        CancellationToken cancellationToken)
    {
        var revalidation = await _revalidator.RevalidateAsync(
            held.Plan,
            held.Lease,
            cancellationToken).ConfigureAwait(false);
        if (revalidation.State != RouteMovePlanRevalidationState.Exact)
        {
            return RouteMoveApplicationProgressProjector.RevalidationBoundary(
                held.Plan,
                revalidation);
        }

        var preparation = await _recoveryLifecycle.PrepareAsync(
            new RouteMoveRecoveryPreparationInput
            {
                Plan = held.Plan,
                OperationId = held.OperationId,
                Lease = held.Lease,
            },
            cancellationToken).ConfigureAwait(false);
        if (preparation.State != RouteMoveRecoveryPreparationState.Prepared)
        {
            return RouteMoveApplicationProgressProjector.PreparationBoundary(
                held.Plan,
                preparation);
        }

        return await ExecutePreparedAsync(held, preparation, cancellationToken)
            .ConfigureAwait(false);
    }

    private async ValueTask<RouteMoveApplicationProgress> ExecutePreparedAsync(
        RouteMoveHeldApplication held,
        RouteMoveRecoveryPreparationResult preparation,
        CancellationToken cancellationToken)
    {
        RouteMoveApplicationProgress? progress = null;
        try
        {
            progress = await _effectApplication.ApplyAsync(
                new RouteMoveEffectApplicationInput
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

    private static RouteMoveApplicationProgress ClosePreparedFailure(
        RouteMovePlan plan,
        RouteMoveRecoveryPreparationResult preparation,
        RouteMoveApplicationProgress? progress,
        bool interrupted)
    {
        if (progress is null)
        {
            return RouteMoveApplicationProgressProjector.UnexpectedAfterPreparation(
                plan,
                preparation,
                interrupted);
        }

        var cause = interrupted
            ? "Route Move completion was interrupted unexpectedly."
            : "Route Move completion failed unexpectedly.";
        return RouteMoveApplicationProgressProjector.UnexpectedAfterApplication(
            plan,
            progress,
            interrupted,
            cause);
    }
}

internal sealed record RouteMoveHeldApplication(
    RouteMovePlan Plan,
    Guid OperationId,
    WorkspaceLockLease Lease);
