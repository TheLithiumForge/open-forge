using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class LocalReferenceDoctorCandidateProjector
{
    private const DoctorDomainKind Domain = DoctorDomainKind.LocalReferences;

    internal static DoctorCandidateSet Project(
        LocalReferenceObservation reference,
        IReadOnlyList<LocalReferenceCandidateObservation> observations)
    {
        var candidates = observations
            .Where(candidate => string.Equals(
                    candidate.SourcePath,
                    reference.SourcePath,
                    StringComparison.Ordinal)
                && string.Equals(
                    candidate.Destination,
                    reference.Destination,
                    StringComparison.Ordinal))
            .Select(candidate => new DoctorCandidate
            {
                Subject = DoctorDomainSupport.Subject(
                    DoctorSubjectKind.Target,
                    candidate.Candidate.Path,
                    candidate.Candidate.Id),
                Evidence = candidate.Bases.Select(ProjectBasis).ToArray(),
                Provenance = DoctorDomainSupport.Provenance(
                    Domain,
                    DoctorProvenanceSource.LocalReferences,
                    candidate.Candidate.Path),
            })
            .OrderBy(candidate => candidate.Subject.Path, StringComparer.Ordinal)
            .ToArray();
        return new DoctorCandidateSet
        {
            Cardinality = candidates.Length switch
            {
                0 => DoctorCandidateCardinality.None,
                1 => DoctorCandidateCardinality.One,
                _ => DoctorCandidateCardinality.Several,
            },
            Items = candidates,
        };
    }

    private static DoctorCandidateBasis ProjectBasis(LocalReferenceCandidateBasis basis)
        => new()
        {
            Kind = basis.Kind switch
            {
                LocalReferenceCandidateBasisKind.Filename => DoctorCandidateBasisKind.Filename,
                LocalReferenceCandidateBasisKind.Title => DoctorCandidateBasisKind.Title,
                LocalReferenceCandidateBasisKind.LiteralContent => DoctorCandidateBasisKind.LiteralContent,
                LocalReferenceCandidateBasisKind.RouteNeighborhood => DoctorCandidateBasisKind.RouteNeighborhood,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(basis),
                    basis.Kind,
                    "The local-reference candidate basis is not defined."),
            },
            Value = basis.Value,
            Location = basis.Location,
        };
}
