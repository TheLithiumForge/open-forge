using System.Text;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Library.Inspect.Shared.Rendering;

internal static class LibraryInspectPathsHumanRenderer
{
    internal static void Append(StringBuilder builder, CliPresentationRequest<LibraryInspectResult> presentation)
    {
        var payload = presentation.Result.Result;
        var eligible = payload.Source.EligiblePaths.Select(path => (path.SourcePath, path.DestinationPath, path.SourceId)).ToHashSet();
        var compared = payload.Projection.Comparisons.Select(path => (path.SourcePath, path.DestinationPath, path.SourceId)).ToHashSet();
        var registrations = new HashSet<LibraryRegisteredPath>();
        builder.AppendLine();
        builder.AppendLine("Links");
        foreach (var comparison in payload.Projection.Comparisons)
        {
            var key = (comparison.SourcePath, comparison.DestinationPath, comparison.SourceId);
            var membership = eligible.Contains(key) ? "eligible" : "not in the observed eligible inventory";
            var registration = comparison.Registered is null ? "not registered" : "registered";
            AppendPath(builder, key);
            builder.AppendLine($"    {LibraryHumanText.Relation(comparison.Relation)}; {registration}; {membership}");
            if (comparison.Registered is { } record)
            {
                registrations.Add(record);
                if ((record.SourcePath, record.DestinationPath, record.SourceId) != key)
                {
                    builder.AppendLine("    Registered identity:");
                    AppendPath(builder, (record.SourcePath, record.DestinationPath, record.SourceId));
                }
            }
            if (presentation.Presentation.View == CliView.Expanded)
            {
                builder.AppendLine($"""
                        Expected target: {LibraryHumanText.Value(comparison.Registered?.ExpectedRelativeLink)}
                        Observed target: {LibraryHumanText.Value(comparison.ObservedRelativeLink)}
                    """);
            }
        }
        var representedEligible = new HashSet<(string, string, string?)>(compared);
        foreach (var record in payload.Record.RegisteredPaths.Where(path => !registrations.Contains(path)))
        {
            AppendPath(builder, (record.SourcePath, record.DestinationPath, record.SourceId));
            var key = (record.SourcePath, record.DestinationPath, record.SourceId);
            var membership = eligible.Contains(key) ? "registered; eligible; not compared" : "registered; not compared";
            builder.AppendLine($"    {membership}");
            representedEligible.Add(key);
            if (presentation.Presentation.View == CliView.Expanded)
            {
                builder.AppendLine($"    Expected target: {LibraryHumanText.Value(record.ExpectedRelativeLink)}");
            }
        }
        foreach (var path in payload.Source.EligiblePaths.Where(path => !representedEligible.Contains((path.SourcePath, path.DestinationPath, path.SourceId))))
        {
            AppendPath(builder, (path.SourcePath, path.DestinationPath, path.SourceId));
            builder.AppendLine("    eligible; not compared");
        }
        if (payload.Projection.Comparisons.Length == 0 && payload.Record.RegisteredPaths.Length == 0 && payload.Source.EligiblePaths.Length == 0)
        {
            builder.AppendLine("  No paths observed.");
        }
    }

    private static void AppendPath(StringBuilder builder, (string Source, string Destination, string? Id) path)
        => builder.AppendLine($"  {LibraryHumanText.Value(path.Source)} -> {LibraryHumanText.Value(path.Destination)}; ID {LibraryHumanText.Value(path.Id)}");
}
