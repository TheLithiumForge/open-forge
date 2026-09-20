namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;

internal enum ExtensionInspectPathState
{
    NotStarted,
    Complete,
    Incomplete,
    Invalid,
    Blocked,
    Failed,
    Interrupted,
}

internal enum ExtensionInspectDeclaredPathState
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

internal enum ExtensionInspectCurrentPathState
{
    NotStarted,
    Present,
    Missing,
    Invalid,
    Blocked,
    Unavailable,
    Failed,
    Interrupted,
}

internal sealed record ExtensionInspectDeclaredPath
{
    public required string Path { get; init; }

    public required string? SourcePath { get; init; }

    public required ExtensionInspectDeclaredPathState State { get; init; }
}

internal sealed record ExtensionInspectCurrentPath
{
    public required string Path { get; init; }

    public required ExtensionInspectCurrentPathState State { get; init; }

    public required string? PhysicalIdentity { get; init; }

    public required long? ByteLength { get; init; }

    public required string? ExactSha256 { get; init; }

    internal ReadOnlyMemory<byte>? Bytes { get; init; }
}

internal sealed record ExtensionInspectPathFacts
{
    public required ExtensionInspectPathState State { get; init; }

    public required IReadOnlyList<ExtensionInspectDeclaredPath> Declared { get; init; }

    public required IReadOnlyList<ExtensionInspectCurrentPath> Current { get; init; }
}
