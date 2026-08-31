namespace OpenForge.Cli.Core.Commands.Route.Create.Models.Presentation;

internal sealed record RouteCreateJsonDocument
{
    public required int SchemaVersion { get; init; }

    public required string Command { get; init; }

    public required string Status { get; init; }

    public required RouteCreateJsonWorkspace? Workspace { get; init; }

    public required RouteCreateJsonResult Result { get; init; }

    public required RouteCreateJsonNext? Next { get; init; }
}

internal sealed record RouteCreateJsonWorkspace
{
    public required string Path { get; init; }

    public required string SelectedBy { get; init; }
}

internal sealed record RouteCreateJsonResult
{
    public required string Mode { get; init; }

    public required RouteCreateJsonTarget Target { get; init; }

    public required RouteCreateJsonParent? Parent { get; init; }

    public required RouteCreateJsonMetadata Metadata { get; init; }

    public required RouteCreateJsonTemplate? Template { get; init; }

    public required RouteCreateJsonPlan Plan { get; init; }

    public required RouteCreateJsonEffect[] Effects { get; init; }

    public required string[] UnchangedPaths { get; init; }

    public required RouteCreateJsonRecovery Recovery { get; init; }

    public required string Verification { get; init; }

    public required RouteCreateJsonFinding[] Findings { get; init; }
}

internal sealed record RouteCreateJsonTarget
{
    public required string Requested { get; init; }

    public required string? Id { get; init; }

    public required string? Path { get; init; }
}

internal sealed record RouteCreateJsonParent
{
    public required string Id { get; init; }

    public required string Path { get; init; }

    public required string Form { get; init; }
}

internal sealed record RouteCreateJsonMetadata
{
    public required string Description { get; init; }

    public required string? Responsibility { get; init; }

    public required string[] Tags { get; init; }
}

internal sealed record RouteCreateJsonTemplate
{
    public required string Requested { get; init; }

    public required string Id { get; init; }

    public required string Path { get; init; }

    public required string Classification { get; init; }

    public required long BodyByteLength { get; init; }
}

internal sealed record RouteCreateJsonPlan
{
    public required string Completeness { get; init; }

    public required string Safety { get; init; }
}

internal sealed record RouteCreateJsonEffect
{
    public required string Path { get; init; }

    public required string Kind { get; init; }

    public required string Action { get; init; }

    public required RouteCreateJsonChange Change { get; init; }

    public required string Outcome { get; init; }

    public required string Residual { get; init; }
}

internal sealed record RouteCreateJsonChange
{
    public required string? Before { get; init; }

    public required string Expected { get; init; }
}

internal sealed record RouteCreateJsonRecovery
{
    public required string State { get; init; }

    public required string? ResidualPath { get; init; }
}

internal sealed record RouteCreateJsonFinding
{
    public required string Code { get; init; }

    public required string Status { get; init; }

    public required string? Target { get; init; }

    public required string Cause { get; init; }
}

internal sealed record RouteCreateJsonNext
{
    public required string Command { get; init; }

    public required string Reason { get; init; }
}
