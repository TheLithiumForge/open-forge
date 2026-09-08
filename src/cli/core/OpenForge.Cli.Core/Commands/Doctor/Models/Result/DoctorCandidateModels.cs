using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
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
    LibraryResidualRecovery,
}

internal enum DoctorProposalVerificationKind
{
    SameTargetIdentity,
    ResultingBytes,
    NoFollowPriorState,
}

internal enum DoctorProposalRecoveryKind
{
    NoPersistentState,
    RepairReceiptRequired,
    VerifiedLibraryResidual,
}

internal sealed record DoctorExactProposal
{
    internal DoctorExactProposal(DoctorProposalKind kind, DoctorReferenceProposal? reference, LibraryResidualEvidence? libraryRecovery)
    {
        var coherent = kind switch
        {
            DoctorProposalKind.ReferenceCanonicalization => reference is not null && libraryRecovery is null,
            DoctorProposalKind.LibraryResidualRecovery => reference is null && libraryRecovery is not null,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Doctor proposal kind is not defined."),
        };
        if (!coherent)
        {
            throw new ArgumentException("A Doctor proposal requires exactly its finite typed payload.", nameof(kind));
        }

        if (reference is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(reference.ExpectedValue);
            ArgumentException.ThrowIfNullOrWhiteSpace(reference.IntendedValue);
        }

        Kind = kind;
        Reference = reference;
        LibraryRecovery = libraryRecovery;
    }

    public DoctorProposalKind Kind { get; }
    public DoctorReferenceProposal? Reference { get; }
    public LibraryResidualEvidence? LibraryRecovery { get; }
    public required DoctorSubject Subject { get; init; }
    public required DoctorBoundary Boundary { get; init; }
    public required DoctorProposalVerificationKind Verification { get; init; }
    public required DoctorProposalRecoveryKind Recovery { get; init; }
}

internal sealed record DoctorReferenceProposal(string ExpectedValue, string IntendedValue);
