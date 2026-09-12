using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using static OpenForge.Cli.Core.Commands.Context.Shared.Rendering.ContextHumanRenderingSupport;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Rendering;

internal static class ContextExpandedHumanRenderer
{
    internal static string Render(CliPresentationRequest<ContextResult> presentation)
    {
        var result = presentation.Result;
        var builder = new StringBuilder();
        ContextFramingHumanRenderer.AppendHeader(builder, presentation);
        if (IsPathsOnly(result))
        {
            RenderPaths(builder, result.Paths);
        }
        if (!IsPathsOnly(result))
        {
            foreach (var source in result.Sources)
            {
                RenderSource(builder, source);
            }
        }

        ContextFramingHumanRenderer.AppendLinks(builder, presentation);
        EnsureLineBoundary(builder);
        CliHumanText.AppendNext(builder, presentation);

        return builder.ToString();
    }

    private static void RenderPaths(StringBuilder builder, IReadOnlyList<ContextPathProjection> paths)
    {
        if (paths.Count == 0)
        {
            return;
        }

        builder.AppendLine();
        builder.AppendLine("Ordered paths");
        foreach (var path in paths)
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"{path.Position}  {ContextTextEscaping.Escape(path.Path)}");
            builder.AppendLine($$"""
                   ID: {{ContextTextEscaping.Escape(path.Id ?? "none")}}
                   Layer: {{Layer(path.Layer)}}
                """);
            foreach (var reason in path.InclusionReasons)
            {
                builder.AppendLine($"   Included because: {ContextTextEscaping.Escape(Reason(reason))}");
            }
        }
    }

    private static void RenderSource(StringBuilder builder, ContextSource source)
    {
        foreach (var layer in source.Layers)
        {
            EnsureBlankFramingLine(builder);
            if (layer.Kind == ContextSourceLayerKind.Overwrite && layer.Projections.Any(HasAuthoredText))
            {
                builder.AppendLine("=== Overwrite ===");
                builder.AppendLine();
            }

            builder.AppendLine(CultureInfo.InvariantCulture, $$"""
                Path: {{ContextTextEscaping.Escape(layer.Path)}}
                ID: {{ContextTextEscaping.Escape(source.Id ?? "none")}}
                Route: {{ContextTextEscaping.Escape(source.Route ?? "none")}}
                Scope: {{ContextTextEscaping.Escape(source.Scope ?? "none")}}
                Order: {{layer.PathPosition}}
                Layer: {{Layer(layer.Kind)}}
                """);
            foreach (var reason in layer.InclusionReasons)
            {
                builder.AppendLine($"Included because: {ContextTextEscaping.Escape(Reason(reason))}");
            }

            foreach (var projection in layer.Projections)
            {
                RenderProjection(builder, projection, layer.Kind);
            }
        }
    }

    private static void RenderProjection(
        StringBuilder builder,
        ContextProjection projection,
        ContextSourceLayerKind layer)
    {
        EnsureLineBoundary(builder);
        if (projection.Part == ContextProjectionPart.Headings)
        {
            builder.AppendLine("Headings:");
            foreach (var heading in projection.Headings)
            {
                var form = heading.Form == ContextHeadingForm.Atx ? "ATX" : "Setext";
                var canonical = heading.Canonical ? "canonical" : "noncanonical";
                builder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"{Layer(layer)}  line {heading.Location.Line}  level {heading.Level}  {form}  {canonical}  {heading.Text}");
            }

            return;
        }

        builder.AppendLine($"{ProjectionName(projection)}: {ProjectionState(projection.State)}");
        if (projection.Text is { } text)
        {
            builder.Append(text);
            EnsureLineBoundary(builder);
        }
    }

}
