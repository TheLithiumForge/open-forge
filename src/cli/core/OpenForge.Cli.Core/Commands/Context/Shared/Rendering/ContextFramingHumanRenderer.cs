using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using static OpenForge.Cli.Core.Commands.Context.Shared.Rendering.ContextHumanRenderingSupport;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Rendering;

internal static class ContextFramingHumanRenderer
{
    internal static void AppendHeader(StringBuilder builder, CliPresentationRequest<ContextResult> presentation)
    {
        var result = presentation.Result;
        var style = CliHumanStyle.For(presentation);
        CliHumanText.AppendHeader(builder, presentation, "Context");
        builder.AppendLine($"Sources: {ReadCount(result)}; coverage {Coverage(result.Coverage.State)}");
        builder.AppendLine($"Content: {string.Join(", ", result.Presentation.Content.Effective.Select(part => ContextTextEscaping.Escape(part.CanonicalValue)))}");
        builder.AppendLine($"Startup context included: {(result.Selection.StartupIncluded ? "yes" : "no")}; additions only: {(result.Selection.AdditionsOnly ? "yes" : "no")}");
        builder.AppendLine($"Follow links: {LinkExpansion(result.Selection.LinkExpansion)}");
        if (presentation.Presentation.View == CliView.Expanded)
        {
            builder.AppendLine($"Checks: selection {Coverage(result.Coverage.Selection)}; links {OptionalCoverage(result.Coverage.Links)}; content {Coverage(result.Coverage.Projection)}");
        }
        foreach (var requested in result.Selection.RequestedSources)
        {
            var identity = requested.Source is { } source
                ? $"{ContextTextEscaping.Escape(source.Id ?? "unavailable")}; {ContextTextEscaping.Escape(source.Path)}"
                : "source unavailable";
            builder.AppendLine($"Requested: {ContextTextEscaping.Escape(requested.Supplied)} -> {SourceResolution(requested.Resolution)}; {identity}");
            foreach (var candidate in requested.Candidates)
            {
                builder.AppendLine($"  Candidate: {ContextTextEscaping.Escape(candidate.Id ?? "unavailable")}; {ContextTextEscaping.Escape(candidate.Path)}");
            }
        }
        ContextFindingHumanRenderer.Append(builder, result.Findings, style);
    }

    internal static void AppendLinks(StringBuilder builder, CliPresentationRequest<ContextResult> presentation)
    {
        foreach (var link in presentation.Result.Links)
        {
            if (presentation.Presentation.View == CliView.Compact && link.Disposition != ContextLinkDisposition.Unresolved)
            {
                continue;
            }
            builder.AppendLine(CultureInfo.InvariantCulture,
                $"Link: {ContextTextEscaping.Escape(link.Source.Path)}:{link.Location.Line}:{link.Location.Column} -> {ContextTextEscaping.Escape(link.Target.Path ?? link.RawDestination)}; {LinkResolution(link.Target.Resolution)}");
            if (presentation.Presentation.View == CliView.Expanded)
            {
                builder.AppendLine(CultureInfo.InvariantCulture, $"  Depth: {link.Depth}; {LinkDisposition(link.Disposition)}; source {Layer(link.Source.Layer)}");
                if (link.RawDestination != link.Target.Path)
                {
                    builder.AppendLine($"  Written as: {ContextTextEscaping.Escape(link.RawDestination)}");
                }
            }
        }
    }
}
