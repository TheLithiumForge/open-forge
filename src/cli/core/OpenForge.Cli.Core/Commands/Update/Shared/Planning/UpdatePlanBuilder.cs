using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Planning;

internal sealed class UpdatePlanBuilder
{
    private readonly LifecycleStore _lifecycleStore;
    private readonly UpdateIntendedStateBuilder _intendedStateBuilder;

    private UpdatePlanBuilder(
        LifecycleStore lifecycleStore,
        UpdateIntendedStateBuilder intendedStateBuilder)
    {
        _lifecycleStore = lifecycleStore;
        _intendedStateBuilder = intendedStateBuilder;
    }

    internal static UpdatePlanBuilder Create()
    {
        var physicalPathResolver = new PhysicalPathResolver();
        return new UpdatePlanBuilder(
            new LifecycleStore(physicalPathResolver),
            new UpdateIntendedStateBuilder(
                new UpdateComparisonReader(physicalPathResolver),
                new UpdateGeneratedNavigationPlanner(physicalPathResolver)));
    }

    internal async ValueTask<UpdatePlanBuild> BuildAsync(
        UpdateRequest request,
        CancellationToken cancellationToken)
    {
        var resolution = await BuildExecutionAsync(request, cancellationToken)
            .ConfigureAwait(false);
        return resolution.Build;
    }

    internal async ValueTask<UpdatePlanResolution> BuildExecutionAsync(
        UpdateRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (cancellationToken.IsCancellationRequested)
        {
            return Boundary(
                request,
                source: null,
                [],
                UpdateFindingCode.Interrupted,
                target: null,
                "Update planning was interrupted.",
                UpdateLifecycleTrust.NotRequested,
                UpdateLifecycleCoverage.NotRequested);
        }

        var payloadRead = EmbeddedFrameworkPayloadReader.Read();
        if (payloadRead.State != FrameworkPayloadReadState.Available
            || payloadRead.Payload is not { } payload)
        {
            var code = payloadRead.State == FrameworkPayloadReadState.Unavailable
                ? UpdateFindingCode.PayloadUnavailable
                : UpdateFindingCode.PayloadInvalid;
            return Boundary(
                request,
                source: null,
                [],
                code,
                target: null,
                payloadRead.Cause ?? "The embedded Framework payload is unavailable.",
                UpdateLifecycleTrust.NotRequested,
                UpdateLifecycleCoverage.NotRequested);
        }

        var lifecycle = await _lifecycleStore
            .ReadAsync(request.Workspace, LifecycleSection.Framework, cancellationToken)
            .ConfigureAwait(false);
        var lifecycleBoundary = ReadLifecycleBoundary(request, payload, lifecycle);
        if (lifecycleBoundary is not null)
        {
            return lifecycleBoundary;
        }

        var intended = await _intendedStateBuilder
            .BuildAsync(request, payload, lifecycle, cancellationToken)
            .ConfigureAwait(false);
        if (intended.Finding is { } finding)
        {
            return new UpdatePlanResolution(
                new UpdatePlanBuild(
                    plan: null,
                    UpdatePlanResultFactory.Boundary(
                        request,
                        UpdatePlanResultFactory.Source(payload),
                        [],
                        finding,
                        UpdateLifecycleTrust.Trusted,
                        finding.Status == OpenForge.Cli.Core.Shell.Definitions.CliSemanticStatus.Incomplete
                            ? UpdateLifecycleCoverage.Incomplete
                            : UpdateLifecycleCoverage.Blocked)),
                Execution: null);
        }

        var observations = intended.Observations;
        var plan = UpdatePlanningPolicy.Plan(
            observations.Select(value => value.Comparison).ToArray(),
            new UpdatePlanningAuthority(request.Force, request.Prune));
        var findings = ReadFindings(plan);
        if (plan.IsBlocked)
        {
            var preview = UpdatePlanResultFactory.Boundary(
                request,
                UpdatePlanResultFactory.Source(payload),
                observations.Select(value => value.Comparison).ToArray(),
                findings.First(value => value.Status
                    == OpenForge.Cli.Core.Shell.Definitions.CliSemanticStatus.Blocked),
                UpdateLifecycleTrust.Trusted,
                UpdateLifecycleCoverage.Blocked);
            return new UpdatePlanResolution(new UpdatePlanBuild(plan, preview), Execution: null);
        }

        var effects = UpdatePhysicalEffectPlanner.Plan(plan, observations);
        var intendedLifecycle = BuildLifecycle(payload, lifecycle, observations, plan);
        var lifecyclePlan = _lifecycleStore.PlanFrameworkUpdate(lifecycle, intendedLifecycle);
        if (lifecyclePlan.State == LifecycleWritePlanState.Blocked)
        {
            return Boundary(
                request,
                UpdatePlanResultFactory.Source(payload),
                observations.Select(value => value.Comparison).ToArray(),
                UpdateFindingCode.LifecycleBlocked,
                LifecycleSchema.RelativePath,
                lifecyclePlan.Cause ?? "The intended Framework lifecycle state is blocked.",
                UpdateLifecycleTrust.Blocked,
                UpdateLifecycleCoverage.Blocked);
        }

        var lifecycleChange = lifecyclePlan.Change;
        var completePreview = UpdatePlanResultFactory.Preview(
            request,
            payload,
            observations,
            plan,
            effects,
            lifecycleChange,
            findings);
        var build = new UpdatePlanBuild(plan, completePreview);
        return new UpdatePlanResolution(
            build,
            new UpdatePlanExecution(
                request,
                build,
                payload,
                lifecycle,
                observations,
                intended.ProjectionInputs,
                effects,
                lifecycleChange));
    }

