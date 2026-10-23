using OpenForge.Cli.Core.Commands.Shared.Mutation;
using OpenForge.Cli.Core.Shell.Definitions;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
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
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Application;

internal sealed class RouteInclusionApplication(WorkspaceLockStoreRoot? lockStoreRoot)
{
    internal async ValueTask<RouteInclusionOutcome> ApplyAsync(RouteInclusionPlan plan, CancellationToken token)
    {
        var manager = lockStoreRoot is null ? WorkspaceLockManager.CreateForCurrentUser() : new WorkspaceLockManager(lockStoreRoot);
        var operationId = Guid.NewGuid();
        var result = plan.Preview with { Effects = plan.Preview.Effects.Select(effect => effect with { State = RouteInclusionEffectState.NotStarted }).ToImmutableArray() };
        RecoveryBundlePreparation? preparation = null;
        var started = false;
        try
        {
            var acquired = await manager.AcquireAsync(new WorkspaceLockRequest(plan.Request.Workspace, plan.Request.Command, operationId), token).ConfigureAwait(false);
            if (acquired.Lease is not { } lease)
            {
                return Stop(result,
                    acquired.State == WorkspaceLockState.Cancelled ? CliSemanticStatus.Interrupted : CliSemanticStatus.Blocked,
                    acquired.State == WorkspaceLockState.Cancelled ? RouteInclusionFindingCode.Interrupted : RouteInclusionFindingCode.WorkspaceLockUnavailable,
                    acquired.Cause ?? "The workspace lock could not be acquired.") with
                {
                    LockFailure = WorkspaceLockFailureProjection.From(acquired.LockFailure ?? throw new InvalidOperationException("A failed workspace lock requires its typed failure.")),
                };
            }
            await using (lease.ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                var fresh = await new RouteInclusionPlanBuilder().BuildAsync(plan.Request, token).ConfigureAwait(false);
                if (fresh.Plan is not { } current || !Equivalent(plan, current))
                    return Stop(result, CliSemanticStatus.Blocked, RouteInclusionFindingCode.TargetChanged, "The plan changed. Review a fresh preview and retry.");
                var validator = new FileExpectationValidator(new PhysicalPathResolver());
                foreach (var observation in plan.Observations)
                {
                    var check = await validator.ValidateAsync(plan.Request.Workspace, observation.Expectation, token).ConfigureAwait(false);
                    if (check.State != FileExpectationValidationState.Matched)
                        return Stop(result, CliSemanticStatus.Blocked, RouteInclusionFindingCode.TargetChanged, "A loader, settings or source observation changed. Review a fresh preview and retry.");
                }
                var revalidator = new MutationRevalidator(validator);
                var validation = await revalidator.ValidateAsync(lease, plan.Changes, token).ConfigureAwait(false);
                if (validation.State != MutationValidationState.Valid || validation.Checks.Count != plan.Changes.Length)
                    return Stop(result, CliSemanticStatus.Blocked, RouteInclusionFindingCode.TargetChanged, validation.Cause ?? "Review a fresh preview and retry.");
                var catalogue = await RecoveryBundleCatalogue.ReadAsync(plan.Request.Workspace, token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                if (catalogue.State != RecoveryBundleCatalogueState.Available)
                    return Stop(result, CliSemanticStatus.Incomplete, RouteInclusionFindingCode.RecoveryUnavailable, catalogue.Cause ?? "Recovery storage is unavailable. Retry when it is readable.");
                if (!catalogue.Candidates.IsEmpty)
                    return Stop(result with { Recovery = RouteInclusionRecoveryState.Retained, RecoveryPath = catalogue.Candidates[0].Path },
                        CliSemanticStatus.Blocked, RouteInclusionFindingCode.RecoveryConflict, "Resolve the existing recovery artifact before retrying.");
                var operation = plan.Request.Action == RouteInclusionAction.Include ? RecoveryBundleOperation.Include : RecoveryBundleOperation.Exclude;
                result = result with { Recovery = RouteInclusionRecoveryState.Unknown };
                var prepared = await RecoveryBundleStore.PrepareAsync(RecoveryBundleInput.Create(
                    plan.Request.Workspace, plan.Request.Command,
                    RecoveryBundleAttribution.Create(RecoveryBundleProducer.Route, operation, plan.Request.Workspace),
                    operationId, plan.RecoveryTargets), token).ConfigureAwait(false);
                preparation = prepared.Preparation;
                if (preparation is null)
                    return Stop(result with { Recovery = prepared.ResidualPath is null ? RouteInclusionRecoveryState.NotCreated : RouteInclusionRecoveryState.Unknown, RecoveryPath = prepared.ResidualPath },
                        prepared.State == RecoveryBundlePreparationState.Cancelled ? CliSemanticStatus.Interrupted : CliSemanticStatus.Incomplete,
                        prepared.State == RecoveryBundlePreparationState.Cancelled ? RouteInclusionFindingCode.Interrupted : RouteInclusionFindingCode.RecoveryUnavailable, prepared.Cause ?? "Recovery preparation did not complete. Retry when storage is available.");
                result = result with { Recovery = RouteInclusionRecoveryState.Retained, RecoveryPath = preparation.BundlePath };
                var applier = new FileChangeApplier(revalidator, validator);
                for (var index = 0; index < plan.Changes.Length; index++)
                {
                    started = true;
                    result = SetEffect(result, index, RouteInclusionEffectState.Unknown);
                    var receipt = await applier.ApplyAsync(lease, plan.Changes[index], validation.Checks[index],
                        plan.Changes[index].Kind == PlannedFileChangeKind.Create ? null : preparation, token).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (receipt.EffectState != FilesystemEffectState.Applied || receipt.VerificationState != FilesystemVerificationState.Verified)
                        return Stop(result, CliSemanticStatus.Failed, RouteInclusionFindingCode.WriteFailed, receipt.Cause ?? "A planned write did not verify. Review the retained recovery artifact.");
                    result = SetEffect(result, index, RouteInclusionEffectState.Verified);
                }
                foreach (var observation in plan.Observations)
                {
                    var change = plan.Changes.FirstOrDefault(candidate => candidate.LogicalPath == observation.LogicalPath);
                    var expected = change is null ? observation.Expectation : FileExpectation.File(
                        change.LogicalPath, observation.PhysicalPath ?? Path.GetFullPath(
                            Path.GetRelativePath(plan.Request.Workspace.LexicalRoot, change.LogicalPath), plan.Request.Workspace.PhysicalRoot),
                        FileExpectation.Hash(change.IntendedBytes.AsSpan()));
                    var check = await validator.ValidateAsync(plan.Request.Workspace, expected, token).ConfigureAwait(false);
                    if (check.State != FileExpectationValidationState.Matched)
                        return Stop(result, CliSemanticStatus.Failed, RouteInclusionFindingCode.VerificationFailed, "Final bytes did not verify. Review the retained recovery artifact.");
                }
                var post = await new RouteInclusionPlanBuilder().BuildAsync(plan.Request, token).ConfigureAwait(false);
                if (post.Plan is null || !post.Outcome.NoOp)
                    return Stop(result, CliSemanticStatus.Failed, RouteInclusionFindingCode.VerificationFailed, "The final loader and registration did not verify. Review the recovery artifact.");
                result = result with { Verified = true };
                return await CompleteRecoveryAsync(result, lease, preparation, token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        { return Stop(result, CliSemanticStatus.Interrupted, RouteInclusionFindingCode.Interrupted, "The request was interrupted. Review any retained recovery artifact before retrying."); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        { return Stop(result, started ? CliSemanticStatus.Failed : CliSemanticStatus.Incomplete, started ? RouteInclusionFindingCode.WriteFailed : RouteInclusionFindingCode.RecoveryUnavailable, exception.Message); }
    }

    private static RouteInclusionOutcome SetEffect(RouteInclusionOutcome outcome, int index, RouteInclusionEffectState state)
        => outcome with { Effects = outcome.Effects.SetItem(index, outcome.Effects[index] with { State = state }) };

    private static RouteInclusionOutcome Stop(RouteInclusionOutcome outcome, CliSemanticStatus status, RouteInclusionFindingCode code, string cause)
        => outcome with { Status = status, Code = code, Cause = cause };

    private static bool Equivalent(RouteInclusionPlan expected, RouteInclusionPlan current)
        => expected.Preview.Before == current.Preview.Before && expected.Preview.After == current.Preview.After
            && expected.Changes.Length == current.Changes.Length
            && expected.Changes.Zip(current.Changes).All(pair => pair.First.Expectation == pair.Second.Expectation
                && pair.First.Kind == pair.Second.Kind && pair.First.IntendedBytes.AsSpan().SequenceEqual(pair.Second.IntendedBytes.AsSpan()))
            && expected.Observations.Length == current.Observations.Length
            && expected.Observations.Zip(current.Observations).All(pair => pair.First.Expectation == pair.Second.Expectation);

    private static async ValueTask<RouteInclusionOutcome> CompleteRecoveryAsync(
        RouteInclusionOutcome outcome, WorkspaceLockLease lease, RecoveryBundlePreparation preparation, CancellationToken token)
    {
        var catalogue = await RecoveryBundleCatalogue.ReadAsync(lease.Request.Workspace, token).ConfigureAwait(false);
        var candidates = catalogue.Candidates.Where(candidate => RecoveryBundleIdentity.Matches(candidate, preparation)).ToArray();
        if (catalogue.State != RecoveryBundleCatalogueState.Available || candidates.Length != 1)
            return Stop(outcome with { Recovery = RouteInclusionRecoveryState.Unknown }, CliSemanticStatus.Failed, RouteInclusionFindingCode.RecoveryFailed, "The exact recovery artifact could not be selected for deletion.");
        var deletion = await RecoveryBundleDeletionGuard.DeleteAsync(lease, candidates[0], token).ConfigureAwait(false);
        if (deletion.State == RecoveryBundleDeletionState.Deleted && deletion.Disposition == RecoveryBundleDisposition.Removed)
            return outcome with { Recovery = RouteInclusionRecoveryState.Removed, RecoveryPath = null };
        if (deletion.State == RecoveryBundleDeletionState.Failed && deletion.Disposition == RecoveryBundleDisposition.Retained)
            return Stop(outcome, CliSemanticStatus.Attention, RouteInclusionFindingCode.RecoveryArtifactRetained, deletion.Cause ?? "The changes verified. Remove the retained recovery artifact with cleanup.");
        return Stop(outcome with { Recovery = deletion.Disposition == RecoveryBundleDisposition.Retained ? RouteInclusionRecoveryState.Retained : RouteInclusionRecoveryState.Unknown },
            CliSemanticStatus.Failed, RouteInclusionFindingCode.RecoveryFailed, deletion.Cause ?? "The recovery artifact could not be removed.");
    }
}
