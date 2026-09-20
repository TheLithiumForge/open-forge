namespace OpenForge.Cli.Core.Commands.References.Models.Presentation;

internal sealed class ReferencesJsonFinding
{
    public required string Code { get; init; }

    public required string Status { get; init; }

    public required string? Direction { get; init; }

    public required string? Subject { get; init; }

    public required string Cause { get; init; }

    public required string? SelectorRole { get; init; }

    public required int? SelectorOccurrence { get; init; }

    public required ReferencesJsonIdentity? Source { get; init; }

    public required string? Layer { get; init; }

    public required string? Path { get; init; }

    public required ReferencesJsonLocation? Location { get; init; }

    public required ReferencesJsonLocation? DestinationLocation { get; init; }

    public required ReferencesJsonIdentity[] Candidates { get; init; }
}
