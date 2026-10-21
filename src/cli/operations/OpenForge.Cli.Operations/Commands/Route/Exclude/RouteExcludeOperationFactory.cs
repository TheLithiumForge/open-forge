using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

namespace OpenForge.Cli.Core.Commands.Route.Exclude;

internal static class RouteExcludeOperationFactory
{
    internal static RouteExcludeOperation Create(WorkspaceLockStoreRoot? lockStoreRoot = null) => new(lockStoreRoot);
}
