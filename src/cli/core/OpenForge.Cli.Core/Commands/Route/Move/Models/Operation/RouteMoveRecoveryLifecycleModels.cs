using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

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

internal sealed record RouteMoveRecoveryPreparationResult
{
    public required RouteMoveRecoveryPreparationState State { get; init; }

    public RecoveryBundlePreparation? Preparation { get; init; }

    public required RouteMoveRecovery Recovery { get; init; }

    public RouteMoveFinding? Finding { get; init; }
}

internal sealed record RouteMoveRecoveryDeletionInput
{
    public required RouteMoveHeldApplication Held { get; init; }

    public required RecoveryBundlePreparation Preparation { get; init; }
}

internal sealed record RouteMoveRecoveryDeletionResult
{
    public required RouteMoveRecovery Recovery { get; init; }

    public RouteMoveFinding? Finding { get; init; }
}
