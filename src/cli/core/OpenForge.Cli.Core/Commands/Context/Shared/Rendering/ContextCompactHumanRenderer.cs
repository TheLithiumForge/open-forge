using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using static OpenForge.Cli.Core.Commands.Context.Shared.Rendering.ContextHumanRenderingSupport;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Rendering;

internal static class ContextCompactHumanRenderer
{
    internal static string Render(CliPresentationRequest<ContextResult> presentation)
    {
        var result = presentation.Result;
        var builder = new StringBuilder();
        ContextFramingHumanRenderer.AppendHeader(builder, presentation);
        if (IsPathsOnly(result))
        {
            foreach (var path in result.Paths)
            {
                builder.AppendLine(ContextTextEscaping.Escape(path.Path));
            }
        }

        if (!IsPathsOnly(result))
        {
            foreach (var source in result.Sources)
            {
                foreach (var layer in source.Layers)
                {
                    builder.AppendLine(
                        CultureInfo.InvariantCulture,
                        $"Source: {ContextTextEscaping.Escape(layer.Path)}; {ContextTextEscaping.Escape(source.Id ?? "unavailable")}; {Layer(layer.Kind)}; order {layer.PathPosition}");
                    RenderProjections(builder, layer);
                }
            }
        }

        ContextFramingHumanRenderer.AppendLinks(builder, presentation);
        CliHumanText.AppendNext(builder, presentation);
        return builder.ToString();
    }

    private static void RenderProjections(StringBuilder builder, ContextLayer layer)
    {
        if (layer.Kind == ContextSourceLayerKind.Overwrite && layer.Projections.Any(HasAuthoredText))
        {
            builder.AppendLine("=== Overwrite ===");
        }

        foreach (var projection in layer.Projections)
        {
            if (projection.Part == ContextProjectionPart.Headings)
            {
                foreach (var heading in projection.Headings)
                {
                    builder.AppendLine($"{new string('#', heading.Level)} {heading.Text}");
                }
            }
            else if (projection.Text is { } text)
            {
                builder.Append(text);
                EnsureLineBoundary(builder);
            }
            else if (projection.State != ContextProjectionState.Available)
            {
                builder.AppendLine($"[{ProjectionName(projection)}: {ProjectionState(projection.State)}]");
            }
        }
    }

}
