using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor.Shared.Rendering;

internal static class DoctorCandidateTestData
{
    internal static DoctorFinding Finding(DoctorFindingKind kind)
    {
        var location = new SourceLocation(12, 4, 200, 40);
        var provenance = new DoctorProvenance
        {
            Domain = DoctorDomainKind.LocalReferences,
            Source = DoctorProvenanceSource.LocalReferences,
            Path = ".agents/directives/review.md",
            Location = location,
        };
        return new DoctorFinding
        {
            Kind = kind,
            Severity = DoctorFindingSeverity.Warning,
            Message = "The linked file was not found.",
            Subject = new DoctorSubject
            {
                Kind = DoctorSubjectKind.SourceOccurrence,
                Path = provenance.Path,
                Identifier = "../guidance/testing.md",
                Location = location,
            },
            Evidence = [new DoctorStateEvidence(DoctorObservedState.Missing)],
            Provenance = provenance,
            Resolution = DoctorResolutionLane.GuidedChoice,
            Candidates = new DoctorCandidateSet
            {
                Cardinality = DoctorCandidateCardinality.One,
                Items = [new DoctorCandidate
                {
                    Subject = new DoctorSubject
                    {
                        Kind = DoctorSubjectKind.Target,
                        Path = ".agents/guidance/testing.md",
                        Identifier = null,
                        Location = null,
                    },
                    Evidence = [new DoctorCandidateBasis { Kind = DoctorCandidateBasisKind.Filename, Value = "testing.md", Location = null }],
                    Provenance = provenance with { Path = ".agents/guidance/testing.md", Location = null },
                }],
            },
            Proposal = null,
            Actions = [],
        };
    }
}
