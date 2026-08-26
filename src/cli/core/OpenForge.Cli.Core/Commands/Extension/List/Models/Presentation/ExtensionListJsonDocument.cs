namespace OpenForge.Cli.Core.Commands.Extension.List.Models.Presentation;

internal sealed class ExtensionListJsonDocument
{
    public required int SchemaVersion { get; init; }

    public required string Command { get; init; }

    public required string Status { get; init; }

    public required ExtensionListJsonWorkspace? Workspace { get; init; }

    public required ExtensionListJsonResult Result { get; init; }

    public required ExtensionListJsonNext? Next { get; init; }
}

internal sealed class ExtensionListJsonWorkspace
{
    public required string Path { get; init; }

    public required string SelectedBy { get; init; }
}

internal sealed class ExtensionListJsonResult
{
    public required ExtensionListJsonSource? Source { get; init; }

    public required ExtensionListJsonSelection Selection { get; init; }

    public required ExtensionListJsonCoverage Coverage { get; init; }

    public required ExtensionListJsonInstalledRow[] Installed { get; init; }

    public required ExtensionListJsonAvailableRow[] Available { get; init; }

    public required ExtensionListJsonFinding[] Findings { get; init; }

    public required ExtensionListJsonCounts Counts { get; init; }
}

internal sealed class ExtensionListJsonSource
{
    public required string Identity { get; init; }

    public required string? Kind { get; init; }

    public required string State { get; init; }
}

internal sealed class ExtensionListJsonSelection
{
    public required bool Installed { get; init; }

    public required bool Available { get; init; }
}

internal sealed class ExtensionListJsonCoverage
{
    public required string Installed { get; init; }

    public required string Available { get; init; }

    public required string? LifecycleTrust { get; init; }
}

internal sealed class ExtensionListJsonInstalledRow
{
    public required string Id { get; init; }

    public required string? Version { get; init; }

    public required string Trust { get; init; }

    public required int ManagedPathCount { get; init; }

    public required bool SourceAvailable { get; init; }
}

internal sealed class ExtensionListJsonAvailableRow
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string Version { get; init; }

    public required int PackageCount { get; init; }

    public required int DependencyCount { get; init; }
}

internal sealed class ExtensionListJsonFinding
{
    public required string Code { get; init; }

    public required string Status { get; init; }

    public required string? Subject { get; init; }

    public required string Cause { get; init; }
}

internal sealed class ExtensionListJsonCounts
{
    public required int Installed { get; init; }

    public required int Available { get; init; }
}

internal sealed class ExtensionListJsonNext
{
    public required string Command { get; init; }

    public required string Reason { get; init; }
}
