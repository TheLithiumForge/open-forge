using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using static OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Application.ExtensionRemoveApplicationResultFactory;
using static OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Application.ExtensionRemovePlanComparer;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Application;

internal sealed class ExtensionRemoveApplicationOperation(
    WorkspaceLockManager lockManager,
    ExtensionRemovePlanner planner,
    MutationRevalidator revalidator,
    FileChangeApplier fileApplier)
{
    private readonly WorkspaceLockManager _lockManager = lockManager;
    private readonly ExtensionRemovePlanner _planner = planner;
    private readonly MutationRevalidator _revalidator = revalidator;
    private readonly FileChangeApplier _fileApplier = fileApplier;

    internal async ValueTask<ExtensionRemoveResult> ExecuteAsync(
        ExtensionRemovePlan plan,
        ExtensionRemoveResult planned,
        CancellationToken cancellationToken)
    {
        var operationId = Guid.NewGuid();
        WorkspaceLockResult lockResult;
        try
        {
            lockResult = await _lockManager.AcquireAsync(
                new WorkspaceLockRequest(
                    plan.Request.Workspace,
                    ExtensionRemoveDefinitions.CommandIdentity,
                    operationId),
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return BeforeEffects(
                plan,
                planned,
                ExtensionRemoveFindingCode.Interrupted,
                "Extension Remove lock acquisition was interrupted.");
        }
        catch (Exception)
        {
            return BeforeEffects(
                plan,
                planned,
                ExtensionRemoveFindingCode.WorkspaceLockUnavailable,
                "Extension Remove lock acquisition failed unexpectedly.");
        }

        if (lockResult.State != WorkspaceLockState.Acquired
            || lockResult.Lease is not { } lease)
        {
            return BeforeEffects(
                plan,
                planned,
                lockResult.State == WorkspaceLockState.Cancelled
                    ? ExtensionRemoveFindingCode.Interrupted
                    : ExtensionRemoveFindingCode.WorkspaceLockUnavailable,
                lockResult.Cause
                    ?? "The persistent Extension Remove workspace lease could not be acquired.");
        }

        await using (lease.ConfigureAwait(false))
        {
            return await ExecuteUnderLeaseAsync(
                plan,
                planned,
                lease,
                operationId,
                cancellationToken).ConfigureAwait(false);
        }
    }

    internal static IReadOnlyList<PlannedFileChange> ReadChanges(ExtensionRemovePlan plan)
        => [.. plan.Effects
            .Select(effect => effect.FileChange)
            .Where(change => change is not null)
            .Cast<PlannedFileChange>()
            .Append(plan.LifecycleChange)
            .Where(change => change is not null)
            .Cast<PlannedFileChange>()];

    private async ValueTask<ExtensionRemoveResult> ExecuteUnderLeaseAsync(
        ExtensionRemovePlan plan,
        ExtensionRemoveResult planned,
        WorkspaceLockLease lease,
        Guid operationId,
        CancellationToken cancellationToken)
    {
        ExtensionRemovePlanBuild rebuilt;
        try
        {
            rebuilt = await _planner.BuildAsync(plan.Request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return BeforeEffects(
                plan,
                planned,
                ExtensionRemoveFindingCode.Interrupted,
                "Extension Remove semantic revalidation was interrupted.");
        }
        catch (Exception)
        {
            return BeforeEffects(
                plan,
                planned,
                ExtensionRemoveFindingCode.OperationFailed,
                "Extension Remove semantic revalidation failed unexpectedly.");
        }

        if (rebuilt.Plan is not { } current || !Matches(plan, current))
        {
            return BeforeEffects(
                plan,
                planned,
                ExtensionRemoveFindingCode.TargetChanged,
                "Lifecycle, ownership, topology, or target facts changed after planning.");
        }

        MutationValidationResult validation;
        try
        {
            validation = await _revalidator.ValidateAsync(
                lease,
                ReadChanges(plan),
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return BeforeEffects(
                plan,
                planned,
                ExtensionRemoveFindingCode.Interrupted,
                "Extension Remove plan revalidation was interrupted.");
        }
        catch (Exception)
        {
            return BeforeEffects(
                plan,
                planned,
                ExtensionRemoveFindingCode.OperationFailed,
                "Extension Remove plan revalidation failed unexpectedly.");
        }

        if (validation.State != MutationValidationState.Valid)
        {
            return BeforeEffects(
                plan,
                planned,
                validation.State == MutationValidationState.Cancelled
                    ? ExtensionRemoveFindingCode.Interrupted
                    : ExtensionRemoveFindingCode.TargetChanged,
                validation.Cause ?? "An Extension Remove target changed before effects.");
        }

        RecoveryBundlePreparationResult preparationResult;
        try
        {
            preparationResult = await ExtensionRemoveRecoveryApplication.PrepareAsync(
                plan,
                operationId,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return BeforeEffects(
                plan,
                planned,
                ExtensionRemoveFindingCode.Interrupted,
                "Extension Remove recovery preparation was interrupted.",
                RecoveryUnknown(residualPath: null));
        }
        catch (Exception)
        {
            return BeforeEffects(
                plan,
                planned,
                ExtensionRemoveFindingCode.RecoveryUnavailable,
                "Extension Remove recovery preparation failed unexpectedly.",
                RecoveryUnknown(residualPath: null));
        }

        if (preparationResult.State is not RecoveryBundlePreparationState.Prepared
            and not RecoveryBundlePreparationState.NotNeeded)
        {
            return BeforeEffects(
                plan,
                planned,
                ReadPreparationFindingCode(preparationResult.State),
                preparationResult.Cause ?? "Extension Remove recovery preparation is unavailable.",
                RecoveryUnknown(preparationResult.ResidualPath));
        }

        var preparation = preparationResult.Preparation;
        var effects = plan.Effects
            .Select(effect => WithOutcome(effect.Result, ExtensionRemoveEffectOutcome.NotStarted))
            .ToArray();
        var lifecycleOutcome = plan.LifecycleChange is null
            ? ExtensionRemoveLifecycleOutcome.AlreadyCurrent
            : ExtensionRemoveLifecycleOutcome.NotStarted;
        var verificationState = NotRequestedVerification();
        var applicationStage = ExtensionRemoveApplicationStage.TargetEffect;
        var currentEffectIndex = -1;
        var checkIndex = 0;
        try
        {
            for (var effectIndex = 0; effectIndex < plan.Effects.Count; effectIndex++)
            {
                currentEffectIndex = effectIndex;
                applicationStage = ExtensionRemoveApplicationStage.TargetEffect;
                var effect = plan.Effects[effectIndex];
                var change = effect.FileChange
                    ?? throw new InvalidOperationException(
                        "Every Extension Remove target effect requires one typed file change.");
                var receipt = await _fileApplier.ApplyAsync(
                    lease,
                    change,
                    validation.Checks[checkIndex++],
                    preparation,
                    cancellationToken).ConfigureAwait(false);
                if (!IsVerified(receipt))
                {
                    effects[effectIndex] = WithOutcome(effect.Result, ReadOutcome(receipt));
                    for (var remaining = effectIndex + 1; remaining < effects.Length; remaining++)
                    {
                        effects[remaining] = WithOutcome(
                            effects[remaining],
                            ExtensionRemoveEffectOutcome.NotStarted);
                    }

                    return AfterPreparation(
                        plan,
                        planned,
                        effects,
                        preparation,
                        ReadFinding(receipt),
                        receipt.Cause ?? "An Extension Remove target could not be applied and verified.",
                        effect.Result.Path);
                }

                effects[effectIndex] = WithOutcome(
                    effect.Result,
                    ExtensionRemoveEffectOutcome.Verified);
            }

            currentEffectIndex = -1;
            if (plan.LifecycleChange is { } lifecycleChange)
            {
                applicationStage = ExtensionRemoveApplicationStage.Lifecycle;
                var receipt = await _fileApplier.ApplyAsync(
                    lease,
                    lifecycleChange,
                    validation.Checks[checkIndex],
                    preparation,
                    cancellationToken).ConfigureAwait(false);
                if (!IsVerified(receipt))
                {
                    lifecycleOutcome = ReadLifecycleOutcome(receipt);
                    return AfterPreparation(
                        plan,
                        planned,
                        effects,
                        preparation,
                        ReadLifecycleFinding(receipt),
                        receipt.Cause
                            ?? "The Extension lifecycle could not be published and verified.",
                        ".agents/open-forge.lifecycle.json",
                        lifecycleOutcome,
                        verificationState);
                }

                lifecycleOutcome = ExtensionRemoveLifecycleOutcome.Verified;
            }

            applicationStage = ExtensionRemoveApplicationStage.Verification;
            var verification = await _planner.BuildForVerificationAsync(
                plan.Request,
                preparation,
                cancellationToken).ConfigureAwait(false);
            if (verification.Plan is not null
                || verification.Result.Status is not (
                    Shell.Definitions.CliSemanticStatus.Complete
                    or Shell.Definitions.CliSemanticStatus.Attention)
                || verification.Result.Effects.Count != 0)
            {
                return AfterPreparation(
                    plan,
                    planned,
                    effects,
                    preparation,
                    ExtensionRemoveFindingCode.VerificationFailed,
                    "Final Extension Remove topology or lifecycle verification did not match the plan.",
                    target: null,
                    lifecycleOutcome,
                    FailedVerification());
            }

            verificationState = Verified();
            applicationStage = ExtensionRemoveApplicationStage.Cleanup;
            var cleanup = await ExtensionRemoveRecoveryApplication.CleanupAsync(
                plan,
                lease,
                preparation,
                cancellationToken).ConfigureAwait(false);
            var finalFindings = cleanup.Finding is null
                ? planned.Findings
                : [.. planned.Findings.Append(cleanup.Finding)];
            return Result(
                plan,
                planned,
                effects,
                Lifecycle(plan, lifecycleOutcome),
                cleanup.Recovery,
                verificationState,
                finalFindings);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            var progress = ReadEscapedProgress(
                effects,
                applicationStage,
                currentEffectIndex,
                lifecycleOutcome,
                verificationState);
            var recovery = applicationStage == ExtensionRemoveApplicationStage.Cleanup
                ? RecoveryAfterCleanupEscape(preparation)
                : RecoveryAfterFailure(preparation);
            return Result(
                plan,
                planned,
                effects,
                Lifecycle(plan, progress.Lifecycle),
                recovery,
                progress.Verification,
                [.. planned.Findings.Append(new ExtensionRemoveFinding(
                    ExtensionRemoveFindingCode.Interrupted,
                    "Extension Remove application was interrupted."))]);
        }
        catch (Exception)
        {
            var progress = ReadEscapedProgress(
                effects,
                applicationStage,
                currentEffectIndex,
                lifecycleOutcome,
                verificationState);
            var recovery = applicationStage == ExtensionRemoveApplicationStage.Cleanup
                ? RecoveryAfterCleanupEscape(preparation)
                : RecoveryAfterFailure(preparation);
            return Result(
                plan,
                planned,
                effects,
                Lifecycle(plan, progress.Lifecycle),
                recovery,
                progress.Verification,
                [.. planned.Findings.Append(new ExtensionRemoveFinding(
                    ExtensionRemoveFindingCode.OperationFailed,
                    "Extension Remove application failed unexpectedly."))]);
        }
    }

    private static (
        ExtensionRemoveLifecycleOutcome Lifecycle,
        ExtensionRemoveVerification Verification) ReadEscapedProgress(
        ExtensionRemoveEffect[] effects,
        ExtensionRemoveApplicationStage stage,
        int currentEffectIndex,
        ExtensionRemoveLifecycleOutcome lifecycle,
        ExtensionRemoveVerification verification)
    {
        switch (stage)
        {
            case ExtensionRemoveApplicationStage.TargetEffect when currentEffectIndex >= 0:
                effects[currentEffectIndex] = WithOutcome(
                    effects[currentEffectIndex],
                    ExtensionRemoveEffectOutcome.CompletionUnknown);
                break;

            case ExtensionRemoveApplicationStage.Lifecycle:
                lifecycle = ExtensionRemoveLifecycleOutcome.CompletionUnknown;
                break;

            case ExtensionRemoveApplicationStage.Verification:
                verification = UnknownVerification();
                break;

            case ExtensionRemoveApplicationStage.Cleanup:
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(stage), stage, "The application stage is not defined.");
        }

        return (lifecycle, verification);
    }

    private static ExtensionRemoveFindingCode ReadPreparationFindingCode(
        RecoveryBundlePreparationState state)
        => state switch
        {
            RecoveryBundlePreparationState.Cancelled => ExtensionRemoveFindingCode.Interrupted,
            RecoveryBundlePreparationState.Blocked => ExtensionRemoveFindingCode.RecoveryConflict,
            RecoveryBundlePreparationState.Incomplete => ExtensionRemoveFindingCode.RecoveryUnavailable,
            RecoveryBundlePreparationState.NotNeeded
                or RecoveryBundlePreparationState.Prepared => throw new InvalidOperationException(
                    "A successful recovery preparation does not require a failure finding."),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The recovery preparation state is not defined."),
        };
}

internal enum ExtensionRemoveApplicationStage
{
    TargetEffect,
    Lifecycle,
    Verification,
    Cleanup,
}
