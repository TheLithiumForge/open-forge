using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;

internal static partial class RouteMoveRecoveryLifecycle
{
    internal static async ValueTask<RouteMoveRecoveryPreparationResult> PrepareAsync(
        RouteMoveHeldApplication input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Plan.Projection.RecoveryTargets.IsEmpty)
        {
            return CompletePreparation(
                input.Plan,
                RouteMoveRecoveryPreparationState.NotRequired,
                RouteMoveRecoveryState.NotRequired);
        }

        var catalogue = await ReadCatalogueAsync(input.Plan, cancellationToken)
            .ConfigureAwait(false);
        if (catalogue.State != RecoveryBundleCatalogueState.Available)
        {
            return FromCatalogue(input.Plan, catalogue);
        }

        if (!catalogue.Candidates.IsEmpty)
        {
            return Conflict(input.Plan, catalogue.Candidates[0].Path);
        }

        return await PrepareBundleAsync(input, cancellationToken).ConfigureAwait(false);
    }

    private static async ValueTask<RouteMoveRecoveryPreparationResult> PrepareBundleAsync(
        RouteMoveHeldApplication input,
        CancellationToken cancellationToken)
    {
        try
        {
            var stored = await RecoveryBundleStore.PrepareAsync(
                RecoveryBundleInput.Create(
                    input.Plan.Request.Workspace,
                    RouteMoveDefinitions.CommandIdentity,
                    RecoveryBundleAttribution.Create(
                        RecoveryBundleProducer.Route,
                        RecoveryBundleOperation.Move,
                        input.Plan.Request.Workspace),
                    input.OperationId,
                    input.Plan.Projection.RecoveryTargets),
                cancellationToken).ConfigureAwait(false);
            return FromStored(input.Plan, stored);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return InterruptedPreparation(input.Plan, residualPath: null);
        }
        catch (Exception)
        {
            return FailedPreparation(input.Plan, residualPath: null);
        }
    }

    private static async ValueTask<RecoveryBundleCatalogueResult> ReadCatalogueAsync(
        RouteMovePlan plan,
        CancellationToken cancellationToken)
    {
        try
        {
            return await RecoveryBundleCatalogue.ReadAsync(plan.Request.Workspace, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RecoveryBundleCatalogueResult.Cancelled();
        }
        catch (Exception)
        {
            return RecoveryBundleCatalogueResult.Unavailable(
                "The Route Move recovery catalogue failed unexpectedly.");
        }
    }

    private static RouteMoveRecoveryPreparationResult Conflict(RouteMovePlan plan, string path)
        => StopPreparation(
            plan,
            RouteMoveRecoveryPreparationState.Blocked,
            RouteMoveRecoveryState.Retained,
            path,
            Finding(
                plan,
                RouteMoveFindingCode.RecoveryConflict,
                CliSemanticStatus.Blocked,
                "An existing recovery candidate conflicts with this Route Move application."));

    private static RouteMoveRecoveryPreparationResult FromCatalogue(
        RouteMovePlan plan,
        RecoveryBundleCatalogueResult catalogue)
        => catalogue.State == RecoveryBundleCatalogueState.Cancelled
            ? InterruptedPreparation(plan, residualPath: null)
            : StopPreparation(
                plan,
                RouteMoveRecoveryPreparationState.Incomplete,
                RouteMoveRecoveryState.Unknown,
                residualPath: null,
                Finding(
                    plan,
                    RouteMoveFindingCode.RecoveryUnavailable,
                    CliSemanticStatus.Incomplete,
                    catalogue.Cause ?? "The Route Move recovery catalogue is unavailable."));

    private static RouteMoveRecoveryPreparationResult FromStored(
        RouteMovePlan plan,
        RecoveryBundlePreparationResult stored)
        => stored.State switch
        {
            RecoveryBundlePreparationState.Prepared when stored.Preparation is { } preparation =>
                CompletePreparation(
                    plan,
                    RouteMoveRecoveryPreparationState.Prepared,
                    RouteMoveRecoveryState.NotCreated,
                    preparation),
            RecoveryBundlePreparationState.Incomplete => IncompletePreparation(plan, stored),
            RecoveryBundlePreparationState.Blocked => BlockedPreparation(plan, stored),
            RecoveryBundlePreparationState.Cancelled =>
                InterruptedPreparation(plan, stored.ResidualPath),
            RecoveryBundlePreparationState.NotNeeded or RecoveryBundlePreparationState.Prepared =>
                FailedPreparation(plan, stored.ResidualPath),
            _ => throw new ArgumentOutOfRangeException(
                nameof(stored), stored.State, "The recovery preparation state is not defined."),
        };

    private static RouteMoveRecoveryPreparationResult IncompletePreparation(
        RouteMovePlan plan,
        RecoveryBundlePreparationResult stored)
        => StopPreparation(
            plan,
            RouteMoveRecoveryPreparationState.Incomplete,
            RouteMoveRecoveryState.Unknown,
            stored.ResidualPath,
            Finding(
                plan,
                RouteMoveFindingCode.RecoveryUnavailable,
                CliSemanticStatus.Incomplete,
                stored.Cause ?? "Route Move recovery preparation is incomplete."));

    private static RouteMoveRecoveryPreparationResult BlockedPreparation(
        RouteMovePlan plan,
        RecoveryBundlePreparationResult stored)
        => StopPreparation(
            plan,
            RouteMoveRecoveryPreparationState.Blocked,
            stored.ResidualPath is null
                ? RouteMoveRecoveryState.NotCreated
                : RouteMoveRecoveryState.Retained,
            stored.ResidualPath,
            Finding(
                plan,
                RouteMoveFindingCode.RecoveryConflict,
                CliSemanticStatus.Blocked,
                stored.Cause ?? "Route Move recovery preparation is blocked."));

    private static RouteMoveRecoveryPreparationResult InterruptedPreparation(
        RouteMovePlan plan,
        string? residualPath)
        => StopPreparation(
            plan,
            RouteMoveRecoveryPreparationState.Interrupted,
            residualPath is null ? RouteMoveRecoveryState.NotCreated : RouteMoveRecoveryState.Retained,
            residualPath,
            Finding(
                plan,
                RouteMoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                "Route Move recovery preparation was interrupted."));

    private static RouteMoveRecoveryPreparationResult FailedPreparation(
        RouteMovePlan plan,
        string? residualPath)
        => StopPreparation(
            plan,
            RouteMoveRecoveryPreparationState.Failed,
            RouteMoveRecoveryState.Unknown,
            residualPath,
            Finding(
                plan,
                RouteMoveFindingCode.RecoveryFailed,
                CliSemanticStatus.Failed,
                "Route Move recovery preparation failed unexpectedly."));

    private static RouteMoveRecoveryPreparationResult CompletePreparation(
        RouteMovePlan plan,
        RouteMoveRecoveryPreparationState state,
        RouteMoveRecoveryState recoveryState,
        RecoveryBundlePreparation? preparation = null)
        => new()
        {
            State = state,
            Preparation = preparation,
            Recovery = Recovery(plan, recoveryState, residualPath: null),
            Finding = null,
        };

    private static RouteMoveRecoveryPreparationResult StopPreparation(
        RouteMovePlan plan,
        RouteMoveRecoveryPreparationState state,
        RouteMoveRecoveryState recoveryState,
        string? residualPath,
        RouteMoveFinding finding)
        => new()
        {
            State = state,
            Preparation = null,
            Recovery = Recovery(plan, recoveryState, residualPath),
            Finding = finding,
        };
}
