using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;

internal enum RouteRemoveRecoveryPreparationState
{
    NotRequired,
    Prepared,
    Incomplete,
    Blocked,
    Failed,
    Interrupted,
}

internal sealed record RouteRemoveRecoveryPreparationResult
{
    public required RouteRemoveRecoveryPreparationState State { get; init; }

    public RecoveryBundlePreparation? Preparation { get; init; }

    public required RouteRemoveRecovery Recovery { get; init; }

    public RouteRemoveFinding? Finding { get; init; }
}

internal sealed record RouteRemoveRecoveryDeletionInput
{
    public required RouteRemoveHeldApplication Held { get; init; }

    public required RecoveryBundlePreparation Preparation { get; init; }
}

internal sealed record RouteRemoveRecoveryDeletionResult
{
    public required RouteRemoveRecovery Recovery { get; init; }

    public RouteRemoveFinding? Finding { get; init; }
}
