using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using static OpenForge.Cli.Core.Commands.Find.Shared.Rendering.FindHumanValues;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Rendering;

internal static class FindContentHumanRenderer
{
    internal static void AddProjectionBlocks(
        ICollection<string> lines,
        IReadOnlyList<FindMatch> matches)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(matches);
        foreach (var match in matches)
        {
            if (match.Projections.Count == 0)
            {
                continue;
            }

            foreach (var projection in match.Projections)
            {
                AddProjection(lines, projection);
            }
        }
    }

    private static void AddProjection(
        ICollection<string> lines,
        FindProjection projection)
    {
        lines.Add($"Projection: {ProjectionPart(projection)}");
        lines.Add($"  Layer: {Layer(projection.Layer)}");
        lines.Add($"  Path: {Optional(projection.Path)}");
        lines.Add($"  State: {ProjectionState(projection.State)}");
        if (projection.State != FindProjectionState.Available)
        {
            return;
        }

        if (projection.Part == FindContentPartKind.Metadata)
        {
            AddMetadata(lines, projection.Metadata);
        }
        else if (projection.Part == FindContentPartKind.Headings)
        {
            AddHeadings(lines, projection.Headings);
        }
        else
        {
            AddText(lines, projection);
        }
    }

    private static void AddMetadata(
        ICollection<string> lines,
        FindMetadata? metadata)
    {
        if (metadata is null)
        {
            throw new InvalidOperationException("An available Find metadata projection requires metadata facts.");
        }

        lines.Add("  Metadata:");
        lines.Add($"    Position: {metadata.Position.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        lines.Add($"    ID: {FindTextEscaping.Escape(metadata.Id)}");
        lines.Add($"    Base path: {FindTextEscaping.Escape(metadata.Path)}");
        lines.Add($"    Route state: {RouteState(metadata.RouteState)}");
        lines.Add($"    Route: {Optional(metadata.Route)}");
        lines.Add("    Layers:");
        foreach (var layer in metadata.Layers)
        {
            lines.Add(
                $"      {Layer(layer.Kind)} frontmatter: {FindTextEscaping.Escape(layer.Path)}");
        }
    }

    private static void AddHeadings(
        ICollection<string> lines,
        IReadOnlyList<FindProjectedHeading> headings)
    {
        lines.Add("  Headings:");
        if (headings.Count == 0)
        {
            lines.Add("    none");
            return;
        }

        foreach (var heading in headings)
        {
            lines.Add(
                $"    - {FindTextEscaping.Escape(heading.Text)} "
                + $"(level {heading.Level}, {HeadingForm(heading.Form)}, "
                + $"{Canonical(heading.Canonical)}, {Location(heading.Location)})");
        }
    }

    private static void AddText(
        ICollection<string> lines,
        FindProjection projection)
    {
        if (projection.Text is null || projection.Location is null)
        {
            throw new InvalidOperationException("An available Find text projection requires text and location facts.");
        }

        lines.Add("  Text:");
        lines.Add(string.Concat(
            "<<<",
            Environment.NewLine,
            projection.Text,
            Environment.NewLine,
            ">>>"));
        lines.Add($"  Location: {Location(projection.Location)}");
    }

    private static string Location(SourceLocation? location)
        => location is null
            ? "none"
            : $"line {location.Line}, column {location.Column}";

    private static string ProjectionPart(FindProjection projection)
        => projection.Part switch
        {
            FindContentPartKind.Metadata => FindDefinitions.Metadata,
            FindContentPartKind.Frontmatter => FindDefinitions.Frontmatter,
            FindContentPartKind.Headings => FindDefinitions.Headings,
            FindContentPartKind.Body => FindDefinitions.Body,
            FindContentPartKind.Section when projection.Name is not null
                => $"{FindDefinitions.SectionPrefix}{FindTextEscaping.Escape(projection.Name)}",
            FindContentPartKind.Section => throw new InvalidOperationException(
                "A Find section projection requires a typed section name."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(projection),
                projection.Part,
                "The Find projection part is not defined."),
        };

    private static string ProjectionState(FindProjectionState state)
        => state switch
        {
            FindProjectionState.Available => "available",
            FindProjectionState.Missing => "missing",
            FindProjectionState.Unavailable => "unavailable",
            FindProjectionState.Ambiguous => "ambiguous",
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Find projection state is not defined."),
        };

    private static string RouteState(FindRouteState state)
        => state switch
        {
            FindRouteState.Routed => "routed",
            FindRouteState.Unrouted => "unrouted",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Find route state is not defined."),
        };

    private static string HeadingForm(MarkdownHeadingForm form)
        => form switch
        {
            MarkdownHeadingForm.Atx => "atx",
            MarkdownHeadingForm.Setext => "setext",
            _ => throw new ArgumentOutOfRangeException(
                nameof(form),
                form,
                "The Markdown heading form is not defined."),
        };

    private static string Canonical(bool canonical)
        => canonical ? "canonical" : "noncanonical";

}
