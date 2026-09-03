using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;

internal enum RouteMoveRecoveryPreparationState
{
    NotRequired,
    Prepared,
    Incomplete,
    Blocked,
    Failed,
    Interrupted,
}

internal sealed record RouteMoveRecoveryPreparationInput
{
    public required RouteMovePlan Plan { get; init; }

    public required Guid OperationId { get; init; }

    public required WorkspaceLockLease Lease { get; init; }
}

internal sealed record RouteMoveRecoveryPreparationResult
{
    public required RouteMoveRecoveryPreparationState State { get; init; }

    public RecoveryBundlePreparation? Preparation { get; init; }

    public required RouteMoveRecovery Recovery { get; init; }

    public RouteMoveFinding? Finding { get; init; }
}

internal sealed record RouteMoveRecoveryDeletionInput
{
    public required RouteMovePlan Plan { get; init; }

    public required Guid OperationId { get; init; }

    public required WorkspaceLockLease Lease { get; init; }

    public required RecoveryBundlePreparation Preparation { get; init; }
}

internal sealed record RouteMoveRecoveryDeletionResult
{
    public required RouteMoveRecovery Recovery { get; init; }

    public RouteMoveFinding? Finding { get; init; }
}
