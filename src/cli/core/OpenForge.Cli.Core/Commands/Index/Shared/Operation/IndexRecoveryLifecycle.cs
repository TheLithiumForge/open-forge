using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Index.Shared.Operation;

internal static class IndexRecoveryLifecycle
{
    internal static async ValueTask<IndexPreparationMapping> PrepareAsync(
        IndexApplicationContext application,
        CancellationToken cancellationToken)
    {
        var input = RecoveryBundleInput.Create(
            workspace: application.Request.Workspace,
            command: IndexDefinitions.CommandIdentity,
            attribution: RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Index,
                RecoveryBundleOperation.Index,
                application.Request.Workspace),
            operationId: application.OperationId,
            targets: application.Plan.RecoveryTargets);
        RecoveryBundlePreparationResult result;
        try
        {
            result = await RecoveryBundleStore.PrepareAsync(
                    input,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RejectedPreparation(IndexFindingCode.Interrupted);
        }
        catch (Exception)
        {
            return RejectedPreparation(IndexFindingCode.OperationFailed);
        }

        return IndexMutationMapper.ReadPreparation(result);
    }

    internal static async ValueTask<IndexDeletionMapping> DeleteAsync(
        IndexPreparedApplication prepared,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Retained(prepared, IndexFindingCode.Interrupted);
        }

        RecoveryBundleCatalogueResult catalogue;
        try
        {
            catalogue = await RecoveryBundleCatalogue.ReadAsync(
                    prepared.Application.Request.Workspace,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Retained(prepared, IndexFindingCode.Interrupted);
        }
        catch (Exception)
        {
            return Unknown(prepared, IndexFindingCode.RecoveryFailed);
        }

        switch (catalogue.State)
        {
            case RecoveryBundleCatalogueState.Cancelled:
                return Retained(prepared, IndexFindingCode.Interrupted);
            case RecoveryBundleCatalogueState.Unavailable:
                return Unknown(prepared, IndexFindingCode.RecoveryFailed);
            case RecoveryBundleCatalogueState.Available:
                break;
            default:
                throw InvalidCatalogueState(catalogue.State);
        }

        var candidates = catalogue.Candidates
            .Where(candidate => MatchesPreparation(candidate, prepared.Preparation))
            .ToArray();
        if (candidates.Length != 1)
        {
            return Unknown(prepared, IndexFindingCode.RecoveryFailed);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Retained(prepared, IndexFindingCode.Interrupted);
        }

        RecoveryBundleDeletionResult deletion;
        try
        {
            deletion = await RecoveryBundleDeletionGuard.DeleteAsync(
                    prepared.Lease,
                    candidates[0],
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Unknown(prepared, IndexFindingCode.Interrupted);
        }
        catch (Exception)
        {
            return Unknown(prepared, IndexFindingCode.RecoveryFailed);
        }

        return IndexMutationMapper.ReadDeletion(deletion);
    }

    private static IndexPreparationMapping RejectedPreparation(IndexFindingCode findingCode)
        => IndexPreparationMapping.Rejected(
            recovery: new IndexRecovery(IndexRecoveryState.Unknown, residualPath: null),
            findingCode: findingCode);

    private static IndexDeletionMapping Retained(
        IndexPreparedApplication prepared,
        IndexFindingCode findingCode)
        => new(
            recovery: new IndexRecovery(
                IndexRecoveryState.Retained,
                prepared.Preparation.BundlePath),
            findingCode: findingCode);

    private static IndexDeletionMapping Unknown(
        IndexPreparedApplication prepared,
        IndexFindingCode findingCode)
        => new(
            recovery: new IndexRecovery(
                IndexRecoveryState.Unknown,
                prepared.Preparation.BundlePath),
            findingCode: findingCode);

    private static ArgumentOutOfRangeException InvalidCatalogueState(
        RecoveryBundleCatalogueState state)
        => new(
            nameof(state),
            state,
            "The recovery catalogue state is not defined.");

    private static bool MatchesPreparation(
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
            && PhysicalIdentityTracker.PathComparer.Equals(verified.BundlePath, preparation.BundlePath)
            && PhysicalIdentityTracker.PathComparer.Equals(
                verified.WorkspacePhysicalPath,
                preparation.WorkspacePhysicalPath)
            && string.Equals(verified.WorkspaceKey, preparation.WorkspaceKey, StringComparison.Ordinal)
            && string.Equals(verified.Command, preparation.Command, StringComparison.Ordinal)
            && verified.OperationId == preparation.OperationId
            && verified.Entries.SequenceEqual(preparation.Entries);
    }
}
