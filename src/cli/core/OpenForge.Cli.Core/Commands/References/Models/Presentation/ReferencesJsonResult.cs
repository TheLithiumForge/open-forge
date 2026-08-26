namespace OpenForge.Cli.Core.Commands.References.Models.Presentation;

internal sealed class ReferencesJsonResult
{
    public required ReferencesJsonSource? Source { get; init; }

    public required string? RequestedDirection { get; init; }

    public required ReferencesJsonIncomingSelection? IncomingSelection { get; init; }

    public required ReferencesJsonSection? Incoming { get; init; }

    public required ReferencesJsonSection? Outgoing { get; init; }

    public required ReferencesJsonFinding[] Findings { get; init; }
}

internal sealed class ReferencesJsonSource
{
    public required string Id { get; init; }

    public required string Path { get; init; }

    public required string[] Layers { get; init; }
}

internal sealed class ReferencesJsonSection
{
    public required string Coverage { get; init; }

    public required string Status { get; init; }

    public required int OccurrenceCount { get; init; }

    public required ReferencesJsonOccurrence[] Occurrences { get; init; }
}
