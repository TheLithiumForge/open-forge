using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;

internal sealed record RouteRemoveHeldApplication(
    RouteRemovePlan Plan,
    Guid OperationId,
    WorkspaceLockLease Lease);
