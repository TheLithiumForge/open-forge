using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;

internal sealed class RouteMovePlanRevalidator(RouteMovePlanBuilder planBuilder)
{
    private readonly RouteMovePlanBuilder _planBuilder = planBuilder;

    internal async ValueTask<RouteMovePlanRevalidation> RevalidateAsync(
        RouteMovePlan plan,
        WorkspaceLockLease lease,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(lease);
        if (!lease.IsHeldFor(plan.Request.Workspace))
        {
            return Failed("Route Move revalidation does not hold the selected workspace lease.");
        }

        RouteMovePlanBuild fresh;
        try
        {
            fresh = await _planBuilder.BuildAsync(plan.Request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Interrupted();
        }
        catch (Exception)
        {
            return Failed("The complete Route Move plan could not be rebuilt under the workspace lease.");
        }

        if (fresh.Formation.Findings.Any(finding =>
                finding.Code == RouteMoveFindingCode.Interrupted))
        {
            return Interrupted();
        }

        if (fresh.Plan is not { } rebuilt || !Matches(plan, rebuilt))
        {
            return new RouteMovePlanRevalidation(
                RouteMovePlanRevalidationState.Changed,
                fresh.Formation.Findings.FirstOrDefault()?.Cause
                    ?? "The complete Route Move plan changed before application.");
        }

        return new RouteMovePlanRevalidation(RouteMovePlanRevalidationState.Exact, cause: null);
    }

    private static bool Matches(RouteMovePlan expected, RouteMovePlan actual)
        => RouteMoveSemanticPlanProjection.Create(expected).Matches(
            RouteMoveSemanticPlanProjection.Create(actual));

    private static RouteMovePlanRevalidation Interrupted()
        => new(
            RouteMovePlanRevalidationState.Interrupted,
            "Complete Route Move plan revalidation was interrupted.");

    private static RouteMovePlanRevalidation Failed(string cause)
        => new(RouteMovePlanRevalidationState.Failed, cause);
}
