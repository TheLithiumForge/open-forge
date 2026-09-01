using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

internal sealed class RouteInitPlanFinalizer
{
    private readonly RouteInitFrameworkLifecycleBuilder _lifecycleBuilder = new();
    private readonly RouteInitProspectiveTopologyPlanner _topologyPlanner = new();
    private readonly PhysicalPathResolver _physicalPathResolver = new();
    private readonly RecoveryBundleCatalogue _recoveryCatalogue;

    internal RouteInitPlanFinalizer(RecoveryBundleCatalogue recoveryCatalogue)
    {
        ArgumentNullException.ThrowIfNull(recoveryCatalogue);
        _recoveryCatalogue = recoveryCatalogue;
    }

    internal async ValueTask<RouteInitPlanFinalizationResult> FinalizeAsync(
        RouteInitRequest request,
        RouteInitInspectionFacts inspection,
        RouteInitProspectivePlanFacts prospective,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(inspection);
        ArgumentNullException.ThrowIfNull(prospective);

        var effectPlan = prospective.Effects;
        FrameworkLifecycleState? intendedLifecycle = null;
        if (inspection.Framework is { } framework)
        {
            var lifecycle = _lifecycleBuilder.BuildPlan(
                framework.Trust,
                framework.Payload,
                effectPlan.FrameworkManagedStates);
            if (lifecycle.State != RouteInitFrameworkLifecyclePlanState.Complete
                || lifecycle.WritePlan is not { } writePlan)
            {
                return Stopped(
                    inspection,
                    RouteInitFindingCode.LifecycleBlocked,
                    lifecycle.Cause ?? "The scoped Framework lifecycle update is blocked.",
                    incomplete: false);
            }

            if (framework.Trust.Read.File is not { } lifecycleBefore)
            {
                return Stopped(
                    inspection,
                    RouteInitFindingCode.LifecycleBlocked,
                    "The trusted Framework lifecycle read has no exact file snapshot.",
                    incomplete: false);
            }

            intendedLifecycle = writePlan.State == LifecycleWritePlanState.Planned
                ? lifecycle.Intended
                : null;
            effectPlan = _topologyPlanner.AssembleEffects(
                prospective.Topology,
                prospective.Projection,
                prospective.DirectoryStates,
                prospective.EffectSources,
                new RouteInitLifecycleEffectInput(writePlan, lifecycleBefore)).Plan
                ?? throw new InvalidOperationException(
                    "A complete scoped lifecycle plan must form complete Route Init effects.");
        }

        if (effectPlan.RecoveryTargets.Count > 0)
        {
            var recovery = await _recoveryCatalogue.ReadAsync(
                    request.Workspace,
                    cancellationToken)
                .ConfigureAwait(false);
            if (ReadRecoveryBoundary(inspection, recovery) is { } recoveryBoundary)
            {
                return new RouteInitPlanFinalizationStopped(recoveryBoundary);
            }
        }

        var preflight = await new MutationPreflight(
                new FileExpectationValidator(_physicalPathResolver))
            .ValidateAsync(
                request.Workspace,
                effectPlan.DirectoryCreations,
                effectPlan.FileChanges,
                cancellationToken)
            .ConfigureAwait(false);
        if (preflight.State != MutationValidationState.Valid)
        {
            var (code, incomplete) = ReadPreflightFinding(preflight.State);
            return Stopped(
                inspection,
                code,
                preflight.Cause ?? "Route Init preflight did not complete.",
                incomplete);
        }

        return new RouteInitPlanFinalizationCompleted(
            new RouteInitPlanFinalizationFacts(effectPlan, intendedLifecycle));
    }

    private static RouteInitPlanningBoundary? ReadRecoveryBoundary(
        RouteInitInspectionFacts inspection,
        RecoveryBundleCatalogueResult recovery)
    {
        return recovery.State switch
        {
            RecoveryBundleCatalogueState.Available when recovery.Candidates.Length == 0 => null,
            RecoveryBundleCatalogueState.Available => Boundary(
                inspection,
                RouteInitFindingCode.RecoveryConflict,
                "A recognized recovery candidate conflicts with this Route Init plan.",
                incomplete: false),
            RecoveryBundleCatalogueState.Unavailable => Boundary(
                inspection,
                RouteInitFindingCode.RecoveryUnavailable,
                recovery.Cause ?? "The Route Init recovery catalogue is unavailable.",
                incomplete: true),
            RecoveryBundleCatalogueState.Cancelled => Boundary(
                inspection,
                RouteInitFindingCode.Interrupted,
                "Route Init recovery inspection was interrupted.",
                incomplete: true),
            _ => throw new ArgumentOutOfRangeException(
                nameof(recovery),
                recovery.State,
                "The recovery catalogue state is not defined."),
        };
    }

    internal static (RouteInitFindingCode Code, bool Incomplete) ReadPreflightFinding(
        MutationValidationState state)
        => state switch
        {
            MutationValidationState.Valid => (RouteInitFindingCode.OperationFailed, false),
            MutationValidationState.Mismatched => (RouteInitFindingCode.TargetChanged, false),
            MutationValidationState.Blocked => (RouteInitFindingCode.TargetUnsafe, false),
            MutationValidationState.Failed => (RouteInitFindingCode.InspectionIncomplete, true),
            MutationValidationState.Cancelled => (RouteInitFindingCode.Interrupted, true),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The mutation validation state is not defined."),
        };

    private static RouteInitPlanFinalizationStopped Stopped(
        RouteInitInspectionFacts inspection,
        RouteInitFindingCode code,
        string cause,
        bool incomplete)
        => new(Boundary(inspection, code, cause, incomplete));

    private static RouteInitPlanningBoundary Boundary(
        RouteInitInspectionFacts inspection,
        RouteInitFindingCode code,
        string cause,
        bool incomplete)
        => new(
            code,
            cause,
            incomplete,
            inspection.Target,
            Alignment: inspection.Framework?.Alignment,
            Payload: inspection.Framework?.Payload);
}
