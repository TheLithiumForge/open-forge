namespace OpenForge.Cli.Core.Commands.Context.Models.Presentation;

internal sealed class ContextJsonFinding
{
    public required string Code { get; init; }

    public required string Status { get; init; }

    public required string? Subject { get; init; }

    public required string Cause { get; init; }

    public required string? Reference { get; init; }

    public required ContextJsonSourceIdentity? Source { get; init; }

    public required string? Layer { get; init; }

    public required string? Path { get; init; }

    public required string? Part { get; init; }

    public required ContextJsonLocation? Location { get; init; }

    public required ContextJsonLocation? DestinationLocation { get; init; }

    public required ContextJsonSourceIdentity[] Candidates { get; init; }
}
