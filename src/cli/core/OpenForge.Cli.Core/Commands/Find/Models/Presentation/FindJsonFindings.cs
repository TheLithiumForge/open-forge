namespace OpenForge.Cli.Core.Commands.Find.Models.Presentation;

internal sealed class FindJsonFinding
{
    public required string Code { get; init; }

    public required string Status { get; init; }

    public required string? Subject { get; init; }

    public required string Cause { get; init; }

    public required string? SelectorRole { get; init; }

    public required int? SelectorOccurrence { get; init; }

    public required FindJsonIdentity? Source { get; init; }

    public required string? Layer { get; init; }

    public required string? Path { get; init; }

    public required string? Region { get; init; }

    public required FindJsonLocation? Location { get; init; }

    public required FindJsonIdentity[] Candidates { get; init; }
}

internal sealed class FindJsonLocation
{
    public required int Line { get; init; }

    public required int Column { get; init; }

    public required long ByteOffset { get; init; }

    public required long ByteLength { get; init; }
}
