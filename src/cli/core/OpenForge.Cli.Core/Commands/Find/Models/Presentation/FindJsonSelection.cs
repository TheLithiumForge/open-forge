namespace OpenForge.Cli.Core.Commands.Find.Models.Presentation;

internal sealed class FindJsonUniverse
{
    public required string Mode { get; init; }

    public required FindJsonSelector[] Include { get; init; }

    public required FindJsonSelector[] Exclude { get; init; }

    public required int? CandidateCount { get; init; }

    public required int? InspectedCount { get; init; }

    public required int? MatchedCount { get; init; }
}

internal sealed class FindJsonSelector
{
    public required string Value { get; init; }

    public required string? Form { get; init; }

    public required string Resolution { get; init; }

    public required FindJsonIdentity? Identity { get; init; }

    public required string? SourceKind { get; init; }

    public required string? Expansion { get; init; }

    public required FindJsonIdentity[] Candidates { get; init; }
}

internal sealed class FindJsonIdentity
{
    public required string Id { get; init; }

    public required string Path { get; init; }
}
