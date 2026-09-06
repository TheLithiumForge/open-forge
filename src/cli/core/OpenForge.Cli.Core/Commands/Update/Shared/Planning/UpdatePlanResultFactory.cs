using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Planning;

internal static class UpdatePlanResultFactory
{
    internal static UpdateResult Boundary(
        UpdateRequest request,
        UpdateSource? source,
        IReadOnlyList<UpdateComparison> comparisons,
        UpdateFinding finding,
        UpdateLifecycleTrust trust,
        UpdateLifecycleCoverage coverage)
        => Create(
            request,
            source,
            comparisons,
            generatedNavigation: null,
            effects: [],
            new UpdateLifecycle
            {
                Trust = trust,
                Coverage = coverage,
                Action = UpdateLifecycleAction.None,
                Outcome = UpdateLifecycleOutcome.NotStarted,
            },
            new UpdateRecovery
            {
                State = UpdateRecoveryState.NotRequired,
                ProtectedPaths = [],
                ResidualPath = null,
            },
            UpdateVerificationState.NotRequested,
            [finding]);

    internal static UpdateResult Preview(
        UpdateRequest request,
        FrameworkPayload payload,
        IReadOnlyList<UpdateComparisonObservation> observations,
        UpdatePlanningPlan plan,
        IReadOnlyList<UpdatePlannedEffect> effects,
        PlannedFileChange? lifecycleChange,
        IReadOnlyList<UpdateFinding> findings)
    {
        var existingTargetPaths = effects
            .Where(value => value.FileChange.Kind != PlannedFileChangeKind.Create)
            .Select(value => value.ResultEffect.Path)
            .ToList();
        if (lifecycleChange?.Kind == PlannedFileChangeKind.Replace)
        {
            existingTargetPaths.Add(LifecycleSchema.RelativePath);
        }

        var requiresRecovery = existingTargetPaths.Count > 0;
        return Create(
            request,
            Source(payload),
            observations.Select(value => value.Comparison).ToArray(),
            Navigation(observations),
            effects.Select(value => value.ResultEffect).ToArray(),
            new UpdateLifecycle
            {
                Trust = UpdateLifecycleTrust.Trusted,
                Coverage = UpdateLifecycleCoverage.Complete,
                Action = lifecycleChange is null
                    ? UpdateLifecycleAction.Preserve
                    : UpdateLifecycleAction.Publish,
                Outcome = lifecycleChange is null
                    ? UpdateLifecycleOutcome.AlreadyCurrent
                    : UpdateLifecycleOutcome.Planned,
            },
            new UpdateRecovery
            {
                State = requiresRecovery
                    ? UpdateRecoveryState.NotCreated
                    : UpdateRecoveryState.NotRequired,
                ProtectedPaths = existingTargetPaths,
                ResidualPath = null,
            },
            request.Mode == UpdateMode.Apply
                && plan.IsNoOp
                && lifecycleChange is null
                && findings.Count == 0
                    ? UpdateVerificationState.Verified
                    : UpdateVerificationState.NotRequested,
            findings);
    }

    internal static UpdateSource Source(FrameworkPayload payload)
        => new()
        {
            Id = "framework",
            Version = null,
            InventoryFingerprint = payload.InventoryFingerprint,
            AssetCount = payload.Assets.Length,
        };

    private static UpdateGeneratedNavigation Navigation(
        IReadOnlyList<UpdateComparisonObservation> observations)
        => new()
        {
            Coverage = UpdateGeneratedNavigationCoverage.Complete,
            Regions = observations
                .Where(value => value.Comparison.Kind == UpdateComparisonTargetKind.GeneratedRegion)
                .Select(value => new UpdateGeneratedNavigationRegion
                {
                    Path = value.Comparison.RelativePath,
                    State = value.Comparison.IntendedState switch
                    {
                        UpdateComparisonIntendedState.Same =>
                            UpdateGeneratedNavigationRegionState.Unchanged,
                        UpdateComparisonIntendedState.Changed =>
                            UpdateGeneratedNavigationRegionState.Changed,
                        UpdateComparisonIntendedState.New =>
                            UpdateGeneratedNavigationRegionState.New,
                        UpdateComparisonIntendedState.Retired =>
                            UpdateGeneratedNavigationRegionState.Retired,
                        UpdateComparisonIntendedState.Unavailable =>
                            UpdateGeneratedNavigationRegionState.Unavailable,
                        UpdateComparisonIntendedState.Blocked =>
                            UpdateGeneratedNavigationRegionState.Blocked,
                        _ => throw new ArgumentOutOfRangeException(
                            nameof(observations),
                            value.Comparison.IntendedState,
                            "The Update intended state is not defined."),
                    },
                })
                .ToArray(),
        };

    private static UpdateResult Create(
        UpdateRequest request,
        UpdateSource? source,
        IReadOnlyList<UpdateComparison> comparisons,
        UpdateGeneratedNavigation? generatedNavigation,
        IReadOnlyList<UpdatePhysicalEffect> effects,
        UpdateLifecycle lifecycle,
        UpdateRecovery recovery,
        UpdateVerificationState verification,
        IReadOnlyList<UpdateFinding> findings)
        => new(new UpdateResultFormation
        {
            Workspace = request.Workspace,
            Mode = request.Mode,
            Force = request.Force,
            Prune = request.Prune,
            Automatic = request.Automatic,
            Source = source,
            Comparisons = comparisons,
            GeneratedNavigation = generatedNavigation,
            Effects = effects,
            Lifecycle = lifecycle,
            Recovery = recovery,
            Verification = verification,
            Findings = findings,
        });
}
