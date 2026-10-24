using OpenForge.Cli.Core.Commands.Route.Shared.Recovery;
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
        var history = await RouteRecoveryHistoryReader.ReadAsync(request.Workspace, cancellationToken).ConfigureAwait(false);
        var result = await ExecuteCoreAsync(request, cancellationToken).ConfigureAwait(false);
        return result with { RecoveryHistory = history };
    }

    private async ValueTask<RouteInitResult> ExecuteCoreAsync(
        RouteInitRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var build = await PlanBuilder.BuildAsync(request, cancellationToken).ConfigureAwait(false);
        RouteInitResult BuildResult(RouteInitResultFormation formation)
        {
            var layout = build.LayoutRead?.Layout;
            return ResultBuilder.Build(formation with { Request = request }) with
            {
                CustomRoot = layout is { IsDefault: false } ? layout.MainRoot : null,
                LoaderPath = layout is { IsDefault: false } ? layout.LoaderPath : null,
            };
        }

        if (build.Plan is not { } plan)
        {
            return BuildResult(build.Formation);
        }

        if (request.IsDryRun)
        {
            return BuildResult(build.Formation);
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
            return BuildResult(formation);
        }

        var outcome = await ApplicationOperation.ExecuteAsync(plan, cancellationToken).ConfigureAwait(false);
        return BuildResult(outcome.Formation);
    }
}
