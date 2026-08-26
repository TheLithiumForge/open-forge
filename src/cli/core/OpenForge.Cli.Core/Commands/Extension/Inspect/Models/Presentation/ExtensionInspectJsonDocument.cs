namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Presentation;

internal sealed class ExtensionInspectJsonDocument
{
    public required int SchemaVersion { get; init; }

    public required string Command { get; init; }

    public required string Status { get; init; }

    public required ExtensionInspectJsonWorkspace? Workspace { get; init; }

    public required ExtensionInspectJsonResult Result { get; init; }

    public required ExtensionInspectJsonNext? Next { get; init; }
}

internal sealed class ExtensionInspectJsonWorkspace
{
    public required string Path { get; init; }

    public required string SelectedBy { get; init; }
}

internal sealed class ExtensionInspectJsonResult
{
    public required ExtensionInspectJsonSubject Subject { get; init; }

    public required ExtensionInspectJsonSource Source { get; init; }

    public required ExtensionInspectJsonLifecycle Lifecycle { get; init; }

    public required ExtensionInspectJsonInstalled Installed { get; init; }

    public required ExtensionInspectJsonAvailable Available { get; init; }

    public required ExtensionInspectJsonDependencyClosure Dependencies { get; init; }

    public required ExtensionInspectJsonPathFacts PathFacts { get; init; }

    public required ExtensionInspectJsonComparison Comparison { get; init; }

    public required ExtensionInspectJsonGenerated Generated { get; init; }

    public required ExtensionInspectJsonFinding[] Findings { get; init; }

    public required ExtensionInspectJsonCounts Counts { get; init; }
}

internal sealed class ExtensionInspectJsonSubject
{
    public required string? Supplied { get; init; }

    public required string? Form { get; init; }

    public required string? Id { get; init; }

    public required string State { get; init; }

    public required ExtensionInspectJsonCandidate[] Candidates { get; init; }
}

internal sealed class ExtensionInspectJsonCandidate
{
    public required string Id { get; init; }

    public required string Path { get; init; }
}

internal sealed class ExtensionInspectJsonSource
{
    public required string? Supplied { get; init; }

    public required bool Explicit { get; init; }

    public required string? Identity { get; init; }

    public required string? Kind { get; init; }

    public required string State { get; init; }
}

internal sealed class ExtensionInspectJsonLifecycle
{
    public required string DocumentPath { get; init; }

    public required string ReadState { get; init; }

    public required string Trust { get; init; }

    public required string Coverage { get; init; }

    public required string WorkspaceBinding { get; init; }

    public required string? FingerprintPolicy { get; init; }
}

internal sealed class ExtensionInspectJsonInstalled
{
    public required string State { get; init; }

    public required ExtensionInspectJsonInstalledPackage? Package { get; init; }
}

internal sealed class ExtensionInspectJsonInstalledPackage
{
    public required string Id { get; init; }

    public required string? Version { get; init; }

    public required string? Source { get; init; }

    public required string[] Dependencies { get; init; }

    public required string[] Paths { get; init; }
}

internal sealed class ExtensionInspectJsonAvailable
{
    public required string State { get; init; }

    public required ExtensionInspectJsonAvailablePackage? Package { get; init; }
}

internal sealed class ExtensionInspectJsonAvailablePackage
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string Version { get; init; }

    public required string ManifestPath { get; init; }

    public required string[] Dependencies { get; init; }

    public required ExtensionInspectJsonPackageFile[] Payload { get; init; }
}

internal sealed class ExtensionInspectJsonPackageFile
{
    public required string Path { get; init; }

    public required string? TargetPath { get; init; }

    public required string State { get; init; }

    public required long? ByteLength { get; init; }

    public required string? Sha256 { get; init; }
}

internal sealed class ExtensionInspectJsonDependencyClosure
{
    public required string State { get; init; }

    public required ExtensionInspectJsonDependencyEdge[] Declared { get; init; }

    public required ExtensionInspectJsonDependencyPackage[] Resolved { get; init; }

    public required string[] Order { get; init; }
}

internal sealed class ExtensionInspectJsonDependencyEdge
{
    public required string From { get; init; }

    public required string To { get; init; }

    public required int Position { get; init; }
}

internal sealed class ExtensionInspectJsonDependencyPackage
{
    public required string Id { get; init; }

