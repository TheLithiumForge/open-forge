using OpenForge.Cli.Core.Commands.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Recovery;

internal static class UpdateRecoveryOperation
{
    internal static ValueTask<RecoveryBundlePreparationResult> PrepareAsync(
        UpdatePlanExecution execution,
        Guid operationId,
        CancellationToken cancellationToken)
    {
        var targets = execution.Effects
            .Select(effect => RecoveryBundleTarget.Create(
                effect.FileChange,
                effect.ObservationsSnapshot(execution.Observations)))
            .ToList();
        if (execution.LifecycleChange is { } lifecycle)
        {
            targets.Add(RecoveryBundleTarget.Create(
                lifecycle,
                execution.LifecycleRead.File
                    ?? throw new InvalidOperationException(
                        "A planned Update lifecycle change requires its exact source snapshot.")));
        }

        return RecoveryBundleStore.PrepareAsync(
            RecoveryBundleInput.Create(
                execution.Request.Workspace,
                UpdateDefinitions.CommandIdentity,
                RecoveryBundleAttribution.Create(
                    RecoveryBundleProducer.Framework,
                    RecoveryBundleOperation.Update,
                    execution.Request.Workspace),
                operationId,
                targets),
            cancellationToken);
    }

    internal static async ValueTask<UpdateRecoveryCleanup> CleanupAsync(
        WorkspaceLockLease lease,
        RecoveryBundlePreparation preparation,
        IReadOnlyList<string> protectedPaths,
        CancellationToken cancellationToken)
    {
        RecoveryBundleCatalogueResult catalogue;
        try
        {
            catalogue = await RecoveryBundleCatalogue.ReadAsync(
                lease.Request.Workspace, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Boundary(
                UpdateFindingCode.Interrupted,
                "Update recovery cleanup was interrupted.",
                UpdateRecoveryState.Unknown,
                protectedPaths,
                residualPath: null);
        }
        catch (Exception)
        {
            return Boundary(
                UpdateFindingCode.RecoveryFailed,
                "Update recovery cleanup failed unexpectedly.",
                UpdateRecoveryState.Unknown,
                protectedPaths,
                residualPath: null);
        }

        var candidate = catalogue.State == RecoveryBundleCatalogueState.Available
            ? catalogue.Candidates.SingleOrDefault(value => Matches(value, preparation))
            : null;
        if (candidate is null)
        {
            return Boundary(
                catalogue.State == RecoveryBundleCatalogueState.Cancelled
                    ? UpdateFindingCode.Interrupted
                    : UpdateFindingCode.RecoveryFailed,
                catalogue.Cause
                    ?? "The prepared Update recovery bundle no longer has one exact recognized identity.",
                UpdateRecoveryState.Unknown,
                protectedPaths,
                residualPath: null);
        }

        RecoveryBundleDeletionResult deletion;
        try
        {
            deletion = await RecoveryBundleDeletionGuard.DeleteAsync(
                    lease,
                    candidate,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Boundary(
                UpdateFindingCode.Interrupted,
                "Update recovery cleanup was interrupted.",
                UpdateRecoveryState.Unknown,
                protectedPaths,
                preparation.BundlePath);
        }
        catch (Exception)
        {
            return Boundary(
                UpdateFindingCode.RecoveryFailed,
                "Update recovery cleanup failed unexpectedly.",
                UpdateRecoveryState.Unknown,
                protectedPaths,
                preparation.BundlePath);
        }

        var mapped = UpdateRecoveryDeletionMapper.Map(deletion);
        return new UpdateRecoveryCleanup(
            new UpdateRecovery
            {
                State = mapped.State,
                ProtectedPaths = protectedPaths,
                ResidualPath = mapped.ResidualPath,
            },
            mapped.Finding);
    }

    private static bool Matches(
        RecoveryBundleCandidateSnapshot candidate,
        RecoveryBundlePreparation preparation)
        => candidate.Kind == RecoveryBundleCandidateKind.Final
            && candidate.Integrity == RecoveryBundleIntegrity.Verified
            && candidate.Verified is { } verified
            && PhysicalIdentityTracker.PathComparer.Equals(
                candidate.Path,
                preparation.BundlePath)
            && verified.OperationId == preparation.OperationId
            && verified.Attribution == preparation.Attribution
            && verified.Entries.SequenceEqual(preparation.Entries);

    private static UpdateRecoveryCleanup Boundary(
        UpdateFindingCode code,
        string cause,
        UpdateRecoveryState state,
        IReadOnlyList<string> protectedPaths,
        string? residualPath)
        => new(
            new UpdateRecovery
            {
                State = state,
                ProtectedPaths = protectedPaths,
                ResidualPath = residualPath,
            },
            new UpdateFinding(code, target: null, cause));
}

internal static class UpdateRecoveryObservationExtensions
{
    internal static FileStateSnapshot ObservationsSnapshot(
        this UpdatePlannedEffect effect,
        IReadOnlyList<UpdateComparisonObservation> observations)
        => observations.First(value =>
            value.Comparison.RelativePath == effect.ResultEffect.Path).Snapshot;
}
