using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Result;

namespace OpenForge.Cli.Core.Commands.Route.Create;

internal static class RouteCreateOperationFactory
{
    internal static RouteCreateOperation Create(
        WorkspaceLockStoreRoot? lockStoreRoot = null)
        => new(
            new RouteCreatePlanBuilder(),
            new RouteCreateApplicationOperation(lockStoreRoot),
            new RouteCreateResultBuilder());
}
