using OpenForge.Cli.Core.Commands.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Remove.Shared.Effects;
using OpenForge.Cli.Core.Commands.Remove.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Remove.Shared.Application;

internal sealed class RemovePathApplication
{
    private const string CommandIdentity = "remove";

    private readonly PhysicalPathResolver _resolver;
    private readonly WorkspaceLockManager _lockManager;
    private readonly FileExpectationValidator _validator;
    private readonly FileChangeApplier _fileApplier;
    private readonly DirectoryCreationApplier _directoryCreationApplier;
    private readonly DirectoryDeletionApplier _directoryDeletionApplier;

    internal RemovePathApplication(
        PhysicalPathResolver resolver,
        WorkspaceLockManager lockManager,
        FileExpectationValidator validator,
        FileChangeApplier fileApplier,
        DirectoryCreationApplier directoryCreationApplier,
        DirectoryDeletionApplier directoryDeletionApplier)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(lockManager);
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(fileApplier);
        ArgumentNullException.ThrowIfNull(directoryCreationApplier);
        ArgumentNullException.ThrowIfNull(directoryDeletionApplier);
        _resolver = resolver;
        _lockManager = lockManager;
        _validator = validator;
        _fileApplier = fileApplier;
        _directoryCreationApplier = directoryCreationApplier;
        _directoryDeletionApplier = directoryDeletionApplier;
    }

    internal async ValueTask<RemoveResult> ApplyAsync(
        RemovePathPlan initial,
        RemovePathPlanner planner,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(initial);
        ArgumentNullException.ThrowIfNull(planner);
        var operationId = Guid.NewGuid();
        var lockResult = await _lockManager.AcquireAsync(
            new WorkspaceLockRequest(initial.Request.Workspace, CommandIdentity, operationId),
            cancellationToken).ConfigureAwait(false);
        if (lockResult.State != WorkspaceLockState.Acquired || lockResult.Lease is not { } lease)
        {
            return RemoveResult.Refused(
                initial.Request.Workspace,
                initial.Target,
                "path",
                lockResult.State == WorkspaceLockState.Cancelled
                    ? RemoveFindingCode.Interrupted
                    : RemoveFindingCode.WorkspaceLockUnavailable,
                lockResult.State == WorkspaceLockState.Cancelled
                    ? CliSemanticStatus.Interrupted
                    : CliSemanticStatus.Blocked,
                lockResult.Cause ?? "The workspace mutation lease could not be acquired.");
        }

        await using (lease.ConfigureAwait(false))
        {
            var freshOutcome = await planner.PlanAsync(initial.Request, cancellationToken).ConfigureAwait(false);
            if (freshOutcome is RemovePathPlanningOutcome.Stopped stopped)
            {
                return stopped.Result;
            }

            var current = ((RemovePathPlanningOutcome.Planned)freshOutcome).Plan;
            if (!initial.Matches(current))
            {
                return planner.Refuse(initial, RemoveFindingCode.TargetChanged, CliSemanticStatus.Blocked,
                    "The selected target, settings, or ownership changed after planning. Nothing was changed.");
            }

            foreach (var input in current.Navigation.Inputs)
            {
                var validation = await _validator.ValidateAsync(
                    initial.Request.Workspace,
                    input.Expectation,
                    cancellationToken).ConfigureAwait(false);
                if (validation.State != FileExpectationValidationState.Matched)
                {
                    return planner.Refuse(initial, RemoveFindingCode.TargetChanged, CliSemanticStatus.Blocked,
                        validation.Cause ?? "A source used for generated navigation changed before removal began.");
                }
            }

            return await ApplyUnderLeaseAsync(lease, current, operationId, cancellationToken).ConfigureAwait(false);
        }
    }

    private async ValueTask<RemoveResult> ApplyUnderLeaseAsync(
        WorkspaceLockLease lease,
        RemovePathPlan plan,
        Guid operationId,
        CancellationToken cancellationToken)
    {
        var recoveryTargets = new List<RecoveryBundleTarget>();
        AddChange(plan.SettingsChange, recoveryTargets);
        AddChange(plan.OwnershipChange, recoveryTargets);
        foreach (var snapshot in plan.Files)
        {
            var change = PlannedFileChange.Delete(snapshot.Expectation);
            recoveryTargets.Add(RecoveryBundleTarget.Create(change, snapshot));
        }
        foreach (var link in plan.Links)
        {
            recoveryTargets.Add(RecoveryBundleTarget.Create(link.Effect, link.Before));
        }
        foreach (var navigation in plan.Navigation.Changes)
        {
            recoveryTargets.Add(RecoveryBundleTarget.Create(navigation.Change, navigation.Snapshot));
        }

        var recoveryInput = RecoveryBundleInput.Create(
            plan.Request.Workspace,
            CommandIdentity,
            RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Workspace,
                RecoveryBundleOperation.Remove,
                plan.Request.Workspace),
            operationId,
            recoveryTargets);
        var recoveryResult = await RecoveryBundleStore.PrepareAsync(recoveryInput, cancellationToken).ConfigureAwait(false);
        if (recoveryResult.State is not (RecoveryBundlePreparationState.Prepared or RecoveryBundlePreparationState.NotNeeded))
        {
            return RemoveResult.Refused(
                plan.Request.Workspace,
                plan.Target,
                "path",
                recoveryResult.State == RecoveryBundlePreparationState.Cancelled
                    ? RemoveFindingCode.Interrupted
                    : RemoveFindingCode.RecoveryUnavailable,
                recoveryResult.State == RecoveryBundlePreparationState.Cancelled
                    ? CliSemanticStatus.Interrupted
                    : CliSemanticStatus.Blocked,
                recoveryResult.Cause ?? "A verified recovery bundle could not be prepared.",
                recoveryPath: recoveryResult.ResidualPath,
                recoveryDisposition: recoveryResult.ResidualPath is null ? "not-required" : "retained");
        }

        var recovery = recoveryResult.Preparation;
        var receipts = new List<RemoveEffect>();
        if (plan.AgentsCreation is { } creation)
        {
            var check = await _validator.ValidateAsync(
                plan.Request.Workspace,
                creation.Expectation,
                cancellationToken).ConfigureAwait(false);
            if (check.State != FileExpectationValidationState.Matched || check.Actual is null)
            {
                return PartialFailure(plan, receipts, recovery?.BundlePath,
                    "The .agents directory changed before creation.");
            }
            var receipt = await _directoryCreationApplier.ApplyAsync(lease, creation, check, cancellationToken).ConfigureAwait(false);
            receipts.Add(new RemoveEffect(
                RelativePath(plan.Request.Workspace, creation.LogicalPath),
                "directory",
                "create",
                ReceiptOutcome(receipt.EffectState, receipt.VerificationState)));
            if (receipt.EffectState != FilesystemEffectState.Applied
                || receipt.VerificationState != FilesystemVerificationState.Verified)
            {
                return PartialFailure(plan, receipts, recovery?.BundlePath,
                    receipt.Cause ?? "The .agents directory could not be created and verified.");
            }
        }

        if (plan.SettingsChange is { } settingsChange)
        {
            var receipt = await ApplyFileChangeAsync(lease, settingsChange.Change, settingsChange.Before, recovery, cancellationToken)
                .ConfigureAwait(false);
            receipts.Add(Effect(
                plan.Request.Workspace,
                settingsChange.Change,
                "setting",
                RemovePathEffectActions.SettingsPersistence(settingsChange.Change),
                receipt));
            if (!IsVerified(receipt))
            {
                return PartialFailure(plan, receipts, recovery?.BundlePath,
                    receipt.Cause ?? "Removal settings could not be persisted and verified.");
            }
        }

        foreach (var snapshot in plan.Files)
        {
            var change = PlannedFileChange.Delete(snapshot.Expectation);
            var receipt = await ApplyFileChangeAsync(lease, change, snapshot, recovery, cancellationToken).ConfigureAwait(false);
            receipts.Add(Effect(plan.Request.Workspace, change, "file", receipt));
            if (!IsVerified(receipt))
            {
                return PartialFailure(plan, receipts, recovery?.BundlePath,
                    receipt.Cause ?? "A selected file could not be removed and verified.");
            }
        }

        foreach (var link in plan.Links)
        {
            var receipt = await RelativeFileLinkApplier.ApplyAsync(
                _resolver,
                lease,
                new RelativeFileLinkApplicationInput
                {
                    Effect = link.Effect,
                    Expected = link.Before,
                    RecoveryPreparation = recovery,
                },
                cancellationToken).ConfigureAwait(false);
            receipts.Add(new RemoveEffect(
                link.Path,
                "link",
                "delete",
                ReceiptOutcome(receipt.EffectState, receipt.VerificationState)));
            if (receipt.EffectState != FilesystemEffectState.Applied
                || receipt.VerificationState != FilesystemVerificationState.Verified)
            {
                return PartialFailure(plan, receipts, recovery?.BundlePath,
                    receipt.Cause ?? "A selected Library link could not be removed and verified.");
            }
        }

        foreach (var navigation in plan.Navigation.Changes)
        {
            var receipt = await ApplyFileChangeAsync(
                lease,
                navigation.Change,
                navigation.Snapshot,
                recovery,
                cancellationToken).ConfigureAwait(false);
            receipts.Add(Effect(plan.Request.Workspace, navigation.Change, "navigation", receipt));
            if (!IsVerified(receipt))
            {
                return PartialFailure(plan, receipts, recovery?.BundlePath,
                    receipt.Cause ?? "Generated navigation could not be updated and verified.");
            }
        }

        foreach (var snapshot in plan.Directories)
        {
            var deletion = PlannedDirectoryDeletion.DeleteIfEmpty(snapshot.Expectation);
            var check = await _validator.ValidateAsync(
                plan.Request.Workspace,
                deletion.Expectation,
                cancellationToken).ConfigureAwait(false);
            if (check.State != FileExpectationValidationState.Matched || check.Actual is null)
            {
                return PartialFailure(plan, receipts, recovery?.BundlePath,
                    "A selected directory changed before removal.");
            }
            var receipt = await _directoryDeletionApplier.ApplyAsync(lease, deletion, check, cancellationToken).ConfigureAwait(false);
            receipts.Add(new RemoveEffect(
                RelativePath(plan.Request.Workspace, snapshot.LogicalPath),
                "directory",
                "delete",
                receipt.State.Disposition == DirectoryDeletionDisposition.Removed ? "done" : "not-started"));
            if (receipt.State.Disposition != DirectoryDeletionDisposition.Removed)
            {
                return PartialFailure(plan, receipts, recovery?.BundlePath,
                    receipt.Cause ?? "A selected directory was not empty or could not be deleted.");
            }
        }

        if (plan.OwnershipChange is { } ownershipChange)
        {
            var receipt = await ApplyFileChangeAsync(
                lease,
                ownershipChange.Change,
                ownershipChange.Before,
                recovery,
                cancellationToken).ConfigureAwait(false);
            receipts.Add(Effect(
                plan.Request.Workspace,
                ownershipChange.Change,
                "record",
                "release-ownership",
                receipt));
            if (!IsVerified(receipt))
            {
                return PartialFailure(plan, receipts, recovery?.BundlePath,
                    receipt.Cause ?? "Ownership release could not be published and verified.");
            }
        }

        return RemoveResult.Complete(
            plan.Request.Workspace,
            plan.Target,
            "path",
            dryRun: false,
            receipts,
            plan.Files.Select(file => RelativePath(plan.Request.Workspace, file.LogicalPath))
                .Concat(plan.Links.Select(link => link.Path)),
            recovery?.BundlePath,
            recovery is null ? "not-required" : "retained");
    }

    private async ValueTask<FileChangeReceipt> ApplyFileChangeAsync(
        WorkspaceLockLease lease,
        PlannedFileChange change,
        FileStateSnapshot before,
        RecoveryBundlePreparation? recovery,
        CancellationToken cancellationToken)
    {
        var check = await _validator.ValidateAsync(lease.Request.Workspace, change.Expectation, cancellationToken)
            .ConfigureAwait(false);
        if (check.State != FileExpectationValidationState.Matched || check.Actual is null)
        {
            return FileChangeReceipt.NotStarted(
                change,
                before,
                FilesystemNotStartedReason.TargetChanged,
                check.Cause ?? "The planned file target changed before its effect.");
        }
        return await _fileApplier.ApplyAsync(lease, change, check, recovery, cancellationToken).ConfigureAwait(false);
    }

    private static void AddChange(
        RemovePathMetadataChange? metadata,
        ICollection<RecoveryBundleTarget> targets)
    {
        if (metadata is null)
        {
            return;
        }
        targets.Add(metadata.Change.Kind == PlannedFileChangeKind.Create
            ? RecoveryBundleTarget.CreateReversible(metadata.Change, metadata.Before)
            : RecoveryBundleTarget.Create(metadata.Change, metadata.Before));
    }

    private static RemoveResult PartialFailure(
        RemovePathPlan plan,
        IReadOnlyList<RemoveEffect> effects,
        string? recoveryPath,
        string cause)
        => RemoveResult.Refused(
            plan.Request.Workspace,
            plan.Target,
            "path",
            RemoveFindingCode.WriteFailed,
            CliSemanticStatus.Incomplete,
            cause,
            "open-forge cleanup",
            "Inspect the retained recovery bundle before continuing.",
            effects: effects,
            recoveryPath: recoveryPath,
            recoveryDisposition: recoveryPath is null ? "unknown" : "retained",
            removed: [.. effects
                .Where(effect => effect.Kind is "file" or "link"
                    && effect.Action == "delete"
                    && effect.Outcome == "done")
                .Select(effect => effect.Path)]);

    private static RemoveEffect Effect(
        CliWorkspace workspace,
        PlannedFileChange change,
        string kind,
        FileChangeReceipt receipt)
        => Effect(
            workspace,
            change,
            kind,
            change.Kind == PlannedFileChangeKind.Delete ? "delete" : "persist",
            receipt);

    private static RemoveEffect Effect(
        CliWorkspace workspace,
        PlannedFileChange change,
        string kind,
        string action,
        FileChangeReceipt receipt)
        => new(
            RelativePath(workspace, change.LogicalPath),
            kind,
            action,
            ReceiptOutcome(receipt.EffectState, receipt.VerificationState));

    private static string ReceiptOutcome(FilesystemEffectState effect, FilesystemVerificationState verification)
        => (effect, verification) switch
        {
            (FilesystemEffectState.Applied, FilesystemVerificationState.Verified) => "done",
            (FilesystemEffectState.Unknown, _) => "unknown",
            _ => "failed",
        };

    private static bool IsVerified(FileChangeReceipt receipt)
        => receipt.EffectState == FilesystemEffectState.Applied
            && receipt.VerificationState == FilesystemVerificationState.Verified;

    private static string RelativePath(CliWorkspace workspace, string logicalPath)
        => Path.GetRelativePath(workspace.LexicalRoot, logicalPath)
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');
}
