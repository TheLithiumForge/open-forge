using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Selection;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.References.Shared.Rendering;

internal static class ReferencesCompactRenderer
{
    internal static string Render(ReferencesResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        var builder = new StringBuilder();
        builder.Append("result=");
        builder.Append(Status(result.Status));
        builder.Append("  references=");
        builder.Append(TotalOccurrences(result).ToString(CultureInfo.InvariantCulture));
        builder.AppendLine();

        if (result.Source is not null)
        {
            builder.Append("Source: ");
            builder.Append(Escape(result.Source.Id));
            builder.Append("  ");
            builder.AppendLine(Escape(result.Source.Path));
        }

        builder.Append("Direction: ");
        builder.AppendLine(result.RequestedDirection is null
            ? "null"
            : Direction(result.RequestedDirection.Value));
        AppendSelection(builder, result.IncomingSelection);
        AppendSection(builder, "Incoming", result.Incoming);
        AppendSection(builder, "Outgoing", result.Outgoing);
        AppendFindings(builder, result.Findings);
        AppendNext(builder, result.Next);
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

        builder.Append("Incoming scan: ");
        builder.Append(selection.Mode == ReferencesSelectionMode.Default ? "default" : "filtered");
        builder.Append(" selectors=");
        builder.AppendLine(selection.Supplied.Count.ToString(CultureInfo.InvariantCulture));
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
        builder.Append("  status=");
        builder.Append(Status(section.Status));
        builder.Append("  occurrences=");
        builder.AppendLine(section.OccurrenceCount.ToString(CultureInfo.InvariantCulture));
        builder.AppendLine("  Level 1");
        foreach (var occurrence in section.Occurrences)
        {
            builder.Append("    ");
            builder.Append(Escape(occurrence.Source.Id ?? "none"));
            builder.Append("  ");
            builder.Append(Escape(occurrence.Source.Path));
            builder.Append(" -> ");
            builder.AppendLine(TargetText(occurrence));
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

    private static void AppendNext(StringBuilder builder, CliNextAction? next)
    {
        if (next is null)
        {
            return;
        }

        builder.Append("Next: ");
        builder.Append(Escape(next.Command));
        builder.Append(" — ");
        builder.AppendLine(Escape(next.Reason));
    }

    private static string TargetText(ReferencesOccurrence occurrence)
    {
        if (occurrence.Target.Kind == ReferencesTargetKind.External)
        {
            return Escape(occurrence.RawDestination);
        }

        if (occurrence.Target.Kind == ReferencesTargetKind.Unsupported)
        {
            return Escape(occurrence.RawDestination);
        }

        var id = occurrence.Target.Id ?? "none";
        var path = occurrence.Target.Path ?? "none";
        return $"{Escape(id)}  {Escape(path)}  raw={Escape(occurrence.RawDestination)}";
    }

    private static int TotalOccurrences(ReferencesResult result)
        => (result.Incoming?.OccurrenceCount ?? 0) + (result.Outgoing?.OccurrenceCount ?? 0);

    private static string Status(CliSemanticStatus status)
        => CliStatusDefinitions.Read(status).MachineName;

    private static string Direction(ReferencesDirection direction)
        => direction switch
        {
            ReferencesDirection.In => ReferencesDefinitions.In,
            ReferencesDirection.Out => ReferencesDefinitions.Out,
            ReferencesDirection.Both => ReferencesDefinitions.Both,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "The References direction is not defined."),
        };

    private static string Coverage(ReferencesCoverage coverage)
        => coverage switch
        {
            ReferencesCoverage.Complete => "complete",
            ReferencesCoverage.Incomplete => "incomplete",
            ReferencesCoverage.Blocked => "blocked",
            _ => throw new ArgumentOutOfRangeException(nameof(coverage), coverage, "The References coverage is not defined."),
        };

    private static string Escape(string value) => ReferencesTextEscaping.Escape(value);
}
