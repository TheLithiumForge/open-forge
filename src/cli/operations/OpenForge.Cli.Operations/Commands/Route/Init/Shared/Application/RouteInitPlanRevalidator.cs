using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

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
            var validator = new FileExpectationValidator(new PhysicalPathResolver());
            foreach (var expectation in original.LayoutRead.Expectations)
            {
                var validation = await validator.ValidateAsync(original.Request.Workspace, expectation, cancellationToken).ConfigureAwait(false);
                if (validation.State == FileExpectationValidationState.Cancelled) return Cancelled();
                if (validation.State != FileExpectationValidationState.Matched)
                    return new(RouteInitPlanRevalidationState.Changed, validation.Cause);
            }
            fresh = await _planBuilder.BuildAsync(original.Request, original.LayoutRead, cancellationToken)
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
