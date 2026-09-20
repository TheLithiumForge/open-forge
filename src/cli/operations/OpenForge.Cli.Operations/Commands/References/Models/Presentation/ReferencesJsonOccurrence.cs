namespace OpenForge.Cli.Core.Commands.References.Models.Presentation;

internal sealed class ReferencesJsonOccurrence
{
    public required string Direction { get; init; }

    public required int Level { get; init; }

    public required ReferencesJsonOccurrenceSource Source { get; init; }

    public required ReferencesJsonLocation Location { get; init; }

    public required ReferencesJsonLocation? DestinationLocation { get; init; }

    public required string RawDestination { get; init; }

    public required string? Fragment { get; init; }

    public required ReferencesJsonTarget Target { get; init; }

    public required string Provenance { get; init; }
}

internal sealed class ReferencesJsonOccurrenceSource
{
    public required string? Id { get; init; }

    public required string Path { get; init; }

    public required string Layer { get; init; }
}

internal sealed class ReferencesJsonTarget
{
    public required string Kind { get; init; }

    public required string? Id { get; init; }

    public required string? Path { get; init; }

    public required string? Layer { get; init; }

    public required string Resolution { get; init; }

    public required string? Network { get; init; }
}

internal sealed class ReferencesJsonLocation
{
    public required int Line { get; init; }

    public required int Column { get; init; }

    public required long ByteOffset { get; init; }

    public required long ByteLength { get; init; }
}
