using OpenForge.Cli.Core.Commands.Extension.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Models.Operation;

internal enum ExtensionUpdatePlanRevalidationState
{
    Exact,
    Changed,
    Failed,
    Interrupted,
}

internal sealed record ExtensionUpdatePlanRevalidation
{
    internal required ExtensionUpdatePlanRevalidationState State { get; init; }

    internal required string? Cause { get; init; }

    internal required IReadOnlyList<FileExpectationValidationResult> Checks { get; init; }
}

internal sealed record ExtensionUpdateApplicationProgress
{
    internal required IReadOnlyList<FileChangeReceipt> EffectReceipts { get; init; }

    internal FileChangeReceipt? LifecycleReceipt { get; init; }

    internal RecoveryBundlePreparation? Recovery { get; init; }

    internal required ExtensionUpdateVerification Verification { get; init; }
}

internal sealed record ExtensionUpdateApplicationAttempt
{
    internal required IReadOnlyList<FileChangeReceipt> EffectReceipts { get; init; }

    internal FileChangeReceipt? LifecycleReceipt { get; init; }

    internal ExtensionUpdateFinding? Finding { get; init; }
}

internal sealed record ExtensionUpdateRecoveryCleanup
{
    internal required ExtensionUpdateRecovery Recovery { get; init; }

    internal ExtensionUpdateFinding? Finding { get; init; }
}

internal sealed record ExtensionUpdateAppliedVerification
{
    internal required ExtensionUpdateVerificationState State { get; init; }

    internal ExtensionUpdateFinding? Finding { get; init; }
}

internal sealed record ExtensionUpdateApplicationLease
{
    internal required WorkspaceLockLease Lease { get; init; }

    internal required Guid OperationId { get; init; }
}

internal sealed record ExtensionUpdateApplicationOutcome
{
    internal required IReadOnlyList<ExtensionUpdateEffect> Effects { get; init; }

    internal required ExtensionUpdateLifecycle Lifecycle { get; init; }

    internal required ExtensionUpdateRecovery Recovery { get; init; }

    internal required ExtensionUpdateVerification Verification { get; init; }

    internal ExtensionUpdateFinding? Finding { get; init; }
}
