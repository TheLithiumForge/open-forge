using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Application;

internal sealed class RepairRecoveryLifecycle(
    RecoveryBundleStore store,
    RecoveryBundleCatalogue catalogue,
    RecoveryBundleDeletionGuard deletionGuard)
{
    private readonly RecoveryBundleStore _store = store;
    private readonly RecoveryBundleCatalogue _catalogue = catalogue;
    private readonly RecoveryBundleDeletionGuard _deletionGuard = deletionGuard;

    internal async ValueTask<RecoveryBundlePreparationResult> PrepareAsync(
        RepairPlan plan,
        Guid operationId,
        CancellationToken cancellationToken)
    {
        var effects = plan.Effects;
        var input = RecoveryBundleInput.Create(
            plan.Request.Workspace,
            RepairDefinitions.CommandIdentity,
            effects[0].RecoveryAttribution,
            operationId,
            [.. effects.Select(effect => RecoveryBundleTarget.Create(
                effect.FileChange,
                effect.ExpectedState))]);
        return await _store.PrepareAsync(input, cancellationToken).ConfigureAwait(false);
    }

    internal async ValueTask<RecoveryBundleDeletionResult> DeleteAsync(
        WorkspaceLockLease lease,
        RecoveryBundlePreparation preparation,
        CancellationToken cancellationToken)
    {
        var catalogue = await _catalogue.ReadAsync(
            lease.Request.Workspace,
            cancellationToken).ConfigureAwait(false);
        if (catalogue.State != RecoveryBundleCatalogueState.Available)
        {
            return RecoveryBundleDeletionResult.BlockedUnknown(
                catalogue.Cause ?? "The Repair recovery catalogue is unavailable.",
                preparation.BundlePath);
        }

        var candidate = catalogue.Candidates.SingleOrDefault(value =>
            string.Equals(value.Path, preparation.BundlePath, PathComparison())
            && value.Kind == RecoveryBundleCandidateKind.Final
            && value.Integrity == RecoveryBundleIntegrity.Verified
            && value.Verified?.OperationId == preparation.OperationId);
        return candidate is null
            ? RecoveryBundleDeletionResult.BlockedUnknown(
                "The exact Repair recovery final changed before deletion.",
                preparation.BundlePath)
            : await _deletionGuard.DeleteAsync(lease, candidate, cancellationToken)
                .ConfigureAwait(false);
    }

    internal static RepairRecovery ReadPreparationFailure(
        RecoveryBundlePreparationResult preparation,
        RecoveryBundleAttribution attribution)
        => new(
            preparation.State == RecoveryBundlePreparationState.Blocked
                ? RepairRecoveryState.Blocked
                : RepairRecoveryState.Incomplete,
            preparation.ResidualPath is null
                ? RepairResidualState.Unknown
                : RepairResidualState.Retained,
            preparation.ResidualPath,
            attribution);

    internal static RepairRecovery ReadDeletion(
        RecoveryBundleDeletionResult deletion,
        RecoveryBundleAttribution attribution)
        => deletion.Disposition switch
        {
            RecoveryBundleDisposition.Removed => new RepairRecovery(
                RepairRecoveryState.Removed,
                RepairResidualState.None,
                residualPath: null,
                attribution),
            RecoveryBundleDisposition.Retained => new RepairRecovery(
                RepairRecoveryState.Retained,
                RepairResidualState.Retained,
                deletion.ResidualPath,
                attribution),
            RecoveryBundleDisposition.Unknown => new RepairRecovery(
                RepairRecoveryState.Unknown,
                RepairResidualState.Unknown,
                deletion.ResidualPath,
                attribution),
            _ => throw new ArgumentOutOfRangeException(
                nameof(deletion),
                deletion.Disposition,
                "The recovery disposition is not defined."),
        };

    private static StringComparison PathComparison()
        => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
}
