using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;

internal static class RepairCandidateMapper
{
    internal static RepairCandidateSet Read(
        LocalReferenceObservation reference,
        LocalReferenceDoctorView view)
    {
        var candidates = view.Candidates
            .Where(candidate => string.Equals(
                    candidate.SourcePath,
                    reference.SourcePath,
                    StringComparison.Ordinal)
                && string.Equals(
                    candidate.Destination,
                    reference.Destination,
                    StringComparison.Ordinal))
            .Select(candidate => ReadCandidate(candidate, reference.Facts.Fragment))
            .OrderBy(candidate => candidate.Target.CanonicalTargetPath, StringComparer.Ordinal)
            .ToArray();
        return new RepairCandidateSet(candidates);
    }

    private static RepairCandidate ReadCandidate(LocalReferenceCandidateObservation candidate, string? fragment)
    {
        var evidence = candidate.Bases.Select(ReadEvidence).ToArray();
        var target = new RepairTargetSelection(
            candidate.Candidate.Path.Replace("%", "%25", StringComparison.Ordinal)
                .Replace("#", "%23", StringComparison.Ordinal).Replace("?", "%3F", StringComparison.Ordinal),
            fragment,
            evidence);
        return new RepairCandidate(target, evidence);
    }

    private static RepairCandidateEvidence ReadEvidence(LocalReferenceCandidateBasis basis)
        => new(
            basis.Kind switch
            {
                LocalReferenceCandidateBasisKind.Filename => RepairCandidateEvidenceKind.Filename,
                LocalReferenceCandidateBasisKind.Title => RepairCandidateEvidenceKind.Title,
                LocalReferenceCandidateBasisKind.LiteralContent => RepairCandidateEvidenceKind.LiteralContent,
                LocalReferenceCandidateBasisKind.RouteNeighborhood
                    => RepairCandidateEvidenceKind.RouteNeighborhood,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(basis),
                    basis.Kind,
                    "The candidate basis kind is not defined."),
            },
            basis.Value,
            basis.Location);
}