    public required string? Version { get; init; }

    public required string? Source { get; init; }

    public required string State { get; init; }
}

internal sealed class ExtensionInspectJsonPathFacts
{
    public required string State { get; init; }

    public required ExtensionInspectJsonDeclaredPath[] Declared { get; init; }

    public required ExtensionInspectJsonCurrentPath[] Current { get; init; }
}

internal sealed class ExtensionInspectJsonDeclaredPath
{
    public required string Path { get; init; }

    public required string? SourcePath { get; init; }

    public required string State { get; init; }
}

internal sealed class ExtensionInspectJsonCurrentPath
{
    public required string Path { get; init; }

    public required string State { get; init; }

    public required string? PhysicalIdentity { get; init; }

    public required long? ByteLength { get; init; }

    public required string? ExactSha256 { get; init; }
}

internal sealed class ExtensionInspectJsonComparison
{
    public required string State { get; init; }

    public required string Mode { get; init; }

    public required ExtensionInspectJsonComparisonSide Baseline { get; init; }

    public required ExtensionInspectJsonComparisonSide Current { get; init; }

    public required ExtensionInspectJsonComparisonSide Intended { get; init; }

    public required ExtensionInspectJsonPathComparison[] Paths { get; init; }

    public required ExtensionInspectJsonDependencyComparison Dependencies { get; init; }
}

internal sealed class ExtensionInspectJsonComparisonSide
{
    public required string State { get; init; }

    public required ExtensionInspectJsonFingerprintFact[] Fingerprints { get; init; }
}

internal sealed class ExtensionInspectJsonFingerprintFact
{
    public required string Path { get; init; }

    public required ExtensionInspectJsonFingerprint? Fingerprint { get; init; }
}

internal sealed class ExtensionInspectJsonFingerprint
{
    public required string Kind { get; init; }

    public required string? Policy { get; init; }

    public required string Sha256 { get; init; }

    public required string Origin { get; init; }
}

internal sealed class ExtensionInspectJsonPathComparison
{
    public required string Path { get; init; }

    public required ExtensionInspectJsonFingerprint? Baseline { get; init; }

    public required ExtensionInspectJsonFingerprint? Current { get; init; }

    public required ExtensionInspectJsonFingerprint? Intended { get; init; }

    public required string Relation { get; init; }

    public required string[] BaselineOwners { get; init; }

    public required string[] CurrentOwners { get; init; }

    public required string[] IntendedOwners { get; init; }
}

internal sealed class ExtensionInspectJsonDependencyComparison
{
    public required string State { get; init; }

    public required string[] Baseline { get; init; }

    public required string[] Current { get; init; }

    public required string[] Intended { get; init; }

    public required string Relation { get; init; }
}

internal sealed class ExtensionInspectJsonGenerated
{
    public required string State { get; init; }

    public required string Ownership { get; init; }

    public required ExtensionInspectJsonGeneratedRegion[] Regions { get; init; }
}

internal sealed class ExtensionInspectJsonGeneratedRegion
{
    public required string Path { get; init; }

    public required string State { get; init; }

    public required string? StartMarker { get; init; }

    public required string? EndMarker { get; init; }

    public required int? StartByteOffset { get; init; }

    public required int? EndByteOffset { get; init; }

    public required int? ExcludedInteriorByteLength { get; init; }

    public required bool MarkerLinesRetained { get; init; }
}

internal sealed class ExtensionInspectJsonFinding
{
    public required string Code { get; init; }

    public required string Status { get; init; }

    public required string? Subject { get; init; }

    public required string? PackageId { get; init; }

    public required string? Dependency { get; init; }

    public required string? Path { get; init; }

    public required string Cause { get; init; }

    public required ExtensionInspectJsonLocation? Location { get; init; }

    public required ExtensionInspectJsonCandidate[] Candidates { get; init; }
}

internal sealed class ExtensionInspectJsonLocation
{
    public required int Line { get; init; }

    public required int Column { get; init; }

    public required long ByteOffset { get; init; }

    public required long ByteLength { get; init; }
}

internal sealed class ExtensionInspectJsonCounts
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

internal sealed class ExtensionInspectJsonNext
{
    public required string Command { get; init; }

    public required string Reason { get; init; }
}