    private static UpdatePlanResolution? ReadLifecycleBoundary(
        UpdateRequest request,
        FrameworkPayload payload,
        LifecycleStoreReadResult lifecycle)
        => lifecycle.State switch
        {
            LifecycleStoreReadState.Available when lifecycle.Framework is not null => null,
            LifecycleStoreReadState.DocumentMissing or LifecycleStoreReadState.SectionMissing => Boundary(
                request,
                UpdatePlanResultFactory.Source(payload),
                [],
                UpdateFindingCode.LifecycleMissing,
                LifecycleSchema.RelativePath,
                lifecycle.Cause ?? "Trusted Framework lifecycle state is missing.",
                UpdateLifecycleTrust.Unavailable,
                UpdateLifecycleCoverage.Incomplete),
            LifecycleStoreReadState.Unavailable => Boundary(
                request,
                UpdatePlanResultFactory.Source(payload),
                [],
                UpdateFindingCode.LifecycleUnavailable,
                LifecycleSchema.RelativePath,
                lifecycle.Cause
                    ?? lifecycle.Failure?.DirectCause
                    ?? "Trusted Framework lifecycle state is unavailable.",
                UpdateLifecycleTrust.Unavailable,
                UpdateLifecycleCoverage.Incomplete),
            LifecycleStoreReadState.Cancelled => Boundary(
                request,
                UpdatePlanResultFactory.Source(payload),
                [],
                UpdateFindingCode.Interrupted,
                LifecycleSchema.RelativePath,
                "Update lifecycle inspection was interrupted.",
                UpdateLifecycleTrust.NotRequested,
                UpdateLifecycleCoverage.NotRequested),
            LifecycleStoreReadState.Invalid or LifecycleStoreReadState.Blocked => Boundary(
                request,
                UpdatePlanResultFactory.Source(payload),
                [],
                UpdateFindingCode.LifecycleBlocked,
                LifecycleSchema.RelativePath,
                lifecycle.Cause ?? "Trusted Framework lifecycle state is blocked.",
                UpdateLifecycleTrust.Blocked,
                UpdateLifecycleCoverage.Blocked),
            _ => Boundary(
                request,
                UpdatePlanResultFactory.Source(payload),
                [],
                UpdateFindingCode.LifecycleBlocked,
                LifecycleSchema.RelativePath,
                "Trusted Framework lifecycle state is incomplete.",
                UpdateLifecycleTrust.Blocked,
                UpdateLifecycleCoverage.Blocked),
        };

