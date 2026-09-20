namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;

internal enum ExtensionInspectSubjectForm
{
    StableId,
}

internal enum ExtensionInspectSubjectState
{
    NotStarted,
    Resolved,
    Invalid,
    Unknown,
    Ambiguous,
    Unsafe,
}

internal enum ExtensionInspectSourceKind
{
    EmbeddedCatalogue,
    Package,
    Catalogue,
}

internal enum ExtensionInspectSourceState
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

internal enum ExtensionInspectLifecycleReadState
{
    NotStarted,
    Complete,
    Missing,
    Invalid,
    Unavailable,
    Blocked,
    Failed,
    Interrupted,
}

internal enum ExtensionInspectLifecycleTrust
{
    NotStarted,
    Trusted,
    Untrusted,
    Incomplete,
    Blocked,
    Absent,
}

internal enum ExtensionInspectCoverageState
{
    NotStarted,
    Complete,
    Incomplete,
    Invalid,
    Blocked,
    Failed,
    Interrupted,
}

internal sealed record ExtensionInspectCandidate
{
    public required string Id { get; init; }

    public required string Path { get; init; }
}

internal sealed record ExtensionInspectSubject
{
    public required string? Supplied { get; init; }

    public required ExtensionInspectSubjectForm? Form { get; init; }

    public required string? Id { get; init; }

    public required ExtensionInspectSubjectState State { get; init; }

    public required IReadOnlyList<ExtensionInspectCandidate> Candidates { get; init; }
}

internal sealed record ExtensionInspectSource
{
    public required string? Supplied { get; init; }

    public required bool Explicit { get; init; }

    public required string? Identity { get; init; }

    public required ExtensionInspectSourceKind? Kind { get; init; }

    public required ExtensionInspectSourceState State { get; init; }
}

internal sealed record ExtensionInspectLifecycle
{
    public required string DocumentPath { get; init; }

    public required ExtensionInspectLifecycleReadState ReadState { get; init; }

    public required ExtensionInspectLifecycleTrust Trust { get; init; }

    public required ExtensionInspectCoverageState Coverage { get; init; }

}
