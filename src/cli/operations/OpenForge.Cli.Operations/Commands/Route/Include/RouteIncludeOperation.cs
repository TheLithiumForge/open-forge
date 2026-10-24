using OpenForge.Cli.Core.Commands.Route.Shared.Recovery;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Application;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

namespace OpenForge.Cli.Core.Commands.Route.Include;

internal sealed class RouteIncludeOperation(WorkspaceLockStoreRoot? lockStoreRoot = null)
{
    internal async ValueTask<RouteIncludeResult> ExecuteAsync(RouteIncludeRequest request, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);
        var history = await RouteRecoveryHistoryReader.ReadAsync(request.Workspace, token).ConfigureAwait(false);
        var built = await new RouteInclusionPlanBuilder().BuildAsync(request.ToInclusion(), token).ConfigureAwait(false);
        var outcome = built.Outcome;
        if (!request.DryRun && built.Plan is { } plan && !outcome.NoOp)
            outcome = await new RouteInclusionApplication(lockStoreRoot).ApplyAsync(plan, token).ConfigureAwait(false);
        return new RouteIncludeResult { RecoveryHistory = history, Workspace = request.Workspace, Requested = request.File, DryRun = request.DryRun, Outcome = outcome };
    }
}
