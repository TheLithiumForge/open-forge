using OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Request;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Commands.Cleanup.Shared.Application;
using OpenForge.Cli.Core.Commands.Cleanup.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Storage;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Deletion;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;

namespace OpenForge.Cli.Core.Commands.Cleanup;

internal sealed class CleanupOperation
{
    private readonly CleanupApplicationOperation _application;

    internal CleanupOperation(WorkspaceLockStoreRoot? lockStoreRoot = null)
    {
        var manager = lockStoreRoot is null ? WorkspaceLockManager.CreateForCurrentUser() : new WorkspaceLockManager(lockStoreRoot);
        _application = new CleanupApplicationOperation(manager);
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

            var layoutRead = await new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(request.Workspace, cancellationToken).ConfigureAwait(false);
            result = new CleanupResultBuilder(emptyPlan with { LayoutRead = layoutRead });
            if (layoutRead.State is not (WorkspaceLayoutReadState.Ready or WorkspaceLayoutReadState.Uninstalled))
            {
                result.Preflight = new CleanupPreflight { State = CleanupPreflightState.Blocked };
                foreach (var issue in layoutRead.Issues)
                    result.AddFinding(CleanupFindingCode.StoreValidationUnavailable, issue.Cause, issue.Path);
                return result.Build();
            }

            var local = RecoveryStoreLocator.Local(request.Workspace);
            var legacy = RecoveryStoreLocator.Legacy(request.Workspace);
            RecoveryStoreObservation[] observations =
            [
                await ReadStoreAsync(RecoveryStoreKind.Workspace, local, cancellationToken).ConfigureAwait(false),
                await ReadStoreAsync(RecoveryStoreKind.Legacy, legacy, cancellationToken).ConfigureAwait(false),
            ];
            var stores = new List<CleanupStoreApplication>();
            foreach (var frozen in observations)
            {
                var plan = CleanupPlanner.Create(request, frozen.Catalogue) with { LayoutRead = layoutRead };
                var builder = new CleanupResultBuilder(plan)
                {
                    Findings = CleanupPlanner.ReadFindings(plan, frozen.Catalogue.Cause),
                    Preflight = new CleanupPreflight { State = ReadPreflight(plan) },
                    Effects = [.. plan.Entries.Where(entry => entry.Action == CleanupPlanAction.Delete).Select(entry => InitialEffect(request, entry, plan.Safety))],
                };
                if (plan.Safety == CleanupPlanSafety.Safe && plan.DeletionEntries.IsEmpty && !request.IsDryRun)
                {
                    builder.Verification = new CleanupVerification { State = CleanupVerificationState.Verified };
                }

                stores.Add(new CleanupStoreApplication(frozen, builder));
            }

            if (!request.IsDryRun && stores.Any(store => !store.Result.Plan.DeletionEntries.IsEmpty))
            {
                await _application.ApplyAsync(stores, cancellationToken).ConfigureAwait(false);
            }

            return CleanupResultBuilder.BuildStores(request, stores);
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

    private static async ValueTask<RecoveryStoreObservation> ReadStoreAsync(
        RecoveryStoreKind kind,
        RecoveryStoreLocation? location,
        CancellationToken cancellationToken)
    {
        if (location is null)
        {
            return new(kind, null, RecoveryBundleCatalogueResult.Unavailable("The legacy recovery store location is unavailable."));
        }

        if (RecoveryDeletionStorageBoundary.ReadLocation(location) is { } beforeFailure)
        {
            return new(kind, location, RecoveryBundleCatalogueResult.Unavailable(beforeFailure.DirectCause, beforeFailure));
        }

        var catalogue = await RecoveryBundleCatalogue.ReadAsync(location, cancellationToken).ConfigureAwait(false);
        if (RecoveryDeletionStorageBoundary.ReadLocation(location) is { } afterFailure)
        {
            catalogue = RecoveryBundleCatalogueResult.Unavailable(afterFailure.DirectCause, afterFailure);
        }

        return new(kind, location, catalogue);
    }

    private static CleanupEffect InitialEffect(CleanupRequest request, CleanupPlanEntry entry, CleanupPlanSafety safety)
    {
        if (request.IsDryRun && safety == CleanupPlanSafety.Safe)
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
