using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using static OpenForge.Cli.Core.Commands.Context.Shared.Rendering.ContextHumanRenderingSupport;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Rendering;

internal static class ContextCompactHumanRenderer
{
    private const int MaximumFindingSubjectLength = 240;

    internal static string Render(ContextResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"context {CliStatusDefinitions.Read(result.Status).MachineName} coverage={Coverage(result.Coverage.State)} sources={ReadCount(result)}");
        foreach (var path in result.Paths)
        {
            builder.AppendLine(path.Path);
        }

        if (!IsPathsOnly(result))
        {
            foreach (var source in result.Sources)
            {
                foreach (var layer in source.Layers)
                {
                    builder.AppendLine(
                        CultureInfo.InvariantCulture,
                        $"{layer.PathPosition} {source.Id ?? "none"} {layer.Path} {Layer(layer.Kind)}");
                    RenderProjections(builder, layer);
                }
            }
        }

        RenderLinks(builder, result.Links);
        RenderFindingsAndNext(builder, result);
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

    private static void RenderLinks(StringBuilder builder, IReadOnlyList<ContextLink> links)
    {
        foreach (var link in links.Where(link => link.Disposition == ContextLinkDisposition.Unresolved))
        {
            builder.AppendLine(
                CultureInfo.InvariantCulture,
                $"link depth={link.Depth} source={link.Source.Path} target={link.Target.Path ?? link.RawDestination} unresolved={LinkResolution(link.Target.Resolution)}");
        }
    }

    private static void RenderFindingsAndNext(StringBuilder builder, ContextResult result)
    {
        foreach (var finding in result.Findings)
        {
            builder.AppendLine(
                $"{ContextDefinitions.Read(finding.Code).Code} subject={FindingSubject(finding)}: {finding.Cause}");
        }

        if (result.Next is { } next)
        {
            builder.AppendLine($"Next: {next.Command} — {next.Reason}");
        }
    }

    private static string FindingSubject(ContextFinding finding)
    {
        var coordinate = finding.Subject ?? finding.Source?.Path ?? finding.Path;
        return coordinate is null
            ? "none"
            : ContextTextEscaping.Escape(coordinate, MaximumFindingSubjectLength);
    }
}
