using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;

internal sealed record RouteMoveHeldApplication(
    RouteMovePlan Plan,
    Guid OperationId,
    WorkspaceLockLease Lease);
