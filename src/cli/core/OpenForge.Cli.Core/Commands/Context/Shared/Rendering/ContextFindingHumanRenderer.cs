using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Rendering;

internal static class ContextFindingHumanRenderer
{
    internal static void Append(StringBuilder builder, IReadOnlyList<ContextFinding> findings, CliHumanStyle style)
    {
        foreach (var finding in findings)
        {
            builder.AppendLine($"{style.Finding(finding.Status)}: {Text(finding.Cause)} [{ContextDefinitions.Read(finding.Code).Code}]");
            var path = finding.Path ?? finding.Source?.Path;
            if (finding.Subject is { } subject && subject != path)
            {
                builder.AppendLine($"  Subject: {Text(subject)}");
            }
            if (path is not null)
            {
                var coordinate = finding.Location is { } location
                    ? string.Create(CultureInfo.InvariantCulture, $":{location.Line}:{location.Column}")
                    : string.Empty;
                builder.AppendLine($"  {Text(path)}{coordinate}");
            }
            if (finding.Source?.Id is { } id)
            {
                builder.AppendLine($"  Source ID: {Text(id)}");
            }
            if (finding.Reference is { } reference && reference != finding.Subject)
            {
                builder.AppendLine($"  Reference: {Text(reference)}");
            }
            if (finding.Part is { } part)
            {
                builder.AppendLine($"  Content: {Text(part.CanonicalValue)}");
            }
            foreach (var candidate in finding.Candidates)
            {
                builder.AppendLine($"  Candidate: {Text(candidate.Id ?? "unavailable")}; {Text(candidate.Path)}");
            }
        }
    }

    private static string Text(string value) => ContextTextEscaping.Escape(value);
}
