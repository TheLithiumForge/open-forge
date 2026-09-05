using OpenForge.Cli.Core.Commands.Doctor.Models.Presentation;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;

internal static class DoctorJsonFindingProjection
{
    internal static DoctorJsonFinding Finding(DoctorFinding finding)
        => new()
        {
            Kind = DoctorDefinitions.ReadFindingKind(finding.Kind),
            Severity = DoctorFindingWireVocabulary.Severity(finding.Severity),
            Message = finding.Message,
            Subject = Subject(finding.Subject),
            Evidence = finding.Evidence.Select(DoctorJsonEvidenceProjection.Create).ToArray(),
            Provenance = Provenance(finding.Provenance),
            Resolution = DoctorFindingWireVocabulary.Resolution(finding.Resolution),
            Candidates = finding.Candidates is { } candidates ? Candidates(candidates) : null,
            Proposal = finding.Proposal is { } proposal ? Proposal(proposal) : null,
            Actions = finding.Actions.Select(DoctorJsonProjection.Action).ToArray(),
        };

    private static DoctorJsonSubject Subject(DoctorSubject subject)
        => new()
        {
            Kind = DoctorFindingWireVocabulary.Subject(subject.Kind),
            Path = subject.Path,
            Id = subject.Identifier,
            Location = DoctorJsonProjection.Location(subject.Location),
        };

    private static DoctorJsonProvenance Provenance(DoctorProvenance provenance)
        => new()
        {
            Domain = DoctorWireVocabulary.Domain(provenance.Domain),
            Source = DoctorFindingWireVocabulary.Provenance(provenance.Source),
            Path = provenance.Path,
            Location = DoctorJsonProjection.Location(provenance.Location),
        };

    private static DoctorJsonCandidates Candidates(DoctorCandidateSet candidates)
        => new()
        {
            Cardinality = DoctorFindingWireVocabulary.Cardinality(candidates.Cardinality),
            Items = candidates.Items.Select(candidate => new DoctorJsonCandidate
            {
                Subject = Subject(candidate.Subject),
                Evidence = candidate.Evidence.Select(basis => new DoctorJsonCandidateBasis
                {
                    Kind = DoctorFindingWireVocabulary.CandidateBasis(basis.Kind),
                    Value = basis.Value,
                    Location = DoctorJsonProjection.Location(basis.Location),
                }).ToArray(),
                Provenance = Provenance(candidate.Provenance),
            }).ToArray(),
        };

    private static DoctorJsonProposal Proposal(DoctorExactProposal proposal)
        => new()
        {
            Kind = DoctorFindingWireVocabulary.Proposal(proposal.Kind),
            Subject = Subject(proposal.Subject),
            Expected = proposal.ExpectedValue,
            Intended = proposal.IntendedValue,
            Boundary = DoctorJsonProjection.Boundary(proposal.Boundary),
            Verification = DoctorFindingWireVocabulary.Verification(proposal.Verification),
            Recovery = DoctorFindingWireVocabulary.Recovery(proposal.Recovery),
        };
}
