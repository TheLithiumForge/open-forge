using System.Text;
using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Projection;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Projection;

internal sealed class FindProjectionBuilder
{
    internal FindProjectionFacts Build(FindProjectionInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var requestedParts = input.Request.Presentation.Content.Effective;
        if (requestedParts.Count == 0)
        {
            return new FindProjectionFacts([], [], FindProjectionCoverageState.NotRequested);
        }

        var projections = new List<FindProjection>();
        var findings = new List<FindFinding>();
        var hasUncertainProjection = false;
        var orderedMatches = input.Matches
            .OrderBy(match => match.Id, StringComparer.Ordinal)
            .ThenBy(match => match.Path, StringComparer.Ordinal)
            .ToArray();

        for (var matchIndex = 0; matchIndex < orderedMatches.Length; matchIndex++)
        {
            var match = orderedMatches[matchIndex];
            var projectionStart = projections.Count;
            var matchInspections = input.Inspections
                .Where(inspection => IsMatch(inspection, match))
                .OrderBy(inspection => ReadLayerRank(inspection.Layer.Kind))
                .ThenBy(inspection => inspection.Layer.CanonicalPath, StringComparer.Ordinal)
                .ToArray();
            var source = matchInspections.FirstOrDefault()?.Source;
            var layers = ReadLayers(match, source, matchInspections);
            var sourceIdentity = new FindSourceIdentity(match.Id, match.Path);

            if (requestedParts.Any(part => part.Kind == FindContentPartKind.Metadata))
            {
                projections.Add(BuildMetadataProjection(
                    match,
                    matchIndex + 1,
                    sourceIdentity,
                    source,
                    layers,
                    input.RouteFacts,
                    findings,
                    ref hasUncertainProjection));
            }

            foreach (var layer in layers)
            {
                foreach (var part in requestedParts.Where(part => part.Kind != FindContentPartKind.Metadata))
                {
                    projections.Add(BuildPhysicalProjection(
                        sourceIdentity,
                        layer,
                        part,
                        findings,
                        ref hasUncertainProjection));
                }
            }

            AddMissingSectionFindings(
                sourceIdentity,
                requestedParts,
                projections.Skip(projectionStart),
                findings);
        }

        var orderedFindings = findings
            .OrderBy(finding => finding.Code)
            .ThenBy(finding => finding.Source?.Id, StringComparer.Ordinal)
            .ThenBy(finding => finding.Source?.Path, StringComparer.Ordinal)
            .ThenBy(finding => ReadLayerRank(finding.Layer))
            .ThenBy(finding => finding.Region?.CanonicalValue, StringComparer.Ordinal)
            .ThenBy(finding => finding.Location?.ByteOffset ?? long.MaxValue)
            .ToArray();
        var coverage = hasUncertainProjection
            ? FindProjectionCoverageState.Incomplete
            : FindProjectionCoverageState.Complete;
        return new FindProjectionFacts(projections, orderedFindings, coverage);
    }

    private static FindProjection BuildMetadataProjection(
        FindMatch match,
        int position,
        FindSourceIdentity sourceIdentity,
        SourceLogicalSource? source,
        IReadOnlyList<ProjectionLayer> layers,
        SourceRouteFacts? routeFacts,
        ICollection<FindFinding> findings,
        ref bool hasUncertainProjection)
    {
        var route = ReadRoute(sourceIdentity, source, routeFacts);
        var metadataLayers = layers
            .Select(layer => new FindMetadataLayer(layer.Kind, layer.Path))
            .ToArray();
        if (route is null)
        {
            hasUncertainProjection = true;
            AddFinding(
                findings,
                FindFindingCode.ProjectionUnavailable,
                sourceIdentity,
                null,
                null,
                FindContentPartKind.Metadata,
                "The effective route facts do not establish metadata for the matched source.");
            return new FindProjection(
                FindContentPartKind.Metadata,
                null,
                null,
                null,
                FindProjectionState.Unavailable,
                null,
                null,
                [],
                null);
        }

        return new FindProjection(
            FindContentPartKind.Metadata,
            null,
            null,
            null,
            FindProjectionState.Available,
            new FindMetadata(
                position,
                match.Id,
                match.Path,
                    route.State,
                    route.Route,
                metadataLayers),
            null,
            [],
            null);
    }

    private static FindProjection BuildPhysicalProjection(
        FindSourceIdentity sourceIdentity,
        ProjectionLayer layer,
        FindContentPart part,
        ICollection<FindFinding> findings,
        ref bool hasUncertainProjection)
    {
        var inspection = layer.Inspection;
        if (inspection?.Document is not { } document || document.BodySpan is null)
        {
            hasUncertainProjection = true;
            if (!HasIncompleteInspectionFinding(inspection))
            {
                AddFinding(
                    findings,
                    FindFindingCode.ProjectionUnavailable,
                    sourceIdentity,
                    layer,
                    null,
                    part.Kind,
                    "The cached inspection does not establish the requested projection.");
            }

            return Unavailable(part, layer);
        }

        return part.Kind switch
        {
            FindContentPartKind.Frontmatter => BuildFrontmatterProjection(layer, part, document),
            FindContentPartKind.Headings => BuildHeadingsProjection(
                layer,
                part,
                document),
            FindContentPartKind.Body => BuildTextProjection(layer, part, document),
            FindContentPartKind.Section => BuildSectionProjection(
                sourceIdentity,
                layer,
                part,
                document,
                findings,
                ref hasUncertainProjection),
            _ => throw new ArgumentOutOfRangeException(nameof(part), part.Kind, "The Find projection part is not defined."),
        };
    }

