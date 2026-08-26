using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using static OpenForge.Cli.Core.Commands.Context.Shared.Rendering.ContextHumanRenderingSupport;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Rendering;

internal static class ContextExpandedHumanRenderer
{
    internal static string Render(ContextResult result)
    {
        var builder = new StringBuilder();
        var selectedBy = SelectedBy(result);
        builder.AppendLine($$"""
            Open Forge context
            Workspace: {{result.Workspace?.LexicalRoot ?? "unavailable"}}
            Selected by: {{selectedBy}}
            Result: {{CliStatusDefinitions.Read(result.Status).MachineName}}
            Coverage: {{Coverage(result.Coverage.State)}} (selection {{Coverage(result.Coverage.Selection)}}, links {{OptionalCoverage(result.Coverage.Links)}}, projection {{Coverage(result.Coverage.Projection)}})
            Startup context included: {{(result.Selection.StartupIncluded ? "yes" : "no (--additions-only)")}}
            Content: {{string.Join(',', result.Presentation.Content.Effective.Select(part => part.CanonicalValue))}}
            Follow links: {{LinkExpansion(result.Selection.LinkExpansion)}}
            Sources: {{ReadCount(result)}}
            """);
        foreach (var requested in result.Selection.RequestedSources)
        {
            builder.AppendLine($"Requested source: {requested.Supplied} -> {SourceResolution(requested.Resolution)}");
        }

        RenderPaths(builder, result.Paths);
        if (!IsPathsOnly(result))
        {
            foreach (var source in result.Sources)
            {
                RenderSource(builder, source);
            }
        }

        RenderLinks(builder, result.Links);
        RenderFindings(builder, result.Findings);
        if (result.Next is { } next)
        {
            EnsureLineBoundary(builder);
            builder.AppendLine($"Next: {next.Command} — {next.Reason}");
        }

        return builder.ToString();
    }

    private static string SelectedBy(ContextResult result)
    {
        if (result.Workspace is null)
        {
            return "unavailable";
        }

        return result.Workspace.SelectedBy == CliWorkspaceSelectionMethod.CurrentDirectory
            ? "current directory"
            : "explicit --workspace";
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
            builder.AppendLine(CultureInfo.InvariantCulture, $"{path.Position}  {path.Path}");
            builder.AppendLine($$"""
                   ID: {{path.Id ?? "none"}}
                   Layer: {{Layer(path.Layer)}}
                """);
            foreach (var reason in path.InclusionReasons)
            {
                builder.AppendLine($"   Included because: {Reason(reason)}");
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
                Path: {{layer.Path}}
                ID: {{source.Id ?? "none"}}
                Route: {{source.Route ?? "none"}}
                Scope: {{source.Scope ?? "none"}}
                Order: {{layer.PathPosition}}
                Layer: {{Layer(layer.Kind)}}
                """);
            foreach (var reason in layer.InclusionReasons)
            {
                builder.AppendLine($"Included because: {Reason(reason)}");
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

    private static void RenderLinks(StringBuilder builder, IReadOnlyList<ContextLink> links)
    {
        if (links.Count == 0)
        {
            return;
        }

        EnsureBlankFramingLine(builder);
        builder.AppendLine("Links");
        foreach (var link in links)
        {
            builder.AppendLine(
                CultureInfo.InvariantCulture,
                $"depth {link.Depth}  {link.Source.Path} -> {link.Target.Path ?? link.RawDestination}  {LinkDisposition(link.Disposition)}  {LinkResolution(link.Target.Resolution)}");
        }
    }

    private static void RenderFindings(StringBuilder builder, IReadOnlyList<ContextFinding> findings)
    {
        if (findings.Count == 0)
        {
            return;
        }

        EnsureBlankFramingLine(builder);
        builder.AppendLine("Findings");
        foreach (var finding in findings)
        {
            builder.AppendLine(
                $"{ContextDefinitions.Read(finding.Code).Code} [{CliStatusDefinitions.Read(finding.Status).MachineName}]: {finding.Cause}");
            if (finding.Subject is not null)
            {
                builder.AppendLine($"  Subject: {finding.Subject}");
            }

            if (finding.Path is not null)
            {
                builder.AppendLine($"  Path: {finding.Path}");
            }
        }
    }
}
