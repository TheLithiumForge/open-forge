using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Document;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;

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
                UpdatePlanResultFactory.Boundary(
                    request,
                    source: null,
                    [],
                    new UpdateFinding(
                        UpdateFindingCode.Interrupted,
                        target: null,
                        cause: "Update planning was interrupted."),
                    UpdatePlanResultFactory.UnstartedLifecycle(
                        UpdateLifecycleTrust.NotRequested,
                        UpdateLifecycleCoverage.NotRequested)));
        }

        var payloadRead = EmbeddedFrameworkPayloadReader.Read();
        if (payloadRead.State != FrameworkPayloadReadState.Available
            || payloadRead.Payload is not { } payload)
        {
            var code = payloadRead.State == FrameworkPayloadReadState.Unavailable
                ? UpdateFindingCode.PayloadUnavailable
                : UpdateFindingCode.PayloadInvalid;
            return Boundary(
                UpdatePlanResultFactory.Boundary(
                    request,
                    source: null,
                    [],
                    new UpdateFinding(
                        code,
                        target: null,
                        cause: payloadRead.Cause ?? "The embedded Framework payload is unavailable."),
                    UpdatePlanResultFactory.UnstartedLifecycle(
                        UpdateLifecycleTrust.NotRequested,
                        UpdateLifecycleCoverage.NotRequested)));
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
                        UpdatePlanResultFactory.UnstartedLifecycle(
                            UpdateLifecycleTrust.Trusted,
                            finding.Status == OpenForge.Cli.Core.Shell.Definitions.CliSemanticStatus.Incomplete
                                ? UpdateLifecycleCoverage.Incomplete
                                : UpdateLifecycleCoverage.Blocked))),
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
                UpdatePlanResultFactory.UnstartedLifecycle(
                    UpdateLifecycleTrust.Trusted,
                    UpdateLifecycleCoverage.Blocked));
            return new UpdatePlanResolution(new UpdatePlanBuild(plan, preview), Execution: null);
        }

        var effects = UpdatePhysicalEffectPlanner.Plan(plan, observations);
        var intendedLifecycle = BuildLifecycle(payload, lifecycle, observations, plan);
        var lifecyclePlan = _lifecycleStore.PlanFrameworkUpdate(lifecycle, intendedLifecycle);
        if (lifecyclePlan.State == LifecycleWritePlanState.Blocked)
        {
            return Boundary(
                UpdatePlanResultFactory.Boundary(
                    request,
                    UpdatePlanResultFactory.Source(payload),
                    observations.Select(value => value.Comparison).ToArray(),
                    new UpdateFinding(
                        UpdateFindingCode.LifecycleBlocked,
                        target: LifecycleSchema.RelativePath,
                        cause: lifecyclePlan.Cause ?? "The intended Framework lifecycle state is blocked."),
                    UpdatePlanResultFactory.UnstartedLifecycle(
                        UpdateLifecycleTrust.Blocked,
                        UpdateLifecycleCoverage.Blocked)));
        }

        var lifecycleChange = lifecyclePlan.Change;
        var completion = new UpdatePlanCompletion
        {
            Request = request,
            Payload = payload,
            Intended = intended,
            Plan = plan,
            Effects = effects,
            LifecycleChange = lifecycleChange,
            Findings = findings,
        };
        var completePreview = UpdatePlanResultFactory.Preview(completion);
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
                UpdatePlanResultFactory.Boundary(
                    request,
                    UpdatePlanResultFactory.Source(payload),
                    [],
                    new UpdateFinding(
                        UpdateFindingCode.LifecycleMissing,
                        target: LifecycleSchema.RelativePath,
                        cause: lifecycle.Cause ?? "Trusted Framework lifecycle state is missing."),
                    UpdatePlanResultFactory.UnstartedLifecycle(
                        UpdateLifecycleTrust.Unavailable,
                        UpdateLifecycleCoverage.Incomplete))),
            LifecycleStoreReadState.Unavailable => Boundary(
                UpdatePlanResultFactory.Boundary(
                    request,
                    UpdatePlanResultFactory.Source(payload),
                    [],
                    new UpdateFinding(
                        UpdateFindingCode.LifecycleUnavailable,
                        target: LifecycleSchema.RelativePath,
                        cause: lifecycle.Cause
                            ?? lifecycle.Failure?.DirectCause
                            ?? "Trusted Framework lifecycle state is unavailable."),
                    UpdatePlanResultFactory.UnstartedLifecycle(
                        UpdateLifecycleTrust.Unavailable,
                        UpdateLifecycleCoverage.Incomplete))),
            LifecycleStoreReadState.Cancelled => Boundary(
                UpdatePlanResultFactory.Boundary(
                    request,
                    UpdatePlanResultFactory.Source(payload),
                    [],
                    new UpdateFinding(
                        UpdateFindingCode.Interrupted,
                        target: LifecycleSchema.RelativePath,
                        cause: "Update lifecycle inspection was interrupted."),
                    UpdatePlanResultFactory.UnstartedLifecycle(
                        UpdateLifecycleTrust.NotRequested,
                        UpdateLifecycleCoverage.NotRequested))),
            LifecycleStoreReadState.Invalid or LifecycleStoreReadState.Blocked => Boundary(
                UpdatePlanResultFactory.Boundary(
                    request,
                    UpdatePlanResultFactory.Source(payload),
                    [],
                    new UpdateFinding(
                        UpdateFindingCode.LifecycleBlocked,
                        target: LifecycleSchema.RelativePath,
                        cause: lifecycle.Cause ?? "Trusted Framework lifecycle state is blocked."),
                    UpdatePlanResultFactory.UnstartedLifecycle(
                        UpdateLifecycleTrust.Blocked,
                        UpdateLifecycleCoverage.Blocked))),
            _ => Boundary(
                UpdatePlanResultFactory.Boundary(
                    request,
                    UpdatePlanResultFactory.Source(payload),
                    [],
                    new UpdateFinding(
                        UpdateFindingCode.LifecycleBlocked,
                        target: LifecycleSchema.RelativePath,
                        cause: "Trusted Framework lifecycle state is incomplete."),
                    UpdatePlanResultFactory.UnstartedLifecycle(
                        UpdateLifecycleTrust.Blocked,
                        UpdateLifecycleCoverage.Blocked))),
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
            GeneratedRegions = ReadGeneratedRegions(ordered).ToArray(),
        };
    }

    private static IEnumerable<FrameworkGeneratedRegion> ReadGeneratedRegions(
        IEnumerable<FrameworkLifecycleTarget> targets)
    {
        foreach (var target in targets)
        {
            if (target.SourceAssetPath is null && target.Region is { } region)
            {
                yield return new FrameworkGeneratedRegion
                {
                    Path = target.Path,
                    Region = region,
                };
            }
        }
    }

    private static IReadOnlyList<UpdateFinding> ReadFindings(UpdatePlanningPlan plan)
    {
        var findings = new List<UpdateFinding>();
        foreach (var decision in plan.Decisions)
        {
            var comparison = decision.Comparison;
            if (decision.Disposition == UpdatePlanningDisposition.Preserve)
            {
                var code = ReadPreservedFinding(comparison);
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

    private static UpdateFindingCode ReadPreservedFinding(Models.Comparison.UpdateComparison comparison)
    {
        if (comparison.IntendedState == Models.Comparison.UpdateComparisonIntendedState.Retired)
        {
            return UpdateFindingCode.RetiredContentPreserved;
        }

        if (comparison.CurrentState == Models.Comparison.UpdateComparisonCurrentState.Missing)
        {
            return UpdateFindingCode.ManagedTargetMissing;
        }

        return UpdateFindingCode.ManagedDivergence;
    }

    private static UpdatePlanResolution Boundary(UpdateResult preview)
        => new(
            new UpdatePlanBuild(plan: null, preview),
            Execution: null);

    private static (string Path, Models.Comparison.UpdateComparisonTargetKind Kind, string? Region) Identity(
        Models.Comparison.UpdateComparison comparison)
        => (comparison.RelativePath, comparison.Kind, comparison.RegionIdentity);
}