    private static FindProjection BuildFrontmatterProjection(
        ProjectionLayer layer,
        FindContentPart part,
        MarkdownDocumentFacts document)
    {
        var boundary = document.Frontmatter;
        if (boundary.State == MarkdownFrontmatterState.Missing)
        {
            var emptySpan = new MarkdownTextSpan(0, 0);
            return new FindProjection(
                part.Kind,
                part.Name,
                layer.Kind,
                layer.Path,
                FindProjectionState.Available,
                null,
                string.Empty,
                [],
                MapLocation(document.Source, emptySpan));
        }

        if (boundary.State != MarkdownFrontmatterState.Complete || boundary.BlockSpan is not { } blockSpan)
        {
            return Unavailable(part, layer);
        }

        return new FindProjection(
            part.Kind,
            part.Name,
            layer.Kind,
            layer.Path,
            FindProjectionState.Available,
            null,
            ReadText(document.Source, blockSpan),
            [],
            MapLocation(document.Source, blockSpan));
    }

    private static FindProjection BuildHeadingsProjection(
        ProjectionLayer layer,
        FindContentPart part,
        MarkdownDocumentFacts document)
    {
        var headings = document.Headings
            .Select(heading => new FindProjectedHeading(
                heading.VisibleText,
                heading.Level,
                heading.Form,
                MapLocation(document.Source, heading.Span),
                heading.IsCanonical))
            .ToArray();
        return new FindProjection(
            part.Kind,
            part.Name,
            layer.Kind,
            layer.Path,
            FindProjectionState.Available,
            null,
            null,
            headings,
            null);
    }

    private static FindProjection BuildTextProjection(
        ProjectionLayer layer,
        FindContentPart part,
        MarkdownDocumentFacts document)
    {
        var establishedSpan = document.BodySpan
            ?? throw new InvalidOperationException("An established Markdown body requires a body span.");
        return new FindProjection(
            part.Kind,
            part.Name,
            layer.Kind,
            layer.Path,
            FindProjectionState.Available,
            null,
            ReadText(document.Source, establishedSpan),
            [],
            MapLocation(document.Source, establishedSpan));
    }

