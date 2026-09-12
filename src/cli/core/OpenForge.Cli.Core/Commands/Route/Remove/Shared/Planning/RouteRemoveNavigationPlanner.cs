using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal sealed partial class RouteRemoveNavigationPlanner(
    GeneratedNavigationFormationBuilder formationBuilder,
    GeneratedNavigationRegionPlanner regionPlanner,
    RouteRemoveNavigationSourceProjector sourceProjector)
{
    private readonly GeneratedNavigationFormationBuilder _formationBuilder = formationBuilder;
    private readonly GeneratedNavigationRegionPlanner _regionPlanner = regionPlanner;
    private readonly RouteRemoveNavigationSourceProjector _sourceProjector = sourceProjector;
    private readonly MarkdownDocumentParser _markdownParser = new();
    private readonly SourceAuthoredMetadataParser _metadataParser = new();

    internal RouteRemoveNavigationPlanningResult Build(RouteRemoveResolvedSubject subject)
    {
        ArgumentNullException.ThrowIfNull(subject);
        return Build(new RouteRemoveNavigationPlanningRequest
        {
            Subject = subject,
            IntendedSources = _sourceProjector.ProjectIntended(subject),
        });
    }

    private RouteRemoveNavigationPlanningResult Build(RouteRemoveNavigationPlanningRequest request)
    {
        var observed = request.Subject.Catalogue;
        var observedFormation = _formationBuilder.Build(observed);
        var formation = _formationBuilder.Build(observed, request.IntendedSources);
        if (formation.IntendedTargetCollisions.Count != 0 || formation.Ambiguities.Count != 0)
        {
            return Stop(
                request,
                RouteRemoveFindingCode.IdentityCollision,
                CliSemanticStatus.Blocked,
                request.Subject.Source.Path,
                "The intended Route Remove topology contains a colliding route identity or physical target.");
        }

        var selected = SelectRegions(request, observedFormation, formation);
        var documents = ReadDocuments(request, selected);
        if (documents.Boundary is { } documentBoundary)
        {
            return new RouteRemoveNavigationPlanningResult(plan: null, documentBoundary);
        }

        var metadata = ReadMetadata(request, formation);
        if (metadata.Boundary is { } metadataBoundary)
        {
            return new RouteRemoveNavigationPlanningResult(plan: null, metadataBoundary);
        }

        return Project(request, formation, selected, documents.Values, metadata.Values);
    }

    private RouteRemoveNavigationPlanningResult Project(
        RouteRemoveNavigationPlanningRequest request,
        GeneratedNavigationFormation formation,
        IReadOnlyList<SelectedRegion> selected,
        IReadOnlyList<RegionDocument> documents,
        IReadOnlyList<GeneratedNavigationMetadata> metadata)
    {
        var projectionRequest = new GeneratedNavigationProjectionRequest(
            formation,
            documents.Select(document => new GeneratedNavigationRegionInput(
                document.Region.Source,
                _markdownParser.Parse(document.Text))),
            metadata);
        var projected = documents.Select((document, index) => new ProjectedRegion(
                document,
                _regionPlanner.Plan(
                    projectionRequest,
                    projectionRequest.Regions[index])))
            .ToArray();
        var unavailable = projected.FirstOrDefault(region =>
            region.Projection.State != GeneratedNavigationRegionState.Available);
        if (unavailable is not null)
        {
            return Stop(
                request,
                RouteRemoveFindingCode.GeneratedRegionUnsafe,
                CliSemanticStatus.Blocked,
                unavailable.Projection.CanonicalPath,
                unavailable.Projection.Cause ?? "An applicable generated navigation region is unsafe.");
        }

        var changes = BuildChanges(request, projected);
        return new RouteRemoveNavigationPlanningResult(
            new RouteRemoveNavigationPlan
            {
                Request = request,
                GeneratedNavigation = new RouteRemoveGeneratedNavigation
                {
                    Coverage = RouteRemoveCoverage.Complete,
                    Regions = [.. selected.Select(region => ProjectRegion(region, projected))
                        .OrderBy(region => region.Path, StringComparer.Ordinal)],
                },
                FileChanges = [.. changes.Select(change => change.Change)],
                DocumentEdits = [.. changes.Select(change => change.Edit)],
            },
            boundary: null);
    }

    private static RouteRemoveGeneratedRegion ProjectRegion(
        SelectedRegion region,
        IReadOnlyList<ProjectedRegion> projected)
    {
        var value = projected.Single(item => string.Equals(
            item.Projection.CanonicalPath,
            region.Source.Identity.CanonicalBasePath,
            StringComparison.Ordinal));
        return new RouteRemoveGeneratedRegion
        {
            Path = region.Source.Identity.CanonicalBasePath,
            Reasons = region.Reasons,
            State = value.Projection.Change?.RequiresUpdate == true
                ? RouteRemoveGeneratedState.Changed
                : RouteRemoveGeneratedState.Unchanged,
        };
    }

    private static IReadOnlyList<NavigationChange> BuildChanges(
        RouteRemoveNavigationPlanningRequest request,
        IEnumerable<ProjectedRegion> projected)
    {
        var changes = new List<NavigationChange>();
        foreach (var region in projected.OrderBy(value => value.Projection.CanonicalPath, StringComparer.Ordinal))
        {
            if (region.Projection.Change is not { RequiresUpdate: true } change)
            {
                continue;
            }

            var logicalPath = Path.Combine(
                request.Subject.Request.Workspace.LexicalRoot,
                region.Projection.CanonicalPath.Replace('/', Path.DirectorySeparatorChar));
            changes.Add(new NavigationChange(
                PlannedFileChange.ReplaceGeneratedRegion(
                    region.Document.Snapshot.Expectation,
                    change.ExpectedDocumentBytes.AsSpan()),
                new RouteRemoveNavigationDocumentEdit
                {
                    Snapshot = region.Document.Snapshot,
                    LogicalPath = logicalPath,
                    Location = change.ContentLocation,
                    BeforeBytes = change.BeforeBodyBytes,
                    ExpectedBytes = change.ExpectedBodyBytes,
                }));
        }

        return changes;
    }

    private static RouteRemoveNavigationPlanningResult Stop(
        RouteRemoveNavigationPlanningRequest request,
        RouteRemoveFindingCode code,
        CliSemanticStatus status,
        string? target,
        string cause)
        => new(plan: null, StopFormation(request, code, status, target, cause));

    private static RouteRemoveResultFormation StopFormation(
        RouteRemoveNavigationPlanningRequest request,
        RouteRemoveFindingCode code,
        CliSemanticStatus status,
        string? target,
        string cause)
        => RouteRemoveBoundary.Stop(
            RouteRemoveBoundary.Start(request.Subject.Request) with
            {
                Source = request.Subject.Source,
                Subject = new RouteRemoveSubject { Kind = request.Subject.Kind },
            },
            code,
            status,
            target,
            cause);

    private sealed record RegionDocument(
        SelectedRegion Region,
        string Text,
        FileStateSnapshot Snapshot);

    private sealed record ProjectedRegion(
        RegionDocument Document,
        GeneratedNavigationRegion Projection);

    private sealed record NavigationChange(
        PlannedFileChange Change,
        RouteRemoveNavigationDocumentEdit Edit);
}
