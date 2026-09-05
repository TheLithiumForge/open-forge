using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Models.Result;

internal enum DoctorEvidenceKind
{
    Availability,
    State,
    Comparison,
    Integrity,
    AuthoredValue,
    CandidateBasis,
}

internal enum DoctorObservedState
{
    Present,
    Absent,
    Current,
    Changed,
    Missing,
    Unavailable,
    Blocked,
    Incomplete,
    Valid,
    Invalid,
    Unsupported,
    Malformed,
    Untrusted,
    Verified,
}

internal enum DoctorIntegrityState
{
    Verified,
    Malformed,
    Unsupported,
    Unavailable,
    Incomplete,
}

internal enum DoctorCandidateBasisKind
{
    Filename,
    Title,
    LiteralContent,
    RouteNeighborhood,
}

internal abstract record DoctorEvidence
{
    public abstract DoctorEvidenceKind Kind { get; }
}

internal sealed record DoctorAvailabilityEvidence(
    OperationalSourceAvailability State) : DoctorEvidence
{
    public override DoctorEvidenceKind Kind => DoctorEvidenceKind.Availability;
}

internal sealed record DoctorStateEvidence(
    DoctorObservedState State) : DoctorEvidence
{
    public override DoctorEvidenceKind Kind => DoctorEvidenceKind.State;
}

internal sealed record DoctorComparisonEvidence(
    string Expected,
    string Actual) : DoctorEvidence
{
    public override DoctorEvidenceKind Kind => DoctorEvidenceKind.Comparison;
}

internal sealed record DoctorIntegrityEvidence(
    DoctorIntegrityState State) : DoctorEvidence
{
    public override DoctorEvidenceKind Kind => DoctorEvidenceKind.Integrity;
}

internal sealed record DoctorAuthoredValueEvidence(
    string Value,
    SourceLocation? Location) : DoctorEvidence
{
    public override DoctorEvidenceKind Kind => DoctorEvidenceKind.AuthoredValue;
}

internal sealed record DoctorCandidateBasisEvidence(
    DoctorCandidateBasisKind Basis,
    string? Value,
    SourceLocation? Location) : DoctorEvidence
{
    public override DoctorEvidenceKind Kind => DoctorEvidenceKind.CandidateBasis;
}

internal sealed record DoctorProvenance
{
    public required DoctorDomainKind Domain { get; init; }

    public required DoctorProvenanceSource Source { get; init; }

    public required string? Path { get; init; }

    public required SourceLocation? Location { get; init; }
}

internal enum DoctorProvenanceSource
{
    WorkspaceEntry,
    RecoveryResiduals,
    RouteInventory,
    RouteMetadata,
    GeneratedNavigation,
    LocalReferences,
    FrameworkLifecycle,
    FrameworkPayload,
    ExtensionLifecycle,
    ExtensionSource,
    LifecycleOwnership,
}