    private static FindProjection BuildSectionProjection(
        FindSourceIdentity sourceIdentity,
        ProjectionLayer layer,
        FindContentPart part,
        MarkdownDocumentFacts document,
        ICollection<FindFinding> findings,
        ref bool hasUncertainProjection)
    {
        var matchingSections = document.Sections
            .Where(section => string.Equals(
                section.Heading.VisibleText,
                part.Name,
                StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (matchingSections.Length == 0)
        {
            return new FindProjection(
                part.Kind,
                part.Name,
                layer.Kind,
                layer.Path,
                FindProjectionState.Missing,
                null,
                null,
                [],
                null);
        }

        if (matchingSections.Length > 1)
        {
            hasUncertainProjection = true;
            AddFinding(
                findings,
                FindFindingCode.SectionAmbiguous,
                sourceIdentity,
                layer,
                new FindRegion(FindRegionKind.Section, part.Name, part.CanonicalValue),
                part.Kind,
                "Several headings establish the requested section in the same layer.");
            return new FindProjection(
                part.Kind,
                part.Name,
                layer.Kind,
                layer.Path,
                FindProjectionState.Ambiguous,
                null,
                null,
                [],
                null);
        }

        var sectionSpan = matchingSections[0].Span;
        return new FindProjection(
            part.Kind,
            part.Name,
            layer.Kind,
            layer.Path,
            FindProjectionState.Available,
            null,
            ReadText(document.Source, sectionSpan),
            [],
            MapLocation(document.Source, sectionSpan));
    }

    private static void AddMissingSectionFindings(
        FindSourceIdentity sourceIdentity,
        IReadOnlyList<FindContentPart> requestedParts,
        IEnumerable<FindProjection> matchProjections,
        ICollection<FindFinding> findings)
    {
        var projections = matchProjections.ToArray();
        foreach (var part in requestedParts.Where(value => value.Kind == FindContentPartKind.Section))
        {
            var sections = projections
                .Where(projection => projection.Part == FindContentPartKind.Section
                    && string.Equals(projection.Name, part.Name, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (sections.Length == 0
                || sections.Any(projection => projection.State != FindProjectionState.Missing))
            {
                continue;
            }

            AddFinding(
                findings,
                FindFindingCode.ProjectionMissing,
                sourceIdentity,
                null,
                new FindRegion(FindRegionKind.Section, part.Name, part.CanonicalValue),
                part.Kind,
                "The requested section is absent from every completely inspected source layer.");
        }
    }

    private static FindProjection Unavailable(
        FindContentPart part,
        ProjectionLayer layer)
        => new(
            part.Kind,
            part.Name,
            layer.Kind,
            layer.Path,
            FindProjectionState.Unavailable,
            null,
            null,
            [],
            null);

    private static IReadOnlyList<ProjectionLayer> ReadLayers(
        FindMatch match,
        SourceLogicalSource? source,
        IReadOnlyList<FindLayerInspectionFacts> inspections)
    {
        var layers = new List<ProjectionLayer>
        {
            new(
                SourceLayerKind.Base,
                match.Path,
                ReadInspection(inspections, SourceLayerKind.Base, match.Path)),
        };
        var overwriteInspection = inspections.FirstOrDefault(inspection =>
            inspection.Layer.Kind == SourceLayerKind.Overwrite);
        if (source?.Overwrite is not null || overwriteInspection is not null)
        {
            layers.Add(new ProjectionLayer(
                SourceLayerKind.Overwrite,
                source?.Overwrite?.CanonicalPath
                    ?? overwriteInspection?.Layer.CanonicalPath
                    ?? ReadOverwritePath(match.Path),
                overwriteInspection));
        }

        return layers;
    }

    private static FindLayerInspectionFacts? ReadInspection(
        IReadOnlyList<FindLayerInspectionFacts> inspections,
        SourceLayerKind kind,
        string path)
        => inspections.FirstOrDefault(inspection =>
            inspection.Layer.Kind == kind
            && string.Equals(inspection.Layer.CanonicalPath, path, StringComparison.Ordinal));

    private static SourceRouteProjection? ReadRoute(
        FindSourceIdentity sourceIdentity,
        SourceLogicalSource? source,
        SourceRouteFacts? routeFacts)
    {
        if (source?.Base.Form == SourceDocumentForm.Loader)
        {
            return new SourceRouteProjection(FindRouteState.Unrouted, null);
        }

        var routeFact = routeFacts?.RouteFacts.FirstOrDefault(fact =>
            string.Equals(fact.Identity.AutomaticId, sourceIdentity.Id, StringComparison.Ordinal)
            && string.Equals(fact.Identity.CanonicalBasePath, sourceIdentity.Path, StringComparison.Ordinal));
        if (routeFact is null)
        {
            return null;
        }

        return routeFact.State switch
        {
            SourceRouteState.Routed when string.Equals(
                routeFact.Route,
                sourceIdentity.Id,
                StringComparison.Ordinal) => new SourceRouteProjection(FindRouteState.Routed, routeFact.Route),
            SourceRouteState.Unrouted => new SourceRouteProjection(FindRouteState.Unrouted, null),
            _ => null,
        };
    }

    private static bool IsMatch(
        FindLayerInspectionFacts inspection,
        FindMatch match)
        => string.Equals(inspection.Source.Identity.AutomaticId, match.Id, StringComparison.Ordinal)
            && string.Equals(inspection.Source.Identity.CanonicalBasePath, match.Path, StringComparison.Ordinal);

    private static bool HasIncompleteInspectionFinding(FindLayerInspectionFacts? inspection)
        => inspection?.Findings.Any(finding => finding.Status == CliSemanticStatus.Incomplete) == true;

    private static void AddFinding(
        ICollection<FindFinding> findings,
        FindFindingCode code,
        FindSourceIdentity source,
        ProjectionLayer? layer,
        FindRegion? region,
        FindContentPartKind part,
        string cause)
    {
        findings.Add(new FindFinding(
            code,
            FindDefinitions.ReadFindingStatus(code),
            part == FindContentPartKind.Section
                ? region?.Name
                : part.ToString().ToLowerInvariant(),
            cause,
            null,
            null,
            source,
            layer?.Kind,
            layer?.Path,
            region,
            null,
            []));
    }

    private static string ReadText(string source, MarkdownTextSpan span)
    {
        if (source.Length == 0)
        {
            return string.Empty;
        }

        return source.Substring(span.Start, span.Length);
    }

    private static SourceLocation MapLocation(string source, MarkdownTextSpan span)
        => new Utf8SourceMap(source).Map(span.Start, span.Length);

    private static int ReadLayerRank(SourceLayerKind? layer)
        => layer switch
        {
            SourceLayerKind.Base => 0,
            SourceLayerKind.Overwrite => 1,
            _ => 2,
        };

    private static string ReadOverwritePath(string basePath)
        => $"{basePath[..^3]}.overwrite.md";

    private sealed record ProjectionLayer(
        SourceLayerKind Kind,
        string Path,
        FindLayerInspectionFacts? Inspection);

    private sealed record SourceRouteProjection(
        FindRouteState State,
        string? Route);
}
