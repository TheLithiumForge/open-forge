namespace OpenForge.Cli.Core.Commands.Context.Models.Presentation;

internal sealed class ContextJsonSourceIdentity
{
    public required string? Id { get; init; }

    public required string Path { get; init; }
}

internal sealed class ContextJsonPathProjection
{
    public required int Position { get; init; }

    public required int SourcePosition { get; init; }

    public required string? Id { get; init; }

    public required string Path { get; init; }

    public required string Layer { get; init; }

    public required ContextJsonInclusionReason[] InclusionReasons { get; init; }
}

internal sealed class ContextJsonSource
{
    public required int Position { get; init; }

    public required string? Id { get; init; }

    public required string Path { get; init; }

    public required string RouteState { get; init; }

    public required string? Route { get; init; }

    public required string? Scope { get; init; }

    public required ContextJsonInclusionReason[] InclusionReasons { get; init; }

    public required ContextJsonLayer[] Layers { get; init; }
}

internal sealed class ContextJsonLayer
{
    public required int PathPosition { get; init; }

    public required string Kind { get; init; }

    public required string Path { get; init; }

    public required ContextJsonInclusionReason[] InclusionReasons { get; init; }

    public required ContextJsonProjection[] Projections { get; init; }
}

internal sealed class ContextJsonInclusionReason
{
    public required string Kind { get; init; }

    public required ContextJsonSourceIdentity? Source { get; init; }

    public required string? Reference { get; init; }

    public required int? Depth { get; init; }

    public required ContextJsonLocation? Location { get; init; }
}

internal sealed class ContextJsonProjection
{
    public required string Part { get; init; }

    public required string? Name { get; init; }

    public required string State { get; init; }

    public required string? Text { get; init; }

    public required ContextJsonProjectedHeading[] Headings { get; init; }

    public required ContextJsonLocation? Location { get; init; }
}

internal sealed class ContextJsonProjectedHeading
{
    public required string Text { get; init; }

    public required int Level { get; init; }

    public required string Form { get; init; }

    public required ContextJsonLocation Location { get; init; }

    public required bool Canonical { get; init; }
}

internal sealed class ContextJsonLocation
{
    public required int Line { get; init; }

    public required int Column { get; init; }

    public required long ByteOffset { get; init; }

    public required long ByteLength { get; init; }
}
