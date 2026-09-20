using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;

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
            .GroupBy(candidate => EncodeTargetPath(candidate.Candidate.Path), StringComparer.Ordinal)
            .Select(group => ReadCandidate(group.Key, group, reference.Facts.Fragment))
            .OrderBy(candidate => candidate.Target.CanonicalTargetPath, StringComparer.Ordinal)
            .ToArray();
        return new RepairCandidateSet(candidates);
    }

    private static RepairCandidate ReadCandidate(
        string targetPath,
        IEnumerable<LocalReferenceCandidateObservation> observations,
        string? fragment)
    {
        var evidence = observations
            .SelectMany(observation => observation.Bases)
            .Select(ReadEvidence)
            .DistinctBy(value => (value.Kind, value.Value, value.Location))
            .ToArray();
        var target = new RepairTargetSelection(targetPath, fragment, evidence);
        return new RepairCandidate(target, evidence);
    }

    private static string EncodeTargetPath(string path)
        => path.Replace("%", "%25", StringComparison.Ordinal)
            .Replace("#", "%23", StringComparison.Ordinal)
            .Replace("?", "%3F", StringComparison.Ordinal);

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
