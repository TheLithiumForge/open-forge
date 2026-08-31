namespace OpenForge.Cli.Core.Commands.Route.Init.Models.Presentation;

internal sealed class RouteInitJsonDocument
{
    public required int SchemaVersion { get; init; }

    public required string Command { get; init; }

    public required string Status { get; init; }

    public required RouteInitJsonWorkspace? Workspace { get; init; }

    public required RouteInitJsonResult Result { get; init; }

    public required RouteInitJsonNext? Next { get; init; }
}

internal sealed class RouteInitJsonWorkspace
{
    public required string Path { get; init; }

    public required string SelectedBy { get; init; }
}

internal sealed class RouteInitJsonResult
{
    public required string Mode { get; init; }

    public required string Scaffold { get; init; }

    public required RouteInitJsonTarget Target { get; init; }

    public required RouteInitJsonPlan Plan { get; init; }

    public required RouteInitJsonFramework? Framework { get; init; }

    public required RouteInitJsonEntrypoint[] Entrypoints { get; init; }

    public required RouteInitJsonEffect[] Effects { get; init; }

    public required string[] UnchangedPaths { get; init; }

    public required RouteInitJsonLifecycle Lifecycle { get; init; }

    public required RouteInitJsonRecovery Recovery { get; init; }

    public required string Verification { get; init; }

    public required RouteInitJsonFinding[] Findings { get; init; }
}

internal sealed class RouteInitJsonTarget
{
    public required string Requested { get; init; }

    public required string? Id { get; init; }

    public required string? Path { get; init; }
}

internal sealed class RouteInitJsonPlan
{
    public required string Completeness { get; init; }

    public required string Safety { get; init; }
}

internal sealed class RouteInitJsonFramework
{
    public required string InventoryFingerprint { get; init; }

    public required RouteInitJsonFrameworkSegment[] Segments { get; init; }
}

internal sealed class RouteInitJsonFrameworkSegment
{
    public required string Path { get; init; }

    public required string Role { get; init; }

    public required string? SourceAssetPath { get; init; }
}

internal sealed class RouteInitJsonEntrypoint
{
    public required string Id { get; init; }

    public required string Path { get; init; }

    public required string Form { get; init; }

    public required string Current { get; init; }

    public required string Ownership { get; init; }

    public required RouteInitJsonMetadata? Metadata { get; init; }

    public required string? SourceAssetPath { get; init; }

    public required string Outcome { get; init; }
}

internal sealed class RouteInitJsonMetadata
{
    public required string Description { get; init; }

    public required string DescriptionSource { get; init; }

    public required string? Responsibility { get; init; }

    public required string ResponsibilitySource { get; init; }

    public required string[] Tags { get; init; }

    public required string TagsSource { get; init; }
}

internal sealed class RouteInitJsonEffect
{
    public required string Path { get; init; }

    public required string Kind { get; init; }

    public required string Action { get; init; }

    public required string? SourceAssetPath { get; init; }

    public required RouteInitJsonChange? Change { get; init; }

    public required string Outcome { get; init; }

    public required string Residual { get; init; }
}

internal sealed class RouteInitJsonChange
{
    public required string? Before { get; init; }

    public required string Expected { get; init; }
}

internal sealed class RouteInitJsonLifecycle
{
    public required string Action { get; init; }

    public required string Outcome { get; init; }
}

internal sealed class RouteInitJsonRecovery
{
    public required string State { get; init; }

    public required string? ResidualPath { get; init; }
}

internal sealed class RouteInitJsonFinding
{
    public required string Code { get; init; }

    public required string Status { get; init; }

    public required string? Target { get; init; }

    public required string Cause { get; init; }
}

internal sealed class RouteInitJsonNext
{
    public required string Command { get; init; }

    public required string Reason { get; init; }
}
