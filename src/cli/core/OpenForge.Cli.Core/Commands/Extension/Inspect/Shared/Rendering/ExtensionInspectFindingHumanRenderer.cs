using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Rendering;

internal static class ExtensionInspectFindingHumanRenderer
{
    internal static void Append(StringBuilder builder, IEnumerable<ExtensionInspectFinding> findings, CliHumanStyle style)
    {
        var displayedCandidates = new List<IReadOnlyList<ExtensionInspectCandidate>>();
        foreach (var finding in findings)
        {
            ExtensionHumanText.AppendFinding(builder, style.Finding(finding.Status), ExtensionInspectDefinitions.ReadFindingCode(finding.Code), finding.Cause, null);
            if (finding.Subject is { } subject && subject != finding.Path && subject != finding.PackageId)
            {
                builder.AppendLine($"  Subject: {ExtensionHumanText.Value(subject)}");
            }

            if (finding.PackageId is { } package)
            {
                builder.AppendLine($"  Package: {ExtensionHumanText.Value(package)}");
            }

            if (finding.Dependency is { } dependency)
            {
                builder.AppendLine($"  Dependency: {ExtensionHumanText.Value(dependency)}");
            }

            if (finding.Location is { } location)
            {
                builder.AppendLine(CultureInfo.InvariantCulture, $"  Line {location.Line}, column {location.Column}");
            }

            if (finding.Candidates.Count > 0 && !displayedCandidates.Any(prior => prior.SequenceEqual(finding.Candidates)))
            {
                AppendCandidates(builder, finding.Candidates);
                displayedCandidates.Add(finding.Candidates);
            }
        }
    }

    internal static void AppendCandidates(StringBuilder builder, IReadOnlyList<ExtensionInspectCandidate> candidates)
    {
        if (candidates.Count == 0)
        {
            return;
        }

        builder.AppendLine("Package matches:");
        foreach (var candidate in candidates)
        {
            builder.AppendLine($"  {ExtensionHumanText.Value(candidate.Id)}: {ExtensionHumanText.Value(candidate.Path)}");
        }
    }
}
