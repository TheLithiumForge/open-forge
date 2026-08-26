using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

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

internal enum ExtensionInspectWorkspaceBinding
{
    NotChecked,
    Matched,
    Mismatched,
    Unavailable,
}

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

internal enum ExtensionInspectFindingCode
{
    InvalidInput,
    InvalidStableId,
    WorkspaceUnavailable,
    WorkspaceUnsafe,
    SourceUnavailable,
    SourceInvalid,
    SourceOverlap,
    SourceAmbiguous,
    IdentityAmbiguous,
    LifecycleUnavailable,
    LifecycleInvalid,
    LifecycleBlocked,
    PackageUnavailable,
    PackageInvalid,
    DependencyIncomplete,
    DependencyCycle,
    DependencyConflict,
    PathUnavailable,
    PathInvalid,
    OwnershipConflict,
    FingerprintUnavailable,
    FingerprintFallback,
    GeneratedBoundaryInvalid,
    DependencyChanged,
    PathChanged,
    PathCurrentDiverged,
    PathMissing,
    PathNew,
    PathRetired,
    OperationFailed,
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

    public required ExtensionInspectWorkspaceBinding WorkspaceBinding { get; init; }

    public required string? FingerprintPolicy { get; init; }
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

internal sealed record ExtensionInspectFinding
{
    public required ExtensionInspectFindingCode Code { get; init; }

    public required CliSemanticStatus Status { get; init; }

    public required string? Subject { get; init; }

    public required string? PackageId { get; init; }

    public required string? Dependency { get; init; }

    public required string? Path { get; init; }

    public required string Cause { get; init; }

    public required SourceLocation? Location { get; init; }

    public required IReadOnlyList<ExtensionInspectCandidate> Candidates { get; init; }
}

internal sealed record ExtensionInspectCounts
{
    public required int? InstalledPackages { get; init; }

    public required int? AvailablePackages { get; init; }

    public required int? DeclaredPaths { get; init; }

    public required int? CurrentPaths { get; init; }

    public required int? BaselinePaths { get; init; }

    public required int? IntendedPaths { get; init; }

    public required int? Dependencies { get; init; }

    public required int? UnchangedPaths { get; init; }

    public required int? ChangedPaths { get; init; }

    public required int? CurrentDivergedPaths { get; init; }

    public required int? MissingPaths { get; init; }

    public required int? NewPaths { get; init; }

    public required int? RetiredPaths { get; init; }

    public required int? SharedPaths { get; init; }

    public required int? GeneratedRegions { get; init; }

    public required int? ExcludedGeneratedBytes { get; init; }

    public required int Findings { get; init; }
}

internal sealed record ExtensionInspectResult : ICliCommandResult
{
    public required CliSemanticStatus Status { get; init; }

    public required CliWorkspace? Workspace { get; init; }

    public required ExtensionInspectSubject Subject { get; init; }

    public required ExtensionInspectSource Source { get; init; }

    public required ExtensionInspectLifecycle Lifecycle { get; init; }

    public required ExtensionInspectInstalled Installed { get; init; }

    public required ExtensionInspectAvailable Available { get; init; }

    public required ExtensionInspectDependencyClosure Dependencies { get; init; }

    public required ExtensionInspectPathFacts PathFacts { get; init; }

    public required ExtensionInspectComparison Comparison { get; init; }

    public required ExtensionInspectGenerated Generated { get; init; }

    public required IReadOnlyList<ExtensionInspectFinding> Findings { get; init; }

    public required ExtensionInspectCounts Counts { get; init; }

    public required CliNextAction? Next { get; init; }

    public string Command => ExtensionInspectDefinitions.CommandIdentity;
}
