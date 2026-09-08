using OpenForge.Cli.Core.Framework.Permissions.Models.Presentation;
namespace OpenForge.Cli.Core.Commands.Extension.Install.Models.Presentation;

internal sealed class ExtensionInstallJsonDocument
{
    public required int SchemaVersion { get; init; }
    public required string Command { get; init; }
    public required string Status { get; init; }
    public required ExtensionInstallJsonWorkspace? Workspace { get; init; }
    public required ExtensionInstallJsonResult Result { get; init; }
    public required ExtensionInstallJsonNext? Next { get; init; }
}

internal sealed class ExtensionInstallJsonWorkspace
{
    public required string Path { get; init; }
    public required string SelectedBy { get; init; }
}

internal sealed class ExtensionInstallJsonResult
{
    public required string Mode { get; init; }
    public required bool Force { get; init; }
    public required bool Automatic { get; init; }
    public required ExtensionInstallJsonSelection? Selection { get; init; }
    public required ExtensionInstallJsonSource? Source { get; init; }
    public required ExtensionInstallJsonPackage[] Packages { get; init; }
    public required ExtensionInstallJsonFramework? Framework { get; init; }
    public required ExtensionInstallJsonFootprint? Footprint { get; init; }
    public required ExtensionInstallJsonEffect[] Effects { get; init; }
    public required ExtensionInstallJsonNavigation? GeneratedNavigation { get; init; }
    public required WorkspacePermissionJson Permissions { get; init; }
    public required ExtensionInstallJsonLifecycle Lifecycle { get; init; }
    public required ExtensionInstallJsonRecovery Recovery { get; init; }
    public required ExtensionInstallJsonVerification Verification { get; init; }
    public required ExtensionInstallJsonFinding[] Findings { get; init; }
}

internal sealed class ExtensionInstallJsonSelection
{
    public required string SelectedBy { get; init; }
    public required string[] RootIds { get; init; }
}

internal sealed class ExtensionInstallJsonSource
{
    public required string Kind { get; init; }
    public required string? Path { get; init; }
    public required string Identity { get; init; }
    public required int PackageCount { get; init; }
}

internal sealed class ExtensionInstallJsonPackage
{
    public required string Id { get; init; }
    public required bool SelectedRoot { get; init; }
    public required string[] Dependencies { get; init; }
}

internal sealed class ExtensionInstallJsonFramework
{
    public required string InventoryFingerprint { get; init; }
    public required int TargetCount { get; init; }
    public required int GeneratedRegionCount { get; init; }
}

internal sealed class ExtensionInstallJsonFootprint
{
    public required int PackageCount { get; init; }
    public required string[] PayloadTargets { get; init; }
    public required string[] GeneratedRegions { get; init; }
    public required string[] Directories { get; init; }
}

internal sealed class ExtensionInstallJsonEffect
{
    public required string Path { get; init; }
    public required string? PackageId { get; init; }
    public required string Kind { get; init; }
    public required string Action { get; init; }
    public required string Outcome { get; init; }
    public required string Residual { get; init; }
}

internal sealed class ExtensionInstallJsonNavigation
{
    public required ExtensionInstallJsonRegion[] Regions { get; init; }
}

internal sealed class ExtensionInstallJsonRegion
{
    public required string Path { get; init; }
    public required string State { get; init; }
}

internal sealed class ExtensionInstallJsonLifecycle
{
    public required string Action { get; init; }
    public required string Outcome { get; init; }
}

internal sealed class ExtensionInstallJsonRecovery
{
    public required string State { get; init; }
    public required string[] ProtectedPaths { get; init; }
    public required string? ResidualPath { get; init; }
}

internal sealed class ExtensionInstallJsonVerification
{
    public required string Targets { get; init; }
    public required string Topology { get; init; }
    public required string ExtensionsLifecycle { get; init; }
    public required string FrameworkLifecycle { get; init; }
}

internal sealed class ExtensionInstallJsonFinding
{
    public required string Code { get; init; }
    public required string Status { get; init; }
    public required string? Target { get; init; }
    public required string Cause { get; init; }
}

internal sealed class ExtensionInstallJsonNext
{
    public required string Command { get; init; }
    public required string Reason { get; init; }
}
