using OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Result;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;

internal sealed class RouteUpdateApplicationOperation(
    RouteUpdateApplicationPipeline pipeline,
    RouteUpdateResultBuilder resultBuilder,
    WorkspaceLockStoreRoot? lockStoreRoot)
{
    private readonly RouteUpdateApplicationPipeline _pipeline = pipeline;
    private readonly RouteUpdateResultBuilder _resultBuilder = resultBuilder;
    private readonly WorkspaceLockStoreRoot? _lockStoreRoot = lockStoreRoot;

    internal async ValueTask<RouteUpdateResult> ExecuteAsync(
        RouteUpdatePlan plan,
        CancellationToken cancellationToken)
    {
        if (plan.IsNoOp)
        {
            return _resultBuilder.Build(plan.Preview with
            {
                Recovery = RouteUpdateRecovery.NotRequired(),
                Verification = RouteUpdateVerificationState.Verified,
            });
        }

        var operationId = Guid.NewGuid();
        WorkspaceLockResult lockResult;
        try
        {
            lockResult = await LockManager().AcquireAsync(
                    new WorkspaceLockRequest(
                        plan.Request.Workspace,
                        RouteUpdateDefinitions.CommandIdentity,
                        operationId),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return FinishBeforeApplication(
                plan,
                RouteUpdateFindingCode.Interrupted,
                "Route Update workspace lock acquisition was interrupted.");
        }
        catch (Exception)
        {
            return FinishBeforeApplication(
                plan,
                RouteUpdateFindingCode.OperationFailed,
                "Route Update workspace lock acquisition failed unexpectedly.");
        }

        if (lockResult.State != WorkspaceLockState.Acquired
            || lockResult.Lease is not { } lease)
        {
            var code = lockResult.State switch
            {
                WorkspaceLockState.Cancelled => RouteUpdateFindingCode.Interrupted,
                WorkspaceLockState.Failed => RouteUpdateFindingCode.WorkspaceLockUnavailable,
                WorkspaceLockState.Acquired => RouteUpdateFindingCode.OperationFailed,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(lockResult),
                    lockResult.State,
                    "The workspace lock state is not defined."),
            };
            return FinishBeforeApplication(
                plan,
                code,
                lockResult.Cause
                    ?? "The persistent Route Update workspace lease could not be acquired.");
        }

        RouteUpdateApplicationProgress progress;
        var lockReleaseFailed = false;
        try
        {
            progress = await _pipeline.ExecuteAsync(
                    new RouteUpdateApplicationPipelineInput
                    {
                        Plan = plan,
                        Lease = lease,
                        OperationId = operationId,
                    },
                    cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            try
            {
                await lease.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                lockReleaseFailed = true;
            }
        }

        if (lockReleaseFailed)
        {
            progress = progress with
            {
                Findings =
                [
                    .. progress.Findings,
                    new RouteUpdateFinding(
                        RouteUpdateFindingCode.OperationFailed,
                        "Route Update workspace lock release failed unexpectedly.",
                        plan.Preview.Target.Path),
                ],
            };
        }

        return _resultBuilder.Build(
            RouteUpdateApplicationResultFactory.Build(plan, progress));
    }

    private RouteUpdateResult FinishBeforeApplication(
        RouteUpdatePlan plan,
        RouteUpdateFindingCode code,
        string cause)
        => _resultBuilder.Build(
            RouteUpdateApplicationResultFactory.Build(
                plan,
                new RouteUpdateApplicationProgress
                {
                    Receipts = [],
                    UncertainAttempt = null,
                    Recovery = RouteUpdateRecovery.NotCreated(),
                    Verification = RouteUpdateVerificationState.NotRequested,
                    Findings =
                    [
                        new RouteUpdateFinding(
                            code,
                            cause,
                            plan.Preview.Target.Path ?? plan.Preview.Target.Requested),
                    ],
                }));

    private WorkspaceLockManager LockManager()
        => _lockStoreRoot is null
            ? WorkspaceLockManager.CreateForCurrentUser()
            : new WorkspaceLockManager(_lockStoreRoot);
}
