using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Result;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Result;

internal static class InstallResultFactsFactory
{
    internal static InstallResultFacts Empty()
        => Create(
            evidence: new InstallPlanningEvidence
            {
                Payload = null,
                IntendedState = null,
            },
            managementState: null,
            effects: [],
            Completion(
                new InstallLifecycle(
                    InstallLifecycleAction.None,
                    InstallLifecycleOutcome.NotRequested),
                new InstallRecovery(
                    InstallResultRecoveryState.NotRequired,
                    residualPath: null),
                InstallResultVerificationState.NotRequested));

    internal static InstallResultFacts FromSummary(InstallOperationSummary summary)
    {
        return Create(
            evidence: new InstallPlanningEvidence
            {
                Payload = null,
                IntendedState = null,
            },
            summary.ManagementState,
            effects: [],
            Completion(
                new InstallLifecycle(
                    summary.ManagementState == InstallManagementState.TrustedExact
                        ? InstallLifecycleAction.Preserve
                        : InstallLifecycleAction.None,
                    summary.ManagementState == InstallManagementState.TrustedExact
                        ? InstallLifecycleOutcome.AlreadyCurrent
                        : InstallLifecycleOutcome.NotRequested),
                new InstallRecovery(
                    summary.RecoveryState switch
                    {
                        InstallRecoveryState.NotRequired => InstallResultRecoveryState.NotRequired,
                        InstallRecoveryState.NotCreated => InstallResultRecoveryState.NotCreated,
                        InstallRecoveryState.Prepared
                            or InstallRecoveryState.Retained => InstallResultRecoveryState.Retained,
                        InstallRecoveryState.Removed => InstallResultRecoveryState.Removed,
                        InstallRecoveryState.Unknown => InstallResultRecoveryState.Unknown,
                        _ => throw new ArgumentOutOfRangeException(nameof(summary)),
                    },
                    summary.RecoveryResidualPath),
                InstallResultVerificationState.NotRequested));
    }

    internal static InstallResultFacts FromBuild(InstallPlanBuild build)
    {
        var lifecycle = build.ManagementState == InstallManagementState.ManagedDivergence
            ? new InstallLifecycle(
                InstallLifecycleAction.Preserve,
                InstallLifecycleOutcome.NotRequested)
            : new InstallLifecycle(
                InstallLifecycleAction.None,
                InstallLifecycleOutcome.NotRequested);
        return Create(
            build.Evidence,
            build.ManagementState,
            effects: [],
            Completion(
                lifecycle,
                new InstallRecovery(InstallResultRecoveryState.NotRequired, residualPath: null),
                InstallResultVerificationState.NotRequested),
            Migrations(build.Evidence.IntendedState, []));
    }

    internal static InstallResultFacts PlanBoundary(InstallPlan plan)
        => PlanStage(
            plan,
            InstallEffectOutcome.NotStarted,
            InstallLifecycleOutcome.NotStarted,
            plan.RequiresRecovery
                ? InstallResultRecoveryState.NotCreated
                : InstallResultRecoveryState.NotRequired,
            InstallResultVerificationState.NotRequested);

    internal static InstallResultFacts DryRun(InstallPlan plan)
        => PlanStage(
            plan,
            InstallEffectOutcome.Planned,
            InstallLifecycleOutcome.Planned,
            plan.RequiresRecovery
                ? InstallResultRecoveryState.NotCreated
                : InstallResultRecoveryState.NotRequired,
            InstallResultVerificationState.NotRequested);

    internal static InstallResultFacts NoOp(InstallPlan plan)
        => Create(
            Evidence(plan),
            plan.ManagementState,
            effects: [],
            Completion(
                new InstallLifecycle(
                    InstallLifecycleAction.Preserve,
                    InstallLifecycleOutcome.AlreadyCurrent),
                new InstallRecovery(InstallResultRecoveryState.NotRequired, residualPath: null),
                InstallResultVerificationState.Verified),
            migrations: []);

    internal static InstallResultFacts ApplicationUnknown(InstallPlan plan)
    {
        var effects = plan.Effects.Select(identity => Effect(
            identity,
            InstallEffectOutcome.CompletionUnknown,
            InstallEffectResidual.Unknown,
            plan.IntendedState.UserOwnedPaths));
        return Create(
            Evidence(plan),
            plan.ManagementState,
            effects,
            Completion(
                Lifecycle(
                    plan,
                    InstallLifecycleOutcome.CompletionUnknown),
                new InstallRecovery(
                    plan.RequiresRecovery
                        ? InstallResultRecoveryState.Unknown
                        : InstallResultRecoveryState.NotRequired,
                    residualPath: null),
                InstallResultVerificationState.Unknown),
            Migrations(plan.IntendedState, effects));
    }