    private static FrameworkLifecycleState BuildLifecycle(
        FrameworkPayload payload,
        LifecycleStoreReadResult lifecycle,
        IReadOnlyList<UpdateComparisonObservation> observations,
        UpdatePlanningPlan plan)
    {
        var existing = lifecycle.Framework
            ?? throw new InvalidOperationException(
                "A trusted Update lifecycle read requires Framework state.");
        var decisions = plan.Decisions.ToDictionary(
            value => Identity(value.Comparison));
        var targets = new List<FrameworkLifecycleTarget>();
        foreach (var observation in observations)
        {
            var comparison = observation.Comparison;
            var decision = decisions[Identity(comparison)];
            if (decision.Disposition == UpdatePlanningDisposition.Delete)
            {
                continue;
            }

            var prior = observation.LifecycleTarget;
            var baseline = decision.Disposition == UpdatePlanningDisposition.Preserve
                    || comparison.IntendedFingerprint is null
                ? comparison.BaselineFingerprint
                : comparison.IntendedFingerprint;
            targets.Add(new FrameworkLifecycleTarget
            {
                Path = comparison.RelativePath,
                SourceAssetPath = comparison.SourceAssetPath,
                Region = comparison.RegionIdentity,
                BaselineFingerprint = baseline
                    ?? throw new InvalidOperationException(
                        "A retained Update lifecycle target requires a baseline fingerprint."),
                FingerprintKind = prior?.FingerprintKind
                    ?? LifecycleSchema.SemanticFingerprintKind,
            });
        }

        var ordered = targets
            .OrderBy(value => value.Path, StringComparer.Ordinal)
            .ThenBy(value => value.Region, StringComparer.Ordinal)
            .ToArray();
        return new FrameworkLifecycleState
        {
            Coverage = LifecycleSchema.CompleteCoverage,
            Source = new FrameworkLifecycleSource
            {
                Id = existing.Source.Id,
                Version = existing.Source.Version,
                InventoryFingerprint = payload.InventoryFingerprint,
            },
            Targets = ordered,
            GeneratedRegions = ordered
                .Where(value => value.SourceAssetPath is null && value.Region is not null)
                .Select(value => new FrameworkGeneratedRegion
                {
                    Path = value.Path,
                    Region = value.Region!,
                })
                .ToArray(),
        };
    }

    private static IReadOnlyList<UpdateFinding> ReadFindings(UpdatePlanningPlan plan)
    {
        var findings = new List<UpdateFinding>();
        foreach (var decision in plan.Decisions)
        {
            var comparison = decision.Comparison;
            if (decision.Disposition == UpdatePlanningDisposition.Preserve)
            {
                var code = comparison.IntendedState == Models.Comparison.UpdateComparisonIntendedState.Retired
                    ? UpdateFindingCode.RetiredContentPreserved
                    : comparison.CurrentState == Models.Comparison.UpdateComparisonCurrentState.Missing
                        ? UpdateFindingCode.ManagedTargetMissing
                        : UpdateFindingCode.ManagedDivergence;
                findings.Add(new UpdateFinding(
                    code,
                    comparison.RelativePath,
                    "Update preserved trusted managed divergence without the required explicit authority."));
            }
            else if (decision.Disposition == UpdatePlanningDisposition.Blocked)
            {
                findings.Add(new UpdateFinding(
                    comparison.IntendedState == Models.Comparison.UpdateComparisonIntendedState.Retired
                        ? UpdateFindingCode.RetirementIneligible
                        : UpdateFindingCode.PlanBlocked,
                    comparison.RelativePath,
                    "One Update comparison prevents a complete safe plan."));
            }
        }

        if (plan.IsBlocked && findings.All(value => value.Code != UpdateFindingCode.PlanBlocked))
        {
            findings.Add(new UpdateFinding(
                UpdateFindingCode.PlanBlocked,
                target: null,
                "One selected Update fact prevents every effect."));
        }

        return findings;
    }

    private static UpdatePlanResolution Boundary(
        UpdateRequest request,
        UpdateSource? source,
        IReadOnlyList<Models.Comparison.UpdateComparison> comparisons,
        UpdateFindingCode code,
        string? target,
        string cause,
        UpdateLifecycleTrust trust,
        UpdateLifecycleCoverage coverage)
    {
        var preview = UpdatePlanResultFactory.Boundary(
            request,
            source,
            comparisons,
            new UpdateFinding(code, target, cause),
            trust,
            coverage);
        return new UpdatePlanResolution(
            new UpdatePlanBuild(plan: null, preview),
            Execution: null);
    }

    private static (string Path, Models.Comparison.UpdateComparisonTargetKind Kind, string? Region) Identity(
        Models.Comparison.UpdateComparison comparison)
        => (comparison.RelativePath, comparison.Kind, comparison.RegionIdentity);
}
