using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Doctor.Models.Result;

internal enum DoctorCandidateCardinality
{
    None,
    One,
    Several,
}

internal sealed record DoctorCandidateBasis
{
    public required DoctorCandidateBasisKind Kind { get; init; }

    public required string? Value { get; init; }

    public required SourceLocation? Location { get; init; }
}

internal sealed record DoctorCandidate
{
    public required DoctorSubject Subject { get; init; }

    public required IReadOnlyList<DoctorCandidateBasis> Evidence { get; init; }

    public required DoctorProvenance Provenance { get; init; }
}

internal sealed record DoctorCandidateSet
{
    public required DoctorCandidateCardinality Cardinality { get; init; }

    public required IReadOnlyList<DoctorCandidate> Items { get; init; }
}

internal enum DoctorProposalKind
{
    ReferenceCanonicalization,
}

internal enum DoctorProposalVerificationKind
{
    SameTargetIdentity,
    ResultingBytes,
}

internal enum DoctorProposalRecoveryKind
{
    NoPersistentState,
    RepairReceiptRequired,
}

internal sealed record DoctorExactProposal
{
    public required DoctorProposalKind Kind { get; init; }

    public required DoctorSubject Subject { get; init; }

    public required string ExpectedValue { get; init; }

    public required string IntendedValue { get; init; }

    public required DoctorBoundary Boundary { get; init; }

    public required DoctorProposalVerificationKind Verification { get; init; }

    public required DoctorProposalRecoveryKind Recovery { get; init; }
}
