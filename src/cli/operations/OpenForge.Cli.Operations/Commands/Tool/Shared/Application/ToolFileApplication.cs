using OpenForge.Cli.Core.Commands.Tool.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Application;

/// <summary>One planned edit of a workspace file, with the observed state it was planned from.</summary>
internal sealed record ToolFileEdit(PlannedFileChange Change, FileStateSnapshot Before);

/// <summary>How many edits completed and, when one didn't, why.</summary>
internal sealed record ToolFileApplicationResult(int Applied, ToolFindingCode? Failure, string? Cause);

/// <summary>
/// Applies Open Forge's own Tool edits through the shared write path: the workspace lock, one recovery bundle, an
/// expected-state check right before each effect and verified atomic replacement. A verified run deletes its bundle.
/// </summary>
internal sealed class ToolFileApplication(WorkspaceLockStoreRoot? lockStoreRoot)
{
    internal async ValueTask<ToolFileApplicationResult> ApplyAsync(
        CliWorkspace workspace, ToolCommandKind kind, IReadOnlyList<ToolFileEdit> edits, CancellationToken cancellationToken)
    {
        if (edits.Count == 0) return new(0, null, null);
        var operationId = Guid.NewGuid();
        var identity = ToolCommandDefinitions.Identity(kind);
        var locks = lockStoreRoot is null ? WorkspaceLockManager.CreateForCurrentUser() : new WorkspaceLockManager(lockStoreRoot);
        var acquired = await locks.AcquireAsync(new WorkspaceLockRequest(workspace, identity, operationId), cancellationToken).ConfigureAwait(false);
        if (acquired.State != WorkspaceLockState.Acquired || acquired.Lease is not { } lease)
            return new(0, acquired.State == WorkspaceLockState.Cancelled ? ToolFindingCode.Interrupted : ToolFindingCode.WriteFailed, acquired.Cause);
        await using (lease.ConfigureAwait(false))
        {
            var attribution = RecoveryBundleAttribution.Create(RecoveryBundleProducer.Tool,
                kind == ToolCommandKind.Remove ? RecoveryBundleOperation.Remove : RecoveryBundleOperation.Install, workspace);
            var targets = edits.Select(edit => edit.Change.Kind == PlannedFileChangeKind.Create
                ? RecoveryBundleTarget.CreateReversible(edit.Change, edit.Before)
                : RecoveryBundleTarget.Create(edit.Change, edit.Before)).ToArray();
            var prepared = await RecoveryBundleStore.PrepareAsync(
                RecoveryBundleInput.Create(workspace, identity, attribution, operationId, targets), cancellationToken).ConfigureAwait(false);
            if (prepared.State is not (RecoveryBundlePreparationState.Prepared or RecoveryBundlePreparationState.NotNeeded))
                return new(0, prepared.State == RecoveryBundlePreparationState.Cancelled ? ToolFindingCode.Interrupted : ToolFindingCode.WriteFailed, prepared.Cause);
            var validator = new FileExpectationValidator(new PhysicalPathResolver());
            var applier = new FileChangeApplier(new MutationRevalidator(validator), validator);
            var applied = 0;
            foreach (var edit in edits)
            {
                var check = await validator.ValidateAsync(workspace, edit.Change.Expectation, cancellationToken).ConfigureAwait(false);
                if (check.State != FileExpectationValidationState.Matched)
                    return new(applied, check.State == FileExpectationValidationState.Cancelled ? ToolFindingCode.Interrupted : ToolFindingCode.TargetChangedDuringApply,
                        check.Cause);
                var receipt = await applier.ApplyAsync(lease, edit.Change, check, prepared.Preparation, cancellationToken).ConfigureAwait(false);
                if (receipt.EffectState != FilesystemEffectState.Applied || receipt.VerificationState != FilesystemVerificationState.Verified)
                {
                    return new(applied, receipt.NotStartedReason switch
                    {
                        FilesystemNotStartedReason.TargetChanged => ToolFindingCode.TargetChangedDuringApply,
                        FilesystemNotStartedReason.Cancelled => ToolFindingCode.Interrupted,
                        _ => ToolFindingCode.WriteFailed,
                    }, receipt.Cause);
                }
                applied++;
            }
            if (prepared.Preparation is { } preparation) await DeleteRecoveryAsync(lease, workspace, preparation, cancellationToken).ConfigureAwait(false);
            return new(applied, null, null);
        }
    }

    // A bundle left behind after a verified run is a recognized residual that status and cleanup report. It never undoes the run.
    private static async ValueTask DeleteRecoveryAsync(
        WorkspaceLockLease lease, CliWorkspace workspace, RecoveryBundlePreparation preparation, CancellationToken cancellationToken)
    {
        var catalogue = await RecoveryBundleCatalogue.ReadAsync(workspace, cancellationToken).ConfigureAwait(false);
        if (catalogue.State != RecoveryBundleCatalogueState.Available) return;
        var candidates = catalogue.Candidates.Where(candidate => RecoveryBundleIdentity.Matches(candidate, preparation)).ToArray();
        if (candidates.Length == 1) await RecoveryBundleDeletionGuard.DeleteAsync(lease, candidates[0], cancellationToken).ConfigureAwait(false);
    }
}
