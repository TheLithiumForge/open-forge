using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Application;

internal enum RouteInitPlanRevalidationState
{
    Exact,
    Changed,
    Cancelled,
    Failed,
}

internal sealed record RouteInitPlanRevalidation(
    RouteInitPlanRevalidationState State,
    string? Cause);

internal sealed class RouteInitPlanRevalidator(RouteInitPlanBuilder planBuilder)
{
    private readonly RouteInitPlanBuilder _planBuilder = planBuilder;

    internal async ValueTask<RouteInitPlanRevalidation> RevalidateAsync(
        RouteInitPlan original,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(original);
        RouteInitPlanBuild fresh;
        try
        {
            fresh = await _planBuilder.BuildAsync(original.Request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception)
        {
            return new RouteInitPlanRevalidation(
                RouteInitPlanRevalidationState.Failed,
                "The complete Route Init plan could not be rebuilt under the workspace lease.");
        }

        if (fresh.Formation.Findings.Any(finding => finding.Code == RouteInitFindingCode.Interrupted))
        {
            return Cancelled();
        }

        if (fresh.Plan is not { } freshPlan
            || !RouteInitPlanEquivalence.Matches(original, freshPlan))
        {
            return new RouteInitPlanRevalidation(
                RouteInitPlanRevalidationState.Changed,
                fresh.Formation.Findings.FirstOrDefault()?.Cause
                    ?? "The complete Route Init plan changed before application.");
        }

        return new RouteInitPlanRevalidation(RouteInitPlanRevalidationState.Exact, Cause: null);
    }

    private static RouteInitPlanRevalidation Cancelled()
        => new(
            RouteInitPlanRevalidationState.Cancelled,
            "Complete Route Init plan revalidation was interrupted.");
}
