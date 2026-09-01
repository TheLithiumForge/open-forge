using System.Text;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

internal sealed class RouteInitPlanResultProjector
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    internal RouteInitPlanBuild ProjectStopped(
        RouteInitRequest request,
        RouteInitPlanningBoundary boundary)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(boundary);

        var target = boundary.Target;
        var targetFacts = new RouteInitTarget(
            boundary.Requested ?? target?.Requested ?? request.RouteTarget,
            target?.Id,
            target?.CanonicalPath);
        var formation = new RouteInitResultFormation(
            request.Workspace,
            request.Mode,
            request.Scaffold,
            targetFacts,
            new RouteInitPlanFacts(
                boundary.Incomplete ? RouteInitPlanCompleteness.Incomplete : RouteInitPlanCompleteness.Complete,
                boundary.Incomplete ? RouteInitPlanSafety.Safe : RouteInitPlanSafety.Blocked),
            boundary.Alignment is null || boundary.Payload is null
                ? null
                : new RouteInitFramework(
                    boundary.Payload.InventoryFingerprint,
                    boundary.Alignment.Segments.Select(segment => new RouteInitFrameworkSegment(
                        segment.IntendedSource.Identity.CanonicalBasePath,
                        segment.Role,
                        segment.SourceAssetPath))),
            entrypoints: [],
            effects: [],
            unchangedPaths: [],
            new RouteInitLifecycle(
                request.Scaffold == RouteInitScaffold.Framework
                    ? RouteInitLifecycleAction.Preserve
                    : RouteInitLifecycleAction.None,
                request.Scaffold == RouteInitScaffold.Framework
                    ? RouteInitLifecycleOutcome.NotStarted
                    : RouteInitLifecycleOutcome.NotRequested),
            new RouteInitRecovery(RouteInitRecoveryState.NotRequired, ResidualPath: null),
            RouteInitVerificationState.NotRequested,
            [new RouteInitFinding(
                boundary.Code,
                boundary.Cause,
                target?.Id ?? request.RouteTarget)]);
        return new RouteInitPlanBuild(Plan: null, formation);
    }

    internal RouteInitPlanBuild ProjectCompleted(
        RouteInitRequest request,
        RouteInitInspectionFacts inspection,
        RouteInitIntendedChain intended,
        RouteInitProspectivePlanFacts prospective,
        RouteInitPlanFinalizationFacts finalization)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(inspection);
        ArgumentNullException.ThrowIfNull(intended);
        ArgumentNullException.ThrowIfNull(prospective);
        ArgumentNullException.ThrowIfNull(finalization);

        var preview = BuildPreview(
            request,
            inspection,
            intended,
            prospective,
            finalization);
        var effects = finalization.Effects;
        var plan = new RouteInitPlan(
            request,
            preview,
            effects.DirectoryCreations,
            effects.FileChanges,
            effects.RecoveryTargets,
            finalization.IntendedLifecycle);
        return new RouteInitPlanBuild(plan, preview);
    }

    private static RouteInitResultFormation BuildPreview(
        RouteInitRequest request,
        RouteInitInspectionFacts inspection,
        RouteInitIntendedChain intended,
        RouteInitProspectivePlanFacts prospective,
        RouteInitPlanFinalizationFacts finalization)
    {
        var target = inspection.Target;
        var framework = inspection.Framework;
        var effects = finalization.Effects;
        var entrypoints = intended.Entries.Select(entry => new RouteInitEntrypoint(
            entry.Chain.Id,
            entry.Source.Identity.CanonicalBasePath,
            SourceFormClassifier.IsCompatibilityEntrypoint(entry.Source.Base.Form)
                ? RouteInitEntrypointForm.Compatibility
                : RouteInitEntrypointForm.Canonical,
            entry.Chain.IsMissing
                ? RouteInitEntrypointCurrent.Missing
                : RouteInitEntrypointCurrent.Existing,
            entry.Ownership,
            entry.Chain.IsMissing ? entry.Metadata : null,
            entry.SourceAssetPath,
            entry.Chain.IsMissing
                ? RouteInitEntrypointOutcome.Planned
                : RouteInitEntrypointOutcome.Unchanged)).ToArray();
        var resultEffects = BuildResultEffects(
            request,
            intended,
            prospective.Projection,
            effects);
        var unchanged = entrypoints
            .Where(entrypoint => entrypoint.Outcome == RouteInitEntrypointOutcome.Unchanged)
            .Select(entrypoint => entrypoint.Path)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var findings = entrypoints.Any(entrypoint =>
                entrypoint.Current == RouteInitEntrypointCurrent.Missing
                && entrypoint.Metadata?.Tags.Contains(
                    "NeedsAuthoring",
                    StringComparer.Ordinal) == true)
            ? new[]
            {
                new RouteInitFinding(
                    RouteInitFindingCode.NeedsAuthoring,
                    "One or more new Route Init entrypoints require authoring.",
                    target.Id),
            }
            : [];
        var lifecycleAction = ReadLifecycleAction(request, finalization);
        var lifecycleOutcome = lifecycleAction switch
        {
            RouteInitLifecycleAction.None => RouteInitLifecycleOutcome.NotRequested,
            RouteInitLifecycleAction.Preserve => RouteInitLifecycleOutcome.AlreadyCurrent,
            RouteInitLifecycleAction.Publish => RouteInitLifecycleOutcome.Planned,
            _ => throw new ArgumentOutOfRangeException(nameof(lifecycleAction)),
        };
        return new RouteInitResultFormation(
            request.Workspace,
            request.Mode,
            request.Scaffold,
            new RouteInitTarget(target.Requested, target.Id, target.CanonicalPath),
            new RouteInitPlanFacts(
                RouteInitPlanCompleteness.Complete,
                RouteInitPlanSafety.Safe),
            framework is null
                ? null
                : new RouteInitFramework(
                    framework.Payload.InventoryFingerprint,
                    framework.Alignment.Segments.Select(segment => new RouteInitFrameworkSegment(
                        segment.IntendedSource.Identity.CanonicalBasePath,
                        segment.Role,
                        segment.SourceAssetPath))),
            entrypoints,
            resultEffects,
            unchanged,
            new RouteInitLifecycle(lifecycleAction, lifecycleOutcome),
            new RouteInitRecovery(
                RouteInitRecoveryState.NotCreated,
                ResidualPath: null),
            RouteInitVerificationState.NotRequested,
            findings);
    }

    private static IReadOnlyList<RouteInitEffect> BuildResultEffects(
        RouteInitRequest request,
        RouteInitIntendedChain intended,
        GeneratedNavigationProjection projection,
        RouteInitProspectiveEffectPlan effects)
    {
        var result = new List<RouteInitEffect>();
        result.AddRange(effects.DirectoryCreations.Select(directory => new RouteInitEffect(
            Relative(request, directory.LogicalPath),
            RouteInitEffectKind.Directory,
            RouteInitEffectAction.Create,
            SourceAssetPath: null,
            Change: null,
            RouteInitEffectOutcome.Planned,
            RouteInitEffectResidual.None)));
        var intendedByLogical = intended.Entries.ToDictionary(
            entry => entry.Content.Before.LogicalPath,
            entry => entry,
            PathComparer());
        var regionsByLogical = projection.Regions.ToDictionary(
            region => region.PhysicalPath,
            region => region,
            PathComparer());
        foreach (var change in effects.FileChanges)
        {
            if (IsLifecyclePath(request, change.LogicalPath))
            {
                continue;
            }

            if (change.Kind == PlannedFileChangeKind.Create
                && intendedByLogical.TryGetValue(change.LogicalPath, out var entry))
            {
                result.Add(new RouteInitEffect(
                    entry.Source.Identity.CanonicalBasePath,
                    RouteInitEffectKind.Entrypoint,
                    RouteInitEffectAction.Create,
                    SourceAssetPath: null,
                    new RouteInitEffectChange(
                        Before: null,
                        StrictUtf8.GetString(change.IntendedBytes.AsSpan())),
                    RouteInitEffectOutcome.Planned,
                    RouteInitEffectResidual.None));
                continue;
            }

            var region = regionsByLogical.Values.FirstOrDefault(candidate =>
                PathComparer().Equals(
                    candidate.PhysicalPath,
                    change.Expectation.PhysicalPath));
            if (region?.Change is not { } bounded)
            {
                throw new InvalidOperationException(
                    "A generated Route Init replacement requires one bounded projected change.");
            }

            result.Add(new RouteInitEffect(
                region.CanonicalPath,
                RouteInitEffectKind.GeneratedRegion,
                RouteInitEffectAction.Replace,
                SourceAssetPath: null,
                new RouteInitEffectChange(bounded.BeforeBody, bounded.ExpectedBody),
                RouteInitEffectOutcome.Planned,
                RouteInitEffectResidual.None));
        }

        return result;
    }

    private static RouteInitLifecycleAction ReadLifecycleAction(
        RouteInitRequest request,
        RouteInitPlanFinalizationFacts finalization)
    {
        if (request.Scaffold != RouteInitScaffold.Framework)
        {
            return RouteInitLifecycleAction.None;
        }

        if (finalization.IntendedLifecycle is null
            || finalization.Effects.FileChanges.All(change =>
                !IsLifecyclePath(request, change.LogicalPath)))
        {
            return RouteInitLifecycleAction.Preserve;
        }

        return RouteInitLifecycleAction.Publish;
    }

    private static bool IsLifecyclePath(
        RouteInitRequest request,
        string logicalPath)
        => PathComparer().Equals(
            logicalPath,
            SourceLogicalPath.ToLexicalPath(
                request.Workspace.LexicalRoot,
                LifecycleSchema.RelativePath));

    private static string Relative(
        RouteInitRequest request,
        string logicalPath)
        => Path.GetRelativePath(request.Workspace.LexicalRoot, logicalPath)
            .Replace(Path.DirectorySeparatorChar, '/');

    private static StringComparer PathComparer()
        => OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
}
