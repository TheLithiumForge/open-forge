using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using static OpenForge.Cli.Core.Commands.Find.Shared.Rendering.FindHumanValues;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Rendering;

internal static class FindFindingHumanRenderer
{
    internal static void Add(ICollection<string> lines, FindResult result)
    {
        foreach (var finding in result.Findings)
        {
            lines.Add($"{CliHumanText.Status(finding.Status).ToUpperInvariant()}: {FindTextEscaping.Escape(finding.Cause)} [{FindDefinitions.ReadFindingCode(finding.Code)}]");
            if (finding.Subject is { } subject && subject != finding.Path)
            {
                lines.Add($"  Subject: {FindTextEscaping.Escape(subject)}");
            }
            var path = finding.Path ?? finding.Source?.Path;
            if (path is not null)
            {
                var location = finding.Location is { } coordinate ? $":{coordinate.Line}:{coordinate.Column}" : string.Empty;
                lines.Add($"  {FindTextEscaping.Escape(path)}{location}");
            }
            if (finding.Source is not null)
            {
                lines.Add($"  Source: {Identity(finding.Source)}");
            }
            if (finding.SelectorRole is not null)
            {
                lines.Add($"  Filter: {SelectorRole(finding.SelectorRole)} occurrence {Number(finding.SelectorOccurrence)}");
            }
            if (finding.Layer is not null)
            {
                lines.Add($"  Layer: {Layer(finding.Layer)}");
            }
            if (finding.Region is not null)
            {
                lines.Add($"  Region: {Region(finding.Region)}");
            }
            foreach (var candidate in finding.Candidates)
            {
                lines.Add($"  Candidate: {Identity(candidate)}");
            }
        }
    }
}
