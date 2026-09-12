namespace OpenForge.Cli.Core.Commands.Context.Models.Presentation;

internal sealed class ContextCompactJsonResult
{
    public required ContextJsonSelection Selection { get; init; }

    public required ContextJsonPresentation Presentation { get; init; }

    public required ContextJsonCoverage Coverage { get; init; }

    public required ContextCompactJsonPathProjection[] Paths { get; init; }

    public required ContextJsonLink[] Links { get; init; }

    public required ContextCompactJsonSource[] Sources { get; init; }

    public required ContextJsonFinding[] Findings { get; init; }
}

internal sealed class ContextCompactJsonPathProjection
{
    public required int Position { get; init; }

    public required int SourcePosition { get; init; }

    public required string? Id { get; init; }

    public required string Path { get; init; }

    public required string Layer { get; init; }

}

internal sealed class ContextCompactJsonSource
{
    public required int Position { get; init; }

    public required string? Id { get; init; }

    public required string Path { get; init; }

    public required string RouteState { get; init; }

    public required string? Route { get; init; }

    public required string? Scope { get; init; }


    public required ContextCompactJsonLayer[] Layers { get; init; }
}

internal sealed class ContextCompactJsonLayer
{
    public required int PathPosition { get; init; }

    public required string Kind { get; init; }

    public required string Path { get; init; }


    public required ContextJsonProjection[] Projections { get; init; }
}
