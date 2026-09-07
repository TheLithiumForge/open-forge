using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Request;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Commands.Cleanup.Shared.Application;
using OpenForge.Cli.Core.Commands.Cleanup.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Deletion;

namespace OpenForge.Cli.Core.Commands.Cleanup;

internal sealed class CleanupOperation
{
    private readonly RecoveryBundleCatalogue _catalogue;
    private readonly CleanupApplicationOperation _application;

    internal CleanupOperation(WorkspaceLockStoreRoot? lockStoreRoot = null)
    {
        var reader = new RecoveryBundleReader();
        _catalogue = new RecoveryBundleCatalogue(reader);
        var manager = lockStoreRoot is null ? WorkspaceLockManager.CreateForCurrentUser() : new WorkspaceLockManager(lockStoreRoot);
        _application = new CleanupApplicationOperation(manager, new RecoveryBundleDeletionGuard(_catalogue, reader));
    }

    internal async ValueTask<CleanupResult> ExecuteAsync(CleanupRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var emptyPlan = CleanupPlan.Create(request, CleanupCatalogue.Create(CleanupCatalogueCoverage.NotEstablished, []), [], [], CleanupPlanSafety.NotEstablished);
        var result = new CleanupResultBuilder(emptyPlan);
        try
        {
            if (!RecoveryBundleStorage.TryObservePath(request.Workspace.PhysicalRoot, out var attributes, out var failure)
                || attributes is null)
            {
                result.Preflight = new CleanupPreflight { State = CleanupPreflightState.Blocked };
                result.AddFinding(CleanupFindingCode.WorkspaceUnavailable, failure?.DirectCause ?? "The selected workspace is unavailable.", request.Workspace.LexicalRoot);
                return result.Build();
            }

            if ((attributes.Value & FileAttributes.Directory) == 0)
            {
                result.Preflight = new CleanupPreflight { State = CleanupPreflightState.Blocked };
                result.AddFinding(CleanupFindingCode.WorkspaceNotDirectory, "The selected workspace is not a directory.", request.Workspace.LexicalRoot);
                return result.Build();
            }

            var frozen = await ReadCatalogueAsync(request, cancellationToken).ConfigureAwait(false);
            var plan = CleanupPlanner.Create(request, frozen);
            var findings = CleanupPlanner.ReadFindings(plan, frozen.Cause);
            result = new CleanupResultBuilder(plan)
            {
                Findings = findings,
                Preflight = new CleanupPreflight { State = ReadPreflight(plan) },
                Effects = [.. plan.Entries.Where(entry => entry.Action == CleanupPlanAction.Delete).Select(entry => InitialEffect(request, entry))],
            };
            if (plan.Safety != CleanupPlanSafety.Safe || request.IsDryRun)
            {
                return result.Build();
            }

            if (plan.DeletionEntries.IsEmpty)
            {
                result.Verification = new CleanupVerification { State = CleanupVerificationState.Verified };
                return result.Build();
            }

            await _application.ApplyAsync(result, frozen, cancellationToken).ConfigureAwait(false);
            return result.Build();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            result.AddFinding(CleanupFindingCode.Interrupted, "Cleanup was interrupted.");
            return result.Build();
        }
        catch (Exception exception)
        {
            result.AddFinding(CleanupFindingCode.OperationFailed, $"Cleanup could not complete: {exception.Message}");
            return result.Build();
        }
    }

    private async ValueTask<RecoveryBundleCatalogueResult> ReadCatalogueAsync(CleanupRequest request, CancellationToken cancellationToken)
    {
        var failure = RecoveryDeletionStorageBoundary.ReadWorkspace(request.Workspace);
        if (failure is not null)
        {
            return RecoveryBundleCatalogueResult.Unavailable(failure.DirectCause, failure);
        }

        var catalogue = await _catalogue.ReadAsync(request.Workspace, cancellationToken).ConfigureAwait(false);
        failure = RecoveryDeletionStorageBoundary.ReadWorkspace(request.Workspace);
        if (failure is not null)
        {
            return RecoveryBundleCatalogueResult.Unavailable(failure.DirectCause, failure);
        }

        return catalogue;
    }

    private static CleanupEffect InitialEffect(CleanupRequest request, CleanupPlanEntry entry)
    {
        if (request.IsDryRun)
        {
            return CleanupEffect.Create(entry, CleanupEffectOutcome.Planned, CleanupEffectResidual.None);
        }

        return CleanupEffect.Create(entry, CleanupEffectOutcome.NotStarted, CleanupEffectResidual.Retained);
    }

    private static CleanupPreflightState ReadPreflight(CleanupPlan plan)
    {
        if (plan.Catalogue.Coverage == CleanupCatalogueCoverage.Interrupted)
        {
            return CleanupPreflightState.Interrupted;
        }

        if (plan.Catalogue.Coverage == CleanupCatalogueCoverage.Incomplete)
        {
            return CleanupPreflightState.Incomplete;
        }

        return plan.Safety == CleanupPlanSafety.Safe ? CleanupPreflightState.Complete : CleanupPreflightState.Blocked;
    }
}
