using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;

internal static partial class RouteRemoveRecoveryLifecycle
{
    internal static async ValueTask<RouteRemoveRecoveryPreparationResult> PrepareAsync(
        RouteRemoveRecoveryPreparationInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Plan.Projection.RecoveryTargets.IsEmpty)
        {
            return CompletePreparation(
                input.Plan,
                RouteRemoveRecoveryPreparationState.NotRequired,
                RouteRemoveRecoveryState.NotRequired);
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

    private static async ValueTask<RouteRemoveRecoveryPreparationResult> PrepareBundleAsync(
        RouteRemoveRecoveryPreparationInput input,
        CancellationToken cancellationToken)
    {
        try
        {
            var stored = await RecoveryBundleStore.PrepareAsync(
                RecoveryBundleInput.Create(
                    input.Plan.Request.Workspace,
                    RouteRemoveDefinitions.CommandIdentity,
                    RecoveryBundleAttribution.Create(
                        RecoveryBundleProducer.Route,
                        RecoveryBundleOperation.Remove,
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
        RouteRemovePlan plan,
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
                "The Route Remove recovery catalogue failed unexpectedly.");
        }
    }

    private static RouteRemoveRecoveryPreparationResult Conflict(RouteRemovePlan plan, string path)
        => StopPreparation(
            plan,
            RouteRemoveRecoveryPreparationState.Blocked,
            RouteRemoveRecoveryState.Retained,
            path,
            Finding(
                plan,
                RouteRemoveFindingCode.RecoveryConflict,
                CliSemanticStatus.Blocked,
                "An existing recovery candidate conflicts with this Route Remove application."));

    private static RouteRemoveRecoveryPreparationResult FromCatalogue(
        RouteRemovePlan plan,
        RecoveryBundleCatalogueResult catalogue)
        => catalogue.State == RecoveryBundleCatalogueState.Cancelled
            ? InterruptedPreparation(plan, residualPath: null)
            : StopPreparation(
                plan,
                RouteRemoveRecoveryPreparationState.Incomplete,
                RouteRemoveRecoveryState.Unknown,
                residualPath: null,
                Finding(
                    plan,
                    RouteRemoveFindingCode.RecoveryUnavailable,
                    CliSemanticStatus.Incomplete,
                    catalogue.Cause ?? "The Route Remove recovery catalogue is unavailable."));

    private static RouteRemoveRecoveryPreparationResult FromStored(
        RouteRemovePlan plan,
        RecoveryBundlePreparationResult stored)
        => stored.State switch
        {
            RecoveryBundlePreparationState.Prepared when stored.Preparation is { } preparation =>
                CompletePreparation(
                    plan,
                    RouteRemoveRecoveryPreparationState.Prepared,
                    RouteRemoveRecoveryState.NotCreated,
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

    private static RouteRemoveRecoveryPreparationResult IncompletePreparation(
        RouteRemovePlan plan,
        RecoveryBundlePreparationResult stored)
        => StopPreparation(
            plan,
            RouteRemoveRecoveryPreparationState.Incomplete,
            RouteRemoveRecoveryState.Unknown,
            stored.ResidualPath,
            Finding(
                plan,
                RouteRemoveFindingCode.RecoveryUnavailable,
                CliSemanticStatus.Incomplete,
                stored.Cause ?? "Route Remove recovery preparation is incomplete."));

    private static RouteRemoveRecoveryPreparationResult BlockedPreparation(
        RouteRemovePlan plan,
        RecoveryBundlePreparationResult stored)
        => StopPreparation(
            plan,
            RouteRemoveRecoveryPreparationState.Blocked,
            stored.ResidualPath is null
                ? RouteRemoveRecoveryState.NotCreated
                : RouteRemoveRecoveryState.Retained,
            stored.ResidualPath,
            Finding(
                plan,
                RouteRemoveFindingCode.RecoveryConflict,
                CliSemanticStatus.Blocked,
                stored.Cause ?? "Route Remove recovery preparation is blocked."));

    private static RouteRemoveRecoveryPreparationResult InterruptedPreparation(
        RouteRemovePlan plan,
        string? residualPath)
        => StopPreparation(
            plan,
            RouteRemoveRecoveryPreparationState.Interrupted,
            residualPath is null ? RouteRemoveRecoveryState.NotCreated : RouteRemoveRecoveryState.Retained,
            residualPath,
            Finding(
                plan,
                RouteRemoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                "Route Remove recovery preparation was interrupted."));

    private static RouteRemoveRecoveryPreparationResult FailedPreparation(
        RouteRemovePlan plan,
        string? residualPath)
        => StopPreparation(
            plan,
            RouteRemoveRecoveryPreparationState.Failed,
            RouteRemoveRecoveryState.Unknown,
            residualPath,
            Finding(
                plan,
                RouteRemoveFindingCode.RecoveryFailed,
                CliSemanticStatus.Failed,
                "Route Remove recovery preparation failed unexpectedly."));

    private static RouteRemoveRecoveryPreparationResult CompletePreparation(
        RouteRemovePlan plan,
        RouteRemoveRecoveryPreparationState state,
        RouteRemoveRecoveryState recoveryState,
        RecoveryBundlePreparation? preparation = null)
        => new()
        {
            State = state,
            Preparation = preparation,
            Recovery = Recovery(plan, recoveryState, residualPath: null),
            Finding = null,
        };

    private static RouteRemoveRecoveryPreparationResult StopPreparation(
        RouteRemovePlan plan,
        RouteRemoveRecoveryPreparationState state,
        RouteRemoveRecoveryState recoveryState,
        string? residualPath,
        RouteRemoveFinding finding)
        => new()
        {
            State = state,
            Preparation = null,
            Recovery = Recovery(plan, recoveryState, residualPath),
            Finding = finding,
        };
}
