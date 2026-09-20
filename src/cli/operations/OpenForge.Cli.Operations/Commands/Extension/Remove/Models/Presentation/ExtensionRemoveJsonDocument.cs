using OpenForge.Cli.Core.Commands.Shared.Permissions.Models;
namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Presentation;

internal sealed class ExtensionRemoveJsonDocument
{
    public required int SchemaVersion { get; init; }

    public required string Command { get; init; }

    public required string Status { get; init; }

    public required ExtensionRemoveJsonWorkspace? Workspace { get; init; }

    public required ExtensionRemoveJsonResult Result { get; init; }

    public required ExtensionRemoveJsonNext? Next { get; init; }
}

internal sealed class ExtensionRemoveJsonWorkspace
{
    public required string Path { get; init; }

    public required string SelectedBy { get; init; }
}

internal sealed class ExtensionRemoveJsonResult
{
    public required string Mode { get; init; }


    public required bool Automatic { get; init; }

    public required ExtensionRemoveJsonSelection? Selection { get; init; }

    public required ExtensionRemoveJsonDependencyPlan? Dependencies { get; init; }

    public required ExtensionRemoveJsonPath[] Paths { get; init; }

    public required ExtensionRemoveJsonNavigation? GeneratedNavigation { get; init; }

    public required ExtensionRemoveJsonEffect[] Effects { get; init; }

    public required WorkspacePermissionJson Permissions { get; init; }
    public required ExtensionRemoveJsonLifecycle Lifecycle { get; init; }

    public required ExtensionRemoveJsonRecovery Recovery { get; init; }

    public required ExtensionRemoveJsonVerification Verification { get; init; }

    public required bool PackageSourceUnchanged { get; init; }

    public required ExtensionRemoveJsonFinding[] Findings { get; init; }
}

internal sealed class ExtensionRemoveJsonSelection
{
    public required string SelectedBy { get; init; }

    public required string[] Ids { get; init; }
}

internal sealed class ExtensionRemoveJsonDependencyPlan
{
    public required ExtensionRemoveJsonPackage[] Packages { get; init; }

    public required string[] RemovalOrder { get; init; }

    public required ExtensionRemoveJsonRetainedDependentBlocker[] RetainedDependentBlockers { get; init; }

    public required string[] RetainedOrphanDependencyIds { get; init; }
}

internal sealed class ExtensionRemoveJsonPackage
{
    public required string Id { get; init; }

    public required bool SelectedForRemoval { get; init; }

    public required string[] Dependencies { get; init; }
}

internal sealed class ExtensionRemoveJsonRetainedDependentBlocker
{
    public required string DependencyId { get; init; }

    public required string[] RetainedDependentIds { get; init; }
}

internal sealed class ExtensionRemoveJsonPath
{
    public required string Path { get; init; }

    public required string Classification { get; init; }

    public required string[] SelectedOwnerIds { get; init; }

    public required string[] RemainingOwnerIds { get; init; }

    public required string Action { get; init; }
}

internal sealed class ExtensionRemoveJsonNavigation
{
    public required ExtensionRemoveJsonRegion[] Regions { get; init; }
}

internal sealed class ExtensionRemoveJsonRegion
{
    public required string Path { get; init; }

    public required string State { get; init; }
}

internal sealed class ExtensionRemoveJsonEffect
{
    public required string Path { get; init; }

    public required string? PackageId { get; init; }

    public required string Kind { get; init; }

    public required string Action { get; init; }

    public required string Outcome { get; init; }

    public required string Residual { get; init; }
}

internal sealed class ExtensionRemoveJsonLifecycle
{
    public required string Trust { get; init; }

    public required string Coverage { get; init; }

    public required string Action { get; init; }

    public required string Outcome { get; init; }
}

internal sealed class ExtensionRemoveJsonRecovery
{
    public required string State { get; init; }

    public required string[] ProtectedPaths { get; init; }

    public required string? ResidualPath { get; init; }
}

internal sealed class ExtensionRemoveJsonVerification
{
    public required string Targets { get; init; }

    public required string Topology { get; init; }

    public required string ExtensionsLifecycle { get; init; }
}

internal sealed class ExtensionRemoveJsonFinding
{
    public required string Code { get; init; }

    public required string Status { get; init; }

    public required string? Target { get; init; }

    public required string Cause { get; init; }
}

internal sealed class ExtensionRemoveJsonNext
{
    public required string Command { get; init; }

    public required string Reason { get; init; }
}
