namespace OpenForge.Cli.Core.Commands.Find.Models.Presentation;

internal sealed class FindJsonMatch
{
    public required int Position { get; init; }

    public required string Id { get; init; }

    public required string Path { get; init; }

    public required string? Description { get; init; }

    public required FindJsonEvidence[] Evidence { get; init; }

    public required FindJsonProjection[] Projections { get; init; }
}

internal sealed class FindJsonEvidence
{
    public required int Predicate { get; init; }

    public required string Kind { get; init; }

    public required string Query { get; init; }

    public required string Authored { get; init; }

    public required string Region { get; init; }

    public required string Layer { get; init; }

    public required string Path { get; init; }

    public required FindJsonLocation Location { get; init; }

    public required int Occurrence { get; init; }

    public required FindJsonHeadingEvidence? Heading { get; init; }
}

internal sealed class FindJsonHeadingEvidence
{
    public required int Level { get; init; }

    public required string Form { get; init; }

    public required bool Canonical { get; init; }
}

internal sealed class FindJsonProjection
{
    public required string Part { get; init; }

    public required string? Name { get; init; }

    public required string? Layer { get; init; }

    public required string? Path { get; init; }

    public required string State { get; init; }

    public required FindJsonMetadata? Metadata { get; init; }

    public required string? Text { get; init; }

    public required FindJsonProjectedHeading[] Headings { get; init; }

    public required FindJsonLocation? Location { get; init; }
}

internal sealed class FindJsonMetadata
{
    public required int Position { get; init; }

    public required string Id { get; init; }

    public required string Path { get; init; }

    public required string RouteState { get; init; }

    public required string? Route { get; init; }

    public required FindJsonMetadataLayer[] Layers { get; init; }
}

internal sealed class FindJsonMetadataLayer
{
    public required string Kind { get; init; }

    public required string Path { get; init; }
}

internal sealed class FindJsonProjectedHeading
{
    public required string Text { get; init; }

    public required int Level { get; init; }

    public required string Form { get; init; }

    public required FindJsonLocation Location { get; init; }

    public required bool Canonical { get; init; }
}
