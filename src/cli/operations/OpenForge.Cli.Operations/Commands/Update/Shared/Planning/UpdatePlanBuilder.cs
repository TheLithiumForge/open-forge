using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using System.Text;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Planning;

internal sealed class UpdatePlanBuilder
{
    private readonly PhysicalPathResolver _physicalPathResolver;
    private readonly UpdateIntendedStateBuilder _intendedStateBuilder;
    private readonly WorkspaceOwnershipStore _ownershipStore = new();

    private UpdatePlanBuilder(
        PhysicalPathResolver physicalPathResolver,
        UpdateIntendedStateBuilder intendedStateBuilder)
    {
        _physicalPathResolver = physicalPathResolver;
        _intendedStateBuilder = intendedStateBuilder;
    }

    internal static UpdatePlanBuilder Create()
    {
        var physicalPathResolver = new PhysicalPathResolver();
        return new UpdatePlanBuilder(
            physicalPathResolver,
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

        var container = Path.Combine(request.Workspace.LexicalRoot, ".agents");
        var containerResolution = _physicalPathResolver.ResolveCandidate(request.Workspace.LexicalRoot, request.Workspace.PhysicalRoot, container);
        if (containerResolution.State != PhysicalPathState.Contained || !Directory.Exists(container)
            || (File.GetAttributes(container) & (FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
            return Boundary(UpdatePlanResultFactory.Boundary(request, UpdatePlanResultFactory.Source(payload), [],
                new UpdateFinding(UpdateFindingCode.TargetUnsafe, ".agents", "The Framework container is not a contained ordinary directory."),
                UpdatePlanResultFactory.UnstartedLifecycle(UpdateLifecycleTrust.NotRequested, UpdateLifecycleCoverage.Blocked)));

        var ownership = await WorkspaceOwnershipReader.ReadAsync(
                _physicalPathResolver,
                request.Workspace,
                cancellationToken)
            .ConfigureAwait(false);
        var settings = await WorkspaceSettingsReader.ReadAsync(_physicalPathResolver, request.Workspace, cancellationToken).ConfigureAwait(false);
        var intended = await _intendedStateBuilder
            .BuildAsync(request, payload, ownership, settings.Document, cancellationToken)
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
                            finding.Code == UpdateFindingCode.OwnershipObservation ? UpdateLifecycleTrust.Unavailable : UpdateLifecycleTrust.Trusted,
                            finding.Code == UpdateFindingCode.OwnershipObservation || finding.Status == OpenForge.Cli.Core.Shell.Definitions.CliSemanticStatus.Incomplete
                                ? UpdateLifecycleCoverage.Incomplete
                                : UpdateLifecycleCoverage.Blocked))),
                Execution: null);
        }

        var observations = intended.Observations;
        var plan = UpdatePlanningPolicy.Plan(
            observations.Select(value => value.Comparison).ToArray(),
            new UpdatePlanningAuthority(request.Force, request.Prune));
        var findings = ReadFindings(plan);
        if (ownership.State != WorkspaceOwnershipReadState.Complete)
            findings = [.. findings, new UpdateFinding(UpdateFindingCode.OwnershipObservation,
                WorkspaceOwnershipDefinitions.RelativePath,
                ownership.Cause ?? "The ownership lock is unavailable; no previous ownership was inferred.")];
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
        var ownershipPlan = effects.Count == 0 && ownership.Document.Framework is null
            ? OwnershipWritePlanResult.Skipped("No verified Framework writes established ownership.")
            : _ownershipStore.PlanFrameworkOwnership(ownership, BuildOwnership(ownership, observations, plan));
        var ownershipChange = ownershipPlan.Change;
        var completion = new UpdatePlanCompletion
        {
            Request = request,
            Payload = payload,
            Intended = intended,
            Plan = plan,
            Effects = effects,
            OwnershipChange = ownershipChange,
            OwnershipState = ownershipPlan.State,
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
                ownership,
                observations,
                intended.ProjectionInputs,
                effects,
                ownershipChange));
    }

    private static FrameworkOwnership BuildOwnership(
        WorkspaceOwnershipRead ownership,
        IReadOnlyList<UpdateComparisonObservation> observations,
        UpdatePlanningPlan plan)
    {
        var existing = ownership.Document.Framework;
        var paths = (existing?.Paths ?? []).ToHashSet(StringComparer.Ordinal);
        var regions = (existing?.Regions ?? []).ToHashSet();
        var byIdentity = observations.ToDictionary(value => Identity(value.Comparison));
        foreach (var decision in plan.Decisions)
        {
            var comparison = decision.Comparison;
            var path = comparison.RelativePath;
            if (decision.Disposition == UpdatePlanningDisposition.Delete
                || plan.Authority.Prune && comparison.IntendedState == UpdateComparisonIntendedState.Retired
                    && comparison.CurrentState == UpdateComparisonCurrentState.Missing)
            {
                paths.Remove(path);
                regions.RemoveWhere(region => region.Path == path);
                continue;
            }
            if (decision.Disposition is not (UpdatePlanningDisposition.Create
                or UpdatePlanningDisposition.Replace or UpdatePlanningDisposition.Restore)) continue;
            if (comparison.Kind == UpdateComparisonTargetKind.File
                && path is not (FrameworkPayloadAsset.RootAgentPath or FrameworkPayloadAsset.RootClaudePath)) paths.Add(path);
            else if (comparison.Kind == UpdateComparisonTargetKind.GeneratedRegion)
                regions.Add(new OwnedRegion(path, "entries"));
            else regions.Add(new OwnedRegion(path, WorkspaceOwnershipDefinitions.ManagedBlockRegion));
            var observation = byIdentity[Identity(comparison)];
            if (path.StartsWith(".agents/", StringComparison.Ordinal) && observation.IntendedDocumentBytes is { } bytes
                && new MarkdownDocumentParser().Parse(Encoding.UTF8.GetString(bytes)).GeneratedRegion.State == MarkdownGeneratedRegionState.Complete)
                regions.Add(new OwnedRegion(path, "entries"));
        }
        return new FrameworkOwnership(existing?.Source ?? new OwnedSource("framework", null),
            [.. paths.Order(StringComparer.Ordinal)],
            [.. regions.OrderBy(region => region.Path, StringComparer.Ordinal).ThenBy(region => region.Region, StringComparer.Ordinal)]);
    }

    private static IReadOnlyList<UpdateFinding> ReadFindings(UpdatePlanningPlan plan)
    {
        var findings = new List<UpdateFinding>();
        foreach (var decision in plan.Decisions)
        {
            var comparison = decision.Comparison;
            if (decision.Disposition == UpdatePlanningDisposition.Preserve)
            {
                findings.Add(new UpdateFinding(
                    UpdateFindingCode.RetiredContentPreserved,
                    comparison.RelativePath,
                    "Update preserved retired managed content without --prune."));
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

    private static UpdatePlanResolution Boundary(UpdateResult preview)
        => new(
            new UpdatePlanBuild(plan: null, preview),
            Execution: null);

    private static (string Path, Models.Comparison.UpdateComparisonTargetKind Kind, string? Region) Identity(
        Models.Comparison.UpdateComparison comparison)
        => (comparison.RelativePath, comparison.Kind, comparison.RegionIdentity);
}
