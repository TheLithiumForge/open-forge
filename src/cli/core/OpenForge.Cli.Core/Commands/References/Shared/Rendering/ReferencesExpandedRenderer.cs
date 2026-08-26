using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Selection;
using OpenForge.Cli.Core.Commands.References.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.References.Shared.Rendering;

internal static partial class ReferencesExpandedRenderer
{
    internal static string Render(ReferencesResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        var builder = new StringBuilder();
        builder.AppendLine("References");
        builder.Append("Status: ");
        builder.AppendLine(Status(result.Status));
        builder.Append("Workspace: ");
        builder.AppendLine(result.Workspace is null ? "null" : Escape(result.Workspace.LexicalRoot));
        builder.Append("Source: ");
        builder.AppendLine(result.Source is null ? "null" : Source(result.Source));
        builder.Append("Direction: ");
        builder.AppendLine(result.RequestedDirection is null
            ? "null"
            : Direction(result.RequestedDirection.Value));
        AppendSelection(builder, result.IncomingSelection);
        AppendSection(builder, "Incoming", result.Incoming);
        AppendSection(builder, "Outgoing", result.Outgoing);
        AppendFindings(builder, result.Findings);
        if (result.Next is not null)
        {
            builder.Append("Next: ");
            builder.Append(Escape(result.Next.Command));
            builder.Append(" — ");
            builder.AppendLine(Escape(result.Next.Reason));
        }

        return builder.ToString().TrimEnd();
    }

    private static void AppendSelection(
        StringBuilder builder,
        ReferencesIncomingSelection? selection)
    {
        if (selection is null)
        {
            return;
        }

        builder.Append("Incoming selection: mode=");
        builder.AppendLine(selection.Mode == ReferencesSelectionMode.Default ? "default" : "filtered");
        if (selection.Supplied.Count != 0)
        {
            builder.AppendLine("  selectors:");
            foreach (var selector in selection.Supplied)
            {
                builder.Append("    ");
                builder.Append(Role(selector.Role));
                builder.Append(": ");
                builder.AppendLine(Escape(selector.Value));
            }
        }

        if (selection.InspectedSources.Count != 0)
        {
            builder.AppendLine("  inspected layers:");
            foreach (var layer in selection.InspectedSources)
            {
                builder.Append("    ");
                builder.Append(layer.Source.Id);
                builder.Append(" ");
                builder.Append(layer.Layer == SourceLayerKind.Base ? "base" : "overwrite");
                builder.Append(" ");
                builder.AppendLine(Escape(layer.Path));
            }
        }
    }

    private static void AppendSection(
        StringBuilder builder,
        string heading,
        ReferencesSection? section)
    {
        if (section is null)
        {
            return;
        }

        builder.Append(heading);
        builder.Append(" — coverage=");
        builder.Append(Coverage(section.Coverage));
        builder.Append(" status=");
        builder.Append(Status(section.Status));
        builder.Append(" occurrences=");
        builder.AppendLine(section.OccurrenceCount.ToString(CultureInfo.InvariantCulture));
        builder.Append("  Coverage: ");
        builder.AppendLine(Coverage(section.Coverage));
        builder.AppendLine("  Level 1");
        foreach (var occurrence in section.Occurrences)
        {
            AppendOccurrence(builder, occurrence);
        }
    }

    private static void AppendOccurrence(StringBuilder builder, ReferencesOccurrence occurrence)
    {
        builder.Append("    ");
        builder.Append(Escape(occurrence.Source.Path));
        builder.Append(FormatLocation(occurrence.Location));
        builder.Append(" -> ");
        builder.AppendLine(Escape(occurrence.RawDestination));
        builder.Append("      direction: ");
        builder.AppendLine(Direction(occurrence.Direction));
        builder.Append("      raw destination: ");
        builder.AppendLine(Escape(occurrence.RawDestination));
        builder.Append("      target kind: ");
        builder.AppendLine(TargetKind(occurrence.Target.Kind));
        builder.Append("      target: ");
        builder.AppendLine(TargetIdentity(occurrence.Target));
        builder.Append("      resolution: ");
        builder.AppendLine(Resolution(occurrence.Target.Resolution));
        builder.Append("      layer: ");
        builder.AppendLine(occurrence.Source.Layer == SourceLayerKind.Base ? "base" : "overwrite");
        builder.Append("      provenance: ");
        builder.AppendLine(Provenance(occurrence.Provenance));
        if (occurrence.Target.Network is not null)
        {
            builder.AppendLine("      network: network-not-attempted");
        }
        if (occurrence.DestinationLocation is not null)
        {
            builder.Append("      destination location: ");
            builder.AppendLine(FormatLocation(occurrence.DestinationLocation));
        }
    }

    private static void AppendFindings(
        StringBuilder builder,
        IReadOnlyList<ReferencesFinding> findings)
    {
        if (findings.Count == 0)
        {
            return;
        }

        builder.AppendLine("Findings:");
        foreach (var finding in findings)
        {
            builder.Append("  ");
            builder.Append(ReferencesDefinitions.ReadMachineName(finding.Code));
            builder.Append(": ");
            if (finding.Subject is not null)
            {
                builder.Append(Escape(finding.Subject));
                builder.Append(" — ");
            }
            builder.AppendLine(Escape(finding.Cause));
        }
    }

}
