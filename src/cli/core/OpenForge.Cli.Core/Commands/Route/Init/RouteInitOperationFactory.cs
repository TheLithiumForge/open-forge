using OpenForge.Cli.Core.Commands.Route.Init.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Result;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

namespace OpenForge.Cli.Core.Commands.Route.Init;

internal static class RouteInitOperationFactory
{
    internal static RouteInitOperation Create(
        WorkspaceLockStoreRoot? lockStoreRoot = null)
        => new(
            new RouteInitPlanBuilder(),
            new RouteInitApplicationOperation(lockStoreRoot),
            new RouteInitResultBuilder());
}
