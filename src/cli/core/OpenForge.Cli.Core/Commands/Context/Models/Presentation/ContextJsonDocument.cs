namespace OpenForge.Cli.Core.Commands.Context.Models.Presentation;

internal sealed class ContextJsonDocument
{
    public required int SchemaVersion { get; init; }

    public required string Command { get; init; }

    public required string Status { get; init; }

    public required ContextJsonWorkspace? Workspace { get; init; }

    public required ContextJsonResult Result { get; init; }

    public required ContextJsonNext? Next { get; init; }
}

internal sealed class ContextJsonWorkspace
{
    public required string Path { get; init; }

    public required string SelectedBy { get; init; }
}

internal sealed class ContextJsonResult
{
    public required ContextJsonSelection Selection { get; init; }

    public required ContextJsonPresentation Presentation { get; init; }

    public required ContextJsonCoverage Coverage { get; init; }

    public required ContextJsonPathProjection[] Paths { get; init; }

    public required ContextJsonLink[] Links { get; init; }

    public required ContextJsonSource[] Sources { get; init; }

    public required ContextJsonFinding[] Findings { get; init; }
}

internal sealed class ContextJsonNext
{
    public required string Command { get; init; }

    public required string Reason { get; init; }
}
