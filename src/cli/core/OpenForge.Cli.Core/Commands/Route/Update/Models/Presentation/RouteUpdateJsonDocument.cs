namespace OpenForge.Cli.Core.Commands.Route.Update.Models.Presentation;

internal sealed record RouteUpdateJsonDocument
{
    public required int SchemaVersion { get; init; }

    public required string Command { get; init; }

    public required string Status { get; init; }

    public required RouteUpdateJsonWorkspace? Workspace { get; init; }

    public required RouteUpdateJsonResult Result { get; init; }

    public required RouteUpdateJsonNext? Next { get; init; }
}

internal sealed record RouteUpdateJsonWorkspace
{
    public required string Path { get; init; }

    public required string SelectedBy { get; init; }
}

internal sealed record RouteUpdateJsonResult
{
    public required string Mode { get; init; }

    public required RouteUpdateJsonTarget Target { get; init; }

    public required RouteUpdateJsonPatch Patch { get; init; }

    public required RouteUpdateJsonTemplate? Template { get; init; }

    public required RouteUpdateJsonPlan Plan { get; init; }

    public required RouteUpdateJsonEffect[] Effects { get; init; }

    public required string[] UnchangedPaths { get; init; }

    public required RouteUpdateJsonRecovery Recovery { get; init; }

    public required string Verification { get; init; }

    public required RouteUpdateJsonFinding[] Findings { get; init; }
}

internal sealed record RouteUpdateJsonTarget
{
    public required string Requested { get; init; }

    public required string? SelectedBy { get; init; }

    public required string? Id { get; init; }

    public required string? Path { get; init; }

    public required string? Form { get; init; }

    public required string[] OverwritePaths { get; init; }
}

internal sealed record RouteUpdateJsonPatch
{
    public required RouteUpdateJsonDescriptionPatch Description { get; init; }

    public required RouteUpdateJsonResponsibilityPatch Responsibility { get; init; }

    public required RouteUpdateJsonTagsPatch Tags { get; init; }
}

internal sealed record RouteUpdateJsonDescriptionPatch
{
    public required bool Requested { get; init; }

    public required string? Before { get; init; }

    public required string? Expected { get; init; }

    public required string State { get; init; }
}

internal sealed record RouteUpdateJsonResponsibilityPatch
{
    public required bool Requested { get; init; }

    public required string Operation { get; init; }

    public required string? Before { get; init; }

    public required string? Expected { get; init; }

    public required string State { get; init; }
}

internal sealed record RouteUpdateJsonTagsPatch
{
    public required bool Requested { get; init; }

    public required string[]? Before { get; init; }

    public required string[]? Expected { get; init; }

    public required string State { get; init; }
}

internal sealed record RouteUpdateJsonTemplate
{
    public required string Requested { get; init; }

    public required string? Id { get; init; }

    public required string? Path { get; init; }

    public required string? Classification { get; init; }

    public required long? BodyByteLength { get; init; }

    public required string Decision { get; init; }
}

internal sealed record RouteUpdateJsonPlan
{
    public required string Completeness { get; init; }

    public required string Safety { get; init; }

    public required string Body { get; init; }
}

internal sealed record RouteUpdateJsonEffect
{
    public required string Path { get; init; }

    public required string Kind { get; init; }

    public required string Action { get; init; }

    public required RouteUpdateJsonChange Change { get; init; }

    public required RouteUpdateJsonPreviewHunk[] Preview { get; init; }

    public required string Outcome { get; init; }

    public required string Residual { get; init; }
}

internal sealed record RouteUpdateJsonChange
{
    public required string Before { get; init; }

    public required string Expected { get; init; }
}

internal sealed record RouteUpdateJsonPreviewHunk
{
    public required string Kind { get; init; }

    public required string Before { get; init; }

    public required string Expected { get; init; }
}

internal sealed record RouteUpdateJsonRecovery
{
    public required string State { get; init; }

    public required string? ResidualPath { get; init; }
}

internal sealed record RouteUpdateJsonFinding
{
    public required string Code { get; init; }

    public required string Status { get; init; }

    public required string? Target { get; init; }

    public required string Cause { get; init; }
}

internal sealed record RouteUpdateJsonNext
{
    public required string Command { get; init; }

    public required string Reason { get; init; }
}
