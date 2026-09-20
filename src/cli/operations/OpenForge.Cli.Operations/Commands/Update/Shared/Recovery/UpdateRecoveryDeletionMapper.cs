using OpenForge.Cli.Core.Commands.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Recovery;

internal static class UpdateRecoveryDeletionMapper
{
    internal static UpdateRecoveryDeletionOutcome Map(RecoveryBundleDeletionResult deletion)
    {
        ArgumentNullException.ThrowIfNull(deletion);
        return deletion.State switch
        {
            RecoveryBundleDeletionState.Deleted => new UpdateRecoveryDeletionOutcome
            {
                State = UpdateRecoveryState.Removed,
                ResidualPath = null,
                Finding = null,
            },
            RecoveryBundleDeletionState.Failed => FromFailure(deletion),
            RecoveryBundleDeletionState.Blocked => FromStopped(
                deletion,
                UpdateFindingCode.RecoveryFailed),
            RecoveryBundleDeletionState.Cancelled => FromStopped(
                deletion,
                UpdateFindingCode.Interrupted),
            _ => throw new ArgumentOutOfRangeException(
                nameof(deletion),
                deletion.State,
                "The recovery deletion state is not defined."),
        };
    }

    private static UpdateRecoveryDeletionOutcome FromFailure(
        RecoveryBundleDeletionResult deletion)
        => deletion.Disposition switch
        {
            RecoveryBundleDisposition.Retained => FromStopped(
                deletion,
                UpdateFindingCode.RecoveryArtifactRetained),
            RecoveryBundleDisposition.Unknown => FromStopped(
                deletion,
                UpdateFindingCode.RecoveryFailed),
            RecoveryBundleDisposition.Removed => throw new ArgumentException(
                "A failed recovery deletion cannot report a removed disposition.",
                nameof(deletion)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(deletion),
                deletion.Disposition,
                "The recovery deletion disposition is not defined."),
        };

    private static UpdateRecoveryDeletionOutcome FromStopped(
        RecoveryBundleDeletionResult deletion,
        UpdateFindingCode code)
    {
        var state = deletion.Disposition switch
        {
            RecoveryBundleDisposition.Retained => UpdateRecoveryState.Retained,
            RecoveryBundleDisposition.Unknown => UpdateRecoveryState.Unknown,
            RecoveryBundleDisposition.Removed => throw new ArgumentException(
                "A stopped recovery deletion cannot report a removed disposition.",
                nameof(deletion)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(deletion),
                deletion.Disposition,
                "The recovery deletion disposition is not defined."),
        };
        return new UpdateRecoveryDeletionOutcome
        {
            State = state,
            ResidualPath = deletion.ResidualPath,
            Finding = new UpdateFinding(
                code,
                deletion.ResidualPath,
                deletion.Cause ?? ReadDefaultCause(deletion.State)),
        };
    }

    private static string ReadDefaultCause(RecoveryBundleDeletionState state)
        => state switch
        {
            RecoveryBundleDeletionState.Cancelled =>
                "Update recovery artifact cleanup was interrupted.",
            RecoveryBundleDeletionState.Blocked =>
                "Update recovery artifact cleanup was blocked.",
            RecoveryBundleDeletionState.Failed =>
                "Update recovery artifact cleanup failed.",
            RecoveryBundleDeletionState.Deleted =>
                throw new ArgumentException("A removed recovery artifact requires no finding."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The recovery deletion state is not defined."),
        };
}
