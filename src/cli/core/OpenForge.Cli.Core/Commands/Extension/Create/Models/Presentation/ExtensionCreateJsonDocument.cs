namespace OpenForge.Cli.Core.Commands.Extension.Create.Models.Presentation;

internal sealed class ExtensionCreateJsonDocument
{
    public required int SchemaVersion { get; init; }

    public required string Command { get; init; }

    public required string Status { get; init; }

    public required ExtensionCreateJsonWorkspace? Workspace { get; init; }

    public required ExtensionCreateJsonResult Result { get; init; }

    public required ExtensionCreateJsonNext? Next { get; init; }
}

internal sealed class ExtensionCreateJsonWorkspace
{
    public required string Path { get; init; }

    public required string SelectedBy { get; init; }
}

internal sealed class ExtensionCreateJsonResult
{
    public required string? Catalogue { get; init; }

    public required string? Destination { get; init; }

    public required string? Id { get; init; }

    public required ExtensionCreateJsonManifest? Manifest { get; init; }

    public required string Mode { get; init; }

    public required ExtensionCreateJsonEffect[] IntendedEffects { get; init; }

    public required ExtensionCreateJsonEffect[] AppliedEffects { get; init; }

    public required ExtensionCreateJsonVerification Verification { get; init; }

    public required bool WorkspaceLifecycleChanged { get; init; }
}

internal sealed class ExtensionCreateJsonManifest
{
    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string Version { get; init; }

    public required string[] Dependencies { get; init; }
}

internal sealed class ExtensionCreateJsonEffect
{
    public required string Kind { get; init; }

    public required string Path { get; init; }
}

internal sealed class ExtensionCreateJsonVerification
{
    public required string Catalogue { get; init; }

    public required string Destination { get; init; }

    public required string Manifest { get; init; }

    public required string Payload { get; init; }

    public required string? Cause { get; init; }
}

internal sealed class ExtensionCreateJsonNext
{
    public required string Command { get; init; }

    public required string Reason { get; init; }
}
