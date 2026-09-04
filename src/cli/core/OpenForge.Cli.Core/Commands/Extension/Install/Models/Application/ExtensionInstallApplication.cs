using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Models.Application;

internal sealed record ExtensionInstallApplicationPrecondition(
    MutationValidationResult? Validation,
    ExtensionInstallFinding? Finding)
{
    internal bool IsValid => Finding is null && Validation?.State == MutationValidationState.Valid;
}

internal sealed record ExtensionInstallApplicationLease(
    ExtensionInstallPlan Plan,
    WorkspaceLockLease Lease,
    Guid OperationId);

internal sealed record ExtensionInstallEffectApplicationInput
{
    internal required ExtensionInstallPlan Plan { get; init; }

    internal required WorkspaceLockLease Lease { get; init; }

    internal required MutationValidationResult Validation { get; init; }

    internal required ExtensionInstallApplicationProgress Progress { get; init; }
}

internal sealed record ExtensionInstallApplicationStageResult(
    ExtensionInstallApplicationProgress Progress,
    ExtensionInstallFinding? Finding);

internal sealed record ExtensionInstallVerificationResult(
    ExtensionInstallVerification Verification,
    ExtensionInstallFinding? Finding)
{
    internal bool IsVerified => Finding is null
        && Verification.Targets == ExtensionInstallVerificationState.Verified
        && Verification.Topology == ExtensionInstallVerificationState.Verified
        && Verification.ExtensionsLifecycle == ExtensionInstallVerificationState.Verified
        && Verification.FrameworkLifecycle == ExtensionInstallVerificationState.Verified;
}

internal sealed record ExtensionInstallRecoveryCleanupResult(
    ExtensionInstallRecovery Recovery,
    ExtensionInstallFinding? Finding);

internal sealed record ExtensionInstallRecoveryCleanupRequest(
    ExtensionInstallPlan Plan,
    WorkspaceLockLease Lease,
    RecoveryBundlePreparation Preparation);

internal sealed record ExtensionInstallApplicationProgress
{
    private ExtensionInstallApplicationProgress(
        ImmutableArray<ExtensionInstallEffect> effects,
        ExtensionInstallLifecycleOutcome lifecycleOutcome,
        ExtensionInstallRecovery recovery,
        ExtensionInstallVerification verification,
        RecoveryBundlePreparation? recoveryPreparation)
    {
        Effects = effects;
        LifecycleOutcome = lifecycleOutcome;
        Recovery = recovery;
        Verification = verification;
        RecoveryPreparation = recoveryPreparation;
    }

    internal ImmutableArray<ExtensionInstallEffect> Effects { get; init; }

    internal ExtensionInstallLifecycleOutcome LifecycleOutcome { get; init; }

    internal ExtensionInstallRecovery Recovery { get; init; }

    internal ExtensionInstallVerification Verification { get; init; }

    internal RecoveryBundlePreparation? RecoveryPreparation { get; init; }

    internal static ExtensionInstallApplicationProgress Start(
        ExtensionInstallPlan plan,
        RecoveryBundlePreparation? preparation)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var protectedPaths = preparation?.Entries
            .OrderBy(entry => entry.Ordinal)
            .Select(entry => entry.TargetPath)
            .ToArray() ?? [];
        return new ExtensionInstallApplicationProgress(
            plan.Facts.Effects.Select(effect => effect with
            {
                Outcome = ExtensionInstallEffectOutcome.NotStarted,
                Residual = ExtensionInstallEffectResidual.None,
            }).ToImmutableArray(),
            plan.Facts.Lifecycle.Action switch
            {
                ExtensionInstallLifecycleAction.None => ExtensionInstallLifecycleOutcome.NotRequested,
                ExtensionInstallLifecycleAction.Preserve => ExtensionInstallLifecycleOutcome.AlreadyCurrent,
                ExtensionInstallLifecycleAction.Publish => ExtensionInstallLifecycleOutcome.NotStarted,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(plan),
                    plan.Facts.Lifecycle.Action,
                    "The Extension lifecycle action is not defined."),
            },
            new ExtensionInstallRecovery(
                ReadInitialRecoveryState(plan, preparation),
                protectedPaths,
                preparation?.BundlePath),
            UnknownVerification(),
            preparation);
    }

    internal ExtensionInstallApplicationProgress RecordEffect(
        int index,
        ExtensionInstallEffectOutcome outcome)
    {
        if (index < 0 || index >= Effects.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (Effects[index].Outcome != ExtensionInstallEffectOutcome.NotStarted
            || outcome is ExtensionInstallEffectOutcome.Planned or ExtensionInstallEffectOutcome.NotStarted)
        {
            throw new InvalidOperationException(
                "Extension Install application progress can record each started effect exactly once.");
        }

        return this with
        {
            Effects = Effects.SetItem(index, Effects[index] with { Outcome = outcome }),
        };
    }

    internal ExtensionInstallApplicationProgress WithLifecycle(
        ExtensionInstallLifecycleOutcome outcome)
        => this with { LifecycleOutcome = outcome };

    internal ExtensionInstallApplicationProgress WithVerification(
        ExtensionInstallVerification verification)
        => this with { Verification = verification };

    internal ExtensionInstallApplicationProgress WithRecovery(
        ExtensionInstallRecovery recovery)
        => this with { Recovery = recovery };

    internal ExtensionInstallApplicationProgress RetainWorkspaceEffects()
        => this with
        {
            Effects = Effects.Select(effect => effect.Outcome switch
            {
                ExtensionInstallEffectOutcome.Planned
                    or ExtensionInstallEffectOutcome.NotStarted => effect,
                ExtensionInstallEffectOutcome.Verified
                    or ExtensionInstallEffectOutcome.VerificationFailed => effect with
                    {
                        Residual = ExtensionInstallEffectResidual.Retained,
                    },
                ExtensionInstallEffectOutcome.CompletionUnknown => effect with
                {
                    Residual = ExtensionInstallEffectResidual.Unknown,
                },
                _ => throw new ArgumentOutOfRangeException(
                    nameof(effect),
                    effect.Outcome,
                    "The Extension effect outcome is not defined."),
            }).ToImmutableArray(),
        };

    private static ExtensionInstallRecoveryState ReadInitialRecoveryState(
        ExtensionInstallPlan plan,
        RecoveryBundlePreparation? preparation)
    {
        if (preparation is not null)
        {
            return ExtensionInstallRecoveryState.Retained;
        }

        if (plan.RequiresRecovery)
        {
            return ExtensionInstallRecoveryState.NotCreated;
        }

        return ExtensionInstallRecoveryState.NotRequired;
    }

    private static ExtensionInstallVerification UnknownVerification()
        => new(
            ExtensionInstallVerificationState.Unknown,
            ExtensionInstallVerificationState.Unknown,
            ExtensionInstallVerificationState.Unknown,
            ExtensionInstallVerificationState.Unknown);
}
