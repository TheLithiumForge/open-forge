namespace OpenForge.Cli.Core.Commands.Index.Models.Presentation;

internal sealed class IndexJsonDocument
{
    public required int SchemaVersion { get; init; }

    public required string Command { get; init; }

    public required string Status { get; init; }

    public required IndexJsonWorkspace? Workspace { get; init; }

    public required IndexJsonResult Result { get; init; }

    public required IndexJsonNext? Next { get; init; }
}

internal sealed class IndexJsonWorkspace
{
    public required string Path { get; init; }

    public required string SelectedBy { get; init; }
}

internal sealed class IndexJsonNext
{
    public required string Command { get; init; }

    public required string Reason { get; init; }
}

internal sealed class IndexJsonResult
{
    public required string Mode { get; init; }

    public required IndexJsonSelection Selection { get; init; }

    public required IndexJsonRegion[] Regions { get; init; }

    public required IndexJsonRecovery Recovery { get; init; }

    public required IndexJsonFinding[] Findings { get; init; }

    public required IndexJsonCounts Counts { get; init; }
}

internal sealed class IndexJsonSelection
{
    public required string Origin { get; init; }

    public required string Scope { get; init; }

    public required IndexJsonLogicalSource[] Sources { get; init; }
}

internal sealed class IndexJsonLogicalSource
{
    public required string Id { get; init; }

    public required string Path { get; init; }

    public required string Scope { get; init; }
}

internal sealed class IndexJsonRegion
{
    public required IndexJsonLogicalSource Source { get; init; }

    public required string Action { get; init; }

    public required int? BeforeEntryCount { get; init; }

    public required int? ExpectedEntryCount { get; init; }

    public required IndexJsonChange? Change { get; init; }

    public required string Outcome { get; init; }
}

internal sealed class IndexJsonChange
{
    public required string BeforeBody { get; init; }

    public required string ExpectedBody { get; init; }
}

internal sealed class IndexJsonRecovery
{
    public required string State { get; init; }

    public required string? ResidualPath { get; init; }
}

internal sealed class IndexJsonFinding
{
    public required string Code { get; init; }

    public required string Status { get; init; }

    public required int? SourceOccurrence { get; init; }

    public required IndexJsonLogicalSource? Source { get; init; }

    public required string Cause { get; init; }

    public required IndexJsonLogicalSource[] Candidates { get; init; }
}

internal sealed class IndexJsonCounts
{
    public required int Regions { get; init; }

    public required int Updates { get; init; }

    public required int Unchanged { get; init; }

    public required int Applied { get; init; }

    public required int Verified { get; init; }
}
