using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;

internal enum RouteUpdateApplicationPreparationState
{
    Ready,
    Changed,
    Incomplete,
    Blocked,
    Cancelled,
    Failed,
}

internal sealed record RouteUpdateApplicationPreparation
{
    public required RouteUpdateApplicationPreparationState State { get; init; }

    public RecoveryBundlePreparation? RecoveryPreparation { get; init; }

    public MutationValidationResult? Validation { get; init; }

    public required RouteUpdateRecovery Recovery { get; init; }

    public RouteUpdateFinding? Finding { get; init; }
}
