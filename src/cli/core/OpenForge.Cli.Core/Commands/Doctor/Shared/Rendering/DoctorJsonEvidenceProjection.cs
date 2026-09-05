using OpenForge.Cli.Core.Commands.Doctor.Models.Presentation;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;

internal static class DoctorJsonEvidenceProjection
{
    internal static DoctorJsonEvidence Create(DoctorEvidence evidence)
        => evidence switch
        {
            DoctorAvailabilityEvidence value => new DoctorJsonEvidence
            {
                Kind = "availability",
                Basis = null,
                State = DoctorWireVocabulary.SourceAvailability(value.State),
                Expected = null,
                Actual = null,
                Value = null,
                Path = null,
                Location = null,
            },
            DoctorStateEvidence value => new DoctorJsonEvidence
            {
                Kind = "state",
                Basis = null,
                State = DoctorFindingWireVocabulary.ObservedState(value.State),
                Expected = null,
                Actual = null,
                Value = null,
                Path = null,
                Location = null,
            },
            DoctorComparisonEvidence value => new DoctorJsonEvidence
            {
                Kind = "comparison",
                Basis = null,
                State = null,
                Expected = value.Expected,
                Actual = value.Actual,
                Value = null,
                Path = null,
                Location = null,
            },
            DoctorIntegrityEvidence value => new DoctorJsonEvidence
            {
                Kind = "integrity",
                Basis = null,
                State = DoctorFindingWireVocabulary.Integrity(value.State),
                Expected = null,
                Actual = null,
                Value = null,
                Path = null,
                Location = null,
            },
            DoctorAuthoredValueEvidence value => new DoctorJsonEvidence
            {
                Kind = "authored-value",
                Basis = null,
                State = null,
                Expected = null,
                Actual = null,
                Value = value.Value,
                Path = null,
                Location = DoctorJsonProjection.Location(value.Location),
            },
            DoctorCandidateBasisEvidence value => new DoctorJsonEvidence
            {
                Kind = "candidate-basis",
                Basis = DoctorFindingWireVocabulary.CandidateBasis(value.Basis),
                State = null,
                Expected = null,
                Actual = null,
                Value = value.Value,
                Path = null,
                Location = DoctorJsonProjection.Location(value.Location),
            },
            _ => throw new ArgumentOutOfRangeException(
                nameof(evidence),
                evidence.Kind,
                "The Doctor evidence kind is not defined."),
        };
}
