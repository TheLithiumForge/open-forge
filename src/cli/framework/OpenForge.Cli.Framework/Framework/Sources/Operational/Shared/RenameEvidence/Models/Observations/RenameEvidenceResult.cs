using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

internal enum RenameEvidenceBasis
{
    GitRename,
    CommittedContent,
    RecordedMetadata,
}

internal enum RenameResolution
{
    Proven,
    SeveralStrongCandidates,
    Contradictory,
    WeakOnly,
    NoCandidate,
    IncompleteUniverse,
}

internal sealed record RenameEvidenceObservation
{
    public required string OldPath { get; init; }
    public required string NewPath { get; init; }
    public required RenameEvidenceBasis Basis { get; init; }
    public required string? Head { get; init; }
    public required string? OldBlob { get; init; }
    public required string? OldHash { get; init; }
    public required string NewHash { get; init; }
    public required int? Similarity { get; init; }
    public required bool SameContent { get; init; }
    public required RenameMetadataSignature? Metadata { get; init; }
    public required FileStateSnapshot CandidateSnapshot { get; init; }
    public required FileExpectation MissingOldExpectation { get; init; }
    public required FileExpectation? RecordExpectation { get; init; }
}

internal sealed record RenamePathResolution(
    string OldPath,
    RenameResolution Resolution,
    ImmutableArray<RenameEvidenceObservation> StrongEvidence,
    ImmutableArray<LocalReferenceCandidateObservation> WeakCandidates,
    ImmutableArray<string> GuidedCandidatePaths)
{
    internal RenameEvidenceObservation? Proven => Resolution == RenameResolution.Proven ? StrongEvidence[0] : null;
}

internal sealed record RenameEvidenceFacts
{
    public required ImmutableArray<RenameCurrentCandidate> Candidates { get; init; }
    public required ImmutableArray<RenameMissingPath> MissingPaths { get; init; }
    public required ImmutableArray<RenameRecordedIdentity> OldIdentities { get; init; }
    public required RenameGitObservation Git { get; init; }
    public required bool CandidateUniverseComplete { get; init; }
    public ImmutableArray<RenameWeakCandidate> WeakCandidates { get; init; } = [];
    public ImmutableArray<FileExpectation> PolicyExpectations { get; init; } = [];
}

internal sealed record RenameEvidenceResult(RenameEvidenceFacts Facts, ImmutableArray<RenamePathResolution> Resolutions)
{
    internal RenamePathResolution? Find(string oldPath)
        => Resolutions.FirstOrDefault(resolution => resolution.OldPath == oldPath);
}
