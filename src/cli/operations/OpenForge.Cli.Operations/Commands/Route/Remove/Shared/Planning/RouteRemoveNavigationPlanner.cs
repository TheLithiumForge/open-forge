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
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;

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

    internal RouteRemoveNavigationPlanningResult Build(RouteRemoveResolvedSubject subject, SourceSharing? sharing = null,
        RouteRemoveReferencePlan? references = null)
    {
        ArgumentNullException.ThrowIfNull(subject);
        return Build(new RouteRemoveNavigationPlanningRequest
        {
            Subject = subject,
            Sharing = sharing ?? new SourceSharing([]),
            IntendedSources = _sourceProjector.ProjectIntended(subject),
            RowDocuments = references is null ? [] : [.. references.Documents.Where(document => !document.RowEdits.IsEmpty)],
        });
    }

    private RouteRemoveNavigationPlanningResult Build(RouteRemoveNavigationPlanningRequest request)
    {
        var observed = request.Subject.Catalogue;
        if (request.Subject.OutsideRoute is { } outside)
        {
            var deleted = RouteRemoveAbsenceScopeForSelection(request.Subject);
            foreach (var row in observed.IncludedRows.Where(row => deleted(row.Path)
                || row.AliasOf is { } alias && deleted(alias)))
                if (!RouteRemoveOutsideBoundaryInspector.Contains(outside.DirectoryPath, row.ParentPath))
                    return Stop(request, RouteRemoveFindingCode.ReferenceUnsafe, CliSemanticStatus.Blocked, row.ParentPath,
                        "A required authored row removal would change content outside the selected registration.");
            request = request with
            {
                IntendedSources = [.. request.IntendedSources.Where(source =>
                    RouteRemoveOutsideBoundaryInspector.Contains(outside.DirectoryPath, source.Base.CanonicalPath))],
            };
        }
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
        if (request.Subject.OutsideRoute is not null)
            foreach (var region in selected)
                if (RouteRemoveOutsideBoundaryInspector.ValidateEffect(request.Subject, region.Source.Base.CanonicalPath, CancellationToken.None) is { } cause)
                    return Stop(request, RouteRemoveFindingCode.ReferenceUnsafe, CliSemanticStatus.Blocked, region.Source.Base.CanonicalPath, cause);
        var documents = ReadDocuments(request, selected);
        if (documents.Boundary is { } documentBoundary)
        {
            return new RouteRemoveNavigationPlanningResult(plan: null, documentBoundary);
        }

        var metadata = ReadMetadata(request, formation, selected);
        if (metadata.Boundary is { } metadataBoundary)
        {
            return new RouteRemoveNavigationPlanningResult(plan: null, metadataBoundary);
        }

        return Project(request, formation, selected, documents.Values, metadata.Values, metadata.Inputs);
    }

    private RouteRemoveNavigationPlanningResult Project(
        RouteRemoveNavigationPlanningRequest request,
        GeneratedNavigationFormation formation,
        IReadOnlyList<SelectedRegion> selected,
        IReadOnlyList<RegionDocument> documents,
        IReadOnlyList<GeneratedNavigationMetadata> metadata,
        IReadOnlyList<FileStateSnapshot> inputs)
    {
        var projectionRequest = new GeneratedNavigationProjectionRequest(
            formation,
            documents.Select(document => new GeneratedNavigationRegionInput(
                document.Region.Source,
                _markdownParser.Parse(document.Text))),
            metadata,
            request.Sharing);
        var projected = documents.Select((document, index) => new ProjectedRegion(
                document,
                request.Subject.OutsideRoute is not null
                    ? ProjectAuthoredRows(document)
                    : _regionPlanner.Plan(projectionRequest, projectionRequest.Regions[index])))
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
                ReadonlyInputs = request.Subject.OutsideRoute is null ? [] : [.. inputs],
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
            State = value.Projection.Change is { } change
                && !value.Document.Snapshot.Bytes.AsSpan().SequenceEqual(change.ExpectedDocumentBytes.AsSpan())
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
            if (region.Projection.Change is not { } change
                || region.Document.Snapshot.Bytes.AsSpan().SequenceEqual(change.ExpectedDocumentBytes.AsSpan()))
            {
                continue;
            }

            var logicalPath = Path.Combine(
                request.Subject.Request.Workspace.LexicalRoot,
                region.Projection.CanonicalPath.Replace('/', Path.DirectorySeparatorChar));
            var location = change.ContentLocation;
            var before = change.BeforeBodyBytes;
            if (request.Subject.OutsideRoute is not null)
            {
                var original = StrictUtf8.GetString(region.Document.Snapshot.Bytes.AsSpan());
                if (!original.StartsWith(change.Prefix, StringComparison.Ordinal)
                    || !original.EndsWith(change.Suffix, StringComparison.Ordinal))
                    throw new InvalidOperationException("An outside Entries change must retain its original surrounding bytes.");
                location = new Utf8SourceMap(original).Map(change.Prefix.Length,
                    original.Length - change.Prefix.Length - change.Suffix.Length);
                before = [.. region.Document.Snapshot.Bytes.AsSpan().Slice(checked((int)location.ByteOffset), checked((int)location.ByteLength)).ToArray()];
            }
            changes.Add(new NavigationChange(
                PlannedFileChange.ReplaceGeneratedRegion(
                    region.Document.Snapshot.Expectation,
                    change.ExpectedDocumentBytes.AsSpan()),
                new RouteRemoveNavigationDocumentEdit
                {
                    Snapshot = region.Document.Snapshot,
                    LogicalPath = logicalPath,
                    Location = location,
                    BeforeBytes = before,
                    ExpectedBytes = change.ExpectedBodyBytes,
                }));
        }

        return changes;
    }

    private static Func<string, bool> RouteRemoveAbsenceScopeForSelection(RouteRemoveResolvedSubject subject)
        => subject.Kind == RouteRemoveSubjectKind.Category
            ? path => RouteRemoveOutsideBoundaryInspector.Contains(SourceLogicalPath.ReadParent(subject.SelectedSource.Base.CanonicalPath), path)
            : path => path == subject.SelectedSource.Base.CanonicalPath || path == subject.SelectedSource.Overwrite?.CanonicalPath;

    private static GeneratedNavigationRegion ProjectAuthoredRows(RegionDocument document)
    {
        var original = StrictUtf8.GetString(document.Snapshot.Bytes.AsSpan());
        var facts = new MarkdownDocumentParser().Parse(original);
        if ((!document.Region.Source.IsIncludedCatalogue && !GeneratedNavigationRegionPlanner.IsRegionSource(document.Region.Source))
            || SourceGeneratedEntriesParser.Parse(facts).State != SourceGeneratedEntriesState.Complete
            || facts.GeneratedRegion.EntriesBlock is not { } block)
            return GeneratedNavigationRegion.Unavailable(document.Region.Source,
                GeneratedNavigationRegionUnavailableReason.GeneratedRegionUnavailable,
                "An authored outside row host requires one complete retained Entries section.");
        var content = block.Span;
        var prefix = original[..content.Start];
        var suffix = original[content.End..];
        if (!document.Text.StartsWith(prefix, StringComparison.Ordinal) || !document.Text.EndsWith(suffix, StringComparison.Ordinal))
            return GeneratedNavigationRegion.Unavailable(document.Region.Source,
                GeneratedNavigationRegionUnavailableReason.ProjectionUnavailable,
                "An authored outside row edit changed bytes beyond its retained Entries section.");
        return GeneratedNavigationRegion.Available(document.Region.Source, [], new GeneratedNavigationBoundedChange(new()
        {
            ContentLocation = new Utf8SourceMap(original).Map(content.Start, content.Length),
            BeforeBody = original[content.Start..content.End],
            ExpectedBody = document.Text[prefix.Length..(document.Text.Length - suffix.Length)],
            Prefix = prefix,
            Suffix = suffix,
        }));
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
