using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

internal sealed partial class RouteUpdateNavigationPlanner
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal async ValueTask<RouteUpdateNavigationBuild> BuildAsync(
        RouteUpdateDestinationPlan destination,
        CancellationToken cancellationToken)
    {
        var observation = destination.Body.Metadata.Observation;
        var formation = new GeneratedNavigationFormationBuilder().Build(
            observation.Catalogue);
        var templateChangesEntrypoint = destination.Body.State == RouteUpdateBodyState.TemplateCopied
            && SourceFormClassifier.IsEntrypoint(observation.TargetSource.Base.Form);
        if (!observation.Request.Patch.Description.Requested
            && !observation.Request.Patch.Tags.Requested
            && !templateChangesEntrypoint)
        {
            return RouteUpdateNavigationBuild.Complete(
                new RouteUpdateNavigationPlan
                {
                    Formation = formation,
                    Regions = [],
                });
        }

        var targetNode = formation.Topology.FindByPath(
            observation.TargetSource.Identity.CanonicalBasePath);
        if (targetNode is null)
        {
            return Stop(
                destination,
                RouteUpdateFindingCode.ProjectionIncomplete,
                "The selected source is unavailable in intended route topology.");
        }

        var regionSources = new List<(SourceLogicalSource Source, bool IsTarget)>();
        if (targetNode.ParentState ==
            OpenForge.Cli.Core.Framework.Sources.Models.Routing.SourceRouteParentState.Resolved)
        {
            var parent = formation.FindSource(targetNode.ParentPaths[0])
                ?? throw new InvalidOperationException(
                    "A resolved Route Update parent must remain in formation.");
            regionSources.Add((parent, false));
        }
        else if (formation.Topology.LoaderRootPaths.Contains(
                     observation.TargetSource.Identity.CanonicalBasePath,
                     StringComparer.Ordinal)
                 && formation.Loader is { } loader)
        {
            regionSources.Add((loader, false));
        }
        else
        {
            return Stop(
                destination,
                RouteUpdateFindingCode.GeneratedRegionUnsafe,
                "The selected source has no unique generated-navigation parent.");
        }

        if (SourceFormClassifier.IsEntrypoint(observation.TargetSource.Base.Form)
            && targetNode.ChildPaths.Count != 0)
        {
            regionSources.Insert(0, (observation.TargetSource, true));
        }

        var reader = new SourceDocumentReader(
            observation.Request.Workspace);
        var regionInputs = new List<GeneratedNavigationRegionInput>();
        var snapshots = new Dictionary<string, FileStateSnapshot>(
            StringComparer.Ordinal);
        foreach (var (source, isTarget) in regionSources)
        {
            if (isTarget)
            {
                var text = StrictUtf8.GetString(destination.IntendedTargetBytes.AsSpan());
                regionInputs.Add(new GeneratedNavigationRegionInput(
                    source,
                    new MarkdownDocumentParser().Parse(text)));
                snapshots.Add(
                    source.Identity.CanonicalBasePath,
                    observation.TargetSnapshot);
                continue;
            }

            var read = await reader.ReadAsync(source.Base, cancellationToken)
                .ConfigureAwait(false);
            if (!TryReadText(read, out var regionText, out var finding))
            {
                return Stop(destination, finding.Code, finding.Cause, finding.Target);
            }

            regionInputs.Add(new GeneratedNavigationRegionInput(
                source,
                new MarkdownDocumentParser().Parse(regionText)));
            snapshots.Add(
                source.Identity.CanonicalBasePath,
                await new SourceDocumentSnapshotReader()
                    .ReadAsync(observation.Request.Workspace, read, cancellationToken)
                    .ConfigureAwait(false));
        }

        var metadata = await BuildMetadataAsync(
                destination,
                formation,
                regionSources.Select(region => region.Source),
                reader,
                cancellationToken)
            .ConfigureAwait(false);
        if (metadata.Finding is { } metadataFinding)
        {
            return Stop(
                destination,
                metadataFinding.Code,
                metadataFinding.Cause,
                metadataFinding.Target);
        }

        var projection = new GeneratedNavigationProjector().Project(
            new GeneratedNavigationProjectionRequest(
                formation,
                regionInputs,
                metadata.Values));
        var plans = ImmutableArray.CreateBuilder<RouteUpdateGeneratedRegionPlan>();
        foreach (var (source, isTarget) in regionSources)
        {
            var projected = projection.Regions.Single(candidate =>
                string.Equals(
                    candidate.Source.Identity.CanonicalBasePath,
                    source.Identity.CanonicalBasePath,
                    StringComparison.Ordinal));
            if (projected.State != GeneratedNavigationRegionState.Available
                || projected.Change is not { } change)
            {
                return Stop(
                    destination,
                    projected.UnavailableReason is GeneratedNavigationRegionUnavailableReason.MetadataUnavailable
                        or GeneratedNavigationRegionUnavailableReason.ProjectionUnavailable
                        ? RouteUpdateFindingCode.ProjectionIncomplete
                        : RouteUpdateFindingCode.GeneratedRegionUnsafe,
                    projected.Cause
                        ?? "Generated navigation projection is unavailable.",
                    projected.CanonicalPath);
            }

            plans.Add(new RouteUpdateGeneratedRegionPlan
            {
                Source = source,
                Snapshot = snapshots[source.Identity.CanonicalBasePath],
                Change = change,
                IsTarget = isTarget,
            });
        }

        return RouteUpdateNavigationBuild.Complete(
            new RouteUpdateNavigationPlan
            {
                Formation = formation,
                Regions = plans.ToImmutable(),
            });
    }
}
