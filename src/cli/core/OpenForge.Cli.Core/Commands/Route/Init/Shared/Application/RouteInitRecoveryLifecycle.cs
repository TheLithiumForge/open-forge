using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Application;

internal enum RouteInitRecoveryPreparationState
{
    NotRequired,
    Prepared,
    Incomplete,
    Blocked,
    Cancelled,
    Failed,
}

internal sealed record RouteInitRecoveryPreparationResult(
    RouteInitRecoveryPreparationState State,
    RecoveryBundlePreparation? Preparation,
    RouteInitRecovery Recovery,
    string? Cause);

internal sealed record RouteInitRecoveryDeletionResult(
    RouteInitRecovery Recovery,
    RouteInitFindingCode? FindingCode,
    string? Cause);

internal static class RouteInitRecoveryLifecycle
{
    internal static async ValueTask<RouteInitRecoveryPreparationResult> PrepareAsync(
        RouteInitPlan plan,
        Guid operationId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (plan.RecoveryTargets.Length == 0)
        {
            return new RouteInitRecoveryPreparationResult(
                RouteInitRecoveryPreparationState.NotRequired,
                Preparation: null,
                new RouteInitRecovery(RouteInitRecoveryState.NotCreated, ResidualPath: null),
                Cause: null);
        }

        RecoveryBundleCatalogueResult catalogue;
        try
        {
            catalogue = await RecoveryBundleCatalogue.ReadAsync(
                    plan.Request.Workspace,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled(ResidualPath: null);
        }
        catch (Exception)
        {
            return Failed("The Route Init recovery catalogue failed unexpectedly.", ResidualPath: null);
        }

        if (catalogue.State == RecoveryBundleCatalogueState.Cancelled)
        {
            return Cancelled(ResidualPath: null);
        }

        if (catalogue.State == RecoveryBundleCatalogueState.Unavailable)
        {
            return new RouteInitRecoveryPreparationResult(
                RouteInitRecoveryPreparationState.Incomplete,
                Preparation: null,
                new RouteInitRecovery(RouteInitRecoveryState.Unknown, ResidualPath: null),
                catalogue.Cause ?? "The Route Init recovery catalogue is unavailable.");
        }

        if (catalogue.Candidates.Length != 0)
        {
            return new RouteInitRecoveryPreparationResult(
                RouteInitRecoveryPreparationState.Blocked,
                Preparation: null,
                new RouteInitRecovery(RouteInitRecoveryState.NotCreated, ResidualPath: null),
                "An existing recovery candidate conflicts with this Route Init application.");
        }

        RecoveryBundlePreparationResult preparation;
        try
        {
            preparation = await RecoveryBundleStore.PrepareAsync(
                    RecoveryBundleInput.Create(
                        plan.Request.Workspace,
                        RouteInitDefinitions.CommandIdentity,
                        RecoveryBundleAttribution.Create(
                            RecoveryBundleProducer.Route,
                            RecoveryBundleOperation.Init,
                            plan.Request.Workspace),
                        operationId,
                        plan.RecoveryTargets),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return CancelledAfterStoreEntry();
        }
        catch (Exception)
        {
            return FailedAfterStoreEntry();
        }

        var resultState = ReadPreparationState(
            preparation.State,
            preparation.Preparation is not null);
        return resultState switch
        {
            RouteInitRecoveryPreparationState.Prepared when preparation.Preparation is { } verified =>
                new RouteInitRecoveryPreparationResult(
                    RouteInitRecoveryPreparationState.Prepared,
                    verified,
                    new RouteInitRecovery(RouteInitRecoveryState.Retained, verified.BundlePath),
                    Cause: null),
            RouteInitRecoveryPreparationState.Incomplete =>
                new RouteInitRecoveryPreparationResult(
                    RouteInitRecoveryPreparationState.Incomplete,
                    Preparation: null,
                    Recovery(preparation.ResidualPath),
                    preparation.Cause ?? "Route Init recovery preparation is incomplete."),
            RouteInitRecoveryPreparationState.Blocked =>
                new RouteInitRecoveryPreparationResult(
                    RouteInitRecoveryPreparationState.Blocked,
                    Preparation: null,
                    Recovery(preparation.ResidualPath),
                    preparation.Cause ?? "Route Init recovery preparation is blocked."),
            RouteInitRecoveryPreparationState.Cancelled => Cancelled(preparation.ResidualPath),
            RouteInitRecoveryPreparationState.NotRequired
                or RouteInitRecoveryPreparationState.Failed => Failed(
                "Route Init recovery preparation returned an incoherent result.",
                preparation.ResidualPath),
            _ => throw new ArgumentOutOfRangeException(
                null,
                resultState,
                "The Route Init recovery preparation result state is not defined."),
        };
    }

    internal static RouteInitRecoveryPreparationState ReadPreparationState(
        RecoveryBundlePreparationState state,
        bool hasPreparation)
        => state switch
        {
            RecoveryBundlePreparationState.NotNeeded => RouteInitRecoveryPreparationState.Failed,
            RecoveryBundlePreparationState.Prepared when hasPreparation => RouteInitRecoveryPreparationState.Prepared,
            RecoveryBundlePreparationState.Prepared => RouteInitRecoveryPreparationState.Failed,
            RecoveryBundlePreparationState.Incomplete => RouteInitRecoveryPreparationState.Incomplete,
            RecoveryBundlePreparationState.Blocked => RouteInitRecoveryPreparationState.Blocked,
            RecoveryBundlePreparationState.Cancelled => RouteInitRecoveryPreparationState.Cancelled,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The recovery bundle preparation state is not defined."),
        };

    internal static async ValueTask<RouteInitRecoveryDeletionResult> DeleteAsync(
        WorkspaceLockLease lease,
        RecoveryBundlePreparation? preparation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(lease);
        if (preparation is null)
        {
            return new RouteInitRecoveryDeletionResult(
                new RouteInitRecovery(RouteInitRecoveryState.NotCreated, ResidualPath: null),
                FindingCode: null,
                Cause: null);
        }

        RecoveryBundleCatalogueResult catalogue;
        try
        {
            catalogue = await RecoveryBundleCatalogue.ReadAsync(
                    lease.Request.Workspace,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Retained(preparation, RouteInitFindingCode.Interrupted, "Recovery deletion was interrupted.");
        }
        catch (Exception)
        {
            return Unknown(preparation, RouteInitFindingCode.RecoveryFailed, "The recovery catalogue failed during deletion.");
        }

        if (catalogue.State == RecoveryBundleCatalogueState.Cancelled)
        {
            return Retained(preparation, RouteInitFindingCode.Interrupted, "Recovery deletion was interrupted.");
        }

        if (catalogue.State != RecoveryBundleCatalogueState.Available)
        {
            return Unknown(
                preparation,
                RouteInitFindingCode.RecoveryFailed,
                catalogue.Cause ?? "The recovery catalogue is unavailable during deletion.");
        }

        var candidates = catalogue.Candidates
            .Where(candidate => RecoveryBundleIdentity.Matches(candidate, preparation))
            .ToArray();
        if (candidates.Length != 1)
        {
            return Unknown(
                preparation,
                RouteInitFindingCode.RecoveryFailed,
                "The exact prepared Route Init recovery final could not be selected for deletion.");
        }

        RecoveryBundleDeletionResult deletion;
        try
        {
            deletion = await RecoveryBundleDeletionGuard.DeleteAsync(
                    lease,
                    candidates[0],
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Retained(preparation, RouteInitFindingCode.Interrupted, "Recovery deletion was interrupted.");
        }
        catch (Exception)
        {
            return Unknown(preparation, RouteInitFindingCode.RecoveryFailed, "Recovery deletion failed unexpectedly.");
        }

        var (outcomeState, outcomeFindingCode) = ReadDeletionOutcome(
            deletion.State,
            deletion.Disposition);
        if (outcomeState == RouteInitRecoveryState.Removed)
        {
            return new RouteInitRecoveryDeletionResult(
                new RouteInitRecovery(RouteInitRecoveryState.Removed, ResidualPath: null),
                FindingCode: null,
                Cause: null);
        }

        if (outcomeState == RouteInitRecoveryState.Retained
            && deletion.State == RecoveryBundleDeletionState.Failed)
        {
            return new RouteInitRecoveryDeletionResult(
                new RouteInitRecovery(
                    RouteInitRecoveryState.Retained,
                    deletion.ResidualPath ?? preparation.BundlePath),
                RouteInitFindingCode.RecoveryArtifactRetained,
                deletion.Cause ?? "The verified Route Init recovery artifact was retained.");
        }

        if (outcomeState == RouteInitRecoveryState.Retained)
        {
            return Retained(
                preparation,
                RouteInitFindingCode.Interrupted,
                "Recovery deletion was interrupted.");
        }

        return new RouteInitRecoveryDeletionResult(
            new RouteInitRecovery(
                RouteInitRecoveryState.Unknown,
                deletion.ResidualPath ?? preparation.BundlePath),
                outcomeFindingCode,
            deletion.Cause ?? "The Route Init recovery artifact disposition is unknown.");
    }

    internal static (RouteInitRecoveryState State, RouteInitFindingCode? FindingCode) ReadDeletionOutcome(
        RecoveryBundleDeletionState state,
        RecoveryBundleDisposition disposition)
    {
        _ = state switch
        {
            RecoveryBundleDeletionState.Deleted
                or RecoveryBundleDeletionState.Failed
                or RecoveryBundleDeletionState.Blocked
                or RecoveryBundleDeletionState.Cancelled => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The recovery deletion state is not defined."),
        };
        _ = disposition switch
        {
            RecoveryBundleDisposition.Removed
                or RecoveryBundleDisposition.Retained
                or RecoveryBundleDisposition.Unknown => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(disposition),
                disposition,
                "The recovery bundle disposition is not defined."),
        };

        return (state, disposition) switch
        {
            (RecoveryBundleDeletionState.Deleted, RecoveryBundleDisposition.Removed) =>
                (RouteInitRecoveryState.Removed, null),
            (RecoveryBundleDeletionState.Failed, RecoveryBundleDisposition.Retained) =>
                (RouteInitRecoveryState.Retained, RouteInitFindingCode.RecoveryArtifactRetained),
            (RecoveryBundleDeletionState.Cancelled, RecoveryBundleDisposition.Retained) =>
                (RouteInitRecoveryState.Retained, RouteInitFindingCode.Interrupted),
            _ => (
                RouteInitRecoveryState.Unknown,
                state == RecoveryBundleDeletionState.Cancelled
                    ? RouteInitFindingCode.Interrupted
                    : RouteInitFindingCode.RecoveryFailed),
        };
    }

    private static RouteInitRecoveryPreparationResult Cancelled(string? ResidualPath)
        => new(
            RouteInitRecoveryPreparationState.Cancelled,
            Preparation: null,
            Recovery(ResidualPath),
            "Route Init recovery preparation was interrupted.");

    private static RouteInitRecoveryPreparationResult Failed(
        string cause,
        string? ResidualPath)
        => new(
            RouteInitRecoveryPreparationState.Failed,
            Preparation: null,
            Recovery(ResidualPath),
            cause);

    internal static RouteInitRecoveryPreparationResult CancelledAfterStoreEntry()
        => new(
            RouteInitRecoveryPreparationState.Cancelled,
            Preparation: null,
            new RouteInitRecovery(RouteInitRecoveryState.Unknown, ResidualPath: null),
            "Route Init recovery preparation was interrupted after storage entry.");

    internal static RouteInitRecoveryPreparationResult FailedAfterStoreEntry()
        => new(
            RouteInitRecoveryPreparationState.Failed,
            Preparation: null,
            new RouteInitRecovery(RouteInitRecoveryState.Unknown, ResidualPath: null),
            "Route Init recovery preparation failed unexpectedly after storage entry.");

    private static RouteInitRecovery Recovery(string? residualPath)
        => residualPath is null
            ? new RouteInitRecovery(RouteInitRecoveryState.NotCreated, ResidualPath: null)
            : new RouteInitRecovery(RouteInitRecoveryState.Unknown, residualPath);

    private static RouteInitRecoveryDeletionResult Retained(
        RecoveryBundlePreparation preparation,
        RouteInitFindingCode findingCode,
        string cause)
        => new(
            new RouteInitRecovery(RouteInitRecoveryState.Retained, preparation.BundlePath),
            findingCode,
            cause);

    private static RouteInitRecoveryDeletionResult Unknown(
        RecoveryBundlePreparation preparation,
        RouteInitFindingCode findingCode,
        string cause)
        => new(
            new RouteInitRecovery(RouteInitRecoveryState.Unknown, preparation.BundlePath),
            findingCode,
            cause);
}
