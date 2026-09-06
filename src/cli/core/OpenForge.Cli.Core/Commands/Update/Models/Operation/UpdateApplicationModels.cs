using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Update.Models.Operation;

internal enum UpdatePlanRevalidationState
{
    Exact,
    Changed,
    Failed,
    Interrupted,
}

internal sealed record UpdatePlanRevalidation(
    UpdatePlanRevalidationState State,
    string? Cause,
    IReadOnlyList<FileExpectationValidationResult> Checks);

internal sealed record UpdateApplicationAttempt(
    IReadOnlyList<FileChangeReceipt> EffectReceipts,
    FileChangeReceipt? LifecycleReceipt,
    UpdateFinding? Finding);

internal sealed record UpdateRecoveryCleanup(
    UpdateRecovery Recovery,
    UpdateFinding? Finding);

internal sealed record UpdateAppliedVerification(
    UpdateVerificationState State,
    UpdateFinding? Finding);
