using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Shared.Mutation;
using OpenForge.Cli.Core.Commands.Shared;
using OpenForge.Cli.Core.Commands.Stripping.Models.Application;
using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
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
/// <remarks>
/// <paramref name="effectApplied"/> observes each verified effect by its one-based position. Production passes
/// nothing. Integration evidence uses it to interrupt a run after a known effect, which no real boundary can time.
/// </remarks>
internal sealed class StripApplication(
    PhysicalPathResolver resolver,
    WorkspaceLockStoreRoot? lockStoreRoot,
    Func<int, ValueTask>? effectApplied = null)
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
        var title = StrippingDefinitions.CommandTitle(plan.Request.Command);
        var total = plan.EffectCount;
        var applied = 0;
        var result = StripResultFactory.Preview(plan) with { Plan = [.. plan.Entries.Select(entry => entry with { State = StripEffectState.NotStarted })] };
        RecoveryBundlePreparation? preparation = null;
        try
        {
            var acquired = await manager.AcquireAsync(new WorkspaceLockRequest(plan.Request.Workspace, command, operationId), token).ConfigureAwait(false);
            if (acquired.Lease is not { } lease)
            {
                var cancelled = acquired.State == WorkspaceLockState.Cancelled;
                return Stop(plan, result,
                    cancelled ? CliSemanticStatus.Interrupted : CliSemanticStatus.Blocked,
                    StripResultFactory.Error(
                        cancelled ? StripFindingCode.Interrupted : StripFindingCode.WorkspaceLockUnavailable,
                        cancelled ? Cancelled(title) : acquired.Cause ?? "The workspace lock could not be acquired.",
                        lockFailure: acquired.LockFailure is { } failure ? WorkspaceLockFailureProjection.From(failure) : null));
            }

            await using (lease.ConfigureAwait(false))
            {
                var layout = await new WorkspaceLayoutRevalidator(resolver).ValidateAsync(lease, plan.LayoutRead, token).ConfigureAwait(false);
                if (layout.State != MutationValidationState.Valid)
                {
                    return Changed(plan, result, layout.Cause ?? "The workspace layout changed after the plan was made. Nothing was changed.");
                }

                var fresh = await rebuild(token).ConfigureAwait(false);
                if (fresh.Plan is not { } current || !plan.SameAs(current))
                {
                    var path = FirstDifference(plan, fresh.Plan);
                    return Changed(plan, result, path is null
                        ? "The planned files changed after the plan was made. Nothing was changed."
                        : $"{path} changed after the plan was made. Nothing was changed.", path);
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
                    return Stop(plan, result with { Recovery = prepared.ResidualPath is null ? StripRecoveryState.NotRequired : StripRecoveryState.Unknown, RecoveryPath = prepared.ResidualPath },
                        cancelled ? CliSemanticStatus.Interrupted : CliSemanticStatus.Incomplete,
                        StripResultFactory.Error(cancelled ? StripFindingCode.Interrupted : StripFindingCode.RecoveryUnavailable,
                            cancelled ? Cancelled(title) : prepared.Cause ?? "Recovery data could not be prepared. Nothing was changed."));
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
                    var path = EffectPath(plan, effect);
                    var check = await validator.ValidateAsync(plan.Request.Workspace, effect.Change.Expectation, token).ConfigureAwait(false);
                    if (check.State != FileExpectationValidationState.Matched || check.Actual is null)
                    {
                        return ChangedBeforeEffect(plan, Mark(result, effect.Entry, StripEffectState.NotStarted), path, applied, total);
                    }

                    result = Mark(result, effect.Entry, StripEffectState.Unknown);
                    var receipt = await fileApplier.ApplyAsync(lease, effect.Change, check,
                        targets[index].RequiresRecovery ? preparation : null, token).ConfigureAwait(false);
                    if (receipt.EffectState != FilesystemEffectState.Applied || receipt.VerificationState != FilesystemVerificationState.Verified)
                    {
                        result = Mark(result, effect.Entry, receipt.EffectState == FilesystemEffectState.NotStarted ? StripEffectState.NotStarted : StripEffectState.Failed);
                        return Failed(plan, result, StripFindingCode.WriteFailed, WriteFailed(path, applied, total, result.RecoveryPath), path);
                    }

                    applied++;
                    if (plan.Files.Skip(index + 1).All(next => next.Entry != effect.Entry))
                    {
                        result = Mark(result, effect.Entry, StripEffectState.Done);
                    }

                    await Observe(applied).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                }

                var directoryApplier = new DirectoryDeletionApplier(revalidator, validator);
                foreach (var directory in plan.Directories)
                {
                    var path = plan.Entries[directory.Entry].Path;
                    var deletion = PlannedDirectoryDeletion.DeleteIfEmpty(directory.Expectation);
                    var check = await validator.ValidateAsync(plan.Request.Workspace, deletion.Expectation, token).ConfigureAwait(false);
                    if (check.State != FileExpectationValidationState.Matched || check.Actual is null)
                    {
                        return ChangedBeforeEffect(plan, Mark(result, directory.Entry, StripEffectState.NotStarted), path, applied, total);
                    }

                    var receipt = await directoryApplier.ApplyAsync(lease, deletion, check, token).ConfigureAwait(false);
                    if (receipt.State.Disposition != DirectoryDeletionDisposition.Removed)
                    {
                        return Failed(plan, Mark(result, directory.Entry, StripEffectState.Failed), StripFindingCode.WriteFailed,
                            WriteFailed(path, applied, total, result.RecoveryPath), path);
                    }

                    applied++;
                    result = Mark(result, directory.Entry, StripEffectState.Done);
                    await Observe(applied).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                }

                result = result with { Verified = true };
                if (preparation is not null)
                {
                    result = await RemoveRecoveryAsync(result, lease, preparation, token).ConfigureAwait(false);
                }
            }

            return plan.RuntimeFolderEntry is { } runtime ? await RemoveRuntimeFolderAsync(plan, result, runtime, total).ConfigureAwait(false) : WithKeptNext(plan, result);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            return Stop(plan, result, CliSemanticStatus.Interrupted, StripResultFactory.Error(StripFindingCode.Interrupted, applied == 0
                ? Cancelled(title)
                : $"{title} was cancelled. Stopped after {applied} of {total} changes."));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return preparation is not null
                ? Failed(plan, result, StripFindingCode.WriteFailed, $"{exception.Message} Stopped after {applied} of {total} changes.")
                : Stop(plan, result, CliSemanticStatus.Incomplete, StripResultFactory.Error(StripFindingCode.RecoveryUnavailable, exception.Message));
        }
    }

    private ValueTask Observe(int applied) => effectApplied?.Invoke(applied) ?? ValueTask.CompletedTask;

    // The runtime folder holds the lease's lock file, so its removal runs once the lease is released and is verified by its absence.
    private async ValueTask<StripResult> RemoveRuntimeFolderAsync(StripPlan plan, StripResult result, int entry, int total)
    {
        if (result.Status != CliSemanticStatus.Complete)
        {
            return result;
        }

        var removal = StripRuntimeFolder.TryRemove(plan.Request.Workspace);
        if (removal.State is StripRuntimeFolderState.Absent or StripRuntimeFolderState.Removable)
        {
            await Observe(total).ConfigureAwait(false);
            return WithKeptNext(plan, Mark(result, entry, StripEffectState.Done));
        }

        var recoveryData = removal.State == StripRuntimeFolderState.HoldsRecoveryData;
        return Mark(result, entry, recoveryData ? StripEffectState.NotStarted : StripEffectState.Failed) with
        {
            Status = CliSemanticStatus.Attention,
            Verified = recoveryData && result.Verified,
            Findings = [StripResultFactory.Warning(StripFindingCode.RuntimeRetained, StripRuntimeFolder.Retained(removal), StripRuntimeFolder.Path), .. result.Findings],
            Next = recoveryData ? ReviewRecovery() : new CliNextAction(CommandLines.Doctor, "Check the workspace."),
        };
    }

    // A runtime folder the plan already keeps for its recovery data points at cleanup once the run completes.
    private static StripResult WithKeptNext(StripPlan plan, StripResult result)
        => result.Status == CliSemanticStatus.Complete && plan.Kept.Any(kept => kept.Reason == StripKeptReason.RecoveryData)
            ? result with { Next = ReviewRecovery() }
            : result;

    private static CliNextAction ReviewRecovery() => new(CommandLines.Cleanup, "Review the retained recovery data.");

    // A change found before the first effect leaves the workspace as it was. After that, the run stopped part way.
    private static StripResult ChangedBeforeEffect(StripPlan plan, StripResult result, string path, int applied, int total)
        => applied == 0
            ? Changed(plan, result, $"{path} changed after the plan was made. Nothing was changed.", path)
            : Failed(plan, result, StripFindingCode.TargetChangedDuringApply,
                $"{path} changed while changes were being written. Stopped after {applied} of {total} changes.", path);

    private static string Cancelled(string title) => $"{title} was cancelled. Nothing was changed.";

    private static string EffectPath(StripPlan plan, StripFileEffect effect)
    {
        var entry = plan.Entries[effect.Entry];
        return effect.Change.Kind == PlannedFileChangeKind.Create && entry.To is { } to ? to : entry.Path;
    }

    private static string WriteFailed(string path, int applied, int total, string? recovery)
        => recovery is null
            ? $"Writing {path} failed. Stopped after {applied} of {total} changes."
            : $"Writing {path} failed. Stopped after {applied} of {total} changes. Recovery data: {recovery}.";

    private static string? FirstDifference(StripPlan plan, StripPlan? current)
    {
        if (current is null)
        {
            return null;
        }

        var index = 0;
        while (index < plan.Entries.Length && index < current.Entries.Length && plan.Entries[index].SameAs(current.Entries[index]))
        {
            index++;
        }

        if (index < plan.Entries.Length)
        {
            return plan.Entries[index].Path;
        }

        return index < current.Entries.Length ? current.Entries[index].Path : null;
    }

    private static StripResult Mark(StripResult result, int entry, StripEffectState state)
        => result with { Plan = result.Plan.SetItem(entry, result.Plan[entry] with { State = state }) };

    private static StripResult Changed(StripPlan plan, StripResult result, string cause, string? path = null)
        => Stop(plan, result, CliSemanticStatus.Blocked, StripResultFactory.Error(StripFindingCode.TargetChanged, cause, path));

    private static StripResult Failed(StripPlan plan, StripResult result, StripFindingCode code, string cause, string? path = null)
        => Stop(plan, result, CliSemanticStatus.Failed, StripResultFactory.Error(code, cause, path));

    private static StripResult Stop(StripPlan plan, StripResult result, CliSemanticStatus status, StripFinding finding)
        => result with
        {
            Status = status,
            Findings = [finding, .. result.Findings],
            Verified = false,
            Next = StripResultFactory.NextFor(plan.Request, finding),
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
                $"The changes were applied, but the recovery bundle at {preparation.BundlePath} could not be removed."), .. result.Findings],
            Next = new CliNextAction(CommandLines.Cleanup, "Remove the retained recovery bundle."),
        };
    }
}
