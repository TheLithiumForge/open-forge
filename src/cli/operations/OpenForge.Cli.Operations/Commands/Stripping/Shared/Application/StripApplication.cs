using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Shared.Mutation;
using OpenForge.Cli.Core.Commands.Shared;
using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Stripping.Shared.Application;

/// <summary>
/// Applies one reviewed strip plan under the workspace lease. The strip Behavior
/// contract defines the revalidation, effect order, recovery and verification.
/// </summary>
internal sealed class StripApplication(PhysicalPathResolver resolver, WorkspaceLockStoreRoot? lockStoreRoot)
{
    internal async ValueTask<StripResult> ApplyAsync(
        StripPlan plan,
        Func<CancellationToken, ValueTask<StripPlanBuild>> rebuild,
        CancellationToken token)
    {
        var manager = lockStoreRoot is null ? WorkspaceLockManager.CreateForCurrentUser() : new WorkspaceLockManager(lockStoreRoot);
        var validator = new FileExpectationValidator(resolver);
        var revalidator = new MutationRevalidator(validator);
        var operationId = Guid.NewGuid();
        var command = StrippingDefinitions.CommandIdentity(plan.Request.Command);
        var result = StripResultFactory.Preview(plan) with { Plan = [.. plan.Entries.Select(entry => entry with { State = StripEffectState.NotStarted })] };
        RecoveryBundlePreparation? preparation = null;
        var started = false;
        try
        {
            var acquired = await manager.AcquireAsync(new WorkspaceLockRequest(plan.Request.Workspace, command, operationId), token).ConfigureAwait(false);
            if (acquired.Lease is not { } lease)
            {
                var cancelled = acquired.State == WorkspaceLockState.Cancelled;
                return Stop(result,
                    cancelled ? CliSemanticStatus.Interrupted : CliSemanticStatus.Blocked,
                    StripResultFactory.Error(
                        cancelled ? StripFindingCode.Interrupted : StripFindingCode.WorkspaceLockUnavailable,
                        acquired.Cause ?? "The workspace lock could not be acquired.",
                        lockFailure: acquired.LockFailure is { } failure ? WorkspaceLockFailureProjection.From(failure) : null));
            }

            await using (lease.ConfigureAwait(false))
            {
                var layout = await new WorkspaceLayoutRevalidator(resolver).ValidateAsync(lease, plan.LayoutRead, token).ConfigureAwait(false);
                if (layout.State != MutationValidationState.Valid)
                {
                    return Changed(result, layout.Cause ?? "The workspace layout changed after planning. Nothing was changed.");
                }

                var fresh = await rebuild(token).ConfigureAwait(false);
                if (fresh.Plan is not { } current || !plan.SameAs(current))
                {
                    return Changed(result, "A planned file changed after the preview. Nothing was changed.");
                }

                var targets = plan.Files.Select(effect => effect.Change.Kind == PlannedFileChangeKind.Create
                    ? RecoveryBundleTarget.CreateReversible(effect.Change, effect.Before)
                    : RecoveryBundleTarget.Create(effect.Change, effect.Before)).ToArray();
                var prepared = await RecoveryBundleStore.PrepareAsync(RecoveryBundleInput.Create(
                    plan.Request.Workspace, command,
                    RecoveryBundleAttribution.Create(RecoveryBundleProducer.Workspace, RecoveryBundleOperation.Remove, plan.Request.Workspace),
                    operationId, targets), token).ConfigureAwait(false);
                if (prepared.State is not (RecoveryBundlePreparationState.Prepared or RecoveryBundlePreparationState.NotNeeded))
                {
                    var cancelled = prepared.State == RecoveryBundlePreparationState.Cancelled;
                    return Stop(result with { Recovery = prepared.ResidualPath is null ? StripRecoveryState.NotRequired : StripRecoveryState.Unknown, RecoveryPath = prepared.ResidualPath },
                        cancelled ? CliSemanticStatus.Interrupted : CliSemanticStatus.Incomplete,
                        StripResultFactory.Error(cancelled ? StripFindingCode.Interrupted : StripFindingCode.RecoveryUnavailable,
                            prepared.Cause ?? "Recovery data could not be prepared. Nothing was changed."));
                }

                preparation = prepared.Preparation;
                if (preparation is not null)
                {
                    result = result with { Recovery = StripRecoveryState.Retained, RecoveryPath = preparation.BundlePath };
                }

                var fileApplier = new FileChangeApplier(revalidator, validator);
                for (var index = 0; index < plan.Files.Length; index++)
                {
                    var effect = plan.Files[index];
                    started = true;
                    var check = await validator.ValidateAsync(plan.Request.Workspace, effect.Change.Expectation, token).ConfigureAwait(false);
                    if (check.State != FileExpectationValidationState.Matched || check.Actual is null)
                    {
                        result = Mark(result, effect.Entry, StripEffectState.NotStarted);
                        return Failed(result, StripFindingCode.TargetChanged, check.Cause ?? "A planned file changed before its effect.");
                    }

                    result = Mark(result, effect.Entry, StripEffectState.Unknown);
                    var receipt = await fileApplier.ApplyAsync(lease, effect.Change, check,
                        targets[index].RequiresRecovery ? preparation : null, token).ConfigureAwait(false);
                    if (receipt.EffectState != FilesystemEffectState.Applied || receipt.VerificationState != FilesystemVerificationState.Verified)
                    {
                        result = Mark(result, effect.Entry, receipt.EffectState == FilesystemEffectState.NotStarted ? StripEffectState.NotStarted : StripEffectState.Failed);
                        return Failed(result, StripFindingCode.WriteFailed, receipt.Cause ?? "A planned write did not verify.");
                    }

                    if (plan.Files.Skip(index + 1).All(next => next.Entry != effect.Entry))
                    {
                        result = Mark(result, effect.Entry, StripEffectState.Done);
                    }

                    token.ThrowIfCancellationRequested();
                }

                var directoryApplier = new DirectoryDeletionApplier(revalidator, validator);
                foreach (var directory in plan.Directories)
                {
                    var deletion = PlannedDirectoryDeletion.DeleteIfEmpty(directory.Expectation);
                    var check = await validator.ValidateAsync(plan.Request.Workspace, deletion.Expectation, token).ConfigureAwait(false);
                    if (check.State != FileExpectationValidationState.Matched || check.Actual is null)
                    {
                        return Failed(Mark(result, directory.Entry, StripEffectState.NotStarted), StripFindingCode.TargetChanged, "A planned folder changed before removal.");
                    }

                    var receipt = await directoryApplier.ApplyAsync(lease, deletion, check, token).ConfigureAwait(false);
                    if (receipt.State.Disposition != DirectoryDeletionDisposition.Removed)
                    {
                        return Failed(Mark(result, directory.Entry, StripEffectState.Failed), StripFindingCode.WriteFailed,
                            receipt.Cause ?? "A planned folder was not empty or could not be removed.");
                    }

                    result = Mark(result, directory.Entry, StripEffectState.Done);
                }

                result = result with { Verified = true };
                return preparation is null ? result : await RemoveRecoveryAsync(result, lease, preparation, token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            return Stop(result, CliSemanticStatus.Interrupted, StripResultFactory.Error(StripFindingCode.Interrupted,
                "The run was interrupted. Review any retained recovery data before retrying."));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return started
                ? Failed(result, StripFindingCode.WriteFailed, exception.Message)
                : Stop(result, CliSemanticStatus.Incomplete, StripResultFactory.Error(StripFindingCode.RecoveryUnavailable, exception.Message));
        }
    }

    private static StripResult Mark(StripResult result, int entry, StripEffectState state)
        => result with { Plan = result.Plan.SetItem(entry, result.Plan[entry] with { State = state }) };

    private static StripResult Changed(StripResult result, string cause)
        => Stop(result, CliSemanticStatus.Blocked, StripResultFactory.Error(StripFindingCode.TargetChanged, cause));

    private static StripResult Failed(StripResult result, StripFindingCode code, string cause)
        => Stop(result, CliSemanticStatus.Failed, StripResultFactory.Error(code, cause));

    private static StripResult Stop(StripResult result, CliSemanticStatus status, StripFinding finding)
        => result with
        {
            Status = status,
            Findings = [finding, .. result.Findings],
            Verified = false,
            Next = status is CliSemanticStatus.Failed or CliSemanticStatus.Incomplete && result.RecoveryPath is not null
                ? new CliNextAction(CommandLines.Cleanup, "Review the retained recovery data.")
                : null,
        };

    private static async ValueTask<StripResult> RemoveRecoveryAsync(
        StripResult result, WorkspaceLockLease lease, RecoveryBundlePreparation preparation, CancellationToken token)
    {
        var catalogue = await RecoveryBundleCatalogue.ReadAsync(lease.Request.Workspace, token).ConfigureAwait(false);
        var candidates = catalogue.Candidates.Where(candidate => RecoveryBundleIdentity.Matches(candidate, preparation)).ToArray();
        if (catalogue.State == RecoveryBundleCatalogueState.Available && candidates.Length == 1)
        {
            var deletion = await RecoveryBundleDeletionGuard.DeleteAsync(lease, candidates[0], token).ConfigureAwait(false);
            if (deletion.State == RecoveryBundleDeletionState.Deleted && deletion.Disposition == RecoveryBundleDisposition.Removed)
            {
                return result with { Recovery = StripRecoveryState.Removed, RecoveryPath = null };
            }
        }

        return result with
        {
            Status = CliSemanticStatus.Attention,
            Findings = [StripResultFactory.Warning(StripFindingCode.RecoveryArtifactRetained,
                "The changes verified, but the recovery bundle could not be removed."), .. result.Findings],
            Next = new CliNextAction(CommandLines.Cleanup, "Remove the retained recovery bundle."),
        };
    }
}
