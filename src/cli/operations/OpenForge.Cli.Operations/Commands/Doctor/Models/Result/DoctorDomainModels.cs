using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Models.Result;

internal enum DoctorDomainKind
{
    WorkspaceEntry,
    RecoveryResiduals,
    RoutesMetadataOverwritesGeneratedNavigation,
    LocalReferences,
    FrameworkLifecycle,
    ExtensionLifecycle,
}

internal enum DoctorCoverageState
{
    Complete,
    Incomplete,
    Blocked,
}

internal enum DoctorBoundaryKind
{
    Workspace,
    RecoveryStore,
    RouteUniverse,
    LocalReferenceUniverse,
    FrameworkLifecycle,
    ExtensionLifecycle,
}

internal enum DoctorLimitationKind
{
    Unavailable,
    Unsupported,
    Incomplete,
    Blocked,
}

internal sealed record DoctorBoundary
{
    public required DoctorBoundaryKind Kind { get; init; }

    public required string? Path { get; init; }
}

internal sealed record DoctorLimitation
{
    public required DoctorLimitationKind Kind { get; init; }

    public required string Message { get; init; }
}

internal sealed record DoctorDomainReport
{
    public LibraryDoctorView? Libraries { get; init; }

    public required DoctorDomainKind Domain { get; init; }

    public required DoctorBoundary Boundary { get; init; }

    public required DoctorCoverageState Coverage { get; init; }

    public required OperationalLifecycleState? Lifecycle { get; init; }

    public required OperationalSourceAvailability? SourceAvailability { get; init; }

    public required IReadOnlyList<DoctorLimitation> Limitations { get; init; }

    public required DoctorFindingCounts Counts { get; init; }

    public required IReadOnlyList<DoctorFinding> Findings { get; init; }

    public required IReadOnlyList<DoctorNextAction> Actions { get; init; }
}

internal sealed record DoctorFindingCounts
{
    public required DoctorResolutionCounts Resolution { get; init; }

    public required DoctorSeverityCounts Severity { get; init; }
}

internal sealed record DoctorResolutionCounts
{
    public required DoctorCount SafeExact { get; init; }

    public required DoctorCount GuidedChoice { get; init; }

    public required DoctorCount TargetedOperation { get; init; }

    public required DoctorCount ManualDecision { get; init; }

    public required DoctorCount BlockedRepair { get; init; }

    public required DoctorCount Informational { get; init; }
}

internal sealed record DoctorSeverityCounts
{
    public required DoctorCount Information { get; init; }

    public required DoctorCount Warning { get; init; }

    public required DoctorCount Error { get; init; }
}

internal sealed record DoctorCount
{
    public required OperationalValueState State { get; init; }

    public required long? Value { get; init; }
}

internal sealed record DoctorDiagnosis
{
    public static bool ReadOnly => true;

    public static bool ChangesMade => false;

    public required DoctorCoverageState Coverage { get; init; }

    public required DoctorFindingCounts Counts { get; init; }

    public DoctorCoverageCounts? CoverageCounts { get; init; }

    public required IReadOnlyList<DoctorNextAction> Actions { get; init; }

    public required IReadOnlyList<DoctorDomainReport> Domains { get; init; }
}

internal sealed record DoctorCoverageCounts
{
    public required long? Checks { get; init; }

    public required long? ChecksComplete { get; init; }

    public required long? LinksChecked { get; init; }

    public required long? LinksValid { get; init; }

    public required long? ExternalLinksNotChecked { get; init; }

    public required long? ImageLinks { get; init; }

    public required long? RoutesChecked { get; init; }

    public required long? FrameworkFiles { get; init; }

    public required long? ExtensionsInstalled { get; init; }

    public required long? LibrariesRegistered { get; init; }
}
