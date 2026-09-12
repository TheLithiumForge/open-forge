using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.References;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;

internal sealed partial class RouteMoveNavigationPlanner(
    GeneratedNavigationFormationBuilder formationBuilder,
    GeneratedNavigationRegionPlanner regionPlanner,
    RouteMoveNavigationSourceProjector sourceProjector)
{
    private readonly GeneratedNavigationFormationBuilder _formationBuilder = formationBuilder;
    private readonly GeneratedNavigationRegionPlanner _regionPlanner = regionPlanner;
    private readonly RouteMoveNavigationSourceProjector _sourceProjector = sourceProjector;
    private readonly MarkdownDocumentParser _markdownParser = new();
    private readonly SourceAuthoredMetadataParser _metadataParser = new();

    internal RouteMoveNavigationPlanningResult Build(RouteMoveResolvedDestination destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        return Build(new RouteMoveNavigationPlanningRequest
        {
            Destination = destination,
            IntendedSources = _sourceProjector.ProjectIntended(destination),
        });
    }

    private RouteMoveNavigationPlanningResult Build(
        RouteMoveNavigationPlanningRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var observed = request.Destination.Inventory.Subject.Catalogue;
        var observedFormation = _formationBuilder.Build(observed);
        var formation = _formationBuilder.Build(observed, request.IntendedSources);
        var formationBoundary = ReadFormationBoundary(request, formation);
        if (formationBoundary is not null)
        {
            return new RouteMoveNavigationPlanningResult(plan: null, formationBoundary);
        }

        var selectedRegions = SelectRegions(request, observedFormation, formation);
        IReadOnlyDictionary<string, string>? movedPaths = null;
        var documents = ReadDocuments(request, selectedRegions, ref movedPaths);
        if (documents.Boundary is { } documentBoundary)
        {
            return new RouteMoveNavigationPlanningResult(plan: null, documentBoundary);
        }

        var metadata = ReadMetadata(request, formation, ref movedPaths);
        if (metadata.Boundary is { } metadataBoundary)
        {
            return new RouteMoveNavigationPlanningResult(plan: null, metadataBoundary);
        }

        return Project(request, formation, selectedRegions, documents.Values, metadata.Values);
    }

    private RouteMoveNavigationPlanningResult Project(
        RouteMoveNavigationPlanningRequest request,
        GeneratedNavigationFormation formation,
        IReadOnlyList<SelectedRegion> selectedRegions,
        IReadOnlyList<RegionDocument> documents,
        IReadOnlyList<GeneratedNavigationMetadata> metadata)
    {
        var projectionRequest = new GeneratedNavigationProjectionRequest(
            formation,
            documents.Select(document => new GeneratedNavigationRegionInput(
                document.Region.Source,
                _markdownParser.Parse(document.Text))),
            metadata);
        var regions = ProjectRegions(documents, projectionRequest);
        var unavailable = regions.FirstOrDefault(region =>
            region.Projection.State != GeneratedNavigationRegionState.Available);
        if (unavailable is not null)
        {
            return Stop(
                request,
                RouteMoveFindingCode.GeneratedRegionUnsafe,
                CliSemanticStatus.Blocked,
                unavailable.Projection.CanonicalPath,
                unavailable.Projection.Cause
                    ?? "An applicable generated navigation region is unsafe.");
        }

        return FormPlan(request, selectedRegions, regions);
    }

    private ProjectedRegion[] ProjectRegions(
        IEnumerable<RegionDocument> documents,
        GeneratedNavigationProjectionRequest request)
        => [.. documents.Select((document, index) => new ProjectedRegion(
            document,
            _regionPlanner.Plan(request, request.Regions[index])))];

    private static RouteMoveNavigationPlanningResult FormPlan(
        RouteMoveNavigationPlanningRequest request,
        IReadOnlyList<SelectedRegion> selectedRegions,
        IReadOnlyList<ProjectedRegion> regions)
    {
        var changes = BuildChanges(request, regions);
        return new RouteMoveNavigationPlanningResult(
            new RouteMoveNavigationPlan
            {
                Request = request,
                GeneratedNavigation = new RouteMoveGeneratedNavigation
                {
                    Coverage = RouteMoveCoverage.Complete,
                    Regions = [.. selectedRegions.Select(region => ProjectRegion(region, regions))
                        .OrderBy(region => region.Path, StringComparer.Ordinal)],
                },
                FileChanges = [.. changes.Select(change => change.Change)
                    .OfType<PlannedFileChange>()],
                DocumentEdits = [.. changes.Select(change => change.Edit)],
            },
            boundary: null);
    }

    private static RouteMoveGeneratedRegion ProjectRegion(
        SelectedRegion region,
        IReadOnlyList<ProjectedRegion> regions)
    {
        var projected = regions.Single(value => string.Equals(
            value.Projection.CanonicalPath,
            region.Source.Identity.CanonicalBasePath,
            StringComparison.Ordinal));
        return new RouteMoveGeneratedRegion
        {
            Path = region.Source.Identity.CanonicalBasePath,
            Reasons = region.Reasons,
            State = projected.Projection.Change?.RequiresUpdate == true
                ? RouteMoveGeneratedState.Changed
                : RouteMoveGeneratedState.Unchanged,
        };
    }

    private static IReadOnlyList<NavigationChange> BuildChanges(
        RouteMoveNavigationPlanningRequest request,
        IEnumerable<ProjectedRegion> regions)
    {
        var movedPath = request.Destination.Destination.Path;
        var changes = new List<NavigationChange>();
        foreach (var region in regions.OrderBy(value => value.Projection.CanonicalPath, StringComparer.Ordinal))
        {
            if (region.Projection.Change is not { RequiresUpdate: true } change)
            {
                continue;
            }

            var workspace = request.Destination.Inventory.Subject.Request.Workspace;
            var destinationLogicalPath = Path.Combine(
                workspace.LexicalRoot,
                region.Projection.CanonicalPath.Replace('/', Path.DirectorySeparatorChar));
            var edit = new RouteMoveNavigationDocumentEdit
            {
                Snapshot = region.Document.Snapshot,
                DestinationLogicalPath = destinationLogicalPath,
                Location = change.ContentLocation,
                BeforeBytes = change.BeforeBodyBytes,
                ExpectedBytes = change.ExpectedBodyBytes,
            };
            var fileChange = string.Equals(
                region.Projection.CanonicalPath,
                movedPath,
                StringComparison.Ordinal)
                ? null
                : PlannedFileChange.ReplaceGeneratedRegion(
                    region.Document.Snapshot.Expectation,
                    change.ExpectedDocumentBytes.AsSpan());
            changes.Add(new NavigationChange(fileChange, edit));
        }

        return changes;
    }

    private static RouteMoveResultFormation? ReadFormationBoundary(
        RouteMoveNavigationPlanningRequest request,
        GeneratedNavigationFormation formation)
    {
        if (formation.IntendedTargetCollisions.Count == 0 && formation.Ambiguities.Count == 0)
        {
            return null;
        }

        return StopFormation(
            request,
            RouteMoveFindingCode.IdentityCollision,
            CliSemanticStatus.Blocked,
            request.Destination.Destination.Path,
            "The intended Route Move topology contains a colliding route identity or physical target.");
    }

    private static RouteMoveNavigationPlanningResult Stop(
        RouteMoveNavigationPlanningRequest request,
        RouteMoveFindingCode code,
        CliSemanticStatus status,
        string? target,
        string cause)
        => new(plan: null, StopFormation(request, code, status, target, cause));

    private static RouteMoveResultFormation StopFormation(
        RouteMoveNavigationPlanningRequest request,
        RouteMoveFindingCode code,
        CliSemanticStatus status,
        string? target,
        string cause)
    {
        var destination = request.Destination;
        var formation = RouteMoveBoundary.Start(destination.Inventory.Subject.Request) with
        {
            Source = destination.Inventory.Subject.Source,
            Destination = destination.Destination,
            Subject = destination.Subject,
            Ownership = RouteMoveOwnershipProjector.Project(
                destination.Inventory.Ownership,
                destination.Inventory.Subject),
        };
        return RouteMoveBoundary.Stop(formation, code, status, target, cause);
    }

    private sealed record RegionDocument(
        SelectedRegion Region,
        string Text,
        FileStateSnapshot Snapshot);

    private sealed record ProjectedRegion(
        RegionDocument Document,
        GeneratedNavigationRegion Projection);

    private sealed record NavigationChange(
        PlannedFileChange? Change,
        RouteMoveNavigationDocumentEdit Edit);

}
