using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Update.Shared.Result;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Planning;

internal static class UpdatePlanResultFactory
{
    internal static UpdateResult Boundary(
        UpdateRequest request,
        UpdateSource? source,
        IReadOnlyList<UpdateComparison> comparisons,
        UpdateFinding finding,
        UpdateLifecycle lifecycle)
    {
        IReadOnlyList<UpdatePhysicalEffect> effects = [];
        var recovery = new UpdateRecovery
        {
            State = UpdateRecoveryState.NotRequired,
            ProtectedPaths = [],
            ResidualPath = null,
        };
        IReadOnlyList<UpdateFinding> findings = [finding];
        return new UpdateResult(new UpdateResultFormation
        {
            Workspace = request.Workspace,
            Mode = request.Mode,
            Force = request.Force,
            Prune = request.Prune,
            Automatic = request.Automatic,
            Source = source,
            Comparisons = comparisons,
            GeneratedNavigation = null,
            Effects = effects,
            Lifecycle = lifecycle,
            Recovery = recovery,
            Verification = UpdateVerificationState.NotRequested,
            Findings = findings,
        });
    }

    internal static UpdateResult Preview(UpdatePlanCompletion completion)
    {
        var request = completion.Request;
        var observations = completion.Intended.Observations;
        var effects = completion.Effects;
        var ownershipChange = completion.OwnershipChange;
        var findings = completion.Findings;
        var existingTargetPaths = effects
            .Where(value => value.FileChange.Kind != PlannedFileChangeKind.Create)
            .Select(value => value.ResultEffect.Path)
            .ToList();
        if (ownershipChange?.Kind == PlannedFileChangeKind.Replace)
        {
            existingTargetPaths.Add(WorkspaceOwnershipDefinitions.RelativePath);
        }

        var requiresRecovery = existingTargetPaths.Count > 0;
        var source = Source(completion.Payload);
        var comparisons = observations.Select(value => value.Comparison).ToArray();
        var generatedNavigation = Navigation(observations);
        var directoryEffects = completion.DirectoryCreations.Select(creation =>
            UpdatePhysicalEffect.Directory(
                Path.GetRelativePath(request.Workspace.LexicalRoot, creation.LogicalPath)
                    .Replace(Path.DirectorySeparatorChar, '/'),
                UpdatePhysicalEffectOutcome.Planned,
                UpdatePhysicalEffectResidual.None));
        var resultEffects = directoryEffects.Concat(effects.Select(value => value.ResultEffect)).ToArray();
        var lifecycle = new UpdateLifecycle
        {
            Trust = UpdateLifecycleTrust.Trusted,
            Coverage = UpdateLifecycleCoverage.Complete,
            Action = completion.OwnershipState == OwnershipWritePlanState.Skipped ? UpdateLifecycleAction.None
                : ownershipChange is null ? UpdateLifecycleAction.Preserve : UpdateLifecycleAction.Publish,
            Outcome = completion.OwnershipState == OwnershipWritePlanState.Skipped ? UpdateLifecycleOutcome.NotRequested
                : ownershipChange is null ? UpdateLifecycleOutcome.AlreadyCurrent : UpdateLifecycleOutcome.Planned,
        };
        var recovery = new UpdateRecovery
        {
            State = requiresRecovery
                ? UpdateRecoveryState.NotCreated
                : UpdateRecoveryState.NotRequired,
            ProtectedPaths = existingTargetPaths,
            ResidualPath = null,
        };
        var verification = request.Mode == UpdateMode.Apply
            && completion.Plan.IsNoOp
            && ownershipChange is null
            && findings.Count == 0
                ? UpdateVerificationState.Verified
                : UpdateVerificationState.NotRequested;
        return new UpdateResult(new UpdateResultFormation
        {
            Workspace = request.Workspace,
            Mode = request.Mode,
            Force = request.Force,
            Prune = request.Prune,
            Automatic = request.Automatic,
            Source = source,
            Comparisons = comparisons,
            GeneratedNavigation = generatedNavigation,
            Effects = resultEffects,
            PreviousContentAvailable = UpdatePreviousContentObserver.Read(request.Workspace, resultEffects),
            Lifecycle = lifecycle,
            Recovery = recovery,
            Verification = verification,
            Findings = findings,
        });
    }

    internal static UpdateSource Source(FrameworkPayload payload)
        => new()
        {
            Id = "framework",
            Version = null,
            InventoryFingerprint = payload.InventoryFingerprint,
            AssetCount = payload.Assets.Length,
        };

    internal static UpdateLifecycle UnstartedLifecycle(
        UpdateLifecycleTrust trust,
        UpdateLifecycleCoverage coverage)
        => new()
        {
            Trust = trust,
            Coverage = coverage,
            Action = UpdateLifecycleAction.None,
            Outcome = UpdateLifecycleOutcome.NotStarted,
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

}
