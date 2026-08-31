using OpenForge.Cli.Core.Commands.Route.Create.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Application;

internal sealed class RouteCreatePlanRevalidator(RouteCreatePlanBuilder planBuilder)
{
    private readonly RouteCreatePlanBuilder _planBuilder = planBuilder;

    internal async ValueTask<RouteCreatePlanRevalidation> RevalidateAsync(
        RouteCreatePlan plan,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        RouteCreatePlanBuild fresh;
        try
        {
            fresh = await _planBuilder.BuildAsync(plan.Request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception)
        {
            return new RouteCreatePlanRevalidation
            {
                State = RouteCreatePlanRevalidationState.Failed,
                Cause = "The complete Route Create plan could not be rebuilt under the workspace lease.",
            };
        }

        if (fresh.Formation.Findings.Any(finding =>
                finding.Code == RouteCreateFindingCode.Interrupted))
        {
            return Cancelled();
        }

        if (fresh.Plan is not { } rebuilt
            || !RouteCreatePlanEquivalence.Matches(plan, rebuilt))
        {
            return new RouteCreatePlanRevalidation
            {
                State = RouteCreatePlanRevalidationState.Changed,
                Cause = fresh.Formation.Findings.FirstOrDefault()?.Cause
                    ?? "The complete Route Create plan changed before application.",
            };
        }

        return new RouteCreatePlanRevalidation
        {
            State = RouteCreatePlanRevalidationState.Exact,
            Cause = null,
        };
    }

    private static RouteCreatePlanRevalidation Cancelled()
        => new()
        {
            State = RouteCreatePlanRevalidationState.Cancelled,
            Cause = "Complete Route Create plan revalidation was interrupted.",
        };
}
