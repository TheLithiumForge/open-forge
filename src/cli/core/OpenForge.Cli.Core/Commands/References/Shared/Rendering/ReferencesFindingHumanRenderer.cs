using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using static OpenForge.Cli.Core.Commands.References.Shared.Rendering.ReferencesHumanValues;

namespace OpenForge.Cli.Core.Commands.References.Shared.Rendering;

internal static class ReferencesFindingHumanRenderer
{
    internal static void Append(StringBuilder builder, IReadOnlyList<ReferencesFinding> findings)
    {
        foreach (var finding in findings)
        {
            builder.AppendLine($"{CliHumanText.Status(finding.Status).ToUpperInvariant()}: {Text(finding.Cause)} [{ReferencesDefinitions.ReadMachineName(finding.Code)}]");
            if (finding.Subject is { } subject && subject != finding.Path)
            {
                builder.AppendLine($"  Subject: {Text(subject)}");
            }
            if (finding.Path is { } path)
            {
                builder.AppendLine($"  {Text(path)}{Location(finding.Location)}");
            }
            if (finding.Source is { } source)
            {
                builder.AppendLine($"  Source ID: {Text(source.Id)}");
                if (source.Path != finding.Path)
                {
                    builder.AppendLine($"  Source path: {Text(source.Path)}");
                }
            }
            if (finding.Layer is { } layer)
            {
                builder.AppendLine($"  Source: {Layer(layer)}");
            }
            if (finding.Direction is { } direction)
            {
                builder.AppendLine($"  Direction: {Direction(direction)}");
            }
            if (finding.SelectorRole is { } role)
            {
                builder.AppendLine(CultureInfo.InvariantCulture, $"  Filter: {Role(role)} occurrence {finding.SelectorOccurrence}");
            }
            if (finding.DestinationLocation is { } location)
            {
                builder.AppendLine($"  Destination written at: {Text(finding.Path ?? "unavailable")}{Location(location)}");
            }
            foreach (var candidate in finding.Candidates)
            {
                builder.AppendLine($"  Candidate: {Text(candidate.Id)}; {Text(candidate.Path)}");
            }
        }
    }
}
