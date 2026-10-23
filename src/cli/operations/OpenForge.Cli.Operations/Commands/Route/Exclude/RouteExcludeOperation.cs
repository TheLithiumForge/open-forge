using OpenForge.Cli.Core.Commands.Route.Shared.Recovery;
using OpenForge.Cli.Core.Commands.Route.Exclude.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Exclude.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Application;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

namespace OpenForge.Cli.Core.Commands.Route.Exclude;

internal sealed class RouteExcludeOperation(WorkspaceLockStoreRoot? lockStoreRoot = null)
{
    internal async ValueTask<RouteExcludeResult> ExecuteAsync(RouteExcludeRequest request, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);
        var history = await RouteRecoveryHistoryReader.ReadAsync(request.Workspace, token).ConfigureAwait(false);
        var built = await new RouteInclusionPlanBuilder().BuildAsync(request.ToInclusion(), token).ConfigureAwait(false);
        var outcome = built.Outcome;
        if (!request.DryRun && built.Plan is { } plan && !outcome.NoOp)
            outcome = await new RouteInclusionApplication(lockStoreRoot).ApplyAsync(plan, token).ConfigureAwait(false);
        return new RouteExcludeResult { RecoveryHistory = history, Workspace = request.Workspace, Requested = request.File, DryRun = request.DryRun, Outcome = outcome };
    }
}
