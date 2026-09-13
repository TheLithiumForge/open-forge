using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Selection;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using static OpenForge.Cli.Core.Commands.References.Shared.Rendering.ReferencesHumanValues;

namespace OpenForge.Cli.Core.Commands.References.Shared.Rendering;

internal static class ReferencesHumanRenderer
{
    internal static string Render(CliPresentationRequest<ReferencesResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var expanded = presentation.Presentation.View == CliView.Expanded;
        var style = CliHumanStyle.For(presentation);
        var builder = new StringBuilder();
        var heading = result.Source is null ? "Direct links" : "Direct links for";
        CliHumanText.AppendHeader(builder, presentation, heading, result.Source is { } source ? Text(source.Id) : null);
        builder.AppendLine($"Source: {Text(result.Source?.Path ?? "unavailable")}");
        builder.AppendLine($"Direction: {(result.RequestedDirection is { } direction ? Direction(direction) : "unavailable")}");
        ReferencesFindingHumanRenderer.Append(builder, result.Findings, style);
        AppendSelection(builder, result.IncomingSelection, expanded);
        AppendSection(builder, "Incoming", result.Incoming, expanded, style);
        AppendSection(builder, "Outgoing", result.Outgoing, expanded, style);
        CliHumanText.AppendNext(builder, presentation);
        return builder.ToString().TrimEnd();
    }

    private static void AppendSelection(StringBuilder builder, ReferencesIncomingSelection? selection, bool expanded)
    {
        if (selection is null)
        {
            return;
        }

        builder.AppendLine(CultureInfo.InvariantCulture, $"Incoming scan: {Mode(selection.Mode)}; filters {selection.Supplied.Count}; inspected layers {selection.InspectedSources.Count}");
        if (!expanded)
        {
            return;
        }

        foreach (var selector in selection.Supplied)
        {
            builder.AppendLine($"  {Role(selector.Role)}: {Text(selector.Value)}");
        }
        foreach (var layer in selection.InspectedSources)
        {
            builder.AppendLine($"  Inspected: {Text(layer.Source.Id)}; {Layer(layer.Layer)}; {Text(layer.Path)}");
        }
    }

    private static void AppendSection(StringBuilder builder, string heading, ReferencesSection? section, bool expanded, CliHumanStyle style)
    {
        if (section is null)
        {
            return;
        }

        builder.AppendLine(CultureInfo.InvariantCulture, $"{style.Information(heading)} links: {section.OccurrenceCount}; coverage {Coverage(section.Coverage)}; status {style.Status(CliHumanText.Status(section.Status), section.Status)}");
        if (section.Occurrences.Count == 0)
        {
            builder.AppendLine(section.Coverage == ReferencesCoverage.Complete
                ? "  No direct links found."
                : "  No links established; inspection is not complete.");
        }
        foreach (var occurrence in section.Occurrences)
        {
            ReferencesOccurrenceHumanRenderer.Append(builder, occurrence, expanded);
        }
    }
}
