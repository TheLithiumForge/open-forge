using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class LocalReferenceDoctorCanonicalizationProjector
{
    private const DoctorDomainKind Domain = DoctorDomainKind.LocalReferences;

    internal static DoctorFinding Project(
        LocalReferenceObservation observation,
        LocalReferenceCanonicalization correction)
    {
        var kind = correction.Kind switch
        {
            LocalReferenceCanonicalizationKind.Path => DoctorFindingKind.ReferenceSameTargetPath,
            LocalReferenceCanonicalizationKind.Case => DoctorFindingKind.ReferenceSameTargetCase,
            LocalReferenceCanonicalizationKind.Encoding => DoctorFindingKind.ReferenceSameTargetEncoding,
            LocalReferenceCanonicalizationKind.Fragment => DoctorFindingKind.ReferenceSameTargetFragment,
            _ => throw new ArgumentOutOfRangeException(
                nameof(correction),
                correction.Kind,
                "The local-reference canonicalization kind is not defined."),
        };
        var subject = new DoctorSubject
        {
            Kind = DoctorSubjectKind.SourceOccurrence,
            Path = observation.SourcePath,
            Identifier = observation.Destination,
            Location = observation.Location,
        };
        return DoctorDomainSupport.CreateWithProposal(
            DoctorDomainSupport.Information(
                kind,
                "The authored destination has one exact same-target canonical spelling.",
                DoctorResolutionLane.SafeExact),
            subject,
            new DoctorProvenance
            {
                Domain = Domain,
                Source = DoctorProvenanceSource.LocalReferences,
                Path = observation.SourcePath,
                Location = observation.Location,
            },
            evidence:
            [
                new DoctorAuthoredValueEvidence(correction.Expected, correction.DestinationLocation),
                new DoctorComparisonEvidence(correction.Intended, correction.Expected),
            ],
            new DoctorExactProposal(
                DoctorProposalKind.ReferenceCanonicalization,
                new DoctorReferenceProposal(correction.Expected, correction.Intended),
                libraryRecovery: null)
            {
                Subject = subject,
                Boundary = new DoctorBoundary
                {
                    Kind = DoctorBoundaryKind.LocalReferenceUniverse,
                    Path = observation.SourcePath,
                },
                Verification = DoctorProposalVerificationKind.SameTargetIdentity,
                Recovery = DoctorProposalRecoveryKind.NoPersistentState,
            });
    }
}
