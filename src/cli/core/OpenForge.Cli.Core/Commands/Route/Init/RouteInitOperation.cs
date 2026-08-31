using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Result;

namespace OpenForge.Cli.Core.Commands.Route.Init;

internal sealed class RouteInitOperation(
    RouteInitPlanBuilder planBuilder,
    RouteInitApplicationOperation applicationOperation,
    RouteInitResultBuilder resultBuilder)
{
    internal RouteInitPlanBuilder PlanBuilder { get; } = planBuilder;

    internal RouteInitApplicationOperation ApplicationOperation { get; } = applicationOperation;

    internal RouteInitResultBuilder ResultBuilder { get; } = resultBuilder;

    internal async ValueTask<RouteInitResult> ExecuteAsync(
        RouteInitRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var build = await PlanBuilder.BuildAsync(request, cancellationToken).ConfigureAwait(false);
        if (build.Plan is not { } plan)
        {
            return ResultBuilder.Build(build.Formation);
        }

        if (request.IsDryRun)
        {
            return ResultBuilder.Build(build.Formation);
        }

        if (plan.IsNoOp)
        {
            var preview = plan.Preview;
            var formation = new Models.Result.RouteInitResultFormation(
                preview.Workspace,
                preview.Mode,
                preview.Scaffold,
                preview.Target,
                preview.Plan,
                preview.Framework,
                preview.Entrypoints,
                preview.Effects,
                preview.UnchangedPaths,
                preview.Scaffold == Models.Request.RouteInitScaffold.Framework
                    ? new Models.Result.RouteInitLifecycle(
                        Models.Result.RouteInitLifecycleAction.Preserve,
                        Models.Result.RouteInitLifecycleOutcome.AlreadyCurrent)
                    : preview.Lifecycle,
                new Models.Result.RouteInitRecovery(
                    Models.Result.RouteInitRecoveryState.NotRequired,
                    null),
                Models.Result.RouteInitVerificationState.Verified,
                preview.Findings);
            return ResultBuilder.Build(formation);
        }

        var outcome = await ApplicationOperation.ExecuteAsync(plan, cancellationToken).ConfigureAwait(false);
        return ResultBuilder.Build(outcome.Formation);
    }
}
