using System.Collections.Immutable;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Application;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Update.Shared.Planning;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Shared.Application;

internal sealed class ExtensionUpdateApplicationOperation(
    WorkspaceLockManager lockManager,
    ExtensionUpdatePlanner planner,
    MutationRevalidator revalidator,
    ExtensionPermissionOperation permissions,
    ExtensionUpdateEffectApplier effectApplier)
{
    private readonly ExtensionPermissionOperation _permissions = permissions;
    private readonly WorkspaceLockManager _lockManager = lockManager;
    private readonly ExtensionUpdatePlanner _planner = planner;
    private readonly MutationRevalidator _revalidator = revalidator;
    private readonly ExtensionUpdateEffectApplier _effectApplier = effectApplier;

    internal async ValueTask<ExtensionUpdateApplicationOutcome> ExecuteAsync(
        ExtensionUpdateExecutionPlan execution,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(execution);
        var plan = execution.Content;
        var operationId = Guid.NewGuid();
        WorkspaceLockResult lockResult;
        try
        {
            lockResult = await _lockManager.AcquireAsync(
                new WorkspaceLockRequest(
                    plan.Request.Workspace,
                    ExtensionUpdateDefinitions.CommandIdentity,
                    operationId),
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Stop(execution, ExtensionUpdateFindingCode.Interrupted,
                "Extension Update lock acquisition was interrupted.");
        }
        catch (Exception)
        {
            return Stop(execution, ExtensionUpdateFindingCode.WorkspaceLockUnavailable,
                "Extension Update lock acquisition failed unexpectedly.");
        }

        if (lockResult.State != WorkspaceLockState.Acquired
            || lockResult.Lease is not { } lease)
        {
            return Stop(
                execution,
                lockResult.State == WorkspaceLockState.Cancelled
                    ? ExtensionUpdateFindingCode.Interrupted
                    : ExtensionUpdateFindingCode.WorkspaceLockUnavailable,
                lockResult.Cause ?? "The persistent Extension Update workspace lease could not be acquired.");
        }

        await using (lease.ConfigureAwait(false))
        {
            return await ExecuteUnderLeaseAsync(execution, lease, operationId, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async ValueTask<ExtensionUpdateApplicationOutcome> ExecuteUnderLeaseAsync(
        ExtensionUpdateExecutionPlan execution,
        WorkspaceLockLease lease,
        Guid operationId,
        CancellationToken cancellationToken)
    {
        var plan = execution.Content;
        ExtensionUpdatePlanBuild rebuilt;
        try
        {
            rebuilt = await _planner.BuildAsync(plan.Request, plan.Selection, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Stop(execution, ExtensionUpdateFindingCode.Interrupted,
                "Extension Update semantic revalidation was interrupted.");
        }
        catch (Exception)
        {
            return Stop(execution, ExtensionUpdateFindingCode.OperationFailed,
                "Extension Update semantic revalidation failed unexpectedly.");
        }

        if (rebuilt.Plan is not { } current || !Matches(plan, current))
        {
            return Stop(
                execution,
                ExtensionUpdateFindingCode.TargetChanged,
                "Workspace settings, source, lifecycle, Framework, topology, ownership, or plan facts changed after planning.");
        }

        MutationValidationResult validation;
        try
        {
            validation = plan.IsNoOp ? MutationValidationResult.Valid() : await _revalidator.ValidateAsync(
                lease,
                plan.DirectoryCreations,
                plan.AllFileChanges,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Stop(execution, ExtensionUpdateFindingCode.Interrupted,
                "Extension Update plan revalidation was interrupted.");
        }
        catch (Exception)
        {
            return Stop(execution, ExtensionUpdateFindingCode.OperationFailed,
                "Extension Update plan revalidation failed unexpectedly.");
        }

        if (validation.State != MutationValidationState.Valid)
        {
            return Stop(
                execution,
                validation.State == MutationValidationState.Cancelled
                    ? ExtensionUpdateFindingCode.Interrupted
                    : ExtensionUpdateFindingCode.TargetChanged,
                validation.Cause ?? "An Extension Update target changed before effects.");
        }

        try
        {
            if (!await _permissions.RevalidateAsync(lease, execution.Permission, cancellationToken).ConfigureAwait(false))
            {
                return Stop(execution, ExtensionUpdateFindingCode.PermissionsChanged, "Consumer permission changed after review.");
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Stop(execution, ExtensionUpdateFindingCode.Interrupted, "Extension permission revalidation was interrupted.");
        }
        catch (Exception)
        {
            return Stop(execution, ExtensionUpdateFindingCode.PermissionsChanged, "Consumer permission could not be revalidated.");
        }

        RecoveryBundlePreparationResult preparationResult;
        try
        {
            preparationResult = await ExtensionUpdateRecoveryApplication.PrepareAsync(
                execution,
                operationId,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Stop(execution, ExtensionUpdateFindingCode.Interrupted,
                "Extension Update recovery preparation was interrupted.",
                context: new ExtensionUpdateApplicationFailureContext { RecoveryUnknown = true });
        }
        catch (Exception)
        {
            return Stop(execution, ExtensionUpdateFindingCode.RecoveryUnavailable,
                "Extension Update recovery preparation failed unexpectedly.",
                context: new ExtensionUpdateApplicationFailureContext { RecoveryUnknown = true });
        }
        if (preparationResult.State is not RecoveryBundlePreparationState.NotNeeded
            and not RecoveryBundlePreparationState.Prepared)
        {
            var preparationCode = ExtensionUpdateFindingCode.RecoveryUnavailable;
            if (preparationResult.State == RecoveryBundlePreparationState.Cancelled)
            {
                preparationCode = ExtensionUpdateFindingCode.Interrupted;
            }
            else if (preparationResult.State == RecoveryBundlePreparationState.Blocked)
            {
                preparationCode = ExtensionUpdateFindingCode.RecoveryConflict;
            }
            else if (preparationResult.State != RecoveryBundlePreparationState.Incomplete)
            {
                throw new InvalidOperationException("The recovery preparation state is not defined.");
            }
            return Stop(
                execution,
                preparationCode,
                preparationResult.Cause ?? "Extension Update recovery preparation is unavailable.",
                context: new ExtensionUpdateApplicationFailureContext
                {
                    RecoveryResidualPath = preparationResult.ResidualPath,
                });
        }

        var preparation = preparationResult.Preparation;
        var permissionResult = execution.Permission.Result with
        {
            Outcome = execution.Permission.Change is null ? WorkspacePermissionOutcome.NotRequested : WorkspacePermissionOutcome.NotStarted,
        };
        try
        {
            var permission = await _permissions.ApplyAsync(lease, execution.Permission, preparation, cancellationToken).ConfigureAwait(false);
            permissionResult = permission.Result;
            if (permission.Failure is { } failure)
            {
                return Stop(execution, ExtensionUpdateDefinitions.ReadPermissionFinding(failure), "The consumer permission write was not verified.",
                    context: new ExtensionUpdateApplicationFailureContext { Preparation = preparation, Permissions = permissionResult });
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Stop(execution, ExtensionUpdateFindingCode.Interrupted, "Extension permission application was interrupted.",
                context: new ExtensionUpdateApplicationFailureContext { Preparation = preparation, Permissions = permissionResult });
        }
        catch (Exception)
        {
            return Stop(execution, ExtensionUpdateFindingCode.PermissionWriteFailed, "Consumer permission write completion could not be determined.",
                context: new ExtensionUpdateApplicationFailureContext
                {
                    Preparation = preparation,
                    Permissions = permissionResult with { Outcome = WorkspacePermissionOutcome.CompletionUnknown },
                });
        }
        var effects = plan.Effects.Select(effect => effect.Result).ToArray();
        var checkIndex = 0;
        foreach (var directory in plan.DirectoryCreations)
        {
            var receipt = await _effectApplier.ApplyDirectoryAsync(
                lease,
                directory,
                validation.Checks[checkIndex++],
                cancellationToken).ConfigureAwait(false);
            if (receipt.EffectState != FilesystemEffectState.Applied
                || receipt.VerificationState != FilesystemVerificationState.Verified)
            {
                return Stop(
                    execution,
                    receipt.NotStartedReason == FilesystemNotStartedReason.Cancelled
                        ? ExtensionUpdateFindingCode.Interrupted
                        : ExtensionUpdateFindingCode.WriteFailed,
                    receipt.Cause ?? "An Extension Update parent directory could not be created and verified.",
                    directory.LogicalPath,
                    new ExtensionUpdateApplicationFailureContext
                    {
                        Permissions = permissionResult,
                        Preparation = preparation,
                    });
            }
        }

        for (var effectIndex = 0; effectIndex < plan.Effects.Count; effectIndex++)
        {
            var effect = plan.Effects[effectIndex];
            var change = effect.FileChange
                ?? throw new InvalidOperationException(
                    "Every Extension Update effect requires one typed file change.");
            var receipt = await _effectApplier.ApplyFileAsync(
                lease,
                change,
                validation.Checks[checkIndex++],
                change.Kind == PlannedFileChangeKind.Create ? null : preparation,
                cancellationToken).ConfigureAwait(false);
            if (!IsVerified(receipt))
            {
                effects[effectIndex] = WithOutcome(effect.Result, ReadOutcome(receipt));
                for (var remaining = effectIndex + 1; remaining < effects.Length; remaining++)
                {
                    effects[remaining] = WithOutcome(
                        effects[remaining],
                        ExtensionUpdateEffectOutcome.NotStarted);
                }

                return Stop(
                    execution,
                    ReadFinding(receipt),
                    receipt.Cause ?? "An Extension Update target could not be applied and verified.",
                    effect.Result.Path,
                    new ExtensionUpdateApplicationFailureContext
                    {
                        Permissions = permissionResult,
                        Effects = effects,
                        Preparation = preparation,
                    });
            }

            effects[effectIndex] = WithOutcome(effect.Result, ExtensionUpdateEffectOutcome.Verified);
        }

        var lifecycleOutcome = plan.OwnershipChange is null
            ? plan.Result.Lifecycle.Outcome
            : ExtensionUpdateLifecycleOutcome.NotStarted;
        if (plan.OwnershipChange is { } ownershipChange)
        {
            FileChangeReceipt receipt;
            try
            {
                receipt = await _effectApplier.ApplyFileAsync(
                    lease,
                    ownershipChange,
                    validation.Checks[checkIndex++],
                    preparation,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return Stop(
                    execution,
                    ExtensionUpdateFindingCode.Interrupted,
                    "Workspace ownership lock application was interrupted.",
                    WorkspaceOwnershipDefinitions.RelativePath,
                    new ExtensionUpdateApplicationFailureContext
                    {
                        Permissions = permissionResult,
                        Effects = effects,
                        Preparation = preparation,
                        LifecycleOutcome = lifecycleOutcome,
                    });
            }
            catch (Exception)
            {
                return Stop(
                    execution,
                    ExtensionUpdateFindingCode.OperationFailed,
                    "Workspace ownership lock application failed unexpectedly.",
                    WorkspaceOwnershipDefinitions.RelativePath,
                    new ExtensionUpdateApplicationFailureContext
                    {
                        Permissions = permissionResult,
                        Effects = effects,
                        Preparation = preparation,
                        LifecycleOutcome = lifecycleOutcome,
                    });
            }

            if (!IsVerified(receipt))
            {
                return Stop(
                    execution,
                    receipt.NotStartedReason == FilesystemNotStartedReason.Cancelled
                        ? ExtensionUpdateFindingCode.Interrupted
                        : ExtensionUpdateFindingCode.WriteFailed,
                    receipt.Cause
                        ?? "The workspace ownership lock could not be applied and verified.",
                    WorkspaceOwnershipDefinitions.RelativePath,
                    new ExtensionUpdateApplicationFailureContext
                    {
                        Permissions = permissionResult,
                        Effects = effects,
                        Preparation = preparation,
                        LifecycleOutcome = ExtensionUpdateLifecycleOutcome.VerificationFailed,
                    });
            }
            lifecycleOutcome = ExtensionUpdateLifecycleOutcome.Verified;
        }

        ExtensionUpdatePlanBuild verificationBuild;
        try
        {
            verificationBuild = await _planner.BuildForVerificationAsync(
                plan.Request,
                plan.Selection,
                preparation,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Stop(
                execution,
                ExtensionUpdateFindingCode.Interrupted,
                "Final Extension Update verification was interrupted.",
                context: new ExtensionUpdateApplicationFailureContext
                {
                    Permissions = permissionResult,
                    Effects = effects,
                    Preparation = preparation,
                    LifecycleOutcome = lifecycleOutcome,
                });
        }
        catch (Exception)
        {
            return Stop(
                execution,
                ExtensionUpdateFindingCode.VerificationFailed,
                "Final Extension Update verification failed unexpectedly.",
                context: new ExtensionUpdateApplicationFailureContext
                {
                    Permissions = permissionResult,
                    Effects = effects,
                    Preparation = preparation,
                    LifecycleOutcome = lifecycleOutcome,
                });
        }

        if (verificationBuild.Plan is not { } verifiedPlan
            || !AppliedMatches(plan, verifiedPlan))
        {
            return Stop(
                execution,
                ExtensionUpdateFindingCode.VerificationFailed,
                "Final Extension Update topology or lifecycle verification did not match the plan.",
                context: new ExtensionUpdateApplicationFailureContext
                {
                    Permissions = permissionResult,
                    Effects = effects,
                    Preparation = preparation,
                    LifecycleOutcome = lifecycleOutcome,
                });
        }

        var recovery = await ExtensionUpdateRecoveryApplication.VerifyRetainedAsync(
            plan,
            preparation,
            cancellationToken)
            .ConfigureAwait(false);
        if (recovery.Finding is not null)
        {
            return new ExtensionUpdateApplicationOutcome
            {
                Permissions = permissionResult,
                Effects = effects,
                Lifecycle = Lifecycle(plan, lifecycleOutcome),
                Recovery = recovery.Recovery,
                Verification = Verified(),
                Finding = recovery.Finding,
            };
        }

        return new ExtensionUpdateApplicationOutcome
        {
            Permissions = permissionResult,
            Effects = effects,
            Lifecycle = Lifecycle(plan, lifecycleOutcome),
            Recovery = recovery.Recovery,
            Verification = Verified(),
            Finding = null,
        };
    }

    private static ExtensionUpdateApplicationOutcome Stop(
        ExtensionUpdateExecutionPlan execution,
        ExtensionUpdateFinding finding,
        ExtensionUpdateApplicationFailureContext? context = null)
    {
        var plan = execution.Content;
        context ??= new ExtensionUpdateApplicationFailureContext();
        return new ExtensionUpdateApplicationOutcome
        {
            Permissions = context.Permissions ?? execution.Permission.Result with
            {
                Outcome = execution.Permission.Change is null ? WorkspacePermissionOutcome.NotRequested : WorkspacePermissionOutcome.NotStarted,
            },
            Effects = context.Effects,
            Lifecycle = Lifecycle(plan, context.LifecycleOutcome),
            Recovery = ReadRecovery(execution, context),
            Verification = new ExtensionUpdateVerification(
                ExtensionUpdateVerificationState.NotRequested,
                ExtensionUpdateVerificationState.NotRequested,
                ExtensionUpdateVerificationState.NotRequested),
            Finding = finding,
        };
    }

    private static ExtensionUpdateApplicationOutcome Stop(
        ExtensionUpdateExecutionPlan execution,
        ExtensionUpdateFindingCode code,
        string cause,
        string? target = null,
        ExtensionUpdateApplicationFailureContext? context = null)
        => Stop(execution, new ExtensionUpdateFinding(code, cause, target), context);

    private static ExtensionUpdateLifecycle Lifecycle(
        ExtensionUpdatePlan plan,
        ExtensionUpdateLifecycleOutcome outcome)
        => new(
            ExtensionUpdateLifecycleTrust.Trusted,
            ExtensionUpdateLifecycleCoverage.Complete,
            plan.Result.Lifecycle.Action,
            outcome);

    private static ExtensionUpdateVerification Verified()
        => new(
            ExtensionUpdateVerificationState.Verified,
            ExtensionUpdateVerificationState.Verified,
            ExtensionUpdateVerificationState.Verified);

    private static ExtensionUpdateEffect WithOutcome(
        ExtensionUpdateEffect effect,
        ExtensionUpdateEffectOutcome outcome)
        => new(
            effect.Path,
            effect.PackageId,
            effect.Kind,
            effect.Action,
            effect.Changes,
            outcome,
            outcome == ExtensionUpdateEffectOutcome.Verified
                ? ExtensionUpdateEffectResidual.None
                : ExtensionUpdateEffectResidual.Unknown);

    private static bool IsVerified(FileChangeReceipt receipt)
        => receipt.EffectState == FilesystemEffectState.Applied
            && receipt.VerificationState == FilesystemVerificationState.Verified;

    private static ExtensionUpdateEffectOutcome ReadOutcome(FileChangeReceipt receipt)
        => receipt.EffectState switch
        {
            FilesystemEffectState.NotStarted => ExtensionUpdateEffectOutcome.NotStarted,
            FilesystemEffectState.Applied when receipt.VerificationState == FilesystemVerificationState.Failed
                => ExtensionUpdateEffectOutcome.VerificationFailed,
            FilesystemEffectState.Applied => ExtensionUpdateEffectOutcome.Verified,
            FilesystemEffectState.Unknown => ExtensionUpdateEffectOutcome.CompletionUnknown,
            _ => throw new ArgumentOutOfRangeException(nameof(receipt), receipt.EffectState, "The effect state is not defined."),
        };

    private static ExtensionUpdateFindingCode ReadFinding(FileChangeReceipt receipt)
        => receipt.NotStartedReason switch
        {
            FilesystemNotStartedReason.Cancelled => ExtensionUpdateFindingCode.Interrupted,
            FilesystemNotStartedReason.TargetChanged => ExtensionUpdateFindingCode.TargetChanged,
            FilesystemNotStartedReason.ApplicationFailed => ExtensionUpdateFindingCode.WriteFailed,
            FilesystemNotStartedReason.ContractRejected => ExtensionUpdateFindingCode.OperationFailed,
            null when receipt.VerificationState == FilesystemVerificationState.Failed
                => ExtensionUpdateFindingCode.VerificationFailed,
            null => ExtensionUpdateFindingCode.WriteFailed,
            _ => throw new ArgumentOutOfRangeException(nameof(receipt), receipt.NotStartedReason, "The not-started reason is not defined."),
        };

    private static string[] ProtectedPaths(RecoveryBundlePreparation preparation)
        => [.. preparation.Entries.OrderBy(entry => entry.Ordinal).Select(entry => entry.TargetPath)];

    private static bool Matches(ExtensionUpdatePlan expected, ExtensionUpdatePlan actual)
        => expected.SettingsObservation.MatchesObservation(actual.SettingsObservation)
            && string.Equals(expected.SourceSignature, actual.SourceSignature, StringComparison.Ordinal)
            && string.Equals(
                expected.FrameworkPayload.InventoryFingerprint,
                actual.FrameworkPayload.InventoryFingerprint,
                StringComparison.Ordinal)
            && expected.Ownership.State == actual.Ownership.State
            && expected.Ownership.Snapshot?.Expectation == actual.Ownership.Snapshot?.Expectation
            && DictionaryEquals(
                expected.Topology.IntendedTargetBytes,
                actual.Topology.IntendedTargetBytes)
            && DictionaryEquals(
                expected.Topology.GeneratedTargetBytes,
                actual.Topology.GeneratedTargetBytes)
            && expected.Topology.ExcludedPaths.SequenceEqual(actual.Topology.ExcludedPaths, StringComparer.Ordinal)
            && expected.DirectoryCreations.Select(value => value.Expectation)
                .SequenceEqual(actual.DirectoryCreations.Select(value => value.Expectation))
            && ChangesEqual(expected.AllFileChanges, actual.AllFileChanges)
            && expected.Result.Findings.SequenceEqual(actual.Result.Findings);

    private static bool AppliedMatches(ExtensionUpdatePlan expected, ExtensionUpdatePlan actual)
        => string.Equals(expected.SourceSignature, actual.SourceSignature, StringComparison.Ordinal)
            && (expected.OwnershipChange is { } change
                ? actual.Ownership.Snapshot is { } snapshot
                    && snapshot.Bytes.AsSpan().SequenceEqual(change.IntendedBytes.AsSpan())
                : expected.Ownership.Snapshot?.Expectation == actual.Ownership.Snapshot?.Expectation)
            && DictionaryEquals(
                expected.Topology.IntendedTargetBytes,
                actual.Topology.IntendedTargetBytes)
            && DictionaryEquals(
                expected.Topology.GeneratedTargetBytes,
                actual.Topology.GeneratedTargetBytes)
            && actual.DirectoryCreations.Count == 0
            && actual.Effects.Count == 0
            && actual.OwnershipChange is null;

    private static bool DictionaryEquals(
        IReadOnlyDictionary<string, ImmutableArray<byte>> expected,
        IReadOnlyDictionary<string, ImmutableArray<byte>> actual)
        => expected.Count == actual.Count
            && expected.All(pair => actual.TryGetValue(pair.Key, out var bytes)
                && pair.Value.AsSpan().SequenceEqual(bytes.AsSpan()));

    private static bool ChangesEqual(
        IReadOnlyList<PlannedFileChange> expected,
        IReadOnlyList<PlannedFileChange> actual)
        => expected.Count == actual.Count
            && expected.Zip(actual).All(pair =>
                pair.First.Kind == pair.Second.Kind
                && pair.First.Expectation == pair.Second.Expectation
                && pair.First.IntendedBytes.AsSpan().SequenceEqual(
                    pair.Second.IntendedBytes.AsSpan()));

    private static ExtensionUpdateRecovery ReadRecovery(ExtensionUpdateExecutionPlan execution, ExtensionUpdateApplicationFailureContext context)
    {
        if (context.RecoveryUnknown)
        {
            return new(ExtensionUpdateRecoveryState.Unknown, [], residualPath: null);
        }
        if (context.Preparation is { } preparation)
        {
            return new(ExtensionUpdateRecoveryState.Retained, ProtectedPaths(preparation), preparation.BundlePath);
        }
        if (context.RecoveryResidualPath is { } residual)
        {
            return new(ExtensionUpdateRecoveryState.Unknown, [], residual);
        }
        return new(execution.RequiresRecovery ? ExtensionUpdateRecoveryState.NotCreated : ExtensionUpdateRecoveryState.NotRequired, [], residualPath: null);
    }
}
