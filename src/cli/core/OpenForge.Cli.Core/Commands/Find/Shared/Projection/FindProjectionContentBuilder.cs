using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Projection;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Projection;

internal sealed class FindProjectionContentBuilder
{
    internal FindProjectionBuildFacts BuildMetadata(FindMetadataProjectionInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var source = input.Source;
        var metadataLayers = source.Layers
            .Select(layer => new FindMetadataLayer(layer.Kind, layer.Path))
            .ToArray();
        if (source.Route is null)
        {
            var findings = new List<FindFinding>();
            FindProjectionFindingPolicy.Add(
                findings,
                new FindProjectionFindingInput(
                    FindFindingCode.ProjectionUnavailable,
                    source.SourceIdentity,
                    null,
                    null,
                    FindContentPartKind.Metadata,
                    "The effective route facts do not establish metadata for the matched source."));
            return new FindProjectionBuildFacts(
                new FindProjection(
                    FindContentPartKind.Metadata,
                    null,
                    null,
                    null,
                    FindProjectionState.Unavailable,
                    null,
                    null,
                    [],
                    null),
                findings,
                true);
        }

        return new FindProjectionBuildFacts(
            new FindProjection(
                FindContentPartKind.Metadata,
                null,
                null,
                null,
                FindProjectionState.Available,
                new FindMetadata(
                    input.Position,
                    source.Match.Id,
                    source.Match.Path,
                    source.Route.State,
                    source.Route.Route,
                    metadataLayers),
                null,
                [],
                null),
            [],
            false);
    }

    internal FindProjectionBuildFacts BuildPhysical(FindPhysicalProjectionInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var layer = input.Layer;
        var part = input.Part;
        var inspection = layer.Inspection;
        if (inspection?.Document is not { } document || document.BodySpan is null)
        {
            var findings = new List<FindFinding>();
            if (!HasIncompleteInspectionFinding(inspection))
            {
                FindProjectionFindingPolicy.Add(
                    findings,
                    new FindProjectionFindingInput(
                        FindFindingCode.ProjectionUnavailable,
                        input.SourceIdentity,
                        layer,
                        null,
                        part.Kind,
                        "The cached inspection does not establish the requested projection."));
            }

            return new FindProjectionBuildFacts(
                Unavailable(part, layer),
                findings,
                true);
        }

        return part.Kind switch
        {
            FindContentPartKind.Frontmatter => new FindProjectionBuildFacts(
                BuildFrontmatterProjection(layer, part, document),
                [],
                false),
            FindContentPartKind.Headings => new FindProjectionBuildFacts(
                BuildHeadingsProjection(layer, part, document),
                [],
                false),
            FindContentPartKind.Body => new FindProjectionBuildFacts(
                BuildTextProjection(layer, part, document),
                [],
                false),
            FindContentPartKind.Section => BuildSectionProjection(input, document),
            _ => throw new ArgumentOutOfRangeException(
                nameof(part),
                part.Kind,
                "The Find projection part is not defined."),
        };
    }

    private static FindProjection BuildFrontmatterProjection(
        FindProjectionLayer layer,
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
        FindProjectionLayer layer,
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
        FindProjectionLayer layer,
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

    private static FindProjectionBuildFacts BuildSectionProjection(
        FindPhysicalProjectionInput input,
        MarkdownDocumentFacts document)
    {
        var matchingSections = document.Sections
            .Where(section => string.Equals(
                section.Heading.VisibleText,
                input.Part.Name,
                StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (matchingSections.Length == 0)
        {
            return new FindProjectionBuildFacts(
                new FindProjection(
                    input.Part.Kind,
                    input.Part.Name,
                    input.Layer.Kind,
                    input.Layer.Path,
                    FindProjectionState.Missing,
                    null,
                    null,
                    [],
                    null),
                [],
                false);
        }

        if (matchingSections.Length > 1)
        {
            var findings = new List<FindFinding>();
            FindProjectionFindingPolicy.Add(
                findings,
                new FindProjectionFindingInput(
                    FindFindingCode.SectionAmbiguous,
                    input.SourceIdentity,
                    input.Layer,
                    new FindRegion(FindRegionKind.Section, input.Part.Name, input.Part.CanonicalValue),
                    input.Part.Kind,
                    "Several headings establish the requested section in the same layer."));
            return new FindProjectionBuildFacts(
                new FindProjection(
                    input.Part.Kind,
                    input.Part.Name,
                    input.Layer.Kind,
                    input.Layer.Path,
                    FindProjectionState.Ambiguous,
                    null,
                    null,
                    [],
                    null),
                findings,
                true);
        }

        var sectionSpan = matchingSections[0].Span;
        return new FindProjectionBuildFacts(
            new FindProjection(
                input.Part.Kind,
                input.Part.Name,
                input.Layer.Kind,
                input.Layer.Path,
                FindProjectionState.Available,
                null,
                ReadText(document.Source, sectionSpan),
                [],
                MapLocation(document.Source, sectionSpan)),
            [],
            false);
    }

    private static FindProjection Unavailable(
        FindContentPart part,
        FindProjectionLayer layer)
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

    private static bool HasIncompleteInspectionFinding(FindLayerInspectionFacts? inspection)
        => inspection?.Findings.Any(finding => finding.Status == CliSemanticStatus.Incomplete) == true;

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
}
