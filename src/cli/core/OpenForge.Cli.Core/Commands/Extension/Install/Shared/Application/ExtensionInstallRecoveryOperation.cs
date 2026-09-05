using OpenForge.Cli.Core.Commands.Extension.Install.Models.Application;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Application;

internal sealed class ExtensionInstallRecoveryOperation(
    RecoveryBundleStore store,
    RecoveryBundleCatalogue catalogue,
    RecoveryBundleDeletionGuard deletionGuard)
{
    private readonly RecoveryBundleStore _store = store;
    private readonly RecoveryBundleCatalogue _catalogue = catalogue;
    private readonly RecoveryBundleDeletionGuard _deletionGuard = deletionGuard;

    internal ValueTask<RecoveryBundlePreparationResult> PrepareAsync(
        ExtensionInstallPlan plan,
        Guid operationId,
        CancellationToken cancellationToken)
        => _store.PrepareAsync(
            RecoveryBundleInput.Create(
                plan.Request.Workspace,
                ExtensionInstallDefinitions.CommandIdentity,
                RecoveryBundleAttribution.Create(
                    RecoveryBundleProducer.Extension,
                    RecoveryBundleOperation.Install,
                    plan.Request.Workspace),
                operationId,
                plan.RecoveryTargets),
            cancellationToken);

    internal async ValueTask<ExtensionInstallRecoveryCleanupResult> CleanupAsync(
        ExtensionInstallRecoveryCleanupRequest request,
        CancellationToken cancellationToken)
    {
        RecoveryBundleCatalogueResult catalogue;
        try
        {
            catalogue = await _catalogue.ReadAsync(
                request.Plan.Request.Workspace,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Unknown(
                request.Preparation,
                ExtensionInstallFindingCode.Interrupted,
                "Extension Install recovery cleanup was interrupted.",
                request.Preparation.BundlePath);
        }
        catch (Exception)
        {
            return Unknown(
                request.Preparation,
                ExtensionInstallFindingCode.RecoveryFailed,
                "Extension Install recovery cleanup failed unexpectedly.");
        }

        if (catalogue.State != RecoveryBundleCatalogueState.Available)
        {
            return catalogue.State == RecoveryBundleCatalogueState.Cancelled
                ? Unknown(
                    request.Preparation,
                    ExtensionInstallFindingCode.Interrupted,
                    catalogue.Cause ?? "Extension Install recovery cleanup was interrupted.",
                    request.Preparation.BundlePath)
                : Unknown(
                    request.Preparation,
                    ExtensionInstallFindingCode.RecoveryFailed,
                    catalogue.Cause ?? "The prepared Extension Install recovery bundle is unavailable for cleanup.");
        }

        var candidates = catalogue.Candidates.Where(candidate => Matches(
            candidate,
            request.Preparation)).ToArray();
        if (candidates.Length != 1)
        {
            return Unknown(
                request.Preparation,
                ExtensionInstallFindingCode.RecoveryFailed,
                "The prepared Extension Install recovery bundle no longer has one exact recognized identity.");
        }

        RecoveryBundleDeletionResult deletion;
        try
        {
            deletion = await _deletionGuard.DeleteAsync(
                request.Lease,
                candidates[0],
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Unknown(
                request.Preparation,
                ExtensionInstallFindingCode.Interrupted,
                "Extension Install recovery cleanup was interrupted.");
        }
        catch (Exception)
        {
            return Unknown(
                request.Preparation,
                ExtensionInstallFindingCode.RecoveryFailed,
                "Extension Install recovery cleanup failed unexpectedly.");
        }

        return ReadDeletion(request.Preparation, deletion);
    }

    private static ExtensionInstallRecoveryCleanupResult ReadDeletion(
        RecoveryBundlePreparation preparation,
        RecoveryBundleDeletionResult deletion)
    {
        var protectedPaths = ProtectedPaths(preparation);
        return deletion.State switch
        {
            RecoveryBundleDeletionState.Deleted => new(
                new ExtensionInstallRecovery(
                    ExtensionInstallRecoveryState.Removed,
                    protectedPaths,
                    residualPath: null),
                Finding: null),
            RecoveryBundleDeletionState.Failed
                when deletion.Disposition == RecoveryBundleDisposition.Retained => new(
                    new ExtensionInstallRecovery(
                        ExtensionInstallRecoveryState.Retained,
                        protectedPaths,
                        deletion.ResidualPath ?? preparation.BundlePath),
                    new ExtensionInstallFinding(
                        ExtensionInstallFindingCode.RecoveryArtifactRetained,
                        deletion.Cause
                            ?? "The verified Extension Install recovery bundle remains after cleanup.",
                        deletion.ResidualPath ?? preparation.BundlePath)),
            RecoveryBundleDeletionState.Cancelled => new(
                RecoveryFromDisposition(preparation, deletion),
                new ExtensionInstallFinding(
                    ExtensionInstallFindingCode.Interrupted,
                    deletion.Cause ?? "Extension Install recovery cleanup was interrupted.")),
            RecoveryBundleDeletionState.Failed
                or RecoveryBundleDeletionState.Blocked => new(
                    RecoveryFromDisposition(preparation, deletion),
                    new ExtensionInstallFinding(
                        ExtensionInstallFindingCode.RecoveryFailed,
                        deletion.Cause
                            ?? "The Extension Install recovery artifact disposition is unsafe or unknown.")),
            _ => throw new ArgumentOutOfRangeException(
                nameof(deletion),
                deletion.State,
                "The recovery deletion state is not defined."),
        };
    }

    private static ExtensionInstallRecovery RecoveryFromDisposition(
        RecoveryBundlePreparation preparation,
        RecoveryBundleDeletionResult deletion)
        => new(
            deletion.Disposition == RecoveryBundleDisposition.Retained
                ? ExtensionInstallRecoveryState.Retained
                : ExtensionInstallRecoveryState.Unknown,
            ProtectedPaths(preparation),
            deletion.ResidualPath);

    private static ExtensionInstallRecoveryCleanupResult Unknown(
        RecoveryBundlePreparation preparation,
        ExtensionInstallFindingCode code,
        string cause)
        => Unknown(preparation, code, cause, residualPath: null);

    private static ExtensionInstallRecoveryCleanupResult Unknown(
        RecoveryBundlePreparation preparation,
        ExtensionInstallFindingCode code,
        string cause,
        string? residualPath)
        => new(
            new ExtensionInstallRecovery(
                ExtensionInstallRecoveryState.Unknown,
                ProtectedPaths(preparation),
                residualPath),
            new ExtensionInstallFinding(code, cause));

    private static IReadOnlyList<string> ProtectedPaths(
        RecoveryBundlePreparation preparation)
        => preparation.Entries
            .OrderBy(entry => entry.Ordinal)
            .Select(entry => entry.TargetPath)
            .ToArray();

    private static bool Matches(
        RecoveryBundleCandidateSnapshot candidate,
        RecoveryBundlePreparation preparation)
    {
        if (candidate.Kind != RecoveryBundleCandidateKind.Final
            || candidate.Integrity != RecoveryBundleIntegrity.Verified
            || candidate.Verified is not { } verified)
        {
            return false;
        }

        return PhysicalIdentityTracker.PathComparer.Equals(candidate.Path, preparation.BundlePath)
            && PhysicalIdentityTracker.PathComparer.Equals(
                verified.WorkspacePhysicalPath,
                preparation.WorkspacePhysicalPath)
            && string.Equals(verified.WorkspaceKey, preparation.WorkspaceKey, StringComparison.Ordinal)
            && string.Equals(verified.Command, preparation.Command, StringComparison.Ordinal)
            && verified.OperationId == preparation.OperationId
            && verified.Entries.SequenceEqual(preparation.Entries);
    }
}
