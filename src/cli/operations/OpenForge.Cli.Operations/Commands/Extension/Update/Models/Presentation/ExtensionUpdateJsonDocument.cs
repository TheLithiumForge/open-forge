using OpenForge.Cli.Core.Commands.Shared.Permissions.Models;
namespace OpenForge.Cli.Core.Commands.Extension.Update.Models.Presentation;

internal sealed class ExtensionUpdateJsonDocument
{
    public required int SchemaVersion { get; init; }

    public required string Command { get; init; }

    public required string Status { get; init; }

    public required ExtensionUpdateJsonWorkspace? Workspace { get; init; }

    public required ExtensionUpdateJsonResult Result { get; init; }

    public required ExtensionUpdateJsonNext? Next { get; init; }
}

internal sealed class ExtensionUpdateJsonWorkspace
{
    public required string Path { get; init; }

    public required string SelectedBy { get; init; }
}

internal sealed class ExtensionUpdateJsonResult
{
    public required string Mode { get; init; }

    public required bool Force { get; init; }

    public required bool Prune { get; init; }

    public required bool Automatic { get; init; }

    public required ExtensionUpdateJsonSelection? Selection { get; init; }

    public required ExtensionUpdateJsonSource? Source { get; init; }

    public required ExtensionUpdateJsonPackage[] Packages { get; init; }

    public required ExtensionUpdateJsonComparison[] Comparisons { get; init; }

    public required ExtensionUpdateJsonNavigation? GeneratedNavigation { get; init; }

    public required ExtensionUpdateJsonEffect[] Effects { get; init; }

    public required WorkspacePermissionJson Permissions { get; init; }
    public required ExtensionUpdateJsonLifecycle Lifecycle { get; init; }

    public required ExtensionUpdateJsonRecovery Recovery { get; init; }

    public required ExtensionUpdateJsonVerification Verification { get; init; }

    public required ExtensionUpdateJsonFinding[] Findings { get; init; }
}

internal sealed class ExtensionUpdateJsonSelection
{
    public required string SelectedBy { get; init; }

    public required string[] RootIds { get; init; }
}

internal sealed class ExtensionUpdateJsonSource
{
    public required string Kind { get; init; }

    public required string? Path { get; init; }

    public required string Identity { get; init; }

    public required int PackageCount { get; init; }
}

internal sealed class ExtensionUpdateJsonPackage
{
    public required string Id { get; init; }

    public required bool SelectedRoot { get; init; }

    public required string[] Dependencies { get; init; }
}

internal sealed class ExtensionUpdateJsonComparison
{
    public required string Path { get; init; }

    public required string PackageId { get; init; }

    public required string Kind { get; init; }

    public required string? Region { get; init; }

    public required string? SourceAssetPath { get; init; }

    public required string? CurrentFingerprint { get; init; }

    public required string? IntendedFingerprint { get; init; }

    public required string CurrentState { get; init; }

    public required string IntendedState { get; init; }

    public required string RetirementEligibility { get; init; }
}

internal sealed class ExtensionUpdateJsonNavigation
{
    public required ExtensionUpdateJsonRegion[] Regions { get; init; }
}

internal sealed class ExtensionUpdateJsonRegion
{
    public required string Path { get; init; }

    public required string State { get; init; }
}

internal sealed class ExtensionUpdateJsonEffect
{
    public required string Path { get; init; }

    public required string? PackageId { get; init; }

    public required string Kind { get; init; }

    public required string Action { get; init; }

    public required ExtensionUpdateJsonChange[] Changes { get; init; }

    public required string Outcome { get; init; }

    public required string Residual { get; init; }
}

internal sealed class ExtensionUpdateJsonChange
{
    public required string Kind { get; init; }

    public required string Action { get; init; }

    public required string? Region { get; init; }

    public required string? SourceAssetPath { get; init; }
}

internal sealed class ExtensionUpdateJsonLifecycle
{
    public required string Trust { get; init; }

    public required string Coverage { get; init; }

    public required string Action { get; init; }

    public required string Outcome { get; init; }
}

internal sealed class ExtensionUpdateJsonRecovery
{
    public required string State { get; init; }

    public required string[] ProtectedPaths { get; init; }

    public required string? ResidualPath { get; init; }
}

internal sealed class ExtensionUpdateJsonVerification
{
    public required string Targets { get; init; }

    public required string Topology { get; init; }

    public required string ExtensionsLifecycle { get; init; }
}

internal sealed class ExtensionUpdateJsonFinding
{
    public required string Code { get; init; }

    public required string Status { get; init; }

    public required string? Target { get; init; }

    public required string Cause { get; init; }
}

internal sealed class ExtensionUpdateJsonNext
{
    public required string Command { get; init; }

    public required string Reason { get; init; }
}
