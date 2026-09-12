using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Presentation;
using OpenForge.Cli.Core.Commands.References.Models.Result;

namespace OpenForge.Cli.Core.Commands.References.Shared.Rendering;

internal static partial class ReferencesJsonProjection
{
    internal static ReferencesCompactJsonResult CreateCompact(ReferencesResult result)
        => new()
        {
            Source = result.Source is null ? null : Source(result.Source),
            RequestedDirection = result.RequestedDirection is null
                    ? null
                    : Direction(result.RequestedDirection.Value),
            IncomingSelection = result.IncomingSelection is null
                    ? null
                    : IncomingSelection(result.IncomingSelection),
            Incoming = result.Incoming is null ? null : CompactSection(result.Incoming),
            Outgoing = result.Outgoing is null ? null : CompactSection(result.Outgoing),
            Findings = result.Findings.Select(Finding).ToArray(),
        };

    private static ReferencesCompactJsonSection CompactSection(ReferencesSection section)
        => new()
        {
            Coverage = section.Coverage switch
            {
                ReferencesCoverage.Complete => "complete",
                ReferencesCoverage.Incomplete => "incomplete",
                ReferencesCoverage.Blocked => "blocked",
                _ => throw new ArgumentOutOfRangeException(nameof(section), section.Coverage, "The section coverage is not defined."),
            },
            Status = Status(section.Status),
            OccurrenceCount = section.OccurrenceCount,
            Occurrences = section.Occurrences.Select(CompactOccurrence).ToArray(),
        };

    private static ReferencesCompactJsonOccurrence CompactOccurrence(ReferencesOccurrence occurrence)
        => new()
        {
            Direction = Direction(occurrence.Direction),
            Level = occurrence.Level,
            Source = new ReferencesJsonOccurrenceSource
            {
                Id = occurrence.Source.Id,
                Path = occurrence.Source.Path,
                Layer = Layer(occurrence.Source.Layer),
            },
            Location = new ReferencesCompactJsonLocation { Line = occurrence.Location.Line, Column = occurrence.Location.Column },
            RawDestination = occurrence.RawDestination,
            Fragment = occurrence.Fragment,
            Target = new ReferencesJsonTarget
            {
                Kind = occurrence.Target.Kind switch
                {
                    ReferencesTargetKind.Local => "local",
                    ReferencesTargetKind.External => "external",
                    ReferencesTargetKind.Unsupported => "unsupported",
                    _ => throw new ArgumentOutOfRangeException(nameof(occurrence), occurrence.Target.Kind, "The target kind is not defined."),
                },
                Id = occurrence.Target.Id,
                Path = occurrence.Target.Path,
                Layer = occurrence.Target.Layer is null ? null : Layer(occurrence.Target.Layer.Value),
                Resolution = Resolution(occurrence.Target.Resolution),
                Network = occurrence.Target.Network is null ? null : "network-not-attempted",
            },
        };
}
