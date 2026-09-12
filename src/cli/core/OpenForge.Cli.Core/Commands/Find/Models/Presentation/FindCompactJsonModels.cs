namespace OpenForge.Cli.Core.Commands.Find.Models.Presentation;

internal sealed class FindCompactJsonResult
{
    public required FindJsonUniverse Universe { get; init; }

    public required FindJsonQuery Query { get; init; }

    public required FindJsonPresentation Presentation { get; init; }

    public required FindJsonCoverage Coverage { get; init; }

    public required FindJsonFinding[] Findings { get; init; }

    public required FindCompactJsonMatch[] Matches { get; init; }
}

internal sealed class FindCompactJsonMatch
{
    public required int Position { get; init; }

    public required string Id { get; init; }

    public required string Path { get; init; }

    public required string? Description { get; init; }


    public required FindJsonProjection[] Projections { get; init; }
}
