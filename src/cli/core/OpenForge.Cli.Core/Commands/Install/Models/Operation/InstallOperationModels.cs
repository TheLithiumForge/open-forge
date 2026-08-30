using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Install.Models.Operation;

internal enum InstallRecoveryState
{
    NotRequired,
    Prepared,
    NotCreated,
    Removed,
    Retained,
    Unknown,
}

internal sealed record InstallOperationSummary
{
    public required InstallManagementState ManagementState { get; init; }

    public required int PlannedDirectoryCount { get; init; }

    public required int PlannedFileCount { get; init; }

    public required int AppliedDirectoryCount { get; init; }

    public required int AppliedTargetFileCount { get; init; }

    public required bool LifecyclePublished { get; init; }

    public required InstallRecoveryState RecoveryState { get; init; }

    public required string? RecoveryResidualPath { get; init; }

    public bool HasRetainedWorkspaceEffects =>
        AppliedDirectoryCount > 0
        || AppliedTargetFileCount > 0
        || LifecyclePublished;
}

internal sealed record InstallApplicationOutcome
{
    public required IReadOnlyList<InstallFinding> Findings { get; init; }

    public required InstallOperationSummary Summary { get; init; }

    public required InstallResultFacts Facts { get; init; }
}

internal enum InstallFinalVerificationState
{
    NotReached,
    Failed,
    Verified,
}

internal sealed record InstallEffectApplication
{
    public required InstallEffectIdentity Identity { get; init; }

    public required InstallEffectOutcome Outcome { get; init; }
}

internal sealed record InstallApplicationProgress
{
    public required RecoveryBundlePreparation? RecoveryPreparation { get; init; }

    public required int AppliedDirectoryCount { get; init; }

    public required int AppliedTargetFileCount { get; init; }

    public required bool LifecyclePublished { get; init; }

    public required IReadOnlyList<InstallEffectApplication> Effects { get; init; }

    public required InstallFinalVerificationState FinalVerification { get; init; }
}

internal sealed record InstallApplicationLeaseContext
{
    public required InstallPlan Plan { get; init; }

    public required WorkspaceLockLease Lease { get; init; }

    public required Guid OperationId { get; init; }
}

internal sealed record InstallRecoveryCleanupRequest
{
    public required CliWorkspace Workspace { get; init; }

    public required WorkspaceLockLease Lease { get; init; }

    public required RecoveryBundlePreparation Preparation { get; init; }
}

internal sealed record InstallRecoveryOutcome
{
    public required InstallRecoveryState State { get; init; }

    public required string? ResidualPath { get; init; }
}

internal sealed record InstallRecoveryCleanupResult
{
    public required InstallFinding? Finding { get; init; }

    public required InstallRecoveryOutcome Recovery { get; init; }
}

internal enum InstallVerificationState
{
    Verified,
    Failed,
    Cancelled,
}

internal sealed record InstallVerificationResult(
    InstallVerificationState State,
    string? Cause);
