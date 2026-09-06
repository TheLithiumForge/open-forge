namespace OpenForge.Cli.Core.Commands.Update.Models.Presentation;

internal sealed record UpdateJsonDocument
{
    public required int SchemaVersion { get; init; }

    public required string Command { get; init; }

    public required string Status { get; init; }

    public required UpdateJsonWorkspace? Workspace { get; init; }

    public required UpdateJsonResult Result { get; init; }

    public required UpdateJsonNext? Next { get; init; }
}

internal sealed record UpdateJsonWorkspace
{
    public required string Path { get; init; }

    public required string SelectedBy { get; init; }
}

internal sealed record UpdateJsonNext
{
    public required string Command { get; init; }

    public required string Reason { get; init; }
}

internal sealed record UpdateJsonResult
{
    public required string Mode { get; init; }

    public required bool Force { get; init; }

    public required bool Prune { get; init; }

    public required bool Automatic { get; init; }

    public required UpdateJsonSource? Source { get; init; }

    public required UpdateJsonComparison[] Comparisons { get; init; }

    public required UpdateJsonGeneratedNavigation? GeneratedNavigation { get; init; }

    public required UpdateJsonEffect[] Effects { get; init; }

    public required UpdateJsonLifecycle Lifecycle { get; init; }

    public required UpdateJsonRecovery Recovery { get; init; }

    public required string Verification { get; init; }

    public required UpdateJsonFinding[] Findings { get; init; }
}

internal sealed record UpdateJsonSource
{
    public required string Id { get; init; }

    public required string? Version { get; init; }

    public required string InventoryFingerprint { get; init; }

    public required int AssetCount { get; init; }
}

internal sealed record UpdateJsonComparison
{
    public required string Path { get; init; }

    public required string Kind { get; init; }

    public required string? Region { get; init; }

    public required string? SourceAssetPath { get; init; }

    public required string FingerprintKind { get; init; }

    public required string? BaselineFingerprint { get; init; }

    public required string? CurrentFingerprint { get; init; }

    public required string? IntendedFingerprint { get; init; }

    public required string CurrentState { get; init; }

    public required string IntendedState { get; init; }

    public required string RetirementEligibility { get; init; }
}

internal sealed record UpdateJsonGeneratedNavigation
{
    public required string Coverage { get; init; }

    public required UpdateJsonGeneratedNavigationRegion[] Regions { get; init; }
}

internal sealed record UpdateJsonGeneratedNavigationRegion
{
    public required string Path { get; init; }

    public required string State { get; init; }
}

internal sealed record UpdateJsonEffect
{
    public required string Path { get; init; }

    public required string Action { get; init; }

    public required UpdateJsonLogicalChange[] Changes { get; init; }

    public required string Outcome { get; init; }

    public required string Residual { get; init; }
}

internal sealed record UpdateJsonLogicalChange
{
    public required string Kind { get; init; }

    public required string Action { get; init; }

    public required string? Region { get; init; }

    public required string? SourceAssetPath { get; init; }
}

internal sealed record UpdateJsonLifecycle
{
    public required string Trust { get; init; }

    public required string Coverage { get; init; }

    public required string Action { get; init; }

    public required string Outcome { get; init; }
}

internal sealed record UpdateJsonRecovery
{
    public required string State { get; init; }

    public required string[] ProtectedPaths { get; init; }

    public required string? ResidualPath { get; init; }
}

internal sealed record UpdateJsonFinding
{
    public required string Code { get; init; }

    public required string? Target { get; init; }

    public required string Cause { get; init; }
}
