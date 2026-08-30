using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Operation;

internal sealed class InstallApplicationOperation(
    WorkspaceLockManager lockManager,
    InstallApplicationPreconditionValidator preconditionValidator,
    InstallApplicationEffectApplier effectApplier,
    InstallRecoveryOperation recoveryOperation,
    InstallAppliedVerifier verifier)
{
    private readonly WorkspaceLockManager _lockManager = lockManager;
    private readonly InstallApplicationPreconditionValidator _preconditionValidator = preconditionValidator;
    private readonly InstallApplicationEffectApplier _effectApplier = effectApplier;
    private readonly InstallRecoveryOperation _recoveryOperation = recoveryOperation;
    private readonly InstallAppliedVerifier _verifier = verifier;

    internal async ValueTask<InstallApplicationOutcome> ExecuteAsync(
        InstallPlan plan,
        CancellationToken cancellationToken)
    {
        var operationId = Guid.NewGuid();
        var progress = new InstallApplicationProgress
        {
            RecoveryPreparation = null,
            AppliedDirectoryCount = 0,
            AppliedTargetFileCount = 0,
            LifecyclePublished = false,
            Effects = [],
            FinalVerification = InstallFinalVerificationState.NotReached,
        };
        var lockRequest = new WorkspaceLockRequest(
            plan.Request.Workspace,
            InstallDefinitions.CommandIdentity,
            operationId);
        WorkspaceLockResult lockResult;
        try
        {
            lockResult = await _lockManager.AcquireAsync(
                    lockRequest,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Failed(
                plan,
                progress,
                InstallFindingCode.Interrupted,
                "Install lock acquisition was interrupted.");
        }
        catch (Exception)
        {
            return Failed(
                plan,
                progress,
                InstallFindingCode.OperationFailed,
                "Install lock acquisition failed unexpectedly.");
        }

        if (lockResult.State != WorkspaceLockState.Acquired
            || lockResult.Lease is not { } lease)
        {
            var code = lockResult.State == WorkspaceLockState.Cancelled
                ? InstallFindingCode.Interrupted
                : InstallFindingCode.WorkspaceUnsafe;
            return Failed(
                plan,
                progress,
                code,
                lockResult.Cause
                    ?? "The persistent Install workspace lease could not be acquired.");
        }

        await using (lease.ConfigureAwait(false))
        {
            return await ExecuteUnderLeaseAsync(
                    new InstallApplicationLeaseContext
                    {
                        Plan = plan,
                        Lease = lease,
                        OperationId = operationId,
                    },
                    progress,
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async ValueTask<InstallApplicationOutcome> ExecuteUnderLeaseAsync(
        InstallApplicationLeaseContext context,
        InstallApplicationProgress progress,
        CancellationToken cancellationToken)
    {
        var plan = context.Plan;
        var lease = context.Lease;
        InstallApplicationPreconditionResult precondition;
        try
        {
            precondition = await _preconditionValidator.ValidateUnderLeaseAsync(
                    lease,
                    plan,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Failed(
                plan,
                progress,
                InstallFindingCode.Interrupted,
                "Install plan revalidation was interrupted.");
        }
        catch (Exception)
        {
            return Failed(
                plan,
                progress,
                InstallFindingCode.OperationFailed,
                "Install plan revalidation failed unexpectedly.");
        }

        if (!precondition.IsValid)
        {
            return Failed(
                plan,
                progress,
                precondition.FindingCode
                    ?? throw new InvalidOperationException(
                        "A non-valid Install application precondition requires a finding code."),
                precondition.Validation.Cause
                    ?? "The complete Install plan changed or became unsafe before effects.");
        }

        var validation = precondition.Validation;

        RecoveryBundlePreparationResult preparationResult;
        try
        {
            preparationResult = await _recoveryOperation.PrepareAsync(
                    plan,
                    context.OperationId,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Failed(
                plan,
                progress,
                InstallFindingCode.Interrupted,
                "Install recovery preparation was interrupted.");
        }
        catch (Exception)
        {
            return InstallApplicationResultFactory.Failed(
                plan,
                progress,
                new InstallFinding(
                    InstallFindingCode.OperationFailed,
                    "Install recovery preparation failed unexpectedly."),
                InstallApplicationResultFactory.Recovery(
                    InstallRecoveryState.Unknown));
        }

        var preparationBoundary = ReadPreparationBoundary(
            plan,
            preparationResult,
            progress);
        if (preparationBoundary is not null)
        {
            return preparationBoundary;
        }

        progress = progress with
        {
            RecoveryPreparation = preparationResult.Preparation,
        };
        var checkIndex = 0;
        var effectIndex = 0;
        foreach (var creation in plan.DirectoryCreations)
        {
            var identity = plan.Effects[effectIndex++];
            DirectoryCreationReceipt receipt;
            try
            {
                receipt = await _effectApplier.ApplyDirectoryAsync(
                        lease,
                        creation,
                        validation.Checks[checkIndex++],
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                progress = RecordUnknown(progress, identity);
                return Failed(
                    plan,
                    progress,
                    InstallFindingCode.Interrupted,
                    "Install directory application was interrupted.");
            }
            catch (Exception)
            {
                progress = RecordUnknown(progress, identity);
                return Failed(
                    plan,
                    progress,
                    InstallFindingCode.OperationFailed,
                    "Install directory application failed unexpectedly.");
            }

            if (receipt.EffectState == FilesystemEffectState.Applied)
            {
                progress = progress with
                {
                    AppliedDirectoryCount = progress.AppliedDirectoryCount + 1,
                };
            }

            progress = Record(progress, identity, ReadOutcome(receipt));

            if (receipt.EffectState != FilesystemEffectState.Applied
                || receipt.VerificationState != FilesystemVerificationState.Verified)
            {
                var code = receipt.NotStartedReason == FilesystemNotStartedReason.Cancelled
                    ? InstallFindingCode.Interrupted
                    : InstallFindingCode.WriteFailed;
                return Failed(
                    plan,
                    progress,
                    code,
                    receipt.Cause
                        ?? "A planned Install directory could not be created and verified.");
            }
        }

        foreach (var effect in plan.TargetEffects)
        {
            var identity = plan.Effects[effectIndex++];
            FileChangeReceipt receipt;
            try
            {
                receipt = await _effectApplier.ApplyFileAsync(
                        lease,
                        effect,
                        validation.Checks[checkIndex++],
                        progress.RecoveryPreparation,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                progress = RecordUnknown(progress, identity);
                return Failed(
                    plan,
                    progress,
                    InstallFindingCode.Interrupted,
                    "Install target application was interrupted.",
                    subject: effect.RelativePath);
            }
            catch (Exception)
            {
                progress = RecordUnknown(progress, identity);
                return Failed(
                    plan,
                    progress,
                    InstallFindingCode.OperationFailed,
                    "Install target application failed unexpectedly.",
                    subject: effect.RelativePath);
            }

            if (receipt.EffectState == FilesystemEffectState.Applied)
            {
                progress = progress with
                {
                    AppliedTargetFileCount = progress.AppliedTargetFileCount + 1,
                };
            }

            progress = Record(progress, identity, ReadOutcome(receipt));

            if (receipt.EffectState != FilesystemEffectState.Applied
                || receipt.VerificationState != FilesystemVerificationState.Verified)
            {
                return Failed(
                    plan,
                    progress,
                    ReadTargetReceiptFinding(receipt),
                    receipt.Cause
                        ?? "A planned Install target could not be applied and verified.",
                    subject: effect.RelativePath);
            }
        }

        InstallVerificationResult targetVerification;
        try
        {
            targetVerification = await _verifier.VerifyTargetsAsync(
                    plan,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Failed(
                plan,
                progress,
                InstallFindingCode.Interrupted,
                "Whole-target Install verification was interrupted.");
        }
        catch (Exception)
        {
            return Failed(
                plan,
                progress,
                InstallFindingCode.VerificationFailed,
                "Whole-target Install verification failed unexpectedly.");
        }

        if (targetVerification.State != InstallVerificationState.Verified)
        {
            if (targetVerification.State == InstallVerificationState.Failed)
            {
                progress = progress with
                {
                    FinalVerification = InstallFinalVerificationState.Failed,
                };
            }

            var code = targetVerification.State == InstallVerificationState.Cancelled
                ? InstallFindingCode.Interrupted
                : InstallFindingCode.VerificationFailed;
            return Failed(
                plan,
                progress,
                code,
                targetVerification.Cause
                    ?? "Whole-target Install verification was interrupted.");
        }

        if (plan.LifecycleEffect is { } lifecycleEffect)
        {
            var identity = plan.Effects[effectIndex];
            FileChangeReceipt receipt;
            try
            {
                receipt = await _effectApplier.ApplyFileAsync(
                        lease,
                        lifecycleEffect,
                        validation.Checks[checkIndex],
                        progress.RecoveryPreparation,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                progress = RecordUnknown(progress, identity);
                return Failed(
                    plan,
                    progress,
                    InstallFindingCode.Interrupted,
                    "Framework lifecycle publication was interrupted.",
                    subject: lifecycleEffect.RelativePath);
            }
            catch (Exception)
            {
                progress = RecordUnknown(progress, identity);
                return Failed(
                    plan,
                    progress,
                    InstallFindingCode.OperationFailed,
                    "Framework lifecycle publication failed unexpectedly.",
                    subject: lifecycleEffect.RelativePath);
            }

            if (receipt.EffectState == FilesystemEffectState.Applied)
            {
                progress = progress with
                {
                    LifecyclePublished = true,
                };
            }

            progress = Record(progress, identity, ReadOutcome(receipt));

            if (receipt.EffectState != FilesystemEffectState.Applied
                || receipt.VerificationState != FilesystemVerificationState.Verified)
            {
                var code = receipt.NotStartedReason == FilesystemNotStartedReason.Cancelled
                    ? InstallFindingCode.Interrupted
                    : InstallFindingCode.LifecyclePublicationFailed;
                return Failed(
                    plan,
                    progress,
                    code,
                    receipt.Cause
                        ?? "The Framework lifecycle state could not be published and verified.",
                    subject: lifecycleEffect.RelativePath);
            }
        }

        InstallVerificationResult verification;
        try
        {
            verification = await _verifier.VerifyAsync(plan, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Failed(
                plan,
                progress,
                InstallFindingCode.Interrupted,
                "Whole-operation Install verification was interrupted.");
        }
        catch (Exception)
        {
            return Failed(
                plan,
                progress,
                InstallFindingCode.VerificationFailed,
                "Whole-operation Install verification failed unexpectedly.");
        }

        if (verification.State != InstallVerificationState.Verified)
        {
            if (verification.State == InstallVerificationState.Failed)
            {
                progress = progress with
                {
                    FinalVerification = InstallFinalVerificationState.Failed,
                };
            }

            var code = verification.State == InstallVerificationState.Cancelled
                ? InstallFindingCode.Interrupted
                : InstallFindingCode.VerificationFailed;
            return Failed(
                plan,
                progress,
                code,
                verification.Cause
                    ?? "Whole-operation Install verification was interrupted.");
        }
        progress = progress with
        {
            FinalVerification = InstallFinalVerificationState.Verified,
        };

        if (progress.RecoveryPreparation is not { } preparation)
        {
            return InstallApplicationResultFactory.Complete(
                plan,
                progress,
                InstallApplicationResultFactory.Recovery(
                    InstallRecoveryState.NotRequired));
        }

        var cleanup = await _recoveryOperation.CleanupAsync(
                new InstallRecoveryCleanupRequest
                {
                    Workspace = plan.Request.Workspace,
                    Lease = lease,
                    Preparation = preparation,
                },
                cancellationToken)
            .ConfigureAwait(false);
        return cleanup.Finding is null
            ? InstallApplicationResultFactory.Complete(
                plan,
                progress,
                cleanup.Recovery)
            : InstallApplicationResultFactory.Failed(
                plan,
                progress,
                cleanup.Finding,
                cleanup.Recovery);
    }

    private static InstallApplicationOutcome? ReadPreparationBoundary(
        InstallPlan plan,
        RecoveryBundlePreparationResult result,
        InstallApplicationProgress progress)
    {
        return result.State switch
        {
            RecoveryBundlePreparationState.NotNeeded
                or RecoveryBundlePreparationState.Prepared => null,
            RecoveryBundlePreparationState.Incomplete => InstallApplicationResultFactory.Failed(
                plan,
                progress,
                new InstallFinding(
                    InstallFindingCode.RecoveryUnavailable,
                    result.Cause
                        ?? "Install recovery storage is unavailable before target effects."),
                ReadPreparationRecovery(plan, result)),
            RecoveryBundlePreparationState.Blocked => InstallApplicationResultFactory.Failed(
                plan,
                progress,
                new InstallFinding(
                    InstallFindingCode.RecoveryConflict,
                    result.Cause
                        ?? "Install recovery preparation is blocked before target effects."),
                ReadPreparationRecovery(plan, result)),
            RecoveryBundlePreparationState.Cancelled => InstallApplicationResultFactory.Failed(
                plan,
                progress,
                new InstallFinding(
                    InstallFindingCode.Interrupted,
                    "Install recovery preparation was interrupted."),
                ReadPreparationRecovery(plan, result)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(result),
                result.State,
                "The recovery preparation state is not defined."),
        };
    }

    private static InstallApplicationOutcome Failed(
        InstallPlan plan,
        InstallApplicationProgress progress,
        InstallFindingCode findingCode,
        string cause,
        string? subject = null)
        => InstallApplicationResultFactory.Failed(
            plan,
            progress,
            new InstallFinding(code: findingCode, cause: cause, subject: subject));

    private static InstallRecoveryOutcome ReadPreparationRecovery(
        InstallPlan plan,
        RecoveryBundlePreparationResult result)
    {
        if (result.ResidualPath is null)
        {
            return InstallApplicationResultFactory.Recovery(
                plan.RequiresRecovery
                    ? InstallRecoveryState.NotCreated
                    : InstallRecoveryState.NotRequired);
        }

        return InstallApplicationResultFactory.Recovery(
            InstallRecoveryState.Unknown,
            result.ResidualPath);
    }

    private static InstallFindingCode ReadTargetReceiptFinding(
        FileChangeReceipt receipt)
    {
        if (receipt.NotStartedReason == FilesystemNotStartedReason.Cancelled)
        {
            return InstallFindingCode.Interrupted;
        }

        return receipt.VerificationState == FilesystemVerificationState.Failed
            ? InstallFindingCode.VerificationFailed
            : InstallFindingCode.WriteFailed;
    }

    private static InstallApplicationProgress RecordUnknown(
        InstallApplicationProgress progress,
        InstallEffectIdentity identity)
        => Record(progress, identity, InstallEffectOutcome.CompletionUnknown);

    private static InstallApplicationProgress Record(
        InstallApplicationProgress progress,
        InstallEffectIdentity identity,
        InstallEffectOutcome outcome)
        => progress with
        {
            Effects = progress.Effects.Append(new InstallEffectApplication
            {
                Identity = identity,
                Outcome = outcome,
            }).ToArray(),
        };

    private static InstallEffectOutcome ReadOutcome(
        FilesystemEffectState effect,
        FilesystemVerificationState verification)
        => (effect, verification) switch
        {
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted)
                => InstallEffectOutcome.NotStarted,
            (FilesystemEffectState.Applied, FilesystemVerificationState.Verified)
                => InstallEffectOutcome.Verified,
            (FilesystemEffectState.Applied, FilesystemVerificationState.Failed)
                => InstallEffectOutcome.VerificationFailed,
            (FilesystemEffectState.Unknown, _) => InstallEffectOutcome.CompletionUnknown,
            _ => throw new InvalidOperationException(
                "The filesystem application receipt is internally inconsistent."),
        };

    private static InstallEffectOutcome ReadOutcome(DirectoryCreationReceipt receipt)
        => ReadOutcome(receipt.EffectState, receipt.VerificationState);

    private static InstallEffectOutcome ReadOutcome(FileChangeReceipt receipt)
        => ReadOutcome(receipt.EffectState, receipt.VerificationState);
}
