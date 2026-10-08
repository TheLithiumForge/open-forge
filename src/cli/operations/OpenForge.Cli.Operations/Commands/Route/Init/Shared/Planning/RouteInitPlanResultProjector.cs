using OpenForge.Cli.Core.Framework.Ownership.Models;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Settings;

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
                boundary.Code == RouteInitFindingCode.RecoveryConflict
                    ? boundary.FindingTarget
                    : boundary.FindingTarget ?? target?.Id ?? request.RouteTarget)]);
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
            inspection.SharingOwnership ?? inspection.Framework?.Trust.Ownership)
        {
            Settings = inspection.Settings,
            Restoration = inspection.Restoration,
            SourceSnapshots = [.. prospective.EffectSources.Select(content => content.Before)],
        };
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
            effects,
            inspection.Restoration);
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
                        segment.SourceAssetPath)))
                { IsCanonicalRestoration = framework.Alignment.IsCanonicalRestoration },
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
        RouteInitProspectiveEffectPlan effects,
        RouteInitRestoration? restoration)
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
            if (IsOwnershipPath(request, change.LogicalPath))
            {
                continue;
            }

            var canonicalPath = Relative(request, change.LogicalPath);
            var payloadFile = restoration?.Files.FirstOrDefault(file => file.Asset.Path == canonicalPath);
            if (payloadFile is not null || canonicalPath == WorkspaceSettingsDefinitions.RelativePath)
            {
                result.Add(new RouteInitEffect(canonicalPath,
                    payloadFile is null ? RouteInitEffectKind.Settings : RouteInitEffectKind.Payload,
                    change.Kind == PlannedFileChangeKind.Create ? RouteInitEffectAction.Create : RouteInitEffectAction.Replace,
                    payloadFile?.Asset.Path,
                    ReadRestorationTextChange(payloadFile, restoration, change),
                    RouteInitEffectOutcome.Planned, RouteInitEffectResidual.None));
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

    private static RouteInitEffectChange? ReadRestorationTextChange(
        RouteInitRestorationFile? payloadFile,
        RouteInitRestoration? restoration,
        PlannedFileChange change)
    {
        if (payloadFile is { Source: null })
        {
            return ReadPayloadTextChange(change);
        }

        string? before = null;
        if (payloadFile is null && restoration?.Settings.Snapshot is { HasBytes: true } snapshot)
        {
            before = StrictUtf8.GetString(snapshot.Bytes.AsSpan());
        }

        return new RouteInitEffectChange(before, StrictUtf8.GetString(change.IntendedBytes.AsSpan()));
    }

    private static RouteInitEffectChange? ReadPayloadTextChange(PlannedFileChange change)
    {
        try
        {
            return new RouteInitEffectChange(Before: null, StrictUtf8.GetString(change.IntendedBytes.AsSpan()));
        }
        catch (DecoderFallbackException)
        {
            return null;
        }
    }

    private static RouteInitLifecycleAction ReadLifecycleAction(
        RouteInitRequest request,
        RouteInitPlanFinalizationFacts finalization)
    {
        if (request.Scaffold != RouteInitScaffold.Framework)
        {
            return RouteInitLifecycleAction.None;
        }

        return finalization.OwnershipState switch
        {
            OwnershipWritePlanState.Planned => RouteInitLifecycleAction.Publish,
            OwnershipWritePlanState.Unchanged => RouteInitLifecycleAction.Preserve,
            _ => RouteInitLifecycleAction.None,
        };
    }

    private static bool IsOwnershipPath(
        RouteInitRequest request,
        string logicalPath)
        => PathComparer().Equals(
            logicalPath,
            SourceLogicalPath.ToLexicalPath(
                request.Workspace.LexicalRoot,
                WorkspaceOwnershipDefinitions.RelativePath));

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
