namespace OpenForge.Cli.Core.Commands.Install.Models.Presentation;

internal sealed class InstallJsonDocument
{
    public required int SchemaVersion { get; init; }

    public required string Command { get; init; }

    public required string Status { get; init; }

    public required InstallJsonWorkspace? Workspace { get; init; }

    public required InstallJsonResult Result { get; init; }

    public required InstallJsonNext? Next { get; init; }
}

internal sealed class InstallJsonWorkspace
{
    public required string Path { get; init; }

    public required string SelectedBy { get; init; }
}

internal sealed class InstallJsonResult
{
    public required string Mode { get; init; }

    public required bool Force { get; init; }

    public required bool Automatic { get; init; }

    public required InstallJsonSource? Source { get; init; }

    public required string? Classification { get; init; }

    public required InstallJsonFootprint? Footprint { get; init; }

    public required InstallJsonEffect[] Effects { get; init; }

    public required InstallJsonLifecycle Lifecycle { get; init; }

    public required InstallJsonRecovery Recovery { get; init; }

    public required string Verification { get; init; }

    public required InstallJsonFinding[] Findings { get; init; }
}

internal sealed class InstallJsonSource
{
    public required string InventoryFingerprint { get; init; }

    public required int AssetCount { get; init; }
}

internal sealed class InstallJsonFootprint
{
    public required int PayloadFiles { get; init; }

    public required int ManagedRegions { get; init; }

    public required int GeneratedRegions { get; init; }
}

internal sealed class InstallJsonEffect
{
    public required string Path { get; init; }

    public required string Kind { get; init; }

    public required string Action { get; init; }

    public required string? SourceAssetPath { get; init; }

    public required string Outcome { get; init; }

    public required string Residual { get; init; }
}

internal sealed class InstallJsonLifecycle
{
    public required string Action { get; init; }

    public required string Outcome { get; init; }
}

internal sealed class InstallJsonRecovery
{
    public required string State { get; init; }

    public required string? ResidualPath { get; init; }
}

internal sealed class InstallJsonFinding
{
    public required string Code { get; init; }

    public required string? Target { get; init; }

    public required string Cause { get; init; }
}

internal sealed class InstallJsonNext
{
    public required string Command { get; init; }

    public required string Reason { get; init; }
}
