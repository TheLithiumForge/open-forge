namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;

internal enum ExtensionInspectInstalledState
{
    NotStarted,
    Present,
    Absent,
    Unavailable,
    Invalid,
    Blocked,
    Failed,
    Interrupted,
}

internal enum ExtensionInspectAvailableState
{
    NotStarted,
    Present,
    Absent,
    Unavailable,
    Invalid,
    Blocked,
    Failed,
    Interrupted,
}

internal enum ExtensionInspectPackageFileState
{
    NotStarted,
    Available,
    Missing,
    Invalid,
    Blocked,
    Unavailable,
    Failed,
    Interrupted,
}

internal enum ExtensionInspectDependencyState
{
    NotStarted,
    Complete,
    Incomplete,
    Invalid,
    Blocked,
    Failed,
    Interrupted,
}

internal enum ExtensionInspectDependencyPackageState
{
    NotStarted,
    Available,
    Missing,
    Invalid,
    Blocked,
    Unavailable,
    Failed,
    Interrupted,
}

internal sealed record ExtensionInspectInstalledPackage
{
    public required string Id { get; init; }

    public required string? Version { get; init; }

    public required string? Source { get; init; }

    public required IReadOnlyList<string> Dependencies { get; init; }

    public required IReadOnlyList<string> Paths { get; init; }
}

internal sealed record ExtensionInspectInstalled
{
    public required ExtensionInspectInstalledState State { get; init; }

    public required ExtensionInspectInstalledPackage? Package { get; init; }
}

internal sealed record ExtensionInspectPackageFile
{
    public required string Path { get; init; }

    public required string? TargetPath { get; init; }

    public required ExtensionInspectPackageFileState State { get; init; }

    public required long? ByteLength { get; init; }

    public required string? Sha256 { get; init; }

    internal ReadOnlyMemory<byte>? Bytes { get; init; }
}

internal sealed record ExtensionInspectAvailablePackage
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string Version { get; init; }

    public required string ManifestPath { get; init; }

    public required IReadOnlyList<string> Dependencies { get; init; }

    public required IReadOnlyList<ExtensionInspectPackageFile> Payload { get; init; }
}

internal sealed record ExtensionInspectAvailable
{
    public required ExtensionInspectAvailableState State { get; init; }

    public required ExtensionInspectAvailablePackage? Package { get; init; }
}

internal sealed record ExtensionInspectDependencyEdge
{
    public required string From { get; init; }

    public required string To { get; init; }

    public required int Position { get; init; }
}

internal sealed record ExtensionInspectDependencyPackage
{
    public required string Id { get; init; }

    public required string? Version { get; init; }

    public required string? Source { get; init; }

    public required ExtensionInspectDependencyPackageState State { get; init; }
}

internal sealed record ExtensionInspectDependencyClosure
{
    public required ExtensionInspectDependencyState State { get; init; }

    public required IReadOnlyList<ExtensionInspectDependencyEdge> Declared { get; init; }

    public required IReadOnlyList<ExtensionInspectDependencyPackage> Resolved { get; init; }

    public required IReadOnlyList<string> Order { get; init; }
}
