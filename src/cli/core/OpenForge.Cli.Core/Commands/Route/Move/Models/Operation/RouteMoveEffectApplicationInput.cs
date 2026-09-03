using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;

internal sealed record RouteMoveEffectApplicationInput
{
    public required RouteMovePlan Plan { get; init; }

    public required WorkspaceLockLease Lease { get; init; }

    public required RouteMoveRecoveryPreparationResult RecoveryPreparation { get; init; }
}