    internal static InstallResultFacts FromApplication(
        InstallPlan plan,
        InstallApplicationProgress progress,
        InstallRecoveryOutcome recovery)
    {
        var applied = progress.Effects.ToDictionary(
            effect => effect.Identity,
            effect => effect.Outcome);
        var finalVerified = progress.FinalVerification == InstallFinalVerificationState.Verified;
        var effects = plan.Effects.Select(identity =>
        {
            var outcome = applied.GetValueOrDefault(
                identity,
                InstallEffectOutcome.NotStarted);
            var residual = finalVerified
                ? InstallEffectResidual.None
                : outcome switch
                {
                    InstallEffectOutcome.Verified => InstallEffectResidual.Retained,
                    InstallEffectOutcome.VerificationFailed
                        or InstallEffectOutcome.CompletionUnknown => InstallEffectResidual.Unknown,
                    InstallEffectOutcome.Planned
                        or InstallEffectOutcome.NotStarted => InstallEffectResidual.None,
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(progress),
                        outcome,
                        "The Install effect outcome is not defined."),
                };
            return Effect(
                identity,
                outcome,
                residual,
                plan.IntendedState.UserOwnedPaths);
        }).ToArray();

        var lifecycleOutcome = plan.OwnershipEffect is null
            ? InstallLifecycleOutcome.NotRequested
            : applied.GetValueOrDefault(
                plan.OwnershipEffect.Identity,
                InstallEffectOutcome.NotStarted) switch
            {
                InstallEffectOutcome.Planned => InstallLifecycleOutcome.Planned,
                InstallEffectOutcome.NotStarted => InstallLifecycleOutcome.NotStarted,
                InstallEffectOutcome.Verified => InstallLifecycleOutcome.Verified,
                InstallEffectOutcome.VerificationFailed => InstallLifecycleOutcome.VerificationFailed,
                InstallEffectOutcome.CompletionUnknown => InstallLifecycleOutcome.CompletionUnknown,
                _ => throw new ArgumentOutOfRangeException(nameof(progress)),
            };
        return Create(
            Evidence(plan),
            plan.ManagementState,
            effects,
            Completion(
                Lifecycle(plan, lifecycleOutcome),
                Recovery(recovery),
                Verification(progress)),
            Migrations(plan.IntendedState, effects));
    }

    private static InstallResultFacts PlanStage(
        InstallPlan plan,
        InstallEffectOutcome effectOutcome,
        InstallLifecycleOutcome lifecycleOutcome,
        InstallResultRecoveryState recoveryState,
        InstallResultVerificationState verification)
    {
        var effects = plan.Effects.Select(identity => Effect(
            identity,
            effectOutcome,
            InstallEffectResidual.None,
            plan.IntendedState.UserOwnedPaths)).ToArray();
        return Create(
            Evidence(plan),
            plan.ManagementState,
            effects,
            Completion(
                Lifecycle(plan, lifecycleOutcome),
                new InstallRecovery(recoveryState, residualPath: null),
                verification),
            Migrations(plan.IntendedState, effects));
    }

    private static InstallResultFacts Create(
        InstallPlanningEvidence evidence,
        InstallManagementState? managementState,
        IEnumerable<InstallEffect> effects,
        InstallResultCompletion completion,
        IEnumerable<InstallMigration>? migrations = null)
        => new(new InstallResultFactsInput
        {
            Source = evidence.Payload is { } payload
                ? new InstallSource(payload.InventoryFingerprint, payload.Assets.Length)
                : null,
            Classification = Classification(managementState),
            Footprint = evidence.IntendedState is { } intended
                ? new InstallFootprint(
                    payloadFiles: PayloadFileCount(intended),
                    managedRegions: intended.ManagedBlockBytes.Count,
                    generatedRegions: intended.GeneratedRegionPaths.Count)
                : null,
            Effects = effects,
            Migrations = migrations ?? [],
            Lifecycle = completion.Lifecycle,
            Recovery = completion.Recovery,
            Verification = new InstallVerification(completion.Verification),
        });

    internal static int PayloadFileCount(InstallIntendedState intended)
    {
        ArgumentNullException.ThrowIfNull(intended);
        return intended.TargetBytes.Keys.Count(path => !intended.UserOwnedPaths.Contains(path));
    }

    internal static InstallMigrationOutcome MigrationOutcomeForPath(
        string path,
        IEnumerable<InstallEffect> effects)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(effects);
        var matching = effects
            .Where(effect => effect.Kind != InstallEffectKind.Directory
                && string.Equals(effect.Path, path, StringComparison.Ordinal))
            .ToArray();
        return matching.Length > 0
            && matching.All(effect => effect.Outcome == InstallEffectOutcome.Verified)
                ? InstallMigrationOutcome.Applied
                : InstallMigrationOutcome.Planned;
    }

    internal static string? SourceAssetPathFor(
        InstallEffectIdentity identity,
        IReadOnlySet<string> userOwnedPaths)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(userOwnedPaths);
        return userOwnedPaths.Contains(identity.Path)
            ? null
            : identity.SourceAssetPath;
    }

    private static IReadOnlyList<InstallMigration> Migrations(
        InstallIntendedState? intended,
        IEnumerable<InstallEffect> effects)
    {
        if (intended is null || intended.Migrations.Count == 0)
        {
            return [];
        }

        var effectValues = effects.ToArray();
        var migrationPaths = effectValues
            .Where(effect => effect.Kind != InstallEffectKind.Directory)
            .Select(effect => effect.Path)
            .ToHashSet(StringComparer.Ordinal);
        return intended.Migrations
            .Where(migration => migrationPaths.Contains(migration.Path))
            .OrderBy(migration => migration.Path, StringComparer.Ordinal)
            .Select(migration => new InstallMigration(
                migration.Path,
                migration.Actions,
                migration.Fields,
                migration.Derivation,
                MigrationOutcomeForPath(migration.Path, effectValues)))
            .ToArray();
    }

    private static InstallResultCompletion Completion(
        InstallLifecycle lifecycle,
        InstallRecovery recovery,
        InstallResultVerificationState verification)
        => new()
        {
            Lifecycle = lifecycle,
            Recovery = recovery,
            Verification = verification,
        };

    private static InstallPlanningEvidence Evidence(InstallPlan plan)
        => new()
        {
            Payload = plan.Payload,
            IntendedState = plan.IntendedState,
        };

    private static InstallManagementClassification? Classification(
        InstallManagementState? managementState)
        => managementState switch
        {
            InstallManagementState.SafelyAbsent => InstallManagementClassification.SafeAbsence,
            InstallManagementState.TrustedExact => InstallManagementClassification.TrustedExact,
            InstallManagementState.ManagedAdoption => InstallManagementClassification.ManagedAdoption,
            InstallManagementState.ManagedDivergence => InstallManagementClassification.ManagedDivergence,
            InstallManagementState.EligibleInitialOccupant => InstallManagementClassification.EligibleInitialOccupant,
            InstallManagementState.Incomplete
                or InstallManagementState.Blocked
                or InstallManagementState.Interrupted
                or null => null,
            _ => throw new ArgumentOutOfRangeException(
                nameof(managementState),
                managementState,
                "The Install management state is not defined."),
        };

    private static InstallEffect Effect(
        InstallEffectIdentity identity,
        InstallEffectOutcome outcome,
        InstallEffectResidual residual,
        IReadOnlySet<string> userOwnedPaths)
        => new(new InstallEffectInput
        {
            Path = identity.Path,
            Kind = identity.Kind,
            Action = identity.Action,
            SourceAssetPath = SourceAssetPathFor(identity, userOwnedPaths),
            Outcome = outcome,
            Residual = residual,
        });

    private static InstallLifecycle Lifecycle(
        InstallPlan plan,
        InstallLifecycleOutcome outcome)
        => plan.OwnershipEffect is null
            ? new InstallLifecycle(
                InstallLifecycleAction.None,
                InstallLifecycleOutcome.NotRequested)
            : new InstallLifecycle(
                InstallLifecycleAction.Publish,
                outcome);

    private static InstallRecovery Recovery(InstallRecoveryOutcome recovery)
        => new(
            recovery.State switch
            {
                InstallRecoveryState.NotRequired => InstallResultRecoveryState.NotRequired,
                InstallRecoveryState.NotCreated => InstallResultRecoveryState.NotCreated,
                InstallRecoveryState.Prepared
                    or InstallRecoveryState.Retained => InstallResultRecoveryState.Retained,
                InstallRecoveryState.Removed => InstallResultRecoveryState.Removed,
                InstallRecoveryState.Unknown => InstallResultRecoveryState.Unknown,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(recovery),
                    recovery.State,
                    "The Install recovery state is not defined."),
            },
            recovery.ResidualPath);

    private static InstallResultVerificationState Verification(
        InstallApplicationProgress progress)
        => progress.FinalVerification switch
        {
            InstallFinalVerificationState.Verified => InstallResultVerificationState.Verified,
            InstallFinalVerificationState.Failed => InstallResultVerificationState.Failed,
            InstallFinalVerificationState.NotReached when progress.Effects.Any(
                effect => effect.Outcome == InstallEffectOutcome.VerificationFailed)
                => InstallResultVerificationState.Failed,
            InstallFinalVerificationState.NotReached when progress.Effects.Any(
                effect => effect.Outcome is InstallEffectOutcome.Verified
                    or InstallEffectOutcome.CompletionUnknown)
                => InstallResultVerificationState.Unknown,
            InstallFinalVerificationState.NotReached => InstallResultVerificationState.NotRequested,
            _ => throw new ArgumentOutOfRangeException(
                nameof(progress),
                progress.FinalVerification,
                "The Install final verification state is not defined."),
        };
}
