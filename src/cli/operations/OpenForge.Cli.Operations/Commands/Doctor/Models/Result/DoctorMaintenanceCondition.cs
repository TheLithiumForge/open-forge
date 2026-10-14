namespace OpenForge.Cli.Core.Commands.Doctor.Models.Result;

internal enum DoctorMaintenanceCondition
{
    MissingFields,
    ChainGap,
    OutsideFolders,
    ProvenRename,
    SeveralStrongCandidates,
    WeakOnly,
    NoCandidate,
    MalformedFrontmatter,
    OldNavigation,
    ManagedMetadata,
    NativeMetadata,
    RestorationBoundary,
    UnsafeBoundary,
    ProtectedReference,
    RenamedFragmentMissing,
    NativeResourceMetadata,
}

internal sealed record DoctorMaintenanceAdvice
{
    public required DoctorMaintenanceCondition Condition { get; init; }
    public string? OldPath { get; init; }
    public string? NewPath { get; init; }
    public string? CanonicalName { get; init; }
    public string? ExistingCanonicalPath { get; init; }
    public string? IndexCommand { get; init; }
    public string? Cause { get; init; }
    public bool Moved { get; init; }
    public bool SameContent { get; init; }
    public int? Similarity { get; init; }
    public string? ProtectedPath { get; init; }
    public bool ProtectedByNativePackage { get; init; }
    public string? Fragment { get; init; }
    public bool PreserveRenameIdentity { get; init; }
}
