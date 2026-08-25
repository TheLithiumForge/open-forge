namespace OpenForge.Cli.Core.Commands.Find.Models.Presentation;

internal sealed class FindJsonDocument
{
    public required int SchemaVersion { get; init; }

    public required string Command { get; init; }

    public required string Status { get; init; }

    public required FindJsonWorkspace? Workspace { get; init; }

    public required FindJsonResult Result { get; init; }

    public required FindJsonNext? Next { get; init; }
}

internal sealed class FindJsonWorkspace
{
    public required string Path { get; init; }

    public required string SelectedBy { get; init; }
}

internal sealed class FindJsonResult
{
    public required FindJsonUniverse Universe { get; init; }

    public required FindJsonQuery Query { get; init; }

    public required FindJsonPresentation Presentation { get; init; }

    public required FindJsonCoverage Coverage { get; init; }

    public required FindJsonFinding[] Findings { get; init; }

    public required FindJsonMatch[] Matches { get; init; }
}

internal sealed class FindJsonQuery
{
    public required FindJsonPredicate[] Predicates { get; init; }

    public required FindJsonPredicate[] EffectivePredicates { get; init; }

    public required string Require { get; init; }

    public required FindJsonWithin Within { get; init; }
}

internal sealed class FindJsonPredicate
{
    public required string Kind { get; init; }

    public required string Value { get; init; }
}

internal sealed class FindJsonWithin
{
    public required string[] Supplied { get; init; }

    public required string[] Tag { get; init; }

    public required string[] Heading { get; init; }
}

internal sealed class FindJsonPresentation
{
    public required FindJsonView View { get; init; }

    public required FindJsonContent Content { get; init; }
}

internal sealed class FindJsonView
{
    public required string? Supplied { get; init; }

    public required string Effective { get; init; }
}

internal sealed class FindJsonContent
{
    public required string[] Supplied { get; init; }

    public required string[] Effective { get; init; }
}

internal sealed class FindJsonCoverage
{
    public required string State { get; init; }

    public required string Matching { get; init; }

    public required string Projection { get; init; }
}

internal sealed class FindJsonNext
{
    public required string Command { get; init; }

    public required string Reason { get; init; }
}
