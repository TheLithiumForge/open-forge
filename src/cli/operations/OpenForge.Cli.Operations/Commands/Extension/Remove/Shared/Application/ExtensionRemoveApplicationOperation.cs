using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Application;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Planning;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Framework.Settings.Shared.Completion;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using static OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Application.ExtensionRemoveApplicationResultFactory;
using static OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Application.ExtensionRemovePlanComparer;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Application;

internal sealed class ExtensionRemoveApplicationOperation(
    WorkspaceLockManager lockManager,
    ExtensionRemovePlanner planner,
    MutationRevalidator revalidator,
    ExtensionPermissionOperation permissions,
    DirectoryCreationApplier directoryApplier,
    FileChangeApplier fileApplier)
{
    private readonly ExtensionPermissionOperation _permissions = permissions;
    private readonly WorkspaceLockManager _lockManager = lockManager;
    private readonly ExtensionRemovePlanner _planner = planner;
    private readonly MutationRevalidator _revalidator = revalidator;
    private readonly DirectoryCreationApplier _directoryApplier = directoryApplier;
    private readonly FileChangeApplier _fileApplier = fileApplier;

    internal async ValueTask<ExtensionRemoveResult> ExecuteAsync(
        ExtensionRemoveExecutionPlan execution,
        ExtensionRemoveResult planned,
        CancellationToken cancellationToken)
    {
        var plan = execution.Content;
        planned = planned with
        {
            Permissions = planned.Permissions with
            {
                Outcome = execution.Permission.Change is null ? WorkspacePermissionOutcome.NotRequested : WorkspacePermissionOutcome.NotStarted,
            },
        };
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
                execution,
                planned,
                lease,
                operationId,
                cancellationToken).ConfigureAwait(false);
        }
    }

    internal static IReadOnlyList<PlannedFileChange> ReadChanges(ExtensionRemovePlan plan)
    {
        IEnumerable<PlannedFileChange> settingsChanges = plan.SettingsChange is { } settingsChange
            ? [settingsChange]
            : Array.Empty<PlannedFileChange>();
        return [.. settingsChanges.Concat(plan.Effects
            .Select(effect => effect.FileChange)
            .Where(change => change is not null)
            .Cast<PlannedFileChange>()
            .Append(plan.OwnershipChange)
            .Where(change => change is not null)
            .Cast<PlannedFileChange>())];
    }

    internal static IReadOnlyList<PlannedDirectoryCreation> ReadDirectoryCreations(
        ExtensionRemovePlan plan)
        => [.. plan.Effects
            .Select(effect => effect.DirectoryCreation)
            .Where(creation => creation is not null)
            .Cast<PlannedDirectoryCreation>()];

    private static IReadOnlyList<PlannedFileChange> ReadChanges(ExtensionRemoveExecutionPlan execution)
    {
        IEnumerable<PlannedFileChange> settingsChanges = execution.SettingsChange is { } settingsChange
            ? [settingsChange]
            : Array.Empty<PlannedFileChange>();
        return [.. settingsChanges.Concat(ReadChanges(execution.Content)
            .Where(change => change != execution.Content.SettingsChange))];
    }

    private async ValueTask<ExtensionRemoveResult> ExecuteUnderLeaseAsync(
        ExtensionRemoveExecutionPlan execution,
        ExtensionRemoveResult planned,
        WorkspaceLockLease lease,
        Guid operationId,
        CancellationToken cancellationToken)
    {
        var plan = execution.Content;
        ExtensionRemovePlanBuild rebuilt;
        try
        {
            rebuilt = await _planner.BuildAsync(plan.Request, plan.Selection, cancellationToken)
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
            validation = execution.DirectoryCreations.IsEmpty
                ? await _revalidator.ValidateAsync(
                    lease,
                    ReadChanges(execution),
                    cancellationToken).ConfigureAwait(false)
                : await _revalidator.ValidateAsync(
                    lease,
                    execution.DirectoryCreations,
                    ReadChanges(execution),
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

        try
        {
            if (!await _permissions.RevalidateAsync(lease, execution.Permission, cancellationToken).ConfigureAwait(false))
            {
                return BeforeEffects(plan, planned, ExtensionRemoveFindingCode.PermissionsChanged, "Consumer permission changed after review.");
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return BeforeEffects(plan, planned, ExtensionRemoveFindingCode.Interrupted, "Extension permission revalidation was interrupted.");
        }
        catch (Exception)
        {
            return BeforeEffects(plan, planned, ExtensionRemoveFindingCode.PermissionsChanged, "Consumer permission could not be revalidated.");
        }

        RecoveryBundlePreparationResult preparationResult;
        try
        {
            preparationResult = await ExtensionRemoveRecoveryApplication.PrepareAsync(
                execution,
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
        var lifecycleOutcome = plan.OwnershipChange is null
            ? ExtensionRemoveLifecycleOutcome.AlreadyCurrent
            : ExtensionRemoveLifecycleOutcome.NotStarted;
        var verificationState = NotRequestedVerification();
        var applicationStage = ExtensionRemoveApplicationStage.Directory;
        var currentEffectIndex = -1;
        var checkIndex = 0;
        var settingsOutcome = ExtensionRemoveEffectOutcome.NotStarted;
        try
        {
            foreach (var effectIndex in Enumerable.Range(0, plan.Effects.Count)
                .Where(index => plan.Effects[index].DirectoryCreation is not null))
            {
                currentEffectIndex = effectIndex;
                applicationStage = ExtensionRemoveApplicationStage.Directory;
                var effect = plan.Effects[effectIndex];
                var creation = effect.DirectoryCreation
                    ?? throw new InvalidOperationException(
                        "A workspace settings directory effect requires one typed directory creation.");
                var receipt = await _directoryApplier.ApplyAsync(
                    lease,
                    creation,
                    validation.Checks[checkIndex++],
                    cancellationToken).ConfigureAwait(false);
                if (!IsVerified(receipt))
                {
                    effects[effectIndex] = WithOutcome(effect.Result, ReadOutcome(receipt));
                    return AfterPreparation(
                        plan,
                        planned,
                        effects,
                        preparation,
                        ReadFinding(receipt),
                        receipt.Cause ?? "The workspace settings directory could not be created and verified.",
                        effect.Result.Path,
                        settingsOutcome: settingsOutcome);
                }

                effects[effectIndex] = WithOutcome(
                    effect.Result,
                    ExtensionRemoveEffectOutcome.Verified);
            }

            if (execution.SettingsChange is { } settingsChange)
            {
                applicationStage = ExtensionRemoveApplicationStage.Settings;
                var receipt = await _fileApplier.ApplyAsync(
                    lease,
                    settingsChange,
                    validation.Checks[checkIndex++],
                    preparation,
                    cancellationToken).ConfigureAwait(false);
                settingsOutcome = ReadOutcome(receipt);
                if (execution.Permission.Change is not null)
                {
                    planned = planned with
                    {
                        Permissions = planned.Permissions with
                        {
                            Outcome = WorkspacePermissionReceiptProjection.ReadOutcome(receipt),
                        },
                    };
                }

                if (!IsVerified(receipt))
                {
                    return AfterPreparation(
                        plan,
                        planned,
                        effects,
                        preparation,
                        execution.Permission.Change is null
                            ? ReadFinding(receipt)
                            : ExtensionRemoveFindingCode.PermissionWriteFailed,
                        "The removal settings could not be written and verified.",
                        WorkspaceSettingsDefinitions.RelativePath,
                        settingsOutcome: settingsOutcome);
                }

                if (plan.SettingsEffect is not null)
                {
                    settingsOutcome = ExtensionRemoveEffectOutcome.Verified;
                }
            }

            for (var effectIndex = 0; effectIndex < plan.Effects.Count; effectIndex++)
            {
                currentEffectIndex = effectIndex;
                applicationStage = ExtensionRemoveApplicationStage.TargetEffect;
                var effect = plan.Effects[effectIndex];
                if (effect.DirectoryCreation is not null)
                {
                    continue;
                }

                var change = effect.FileChange
                    ?? throw new InvalidOperationException(
                        "Every Extension Remove file effect requires one typed file change.");
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
                        effect.Result.Path,
                        settingsOutcome: settingsOutcome);
                }

                effects[effectIndex] = WithOutcome(
                    effect.Result,
                    ExtensionRemoveEffectOutcome.Verified);
            }

            currentEffectIndex = -1;
            if (plan.OwnershipChange is { } ownershipChange)
            {
                applicationStage = ExtensionRemoveApplicationStage.Ownership;
                var receipt = await _fileApplier.ApplyAsync(
                    lease,
                    ownershipChange,
                    validation.Checks[checkIndex++],
                    preparation,
                    cancellationToken).ConfigureAwait(false);
                lifecycleOutcome = ReadLifecycleOutcome(receipt);
                if (!IsVerified(receipt))
                {
                    return AfterPreparation(
                        plan,
                        planned,
                        effects,
                        preparation,
                        ReadFinding(receipt),
                        receipt.Cause
                            ?? "The workspace ownership lock could not be applied and verified.",
                        WorkspaceOwnershipDefinitions.RelativePath,
                        lifecycleOutcome,
                        verificationState,
                        settingsOutcome);
                }
            }

            applicationStage = ExtensionRemoveApplicationStage.Verification;
            var verification = await _planner.BuildForVerificationAsync(
                plan.Request,
                plan.Selection,
                preparation,
                cancellationToken).ConfigureAwait(false);
            if ((verification.Plan is not null && !verification.Plan.IsNoOp)
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
                    FailedVerification(),
                    settingsOutcome);
            }

            verificationState = Verified();
            applicationStage = ExtensionRemoveApplicationStage.Cleanup;
            var cleanup = await ExtensionRemoveRecoveryApplication.VerifyRetainedAsync(
                plan,
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
                finalFindings,
                settingsOutcome);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            var progress = ReadEscapedProgress(
                effects,
                applicationStage,
                currentEffectIndex,
                lifecycleOutcome,
                verificationState,
                settingsOutcome);
            if (execution.Permission.Change is not null)
            {
                planned = planned with
                {
                    Permissions = planned.Permissions with
                    {
                        Outcome = ReadPermissionOutcome(progress.Settings),
                    },
                };
            }
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
                    "Extension Remove application was interrupted."))],
                progress.Settings);
        }
        catch (Exception)
        {
            var progress = ReadEscapedProgress(
                effects,
                applicationStage,
                currentEffectIndex,
                lifecycleOutcome,
                verificationState,
                settingsOutcome);
            if (execution.Permission.Change is not null)
            {
                planned = planned with
                {
                    Permissions = planned.Permissions with
                    {
                        Outcome = ReadPermissionOutcome(progress.Settings),
                    },
                };
            }
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
                    "Extension Remove application failed unexpectedly."))],
                progress.Settings);
        }
    }

    private static (
        ExtensionRemoveLifecycleOutcome Lifecycle,
        ExtensionRemoveVerification Verification,
        ExtensionRemoveEffectOutcome Settings) ReadEscapedProgress(
        ExtensionRemoveEffect[] effects,
        ExtensionRemoveApplicationStage stage,
        int currentEffectIndex,
        ExtensionRemoveLifecycleOutcome lifecycle,
        ExtensionRemoveVerification verification,
        ExtensionRemoveEffectOutcome settings)
    {
        switch (stage)
        {
            case ExtensionRemoveApplicationStage.Directory when currentEffectIndex >= 0:
                effects[currentEffectIndex] = WithOutcome(
                    effects[currentEffectIndex],
                    ExtensionRemoveEffectOutcome.CompletionUnknown);
                break;

            case ExtensionRemoveApplicationStage.Directory:
                break;

            case ExtensionRemoveApplicationStage.Settings:
                settings = ExtensionRemoveEffectOutcome.CompletionUnknown;
                break;

            case ExtensionRemoveApplicationStage.TargetEffect when currentEffectIndex >= 0:
                effects[currentEffectIndex] = WithOutcome(
                    effects[currentEffectIndex],
                    ExtensionRemoveEffectOutcome.CompletionUnknown);
                break;

            case ExtensionRemoveApplicationStage.Ownership:
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

        return (lifecycle, verification, settings);
    }

    private static WorkspacePermissionOutcome ReadPermissionOutcome(
        ExtensionRemoveEffectOutcome outcome)
        => outcome switch
        {
            ExtensionRemoveEffectOutcome.Planned => WorkspacePermissionOutcome.Planned,
            ExtensionRemoveEffectOutcome.NotStarted => WorkspacePermissionOutcome.NotStarted,
            ExtensionRemoveEffectOutcome.Verified => WorkspacePermissionOutcome.Verified,
            ExtensionRemoveEffectOutcome.VerificationFailed => WorkspacePermissionOutcome.VerificationFailed,
            ExtensionRemoveEffectOutcome.CompletionUnknown => WorkspacePermissionOutcome.CompletionUnknown,
            _ => throw new ArgumentOutOfRangeException(
                nameof(outcome),
                outcome,
                "The Extension Remove settings effect outcome is not defined."),
        };

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
