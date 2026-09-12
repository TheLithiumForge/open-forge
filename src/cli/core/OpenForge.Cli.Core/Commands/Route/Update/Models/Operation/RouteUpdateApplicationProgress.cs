using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;

internal sealed record RouteUpdateEffectApplicationInput
{
    public required RouteUpdatePlan Plan { get; init; }

    public required RecoveryBundlePreparation Preparation { get; init; }

    public required WorkspaceLockLease Lease { get; init; }

    public required MutationValidationResult Validation { get; init; }
}

internal sealed record RouteUpdateApplicationProgress
{
    public required ImmutableArray<FileChangeReceipt> Receipts { get; init; }

    public PlannedFileChange? UncertainAttempt { get; init; }

    public required RouteUpdateRecovery Recovery { get; init; }

    public required RouteUpdateVerificationState Verification { get; init; }

    public required ImmutableArray<RouteUpdateFinding> Findings { get; init; }
}

internal sealed record RouteUpdateAppliedVerificationInput
{
    public required RouteUpdatePlan Plan { get; init; }

    public required RouteUpdateApplicationProgress Progress { get; init; }

    public required WorkspaceLockLease Lease { get; init; }
}

internal enum RouteUpdateAppliedVerificationState
{
    Verified,
    Failed,
    Cancelled,
}

internal sealed record RouteUpdateAppliedVerification
{
    public required RouteUpdateAppliedVerificationState State { get; init; }

    public string? Cause { get; init; }
}
