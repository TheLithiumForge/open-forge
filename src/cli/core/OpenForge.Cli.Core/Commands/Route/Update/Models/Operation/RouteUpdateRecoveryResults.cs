using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

namespace OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;

internal enum RouteUpdateRecoveryPreparationState
{
    NotRequired,
    Prepared,
    Incomplete,
    Blocked,
    Cancelled,
    Failed,
}

internal sealed record RouteUpdateRecoveryPreparationInput
{
    public required RouteUpdatePlan Plan { get; init; }

    public required string OperationId { get; init; }
}

internal sealed record RouteUpdateRecoveryPreparationResult
{
    public required RouteUpdateRecoveryPreparationState State { get; init; }

    public RecoveryBundlePreparation? Preparation { get; init; }

    public required RouteUpdateRecovery Recovery { get; init; }

    public string? Cause { get; init; }
}

internal sealed record RouteUpdateRecoveryCompletionInput
{
    public required RouteUpdatePlan Plan { get; init; }

    public required RecoveryBundlePreparation Preparation { get; init; }

    public required WorkspaceLockLease Lease { get; init; }
}

internal sealed record RouteUpdateRecoveryCompletionResult
{
    public required RouteUpdateRecovery Recovery { get; init; }

    public RouteUpdateFindingCode? FindingCode { get; init; }

    public string? Cause { get; init; }
}
