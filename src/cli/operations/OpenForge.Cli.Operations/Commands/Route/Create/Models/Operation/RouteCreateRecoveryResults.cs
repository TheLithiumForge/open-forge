using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Route.Create.Models.Operation;

internal enum RouteCreateRecoveryPreparationState
{
    NotRequired,
    Prepared,
    Incomplete,
    Blocked,
    Cancelled,
    Failed,
}

internal sealed record RouteCreateRecoveryPreparationResult
{
    public required RouteCreateRecoveryPreparationState State { get; init; }

    public RecoveryBundlePreparation? Preparation { get; init; }

    public required RouteCreateRecovery Recovery { get; init; }

    public string? Cause { get; init; }
}

internal sealed record RouteCreateRecoveryDeletionResult
{
    public required RouteCreateRecovery Recovery { get; init; }

    public RouteCreateFindingCode? FindingCode { get; init; }

    public string? Cause { get; init; }
}
