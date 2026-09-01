namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;

internal enum ExtensionInspectComparisonState
{
    NotStarted,
    Complete,
    Incomplete,
    Invalid,
    Blocked,
    Failed,
    Interrupted,
}

internal enum ExtensionInspectComparisonMode
{
    None,
    InstalledOnly,
    AvailableOnly,
    ThreeWay,
}

internal enum ExtensionInspectComparisonSideState
{
    NotStarted,
    NotApplicable,
    Available,
    Unavailable,
    Invalid,
    Blocked,
    Failed,
    Interrupted,
}

internal enum ExtensionInspectPathRelation
{
    NotStarted,
    NotApplicable,
    Unchanged,
    Changed,
    CurrentDiverged,
    Missing,
    New,
    Retired,
    Shared,
    Unknown,
    Unavailable,
    Invalid,
    Blocked,
}

internal enum ExtensionInspectDependencyComparisonState
{
    NotStarted,
    NotApplicable,
    Available,
    Unavailable,
    Invalid,
    Blocked,
    Failed,
    Interrupted,
}

internal enum ExtensionInspectDependencyRelation
{
    NotStarted,
    NotApplicable,
    Equal,
    Changed,
    Unavailable,
    Invalid,
    Blocked,
}

internal enum ExtensionInspectGeneratedState
{
    NotStarted,
    Complete,
    Incomplete,
    Invalid,
    Blocked,
    Failed,
    Interrupted,
}

internal enum ExtensionInspectGeneratedRegionState
{
    NotStarted,
    Valid,
    Absent,
    Invalid,
    Ambiguous,
    Unavailable,
}

internal enum ExtensionInspectFingerprintKind
{
    Semantic,
    ExactBytes,
}

internal enum ExtensionInspectFingerprintOrigin
{
    PersistedBaseline,
    OperationTimeCurrent,
    OperationTimeIntended,
}

internal sealed record ExtensionInspectFingerprint
{
    public required ExtensionInspectFingerprintKind Kind { get; init; }

    public required string? Policy { get; init; }

    public required string Sha256 { get; init; }

    public required ExtensionInspectFingerprintOrigin Origin { get; init; }
}

internal sealed record ExtensionInspectFingerprintFact
{
    public required string Path { get; init; }

    public required ExtensionInspectFingerprint? Fingerprint { get; init; }
}

internal sealed record ExtensionInspectComparisonSide
{
    public required ExtensionInspectComparisonSideState State { get; init; }

    public required IReadOnlyList<ExtensionInspectFingerprintFact> Fingerprints { get; init; }
}

internal sealed record ExtensionInspectPathComparison
{
    public required string Path { get; init; }

    public required ExtensionInspectFingerprint? Baseline { get; init; }

    public required ExtensionInspectFingerprint? Current { get; init; }

    public required ExtensionInspectFingerprint? Intended { get; init; }

    public required ExtensionInspectPathRelation Relation { get; init; }

    public required IReadOnlyList<string> BaselineOwners { get; init; }

    public required IReadOnlyList<string> CurrentOwners { get; init; }

    public required IReadOnlyList<string> IntendedOwners { get; init; }
}

internal sealed record ExtensionInspectDependencyComparison
{
    public required ExtensionInspectDependencyComparisonState State { get; init; }

    public required IReadOnlyList<string> Baseline { get; init; }

    public required IReadOnlyList<string> Current { get; init; }

    public required IReadOnlyList<string> Intended { get; init; }

    public required ExtensionInspectDependencyRelation Relation { get; init; }
}

internal sealed record ExtensionInspectComparison
{
    public required ExtensionInspectComparisonState State { get; init; }

    public required ExtensionInspectComparisonMode Mode { get; init; }

    public required ExtensionInspectComparisonSide Baseline { get; init; }

    public required ExtensionInspectComparisonSide Current { get; init; }

    public required ExtensionInspectComparisonSide Intended { get; init; }

    public required IReadOnlyList<ExtensionInspectPathComparison> Paths { get; init; }

    public required ExtensionInspectDependencyComparison Dependencies { get; init; }
}

internal sealed record ExtensionInspectGeneratedRegion
{
    public required string Path { get; init; }

    public required ExtensionInspectGeneratedRegionState State { get; init; }

    public required string? StartMarker { get; init; }

    public required string? EndMarker { get; init; }

    public required int? StartByteOffset { get; init; }

    public required int? EndByteOffset { get; init; }

    public required int? ExcludedInteriorByteLength { get; init; }

    public required bool MarkerLinesRetained { get; init; }
}

internal sealed record ExtensionInspectGenerated
{
    public required ExtensionInspectGeneratedState State { get; init; }

    public required string Ownership { get; init; }

    public required IReadOnlyList<ExtensionInspectGeneratedRegion> Regions { get; init; }
}
