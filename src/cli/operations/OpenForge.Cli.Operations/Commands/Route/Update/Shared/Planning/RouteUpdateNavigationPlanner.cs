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
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Commands.Shared.NavigationSharing;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

internal sealed partial class RouteUpdateNavigationPlanner
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal async ValueTask<RouteUpdateNavigationBuild> BuildAsync(
        RouteUpdateDestinationPlan destination,
        CancellationToken cancellationToken)
    {
        var observation = destination.Body.Metadata.Observation;
        var sharing = NavigationSharingReader.FromOwnership(observation.LayoutRead.Ownership);
        if (!sharing.IsAvailable || sharing.Sharing is not { } sharingPolicy)
            return Stop(destination, RouteUpdateFindingCode.GeneratedRegionUnsafe, sharing.Cause);
        var outside = RouteUpdateOutsideBoundary.Find(observation.LayoutRead, observation.TargetSource.Base.CanonicalPath);
        var formationBuilder = new GeneratedNavigationFormationBuilder();
        var formation = outside is null ? formationBuilder.Build(observation.Catalogue)
            : formationBuilder.Build(observation.Catalogue, observation.Catalogue.Sources.Where(source =>
                !source.Base.CanonicalPath.StartsWith(outside.DirectoryPath + "/", StringComparison.Ordinal)
                || !WorkspaceRemovals.IsPathRemoved(observation.Catalogue.Layout, source.Base.CanonicalPath, observation.LayoutRead.Settings.Document)
                    && sharingPolicy.Includes(source.Base.CanonicalPath)).ToArray());
        var readonlyInputs = observation.SupportingSnapshots.AddRange(destination.TemplateInputs);
        var templateChangesEntrypoint = destination.Body.State == RouteUpdateBodyState.TemplateCopied
            && SourceFormClassifier.IsEntrypoint(observation.TargetSource.Base.Form);
        if (destination.Body.Metadata.Patch.Description.State != RouteUpdatePatchState.Changed
            && destination.Body.Metadata.Patch.Tags.State != RouteUpdatePatchState.Changed
            && destination.Body.Metadata.Patch.ApplyTo.State != RouteUpdatePatchState.Changed
            && !templateChangesEntrypoint)
        {
            return RouteUpdateNavigationBuild.Complete(
                new RouteUpdateNavigationPlan
                {
                    SharingExpectation = sharing.Ownership.Snapshot?.Expectation,
                    Formation = formation,
                    Regions = [],
                    ReadonlySnapshots = readonlyInputs,
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
        var registeredAnchor = outside is not null && SourceFormClassifier.IsEntrypoint(observation.TargetSource.Base.Form)
            && SourceLogicalPath.ReadParent(observation.TargetSource.Base.CanonicalPath) == outside.DirectoryPath;
        if (targetNode.ParentState ==
            OpenForge.Cli.Core.Framework.Sources.Models.Routing.SourceRouteParentState.Resolved)
        {
            var parent = formation.FindSource(targetNode.ParentPaths[0])
                ?? throw new InvalidOperationException(
                    "A resolved Route Update parent must remain in formation.");
            regionSources.Add((parent, false));
        }
        else if (outside is null && formation.Topology.LoaderRootPaths.Contains(
                     observation.TargetSource.Identity.CanonicalBasePath,
                     StringComparer.Ordinal)
                 && formation.Loader is { } loader)
        {
            regionSources.Add((loader, false));
        }
        else if (!registeredAnchor)
        {
            return Stop(
                destination,
                RouteUpdateFindingCode.GeneratedRegionUnsafe,
                "The selected source has no unique generated-navigation parent.");
        }

        if (SourceFormClassifier.IsEntrypoint(observation.TargetSource.Base.Form)
            && (outside is null ? targetNode.ChildPaths.Count != 0 : templateChangesEntrypoint))
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

            if (outside is not null)
            {
                if (!source.Base.CanonicalPath.StartsWith(outside.DirectoryPath + "/", StringComparison.Ordinal))
                    return Stop(destination, RouteUpdateFindingCode.TargetUnsafe, "Outside navigation cannot leave its selected folder.", source.Base.CanonicalPath);
                var observed = await new RouteUpdateLayerObserver(new SourceDocumentSnapshotReader()).ObserveAsync(new()
                {
                    Workspace = observation.Request.Workspace,
                    Layer = source.Base,
                    LayoutRead = observation.LayoutRead,
                }, cancellationToken).ConfigureAwait(false);
                if (observed.Layer is not { } layer)
                    return Stop(destination, observed.FailureCode ?? RouteUpdateFindingCode.ProjectionIncomplete,
                        observed.FailureCause ?? "An outside navigation input is unavailable.", source.Base.CanonicalPath);
                regionInputs.Add(new(source, new MarkdownDocumentParser().Parse(layer.Text)));
                snapshots.Add(source.Base.CanonicalPath, layer.Snapshot);
                readonlyInputs = readonlyInputs.Add(layer.Snapshot);
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

        if (outside is not null)
        {
            foreach (var input in regionInputs)
            {
                var document = input.Document ?? throw new InvalidOperationException("An observed outside region requires its document.");
                var entries = SourceGeneratedEntriesParser.Parse(document);
                if (entries.State != SourceGeneratedEntriesState.Complete)
                    return Stop(destination, RouteUpdateFindingCode.GeneratedRegionUnsafe,
                        entries.Cause ?? "The outside Entries declarations are unavailable.", input.Source.Base.CanonicalPath);
            }
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
        readonlyInputs = readonlyInputs.AddRange(metadata.Snapshots);
        if (readonlyInputs.GroupBy(item => item.Expectation.LogicalPath, StringComparer.Ordinal)
            .Any(group => group.Any(item => item.Expectation != group.First().Expectation)))
            return Stop(destination, RouteUpdateFindingCode.TargetChanged, "An outside supporting input changed during planning.");

        var projection = new GeneratedNavigationProjector().Project(
            new GeneratedNavigationProjectionRequest(
                formation,
                regionInputs,
                metadata.Values,
                sharing.Sharing));
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
            if (change.RequiresUpdate && outside is not null
                && RouteUpdateOutsideBoundary.WriteBoundary(observation.LayoutRead, source.Base.CanonicalPath, entries: true, cancellationToken) is { } protectedRegion)
                return Stop(destination, protectedRegion.Code, protectedRegion.Cause, protectedRegion.Target);

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
                SharingExpectation = sharing.Ownership.Snapshot?.Expectation,
                Formation = formation,
                Regions = plans.ToImmutable(),
                ReadonlySnapshots = readonlyInputs.DistinctBy(item => item.Expectation.LogicalPath, StringComparer.Ordinal).ToImmutableArray(),
            });
    }
}
