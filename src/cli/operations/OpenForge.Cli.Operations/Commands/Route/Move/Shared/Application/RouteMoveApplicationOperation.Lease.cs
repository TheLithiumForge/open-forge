using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;

internal sealed partial class RouteMoveApplicationOperation
{
    internal async ValueTask<RouteMoveApplicationProgress> ExecuteAsync(
        RouteMovePlan plan,
        Guid operationId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (operationId == Guid.Empty)
        {
            throw new ArgumentException(
                "Route Move application requires a nonempty operation identity.",
                nameof(operationId));
        }

        var acquired = await AcquireAsync(plan, operationId, cancellationToken)
            .ConfigureAwait(false);
        if (acquired.Progress is { } boundary)
        {
            return boundary;
        }

        return await ExecuteAndReleaseAsync(
            new RouteMoveHeldApplication(
                plan,
                operationId,
                acquired.Lease
                    ?? throw new InvalidOperationException(
                        "Successful Route Move lock acquisition requires its lease.")),
            cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<LockAcquisition> AcquireAsync(
        RouteMovePlan plan,
        Guid operationId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _lockManager.AcquireAsync(
                new WorkspaceLockRequest(
                    plan.Request.Workspace,
                    RouteMoveDefinitions.CommandIdentity,
                    operationId),
                cancellationToken).ConfigureAwait(false);
            return result.State == WorkspaceLockState.Acquired && result.Lease is { } lease
                ? LockAcquisition.Complete(lease)
                : LockAcquisition.Stop(
                    RouteMoveApplicationProgressProjector.LockBoundary(plan, result));
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

    private async ValueTask<RouteMoveApplicationProgress> ExecuteAndReleaseAsync(
        RouteMoveHeldApplication application,
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
            return RouteMoveApplicationProgressProjector.LockReleaseFailed(
                application.Plan,
                progress);
        }
    }

    private async ValueTask<RouteMoveApplicationProgress> ExecuteHeldSafelyAsync(
        RouteMoveHeldApplication application,
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

    private static RouteMoveApplicationProgress BeforeLockFailure(
        RouteMovePlan plan,
        bool interrupted)
        => RouteMoveApplicationProgressProjector.BeforeApplication(
            plan,
            interrupted ? RouteMoveFindingCode.Interrupted : RouteMoveFindingCode.OperationFailed,
            interrupted ? CliSemanticStatus.Interrupted : CliSemanticStatus.Failed,
            interrupted
                ? "Route Move workspace lock acquisition was interrupted."
                : "Route Move workspace lock acquisition failed unexpectedly.");

    private static RouteMoveApplicationProgress BeforeHeldFailure(
        RouteMovePlan plan,
        bool interrupted)
        => RouteMoveApplicationProgressProjector.BeforeApplication(
            plan,
            interrupted ? RouteMoveFindingCode.Interrupted : RouteMoveFindingCode.OperationFailed,
            interrupted ? CliSemanticStatus.Interrupted : CliSemanticStatus.Failed,
            interrupted
                ? "Route Move application was interrupted before recovery preparation completed."
                : "Route Move application failed before recovery preparation completed.");

    private sealed record LockAcquisition(
        WorkspaceLockLease? Lease,
        RouteMoveApplicationProgress? Progress)
    {
        internal static LockAcquisition Complete(WorkspaceLockLease lease)
            => new(lease, Progress: null);

        internal static LockAcquisition Stop(RouteMoveApplicationProgress progress)
            => new(Lease: null, progress);
    }
}
