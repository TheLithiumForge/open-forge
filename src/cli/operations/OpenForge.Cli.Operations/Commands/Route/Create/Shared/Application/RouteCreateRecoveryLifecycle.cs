using OpenForge.Cli.Core.Commands.Route.Create.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Application;

internal static class RouteCreateRecoveryLifecycle
{
    internal static async ValueTask<RouteCreateRecoveryPreparationResult> PrepareAsync(
        RouteCreatePlan plan,
        Guid operationId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (plan.RecoveryTargets.IsEmpty)
        {
            return new RouteCreateRecoveryPreparationResult
            {
                State = RouteCreateRecoveryPreparationState.NotRequired,
                Preparation = null,
                Recovery = NotCreated(),
                Cause = null,
            };
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
            return Cancelled(residualPath: null);
        }
        catch (Exception)
        {
            return Failed(
                "The Route Create recovery catalogue failed unexpectedly.",
                residualPath: null);
        }

        if (catalogue.State == RecoveryBundleCatalogueState.Cancelled)
        {
            return Cancelled(residualPath: null);
        }

        if (catalogue.State == RecoveryBundleCatalogueState.Unavailable)
        {
            return new RouteCreateRecoveryPreparationResult
            {
                State = RouteCreateRecoveryPreparationState.Incomplete,
                Preparation = null,
                Recovery = Unknown(residualPath: null),
                Cause = catalogue.Cause
                    ?? "The Route Create recovery catalogue is unavailable.",
            };
        }

        if (catalogue.Candidates.Length != 0)
        {
            return new RouteCreateRecoveryPreparationResult
            {
                State = RouteCreateRecoveryPreparationState.Blocked,
                Preparation = null,
                Recovery = NotCreated(),
                Cause = "An existing recovery candidate conflicts with this Route Create application.",
            };
        }

        RecoveryBundlePreparationResult preparation;
        try
        {
            preparation = await RecoveryBundleStore.PrepareAsync(
                    RecoveryBundleInput.Create(
                        plan.Request.Workspace,
                        RouteCreateDefinitions.CommandIdentity,
                        RecoveryBundleAttribution.Create(
                            RecoveryBundleProducer.Route,
                            RecoveryBundleOperation.Create,
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

        return preparation.State switch
        {
            RecoveryBundlePreparationState.Prepared when preparation.Preparation is { } verified =>
                new RouteCreateRecoveryPreparationResult
                {
                    State = RouteCreateRecoveryPreparationState.Prepared,
                    Preparation = verified,
                    Recovery = Retained(verified.BundlePath),
                    Cause = null,
                },
            RecoveryBundlePreparationState.Prepared => Failed(
                "Route Create recovery preparation omitted its verified final.",
                preparation.ResidualPath),
            RecoveryBundlePreparationState.Incomplete =>
                new RouteCreateRecoveryPreparationResult
                {
                    State = RouteCreateRecoveryPreparationState.Incomplete,
                    Preparation = null,
                    Recovery = Residual(preparation.ResidualPath),
                    Cause = preparation.Cause
                        ?? "Route Create recovery preparation is incomplete.",
                },
            RecoveryBundlePreparationState.Blocked =>
                new RouteCreateRecoveryPreparationResult
                {
                    State = RouteCreateRecoveryPreparationState.Blocked,
                    Preparation = null,
                    Recovery = Residual(preparation.ResidualPath),
                    Cause = preparation.Cause
                        ?? "Route Create recovery preparation is blocked.",
                },
            RecoveryBundlePreparationState.Cancelled => Cancelled(preparation.ResidualPath),
            RecoveryBundlePreparationState.NotNeeded => Failed(
                "Route Create recovery preparation unexpectedly reported no need.",
                preparation.ResidualPath),
            _ => throw InvalidPreparationState(preparation.State),
        };
    }

    internal static async ValueTask<RouteCreateRecoveryDeletionResult> DeleteAsync(
        WorkspaceLockLease lease,
        RecoveryBundlePreparation? preparation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(lease);
        if (preparation is null)
        {
            return new RouteCreateRecoveryDeletionResult
            {
                Recovery = NotCreated(),
                FindingCode = null,
                Cause = null,
            };
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
            return RetainedDeletion(
                preparation,
                RouteCreateFindingCode.Interrupted,
                "Recovery deletion was interrupted.");
        }
        catch (Exception)
        {
            return UnknownDeletion(
                preparation,
                RouteCreateFindingCode.RecoveryFailed,
                "The recovery catalogue failed during deletion.");
        }

        if (catalogue.State == RecoveryBundleCatalogueState.Cancelled)
        {
            return RetainedDeletion(
                preparation,
                RouteCreateFindingCode.Interrupted,
                "Recovery deletion was interrupted.");
        }

        if (catalogue.State != RecoveryBundleCatalogueState.Available)
        {
            return UnknownDeletion(
                preparation,
                RouteCreateFindingCode.RecoveryFailed,
                catalogue.Cause
                    ?? "The recovery catalogue is unavailable during deletion.");
        }

        var candidates = catalogue.Candidates
            .Where(candidate => RecoveryBundleIdentity.Matches(candidate, preparation))
            .ToArray();
        if (candidates.Length != 1)
        {
            return UnknownDeletion(
                preparation,
                RouteCreateFindingCode.RecoveryFailed,
                "The exact prepared Route Create recovery final could not be selected for deletion.");
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
            return RetainedDeletion(
                preparation,
                RouteCreateFindingCode.Interrupted,
                "Recovery deletion was interrupted.");
        }
        catch (Exception)
        {
            return UnknownDeletion(
                preparation,
                RouteCreateFindingCode.RecoveryFailed,
                "Recovery deletion failed unexpectedly.");
        }

        return (deletion.State, deletion.Disposition) switch
        {
            (RecoveryBundleDeletionState.Deleted, RecoveryBundleDisposition.Removed) =>
                new RouteCreateRecoveryDeletionResult
                {
                    Recovery = new RouteCreateRecovery
                    {
                        State = RouteCreateRecoveryState.Removed,
                        ResidualPath = null,
                    },
                    FindingCode = null,
                    Cause = null,
                },
            (RecoveryBundleDeletionState.Failed, RecoveryBundleDisposition.Retained) =>
                new RouteCreateRecoveryDeletionResult
                {
                    Recovery = Retained(deletion.ResidualPath ?? preparation.BundlePath),
                    FindingCode = RouteCreateFindingCode.RecoveryArtifactRetained,
                    Cause = deletion.Cause
                        ?? "The verified Route Create recovery artifact was retained.",
                },
            (RecoveryBundleDeletionState.Blocked, RecoveryBundleDisposition.Retained) =>
                new RouteCreateRecoveryDeletionResult
                {
                    Recovery = Retained(deletion.ResidualPath ?? preparation.BundlePath),
                    FindingCode = RouteCreateFindingCode.RecoveryFailed,
                    Cause = deletion.Cause
                        ?? "The verified Route Create recovery artifact could not be deleted.",
                },
            (RecoveryBundleDeletionState.Cancelled, RecoveryBundleDisposition.Retained) =>
                RetainedDeletion(
                    preparation,
                    RouteCreateFindingCode.Interrupted,
                    "Recovery deletion was interrupted."),
            (RecoveryBundleDeletionState.Deleted, RecoveryBundleDisposition.Retained)
                or (RecoveryBundleDeletionState.Deleted, RecoveryBundleDisposition.Unknown)
                or (RecoveryBundleDeletionState.Failed, RecoveryBundleDisposition.Removed)
                or (RecoveryBundleDeletionState.Failed, RecoveryBundleDisposition.Unknown)
                or (RecoveryBundleDeletionState.Blocked, RecoveryBundleDisposition.Removed)
                or (RecoveryBundleDeletionState.Blocked, RecoveryBundleDisposition.Unknown)
                or (RecoveryBundleDeletionState.Cancelled, RecoveryBundleDisposition.Removed)
                or (RecoveryBundleDeletionState.Cancelled, RecoveryBundleDisposition.Unknown) =>
                UnknownDeletion(
                    preparation,
                    deletion.State == RecoveryBundleDeletionState.Cancelled
                        ? RouteCreateFindingCode.Interrupted
                        : RouteCreateFindingCode.RecoveryFailed,
                    deletion.Cause
                        ?? "The Route Create recovery artifact disposition is unknown."),
            _ => throw InvalidDeletionState(deletion.State, deletion.Disposition),
        };
    }

    private static RouteCreateRecoveryPreparationResult Cancelled(string? residualPath)
        => new()
        {
            State = RouteCreateRecoveryPreparationState.Cancelled,
            Preparation = null,
            Recovery = Residual(residualPath),
            Cause = "Route Create recovery preparation was interrupted.",
        };

    private static RouteCreateRecoveryPreparationResult Failed(
        string cause,
        string? residualPath)
        => new()
        {
            State = RouteCreateRecoveryPreparationState.Failed,
            Preparation = null,
            Recovery = Residual(residualPath),
            Cause = cause,
        };

    private static RouteCreateRecoveryPreparationResult CancelledAfterStoreEntry()
        => new()
        {
            State = RouteCreateRecoveryPreparationState.Cancelled,
            Preparation = null,
            Recovery = Unknown(residualPath: null),
            Cause = "Route Create recovery preparation was interrupted after storage entry.",
        };

    private static RouteCreateRecoveryPreparationResult FailedAfterStoreEntry()
        => new()
        {
            State = RouteCreateRecoveryPreparationState.Failed,
            Preparation = null,
            Recovery = Unknown(residualPath: null),
            Cause = "Route Create recovery preparation failed unexpectedly after storage entry.",
        };

    private static RouteCreateRecovery Residual(string? residualPath)
        => residualPath is null ? NotCreated() : Unknown(residualPath);

    private static ArgumentOutOfRangeException InvalidPreparationState(
        RecoveryBundlePreparationState state)
        => new(
            nameof(state),
            state,
            "The recovery preparation state is not defined.");

    private static ArgumentOutOfRangeException InvalidDeletionState(
        RecoveryBundleDeletionState state,
        RecoveryBundleDisposition disposition)
        => new(
            nameof(state),
            (state, disposition),
            "The recovery deletion state or disposition is not defined.");

    private static RouteCreateRecovery NotCreated()
        => new()
        {
            State = RouteCreateRecoveryState.NotCreated,
            ResidualPath = null,
        };

    private static RouteCreateRecovery Retained(string residualPath)
        => new()
        {
            State = RouteCreateRecoveryState.Retained,
            ResidualPath = residualPath,
        };

    private static RouteCreateRecovery Unknown(string? residualPath)
        => new()
        {
            State = RouteCreateRecoveryState.Unknown,
            ResidualPath = residualPath,
        };

    private static RouteCreateRecoveryDeletionResult RetainedDeletion(
        RecoveryBundlePreparation preparation,
        RouteCreateFindingCode findingCode,
        string cause)
        => new()
        {
            Recovery = Retained(preparation.BundlePath),
            FindingCode = findingCode,
            Cause = cause,
        };

    private static RouteCreateRecoveryDeletionResult UnknownDeletion(
        RecoveryBundlePreparation preparation,
        RouteCreateFindingCode findingCode,
        string cause)
        => new()
        {
            Recovery = Unknown(preparation.BundlePath),
            FindingCode = findingCode,
            Cause = cause,
        };
}
