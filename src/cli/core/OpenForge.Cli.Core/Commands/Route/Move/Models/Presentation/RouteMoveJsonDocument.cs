namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Presentation;

internal sealed record RouteMoveJsonDocument
{
    public required int SchemaVersion { get; init; }

    public required string Command { get; init; }

    public required string Status { get; init; }

    public required RouteMoveJsonWorkspace? Workspace { get; init; }

    public required RouteMoveJsonResult Result { get; init; }

    public required RouteMoveJsonNext? Next { get; init; }
}

internal sealed record RouteMoveJsonWorkspace
{
    public required string Path { get; init; }

    public required string SelectedBy { get; init; }
}

internal sealed record RouteMoveJsonResult
{
    public required string Mode { get; init; }

    public required RouteMoveJsonSource Source { get; init; }

    public required RouteMoveJsonDestination Destination { get; init; }

    public required RouteMoveJsonSubject Subject { get; init; }

    public required RouteMoveJsonOwnership Ownership { get; init; }

    public required RouteMoveJsonReferences References { get; init; }

    public required RouteMoveJsonGeneratedNavigation GeneratedNavigation { get; init; }

    public required RouteMoveJsonPlan Plan { get; init; }

    public required RouteMoveJsonEffect[] Effects { get; init; }

    public required string[] UnchangedPaths { get; init; }

    public required RouteMoveJsonRecovery Recovery { get; init; }

    public required string Verification { get; init; }

    public required RouteMoveJsonFinding[] Findings { get; init; }
}

internal sealed record RouteMoveJsonSource
{
    public required string Requested { get; init; }

    public required string? SelectedBy { get; init; }

    public required string? Id { get; init; }

    public required string? Path { get; init; }

    public required string? Form { get; init; }
}

internal sealed record RouteMoveJsonDestination
{
    public required string Requested { get; init; }

    public required string? Id { get; init; }

    public required string? Path { get; init; }

    public required string? ParentId { get; init; }

    public required string? ParentPath { get; init; }
}

internal sealed record RouteMoveJsonSubject
{
    public required string? Kind { get; init; }

    public required RouteMoveJsonSubjectLayer[] Layers { get; init; }

    public required RouteMoveJsonSubjectItem[] Items { get; init; }
}

internal sealed record RouteMoveJsonSubjectLayer
{
    public required string Layer { get; init; }

    public required string SourcePath { get; init; }

    public required string DestinationPath { get; init; }
}

internal sealed record RouteMoveJsonSubjectItem
{
    public required string Kind { get; init; }

    public required string? Layer { get; init; }

    public required string? SourceId { get; init; }

    public required string SourcePath { get; init; }

    public required string DestinationPath { get; init; }
}

internal sealed record RouteMoveJsonOwnership
{
    public required string State { get; init; }

    public required string Framework { get; init; }

    public required string Extensions { get; init; }

    public required RouteMoveJsonOwnershipClaim[] Claims { get; init; }
}

internal sealed record RouteMoveJsonOwnershipClaim
{
    public required string Path { get; init; }

    public required string Manager { get; init; }

    public required string Owner { get; init; }
}

internal sealed record RouteMoveJsonReferences
{
    public required string Coverage { get; init; }

    public required int ScannedSourceCount { get; init; }

    public required int InspectedSourceCount { get; init; }

    public required int OccurrenceCount { get; init; }

    public required RouteMoveJsonReferenceRewrite[] Rewrites { get; init; }
}

internal sealed record RouteMoveJsonReferenceRewrite
{
    public required string SourcePath { get; init; }

    public required string DestinationSourcePath { get; init; }

    public required string? Layer { get; init; }

    public required RouteMoveJsonSourceLocation Location { get; init; }

    public required string Before { get; init; }

    public required string Expected { get; init; }

    public required RouteMoveJsonReferenceTarget OldTarget { get; init; }

    public required RouteMoveJsonReferenceTarget ExpectedTarget { get; init; }
}

internal sealed record RouteMoveJsonReferenceTarget
{
    public required string? Id { get; init; }

    public required string Path { get; init; }
}

internal sealed record RouteMoveJsonSourceLocation
{
    public required int Line { get; init; }

    public required int Column { get; init; }

    public required long ByteOffset { get; init; }

    public required long ByteLength { get; init; }
}

internal sealed record RouteMoveJsonGeneratedNavigation
{
    public required string Coverage { get; init; }

    public required RouteMoveJsonGeneratedRegion[] Regions { get; init; }
}

internal sealed record RouteMoveJsonGeneratedRegion
{
    public required string Path { get; init; }

    public required string[] Reasons { get; init; }

    public required string State { get; init; }
}

internal sealed record RouteMoveJsonPlan
{
    public required string Completeness { get; init; }

    public required string Safety { get; init; }
}

internal sealed record RouteMoveJsonEffect
{
    public required string Path { get; init; }

    public required string Kind { get; init; }

    public required string Action { get; init; }

    public required RouteMoveJsonPathState Before { get; init; }

    public required RouteMoveJsonPathState Expected { get; init; }

    public required string Outcome { get; init; }

    public required string Residual { get; init; }
}

internal sealed record RouteMoveJsonPathState
{
    public required string Kind { get; init; }

    public required string? ContentSha256 { get; init; }
}

internal sealed record RouteMoveJsonRecovery
{
    public required string State { get; init; }

    public required string[] ProtectedPaths { get; init; }

    public required string? ResidualPath { get; init; }
}

internal sealed record RouteMoveJsonFinding
{
    public required string Code { get; init; }

    public required string Status { get; init; }

    public required string? Target { get; init; }

    public required string Cause { get; init; }
}

internal sealed record RouteMoveJsonNext
{
    public required string Command { get; init; }

    public required string Reason { get; init; }
}
