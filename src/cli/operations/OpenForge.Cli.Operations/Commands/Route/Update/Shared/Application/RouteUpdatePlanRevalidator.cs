using OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;

internal sealed class RouteUpdatePlanRevalidator(
    RouteUpdatePlanBuilder planBuilder,
    RouteUpdatePlanEquivalence equivalence)
{
    private readonly RouteUpdatePlanBuilder _planBuilder = planBuilder;
    private readonly RouteUpdatePlanEquivalence _equivalence = equivalence;

    internal async ValueTask<RouteUpdatePlanRevalidation> RevalidateAsync(
        RouteUpdatePlan plan,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }

        RouteUpdatePlanBuild fresh;
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
            return Failed("The complete Route Update plan could not be rebuilt under the workspace lease.");
        }

        if (fresh.Formation.Findings.Any(finding =>
                finding.Code == Models.Result.RouteUpdateFindingCode.Interrupted))
        {
            return Cancelled();
        }

        if (fresh.Plan is not { } rebuilt)
        {
            return Changed(
                fresh.Formation.Findings.FirstOrDefault()?.Cause
                    ?? "The complete Route Update plan no longer forms from the selected workspace facts.");
        }

        return _equivalence.Matches(plan, rebuilt)
            ? new RouteUpdatePlanRevalidation
            {
                State = RouteUpdatePlanRevalidationState.Exact,
                Cause = null,
            }
            : Changed("A target, overwrite, Template, route, or navigation fact changed after planning.");
    }

    private static RouteUpdatePlanRevalidation Changed(string cause)
        => new()
        {
            State = RouteUpdatePlanRevalidationState.Changed,
            Cause = cause,
        };

    private static RouteUpdatePlanRevalidation Cancelled()
        => new()
        {
            State = RouteUpdatePlanRevalidationState.Cancelled,
            Cause = "Route Update plan revalidation was interrupted.",
        };

    private static RouteUpdatePlanRevalidation Failed(string cause)
        => new()
        {
            State = RouteUpdatePlanRevalidationState.Failed,
            Cause = cause,
        };
}
