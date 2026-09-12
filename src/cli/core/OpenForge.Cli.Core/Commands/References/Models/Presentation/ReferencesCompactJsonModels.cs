namespace OpenForge.Cli.Core.Commands.References.Models.Presentation;

internal sealed class ReferencesCompactJsonResult
{
    public required ReferencesJsonSource? Source { get; init; }

    public required string? RequestedDirection { get; init; }

    public required ReferencesJsonIncomingSelection? IncomingSelection { get; init; }

    public required ReferencesCompactJsonSection? Incoming { get; init; }

    public required ReferencesCompactJsonSection? Outgoing { get; init; }

    public required ReferencesJsonFinding[] Findings { get; init; }
}

internal sealed class ReferencesCompactJsonSection
{
    public required string Coverage { get; init; }

    public required string Status { get; init; }

    public required int OccurrenceCount { get; init; }

    public required ReferencesCompactJsonOccurrence[] Occurrences { get; init; }
}

internal sealed class ReferencesCompactJsonOccurrence
{
    public required string Direction { get; init; }

    public required int Level { get; init; }

    public required ReferencesJsonOccurrenceSource Source { get; init; }

    public required ReferencesCompactJsonLocation Location { get; init; }


    public required string RawDestination { get; init; }

    public required string? Fragment { get; init; }

    public required ReferencesJsonTarget Target { get; init; }

}

internal sealed class ReferencesCompactJsonLocation
{
    public required int Line { get; init; }

    public required int Column { get; init; }

}
