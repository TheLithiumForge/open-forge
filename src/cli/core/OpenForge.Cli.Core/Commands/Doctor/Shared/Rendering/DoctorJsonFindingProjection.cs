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
            Evidence = [.. finding.Evidence.Select(DoctorJsonEvidenceProjection.Create)],
            Provenance = Provenance(finding.Provenance),
            Resolution = DoctorFindingWireVocabulary.Resolution(finding.Resolution),
            Candidates = finding.Candidates is { } candidates ? Candidates(candidates) : null,
            Proposal = finding.Proposal is { } proposal ? Proposal(proposal) : null,
            Actions = [.. finding.Actions.Select(DoctorJsonProjection.Action)],
        };

    internal static DoctorJsonSubject Subject(DoctorSubject subject)
    {
        if (subject.Library is { } librarySubject && subject.Kind != librarySubject.Kind)
        {
            throw new ArgumentException("Doctor Library subject facts must match the subject kind.", nameof(subject));
        }

        return new DoctorJsonSubject
        {
            Library = subject.Library is { } library ? DoctorLibraryPresentation.Subject(library) : null,
            Kind = SubjectKind(subject.Kind),
            Path = subject.Path,
            Id = subject.Identifier,
            Location = DoctorJsonProjection.Location(subject.Location),
        };
    }

    private static DoctorJsonProvenance Provenance(DoctorProvenance provenance)
        => new()
        {
            Domain = DoctorWireVocabulary.Domain(provenance.Domain),
            Source = DoctorFindingWireVocabulary.Provenance(provenance.Source),
            Path = provenance.Path,
            Location = DoctorJsonProjection.Location(provenance.Location),
        };

    internal static DoctorJsonCandidates Candidates(DoctorCandidateSet candidates)
        => new()
        {
            Cardinality = DoctorFindingWireVocabulary.Cardinality(candidates.Cardinality),
            Items = [.. candidates.Items.Select(candidate => new DoctorJsonCandidate
            {
                Subject = Subject(candidate.Subject),
                Evidence = [.. candidate.Evidence.Select(basis => new DoctorJsonCandidateBasis
                {
                    Kind = DoctorFindingWireVocabulary.CandidateBasis(basis.Kind),
                    Value = basis.Value,
                    Location = DoctorJsonProjection.Location(basis.Location),
                })],
                Provenance = Provenance(candidate.Provenance),
            })],
        };

    internal static DoctorJsonProposal Proposal(DoctorExactProposal proposal)
    {
        var libraryEntry = proposal.Kind switch
        {
            DoctorProposalKind.ReferenceCanonicalization => null,
            DoctorProposalKind.LibraryResidualRecovery => (proposal.LibraryRecovery
                ?? throw new ArgumentException("A Library recovery payload is required.", nameof(proposal)))
                .Entry.Input.Context.Entry,
            _ => throw new ArgumentOutOfRangeException(
                nameof(proposal),
                proposal.Kind,
                "The Doctor proposal kind is not defined."),
        };
        return new()
        {
            Kind = DoctorFindingWireVocabulary.Proposal(proposal.Kind),
            Subject = Subject(proposal.Subject),
            Expected = libraryEntry is null
                ? proposal.Reference?.ExpectedValue
                : DoctorLibraryRecoveryPresentation.StateIdentity(libraryEntry.Intended),
            Intended = libraryEntry is null
                ? proposal.Reference?.IntendedValue
                : DoctorLibraryRecoveryPresentation.StateIdentity(libraryEntry.Prior),
            Boundary = DoctorJsonProjection.Boundary(proposal.Boundary),
            Verification = libraryEntry is null
                ? DoctorFindingWireVocabulary.Verification(proposal.Verification)
                : "library-no-follow-exact",
            Recovery = libraryEntry is null
                ? DoctorFindingWireVocabulary.Recovery(proposal.Recovery)
                : "repair-receipt-required",
            LibraryRecovery = proposal.Kind switch
            {
                DoctorProposalKind.ReferenceCanonicalization => null,
                DoctorProposalKind.LibraryResidualRecovery => DoctorLibraryRecoveryPresentation.Project(
                    proposal.LibraryRecovery ?? throw new ArgumentException("A Library recovery payload is required.", nameof(proposal))),
                _ => throw new ArgumentOutOfRangeException(nameof(proposal), proposal.Kind, "The Doctor proposal kind is not defined."),
            },
        };
    }

    private static string SubjectKind(DoctorSubjectKind kind)
        => kind switch
        {
            DoctorSubjectKind.Library
                or DoctorSubjectKind.LibrarySourceRoot
                or DoctorSubjectKind.LibraryMapping
                or DoctorSubjectKind.LibraryProjection
                or DoctorSubjectKind.LibraryResidual => "library",
            DoctorSubjectKind.Workspace
                or DoctorSubjectKind.Path
                or DoctorSubjectKind.Route
                or DoctorSubjectKind.GeneratedRegion
                or DoctorSubjectKind.SourceOccurrence
                or DoctorSubjectKind.Target
                or DoctorSubjectKind.RecoveryItem
                or DoctorSubjectKind.ManagedFile
                or DoctorSubjectKind.Extension
                or DoctorSubjectKind.Dependency => DoctorFindingWireVocabulary.Subject(kind),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Doctor subject kind is not defined."),
        };
}
