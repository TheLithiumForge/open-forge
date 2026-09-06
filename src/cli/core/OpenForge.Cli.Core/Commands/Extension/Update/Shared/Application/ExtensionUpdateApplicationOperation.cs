using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Update.Shared.Planning;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Shared.Application;

internal sealed class ExtensionUpdateApplicationOperation(
    WorkspaceLockManager lockManager,
    ExtensionUpdatePlanner planner,
    MutationRevalidator revalidator,
    ExtensionUpdateEffectApplier effectApplier,
    ExtensionUpdateRecoveryApplication recoveryApplication)
{
    private readonly WorkspaceLockManager _lockManager = lockManager;
    private readonly ExtensionUpdatePlanner _planner = planner;
    private readonly MutationRevalidator _revalidator = revalidator;
    private readonly ExtensionUpdateEffectApplier _effectApplier = effectApplier;
    private readonly ExtensionUpdateRecoveryApplication _recoveryApplication = recoveryApplication;

    internal async ValueTask<ExtensionUpdateApplicationOutcome> ExecuteAsync(
        ExtensionUpdatePlan plan,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
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
            return Stop(plan, ExtensionUpdateFindingCode.Interrupted,
                "Extension Update lock acquisition was interrupted.");
        }
        catch (Exception)
        {
            return Stop(plan, ExtensionUpdateFindingCode.WorkspaceLockUnavailable,
                "Extension Update lock acquisition failed unexpectedly.");
        }

        if (lockResult.State != WorkspaceLockState.Acquired
            || lockResult.Lease is not { } lease)
        {
            return Stop(
                plan,
                lockResult.State == WorkspaceLockState.Cancelled
                    ? ExtensionUpdateFindingCode.Interrupted
                    : ExtensionUpdateFindingCode.WorkspaceLockUnavailable,
                lockResult.Cause ?? "The persistent Extension Update workspace lease could not be acquired.");
        }

        await using (lease.ConfigureAwait(false))
        {
            return await ExecuteUnderLeaseAsync(plan, lease, operationId, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async ValueTask<ExtensionUpdateApplicationOutcome> ExecuteUnderLeaseAsync(
        ExtensionUpdatePlan plan,
        WorkspaceLockLease lease,
        Guid operationId,
        CancellationToken cancellationToken)
    {
        ExtensionUpdatePlanBuild rebuilt;
        try
        {
            rebuilt = await _planner.BuildAsync(plan.Request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Stop(plan, ExtensionUpdateFindingCode.Interrupted,
                "Extension Update semantic revalidation was interrupted.");
        }
        catch (Exception)
        {
            return Stop(plan, ExtensionUpdateFindingCode.OperationFailed,
                "Extension Update semantic revalidation failed unexpectedly.");
        }

        if (rebuilt.Plan is not { } current || !Matches(plan, current))
        {
            return Stop(
                plan,
                ExtensionUpdateFindingCode.TargetChanged,
                "Source, lifecycle, Framework, topology, ownership, or plan facts changed after planning.");
        }

        MutationValidationResult validation;
        try
        {
            validation = await _revalidator.ValidateAsync(
                lease,
                plan.DirectoryCreations,
                plan.AllFileChanges,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Stop(plan, ExtensionUpdateFindingCode.Interrupted,
                "Extension Update plan revalidation was interrupted.");
        }
        catch (Exception)
        {
            return Stop(plan, ExtensionUpdateFindingCode.OperationFailed,
                "Extension Update plan revalidation failed unexpectedly.");
        }

        if (validation.State != MutationValidationState.Valid)
        {
            return Stop(
                plan,
                validation.State == MutationValidationState.Cancelled
                    ? ExtensionUpdateFindingCode.Interrupted
                    : ExtensionUpdateFindingCode.TargetChanged,
                validation.Cause ?? "An Extension Update target changed before effects.");
        }

        RecoveryBundlePreparationResult preparationResult;
        try
        {
            preparationResult = await _recoveryApplication.PrepareAsync(
                plan,
                operationId,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Stop(plan, ExtensionUpdateFindingCode.Interrupted,
                "Extension Update recovery preparation was interrupted.",
                context: new ApplicationFailureContext { RecoveryUnknown = true });
        }
        catch (Exception)
        {
            return Stop(plan, ExtensionUpdateFindingCode.RecoveryUnavailable,
                "Extension Update recovery preparation failed unexpectedly.",
                context: new ApplicationFailureContext { RecoveryUnknown = true });
        }
        if (preparationResult.State is not RecoveryBundlePreparationState.NotNeeded
            and not RecoveryBundlePreparationState.Prepared)
        {
            return Stop(
                plan,
                preparationResult.State == RecoveryBundlePreparationState.Cancelled
                    ? ExtensionUpdateFindingCode.Interrupted
                    : preparationResult.State == RecoveryBundlePreparationState.Blocked
                        ? ExtensionUpdateFindingCode.RecoveryConflict
                        : ExtensionUpdateFindingCode.RecoveryUnavailable,
                preparationResult.Cause ?? "Extension Update recovery preparation is unavailable.",
                context: new ApplicationFailureContext
                {
                    RecoveryResidualPath = preparationResult.ResidualPath,
                });
        }

        var preparation = preparationResult.Preparation;
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
                    plan,
                    receipt.NotStartedReason == FilesystemNotStartedReason.Cancelled
                        ? ExtensionUpdateFindingCode.Interrupted
                        : ExtensionUpdateFindingCode.WriteFailed,
                    receipt.Cause ?? "An Extension Update parent directory could not be created and verified.",
                    directory.LogicalPath,
                    new ApplicationFailureContext
                    {
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
                    plan,
                    ReadFinding(receipt),
                    receipt.Cause ?? "An Extension Update target could not be applied and verified.",
                    effect.Result.Path,
                    new ApplicationFailureContext
                    {
                        Effects = effects,
                        Preparation = preparation,
                    });
            }

            effects[effectIndex] = WithOutcome(effect.Result, ExtensionUpdateEffectOutcome.Verified);
        }

        var lifecycleOutcome = plan.LifecycleChange is null
            ? ExtensionUpdateLifecycleOutcome.AlreadyCurrent
            : ExtensionUpdateLifecycleOutcome.NotStarted;
        if (plan.LifecycleChange is { } lifecycleChange)
        {
            var receipt = await _effectApplier.ApplyFileAsync(
                lease,
                lifecycleChange,
                validation.Checks[checkIndex],
                preparation,
                cancellationToken).ConfigureAwait(false);
            if (!IsVerified(receipt))
            {
                return Stop(
                    plan,
                    ExtensionUpdateFindingCode.LifecyclePublicationFailed,
                    receipt.Cause ?? "The Extension lifecycle could not be published and verified.",
                    ".agents/open-forge.lifecycle.json",
                    new ApplicationFailureContext
                    {
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
                preparation,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Stop(
                plan,
                ExtensionUpdateFindingCode.Interrupted,
                "Final Extension Update verification was interrupted.",
                context: new ApplicationFailureContext
                {
                    Effects = effects,
                    Preparation = preparation,
                    LifecycleOutcome = lifecycleOutcome,
                });
        }
        catch (Exception)
        {
            return Stop(
                plan,
                ExtensionUpdateFindingCode.VerificationFailed,
                "Final Extension Update verification failed unexpectedly.",
                context: new ApplicationFailureContext
                {
                    Effects = effects,
                    Preparation = preparation,
                    LifecycleOutcome = lifecycleOutcome,
                });
        }

        if (verificationBuild.Plan is not { } verifiedPlan
            || !AppliedMatches(plan, verifiedPlan))
        {
            return Stop(
                plan,
                ExtensionUpdateFindingCode.VerificationFailed,
                "Final Extension Update topology or lifecycle verification did not match the plan.",
                context: new ApplicationFailureContext
                {
                    Effects = effects,
                    Preparation = preparation,
                    LifecycleOutcome = lifecycleOutcome,
                });
        }

        var recovery = await _recoveryApplication.CleanupAsync(
            plan,
            lease,
            preparation,
            cancellationToken)
            .ConfigureAwait(false);
        if (recovery.Finding is not null)
        {
            return new ExtensionUpdateApplicationOutcome
            {
                Effects = effects,
                Lifecycle = Lifecycle(plan, lifecycleOutcome),
                Recovery = recovery.Recovery,
                Verification = Verified(),
                Finding = recovery.Finding,
            };
        }

        return new ExtensionUpdateApplicationOutcome
        {
            Effects = effects,
            Lifecycle = Lifecycle(plan, lifecycleOutcome),
            Recovery = recovery.Recovery,
            Verification = Verified(),
            Finding = null,
        };
    }

    private static ExtensionUpdateApplicationOutcome Stop(
        ExtensionUpdatePlan plan,
        ExtensionUpdateFinding finding,
        ApplicationFailureContext? context = null)
    {
        context ??= new ApplicationFailureContext();
        return new ExtensionUpdateApplicationOutcome
        {
            Effects = context.Effects,
            Lifecycle = Lifecycle(plan, context.LifecycleOutcome),
            Recovery = context.RecoveryUnknown
                ? new ExtensionUpdateRecovery(
                    ExtensionUpdateRecoveryState.Unknown,
                    [],
                    residualPath: null)
                : context.Preparation is null && context.RecoveryResidualPath is null
                ? new ExtensionUpdateRecovery(
                    plan.RequiresRecovery
                        ? ExtensionUpdateRecoveryState.NotCreated
                        : ExtensionUpdateRecoveryState.NotRequired,
                    [],
                    residualPath: null)
                : context.Preparation is null
                    ? new ExtensionUpdateRecovery(
                        ExtensionUpdateRecoveryState.Unknown,
                        [],
                        context.RecoveryResidualPath)
                : new ExtensionUpdateRecovery(
                    ExtensionUpdateRecoveryState.Retained,
                    ProtectedPaths(context.Preparation),
                    context.Preparation.BundlePath),
            Verification = new ExtensionUpdateVerification(
                ExtensionUpdateVerificationState.NotRequested,
                ExtensionUpdateVerificationState.NotRequested,
                ExtensionUpdateVerificationState.NotRequested),
            Finding = finding,
        };
    }

    private static ExtensionUpdateApplicationOutcome Stop(
        ExtensionUpdatePlan plan,
        ExtensionUpdateFindingCode code,
        string cause,
        string? target = null,
        ApplicationFailureContext? context = null)
        => Stop(plan, new ExtensionUpdateFinding(code, cause, target), context);

    private static ExtensionUpdateLifecycle Lifecycle(
        ExtensionUpdatePlan plan,
        ExtensionUpdateLifecycleOutcome outcome)
        => new(
            ExtensionUpdateLifecycleTrust.Trusted,
            ExtensionUpdateLifecycleCoverage.Complete,
            plan.LifecycleChange is null
                ? ExtensionUpdateLifecycleAction.Preserve
                : ExtensionUpdateLifecycleAction.Publish,
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

    private static IReadOnlyList<string> ProtectedPaths(RecoveryBundlePreparation preparation)
        => preparation.Entries.OrderBy(entry => entry.Ordinal).Select(entry => entry.TargetPath).ToArray();

    private static bool Matches(ExtensionUpdatePlan expected, ExtensionUpdatePlan actual)
        => string.Equals(expected.SourceSignature, actual.SourceSignature, StringComparison.Ordinal)
            && string.Equals(
                expected.FrameworkPayload.InventoryFingerprint,
                actual.FrameworkPayload.InventoryFingerprint,
                StringComparison.Ordinal)
            && LifecycleEquals(expected.FrameworkLifecycle, actual.FrameworkLifecycle)
            && LifecycleEquals(expected.CurrentLifecycle, actual.CurrentLifecycle)
            && LifecycleEquals(expected.IntendedLifecycle, actual.IntendedLifecycle)
            && DictionaryEquals(
                expected.Topology.IntendedTargetBytes,
                actual.Topology.IntendedTargetBytes)
            && DictionaryEquals(
                expected.Topology.GeneratedTargetBytes,
                actual.Topology.GeneratedTargetBytes)
            && expected.DirectoryCreations.Select(value => value.Expectation)
                .SequenceEqual(actual.DirectoryCreations.Select(value => value.Expectation))
            && ChangesEqual(expected.AllFileChanges, actual.AllFileChanges)
            && expected.Result.Findings.SequenceEqual(actual.Result.Findings);

    private static bool AppliedMatches(ExtensionUpdatePlan expected, ExtensionUpdatePlan actual)
        => string.Equals(expected.SourceSignature, actual.SourceSignature, StringComparison.Ordinal)
            && LifecycleEquals(expected.FrameworkLifecycle, actual.FrameworkLifecycle)
            && LifecycleEquals(expected.IntendedLifecycle, actual.CurrentLifecycle)
            && DictionaryEquals(
                expected.Topology.IntendedTargetBytes,
                actual.Topology.IntendedTargetBytes)
            && DictionaryEquals(
                expected.Topology.GeneratedTargetBytes,
                actual.Topology.GeneratedTargetBytes)
            && actual.DirectoryCreations.Count == 0
            && actual.Effects.Count == 0
            && actual.LifecycleChange is null;

    private static bool LifecycleEquals(
        FrameworkLifecycleState expected,
        FrameworkLifecycleState actual)
        => JsonSerializer.SerializeToUtf8Bytes(
                expected,
                LifecycleJsonContext.Default.FrameworkLifecycleState)
            .AsSpan()
            .SequenceEqual(JsonSerializer.SerializeToUtf8Bytes(
                actual,
                LifecycleJsonContext.Default.FrameworkLifecycleState));

    private static bool LifecycleEquals(
        ExtensionLifecycleState expected,
        ExtensionLifecycleState actual)
        => JsonSerializer.SerializeToUtf8Bytes(
                expected,
                LifecycleJsonContext.Default.ExtensionLifecycleState)
            .AsSpan()
            .SequenceEqual(JsonSerializer.SerializeToUtf8Bytes(
                actual,
                LifecycleJsonContext.Default.ExtensionLifecycleState));

    private static bool DictionaryEquals(
        IReadOnlyDictionary<string, byte[]> expected,
        IReadOnlyDictionary<string, byte[]> actual)
        => expected.Count == actual.Count
            && expected.All(pair => actual.TryGetValue(pair.Key, out var bytes)
                && pair.Value.AsSpan().SequenceEqual(bytes));

    private static bool ChangesEqual(
        IReadOnlyList<PlannedFileChange> expected,
        IReadOnlyList<PlannedFileChange> actual)
        => expected.Count == actual.Count
            && expected.Zip(actual).All(pair =>
                pair.First.Kind == pair.Second.Kind
                && pair.First.Expectation == pair.Second.Expectation
                && pair.First.IntendedBytes.AsSpan().SequenceEqual(
                    pair.Second.IntendedBytes.AsSpan()));

    private sealed class ApplicationFailureContext
    {
        internal IReadOnlyList<ExtensionUpdateEffect> Effects { get; init; } = [];

        internal RecoveryBundlePreparation? Preparation { get; init; }

        internal ExtensionUpdateLifecycleOutcome LifecycleOutcome { get; init; }
            = ExtensionUpdateLifecycleOutcome.NotStarted;

        internal string? RecoveryResidualPath { get; init; }

        internal bool RecoveryUnknown { get; init; }
    }
}
