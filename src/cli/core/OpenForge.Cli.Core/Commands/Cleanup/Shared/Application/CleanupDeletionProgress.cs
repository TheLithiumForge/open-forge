using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Cleanup.Shared.Application;

internal sealed class CleanupDeletionProgress
{
    private readonly CleanupPlan _plan;
    private readonly List<CleanupEffect> _recorded = [];
    private CleanupFinding? _finding;

    internal CleanupDeletionProgress(CleanupPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (plan.Safety != CleanupPlanSafety.Safe || plan.Request is null)
        {
            throw new ArgumentException("Deletion progress requires an established safe plan.", nameof(plan));
        }

        _plan = plan;
    }

    internal CleanupPlanEntry? Next
    {
        get
        {
            if (_finding is not null || _recorded.Count == _plan.DeletionEntries.Length)
            {
                return null;
            }

            return _plan.DeletionEntries[_recorded.Count];
        }
    }

    internal ImmutableArray<CleanupEffect> Effects
        => [.. _recorded, .. _plan.DeletionEntries.Skip(_recorded.Count).Select(entry => CleanupEffect.Create(
            entry, CleanupEffectOutcome.NotStarted, CleanupEffectResidual.Retained))];

    internal ImmutableArray<CleanupFinding> Findings => _finding is null ? [] : [_finding];

    internal void Record(RecoveryBundleDeletionResult deletion)
    {
        ArgumentNullException.ThrowIfNull(deletion);
        var entry = Next ?? throw new InvalidOperationException("Cleanup deletion has stopped or exhausted its plan.");
        if (deletion.State == RecoveryBundleDeletionState.Deleted)
        {
            _recorded.Add(CleanupEffect.Create(entry, CleanupEffectOutcome.Verified, CleanupEffectResidual.None));
            return;
        }

        var cause = deletion.Cause ?? "Cleanup was interrupted before the next deletion.";
        var residual = deletion.Disposition switch
        {
            RecoveryBundleDisposition.Retained => CleanupEffectResidual.Retained,
            RecoveryBundleDisposition.Unknown => CleanupEffectResidual.Unknown,
            RecoveryBundleDisposition.Removed => throw new InvalidOperationException("An unsuccessful deletion cannot report removal."),
            _ => throw new ArgumentOutOfRangeException(nameof(deletion), deletion.Disposition, "The recovery disposition is not defined."),
        };
        var outcome = ReadOutcome(deletion);
        var code = ReadFinding(deletion);
        _recorded.Add(CleanupEffect.Create(entry, outcome, residual, cause));
        _finding = CleanupFinding.Create(code, cause, entry.Path);
    }

    private static CleanupEffectOutcome ReadOutcome(RecoveryBundleDeletionResult deletion)
    {
        if (deletion.Disposition == RecoveryBundleDisposition.Unknown)
        {
            return CleanupEffectOutcome.CompletionUnknown;
        }

        return deletion.State switch
        {
            RecoveryBundleDeletionState.Failed => CleanupEffectOutcome.VerificationFailed,
            RecoveryBundleDeletionState.Blocked or RecoveryBundleDeletionState.Cancelled => CleanupEffectOutcome.NotStarted,
            RecoveryBundleDeletionState.Deleted => throw new InvalidOperationException("Verified deletion is recorded separately."),
            _ => throw new ArgumentOutOfRangeException(nameof(deletion), deletion.State, "The recovery deletion state is not defined."),
        };
    }

    private static CleanupFindingCode ReadFinding(RecoveryBundleDeletionResult deletion)
        => deletion.State switch
        {
            RecoveryBundleDeletionState.Blocked => CleanupFindingCode.CandidateChangedDuringApply,
            RecoveryBundleDeletionState.Cancelled => CleanupFindingCode.Interrupted,
            RecoveryBundleDeletionState.Failed => deletion.Disposition == RecoveryBundleDisposition.Retained
                ? CleanupFindingCode.VerificationFailed
                : CleanupFindingCode.DeletionFailed,
            RecoveryBundleDeletionState.Deleted => throw new InvalidOperationException("Verified deletion has no finding."),
            _ => throw new ArgumentOutOfRangeException(nameof(deletion), deletion.State, "The recovery deletion state is not defined."),
        };
}
