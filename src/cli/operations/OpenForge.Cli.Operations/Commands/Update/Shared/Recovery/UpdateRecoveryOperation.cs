using OpenForge.Cli.Core.Commands.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Recovery;

internal static class UpdateRecoveryOperation
{
    internal static ValueTask<RecoveryBundlePreparationResult> PrepareAsync(
        UpdatePlanExecution execution,
        Guid operationId,
        CancellationToken cancellationToken)
    {
        var targets = ReadTargets(execution);

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

    internal static IReadOnlyList<RecoveryBundleTarget> ReadTargets(UpdatePlanExecution execution)
    {
        var targets = execution.Effects
            .Select(effect => RecoveryBundleTarget.Create(
                effect.FileChange,
                ReadCapturedSnapshot(effect, execution)))
            .ToList();
        if (execution.OwnershipChange is { } ownership)
        {
            targets.Add(RecoveryBundleTarget.Create(
                ownership,
                execution.OwnershipRead.Snapshot
                    ?? throw new InvalidOperationException(
                        "A planned Update ownership change requires its exact source snapshot.")));
        }

        return targets;
    }

    internal static async ValueTask<UpdateRecoveryCleanup> VerifyRetainedAsync(
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
                "Update recovery verification was interrupted.",
                UpdateRecoveryState.Unknown,
                protectedPaths,
                residualPath: null);
        }
        catch (Exception)
        {
            return Boundary(
                UpdateFindingCode.RecoveryFailed,
                "Update recovery verification failed unexpectedly.",
                UpdateRecoveryState.Unknown,
                protectedPaths,
                residualPath: null);
        }

        var candidate = catalogue.State == RecoveryBundleCatalogueState.Available
            ? catalogue.Candidates.SingleOrDefault(value => RecoveryBundleIdentity.Matches(value, preparation))
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

        return new UpdateRecoveryCleanup(new UpdateRecovery
        {
            State = UpdateRecoveryState.Retained,
            ProtectedPaths = protectedPaths,
            ResidualPath = preparation.BundlePath,
        }, Finding: null);
    }

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

    private static FileStateSnapshot ReadCapturedSnapshot(
        UpdatePlannedEffect effect,
        UpdatePlanExecution execution)
    {
        var observationSnapshot = execution.Observations
            .Where(value => value.Comparison.RelativePath == effect.ResultEffect.Path)
            .Select(value => value.Snapshot)
            .FirstOrDefault();
        if (observationSnapshot is not null)
        {
            return observationSnapshot;
        }

        var adoptionSnapshots = execution.AdoptionTargets
            .Where(value => value.Path == effect.ResultEffect.Path)
            .Select(value => value.Snapshot)
            .Take(2)
            .ToArray();
        return adoptionSnapshots.Length switch
        {
            1 => adoptionSnapshots[0],
            0 => throw new InvalidOperationException(
                "A planned Update effect requires one exact source snapshot from comparison observations or adoption targets."),
            _ => throw new InvalidOperationException(
                "A planned Update effect requires one exact adoption target snapshot."),
        };
    }
}
