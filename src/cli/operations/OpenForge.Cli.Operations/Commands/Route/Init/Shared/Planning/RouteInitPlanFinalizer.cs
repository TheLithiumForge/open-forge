using OpenForge.Cli.Core.Framework.Ownership.Models;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

internal sealed class RouteInitPlanFinalizer
{
    private readonly RouteInitFrameworkOwnershipBuilder _lifecycleBuilder = new();
    private readonly RouteInitProspectiveTopologyPlanner _topologyPlanner = new();
    private readonly PhysicalPathResolver _physicalPathResolver = new();

    internal async ValueTask<RouteInitPlanFinalizationResult> FinalizeAsync(
        RouteInitRequest request,
        RouteInitInspectionFacts inspection,
        RouteInitProspectivePlanFacts prospective,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(inspection);
        ArgumentNullException.ThrowIfNull(prospective);

        var effectPlan = RouteInitRestorationEffects.Extend(prospective.Effects, inspection.Restoration);
        OwnershipWritePlanState? ownershipState = null;
        if (inspection.Framework is { } framework)
        {
            var lifecycle = _lifecycleBuilder.BuildPlan(
                framework.Trust,
                framework.Payload,
                effectPlan.FrameworkManagedStates);
            if (lifecycle.State != RouteInitFrameworkOwnershipPlanState.Complete
                || lifecycle.WritePlan is not { } writePlan)
            {
                return Stopped(
                    inspection,
                    RouteInitFindingCode.LifecycleBlocked,
                    lifecycle.Cause ?? "The scoped Framework lifecycle update is blocked.",
                    incomplete: false);
            }

            ownershipState = writePlan.State;
            effectPlan = _topologyPlanner.AssembleEffects(
                prospective.Topology,
                prospective.Projection,
                prospective.DirectoryStates,
                prospective.EffectSources,
                new RouteInitOwnershipEffectInput(writePlan, framework.Trust.Ownership.Snapshot)).Plan
                ?? throw new InvalidOperationException(
                    "A complete scoped lifecycle plan must form complete Route Init effects.");
            effectPlan = RouteInitRestorationEffects.Extend(effectPlan, inspection.Restoration);
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
            new RouteInitPlanFinalizationFacts(effectPlan, ownershipState));
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
