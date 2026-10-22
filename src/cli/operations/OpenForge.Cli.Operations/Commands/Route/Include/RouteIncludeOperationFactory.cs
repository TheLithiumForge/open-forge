using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

namespace OpenForge.Cli.Core.Commands.Route.Include;

internal static class RouteIncludeOperationFactory
{
    internal static RouteIncludeOperation Create(WorkspaceLockStoreRoot? lockStoreRoot = null) => new(lockStoreRoot);
}
