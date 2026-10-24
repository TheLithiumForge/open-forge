using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;

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
            var layoutValidation = await new WorkspaceLayoutRevalidator(new PhysicalPathResolver())
                .ValidateAsync(lease, plan.LayoutRead, cancellationToken).ConfigureAwait(false);
            if (layoutValidation.State == MutationValidationState.Cancelled) return Interrupted();
            if (layoutValidation.State != MutationValidationState.Valid)
                return new RouteMovePlanRevalidation(RouteMovePlanRevalidationState.Changed, layoutValidation.Cause);

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
        => expected.LayoutRead.State == actual.LayoutRead.State
            && expected.LayoutRead.Layout?.MainRoot == actual.LayoutRead.Layout?.MainRoot
            && expected.LayoutRead.Settings.MatchesObservation(actual.LayoutRead.Settings)
            && expected.LayoutRead.Expectations.SequenceEqual(actual.LayoutRead.Expectations)
            && RouteMoveSemanticPlanProjection.Create(expected).Matches(RouteMoveSemanticPlanProjection.Create(actual));

    private static RouteMovePlanRevalidation Interrupted()
        => new(
            RouteMovePlanRevalidationState.Interrupted,
            "Complete Route Move plan revalidation was interrupted.");

    private static RouteMovePlanRevalidation Failed(string cause)
        => new(RouteMovePlanRevalidationState.Failed, cause);
}
