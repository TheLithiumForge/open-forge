using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

internal sealed partial class RouteUpdatePlanProjector
{
    private static void AddTargetChange(
        RouteUpdateDestinationPlan destination,
        ImmutableArray<byte> intendedBytes,
        RouteUpdateGeneratedRegionPlan? targetRegion,
        ImmutableArray<PlannedFileChange>.Builder changes,
        ImmutableArray<RecoveryBundleTarget>.Builder targets,
        ImmutableArray<RouteUpdateEffect>.Builder effects)
    {
        var observation = destination.Body.Metadata.Observation;
        if (observation.TargetSnapshot.Bytes.AsSpan().SequenceEqual(intendedBytes.AsSpan()))
        {
            return;
        }

        var change = PlannedFileChange.Replace(
            observation.TargetSnapshot.Expectation,
            intendedBytes.AsSpan());
        changes.Add(change);
        targets.Add(RecoveryBundleTarget.Create(change, observation.TargetSnapshot));
        var preview = destination.Body.Metadata.Preview.AddRange(
            destination.Body.Preview);
        if (targetRegion?.Change.RequiresUpdate == true)
        {
            preview = preview.Add(GeneratedPreview(targetRegion));
        }

        effects.Add(Effect(
            observation.Target.Path
                ?? observation.TargetSource.Identity.CanonicalBasePath,
            RouteUpdateEffectKind.RoutedFile,
            observation.TargetSnapshot,
            intendedBytes,
            preview));
    }

    private static void AddGeneratedChange(
        RouteUpdateGeneratedRegionPlan region,
        ImmutableArray<PlannedFileChange>.Builder changes,
        ImmutableArray<RecoveryBundleTarget>.Builder targets,
        ImmutableArray<RouteUpdateEffect>.Builder effects)
    {
        if (!region.Change.RequiresUpdate)
        {
            return;
        }

        var change = PlannedFileChange.ReplaceGeneratedRegion(
            region.Snapshot.Expectation,
            region.Change.ExpectedDocumentBytes.AsSpan());
        changes.Add(change);
        targets.Add(RecoveryBundleTarget.Create(change, region.Snapshot));
        effects.Add(Effect(
            region.Source.Identity.CanonicalBasePath,
            RouteUpdateEffectKind.GeneratedRegion,
            region.Snapshot,
            region.Change.ExpectedDocumentBytes,
            [GeneratedPreview(region)]));
    }

    private static RouteUpdateEffect Effect(
        string path,
        RouteUpdateEffectKind kind,
        FileStateSnapshot before,
        ImmutableArray<byte> intendedBytes,
        ImmutableArray<RouteUpdatePreviewHunk> preview)
        => new()
        {
            Path = path,
            Kind = kind,
            Action = RouteUpdateEffectAction.Replace,
            Change = new RouteUpdateEffectChange
            {
                Before = before.ContentHash
                    ?? throw new InvalidOperationException(
                        "A Route Update replacement requires one observed content hash."),
                Expected = FileExpectation.Hash(intendedBytes.AsSpan()),
            },
            Preview = preview,
            Outcome = RouteUpdateEffectOutcome.Planned,
            Residual = RouteUpdateEffectResidual.None,
        };

    private static RouteUpdatePreviewHunk GeneratedPreview(
        RouteUpdateGeneratedRegionPlan region)
        => new()
        {
            Kind = RouteUpdatePreviewKind.GeneratedRegion,
            Before = region.Change.BeforeBody,
            Expected = region.Change.ExpectedBody,
        };

    private static IEnumerable<string> ReadCandidatePaths(
        RouteUpdateObservation observation,
        RouteUpdateNavigationPlan navigation)
    {
        yield return observation.TargetSource.Identity.CanonicalBasePath;
        if (observation.TargetSource.Overwrite is { } overwrite)
        {
            yield return overwrite.CanonicalPath;
        }

        foreach (var region in navigation.Regions)
        {
            yield return region.Source.Identity.CanonicalBasePath;
        }

        var node = navigation.Formation.Topology.FindByPath(
            observation.TargetSource.Identity.CanonicalBasePath);
        if (node?.ParentState == Framework.Sources.Models.Routing.SourceRouteParentState.Resolved)
        {
            yield return node.ParentPaths[0];
            yield break;
        }

        var directory = SourceLogicalPath.ReadParent(
            observation.TargetSource.Identity.CanonicalBasePath);
        if (!string.Equals(directory, SourceLogicalPath.AgentsRoot, StringComparison.Ordinal))
        {
            var name = SourceLogicalPath.ReadFileName(directory);
            yield return $"{directory}/_{name}.md";
        }
    }
}
