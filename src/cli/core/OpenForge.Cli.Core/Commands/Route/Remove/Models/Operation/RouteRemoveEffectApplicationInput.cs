using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;

internal sealed record RouteRemoveEffectApplicationInput
{
    public required RouteRemovePlan Plan { get; init; }

    public required WorkspaceLockLease Lease { get; init; }

    public required RouteRemoveRecoveryPreparationResult RecoveryPreparation { get; init; }
}
