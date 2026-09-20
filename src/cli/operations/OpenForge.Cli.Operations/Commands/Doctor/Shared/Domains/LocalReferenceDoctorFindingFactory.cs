using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class LocalReferenceDoctorFindingFactory
{
    internal static DoctorFinding Create(
        LocalReferenceObservation observation,
        DoctorFindingDescriptor descriptor,
        DoctorCandidateSet? candidates = null,
        DoctorObservedState? observedState = null,
        IReadOnlyList<DoctorEvidence>? additionalEvidence = null)
    {
        var finding = DoctorDomainSupport.Create(
            descriptor,
            new DoctorSubject
            {
                Kind = DoctorSubjectKind.SourceOccurrence,
                Path = observation.SourcePath,
                Identifier = observation.Destination,
                Location = observation.Location,
            },
            new DoctorProvenance
            {
                Domain = DoctorDomainKind.LocalReferences,
                Source = DoctorProvenanceSource.LocalReferences,
                Path = observation.SourcePath,
                Location = observation.Location,
            },
            evidence: new DoctorEvidence[]
            {
                new DoctorAuthoredValueEvidence(
                    observation.Destination,
                    observation.DestinationLocation ?? observation.Location),
                new DoctorStateEvidence(observedState ?? ReadState(observation.Facts.Target.Resolution)),
            }.Concat(additionalEvidence ?? []).ToArray());
        return candidates is null
            ? finding
            : finding with { Candidates = candidates };
    }

    private static DoctorObservedState ReadState(SourceLinkTargetResolution resolution)
        => resolution switch
        {
            SourceLinkTargetResolution.Complete => DoctorObservedState.Valid,
            SourceLinkTargetResolution.Missing or SourceLinkTargetResolution.FragmentMissing => DoctorObservedState.Missing,
            SourceLinkTargetResolution.Unreadable or SourceLinkTargetResolution.ExternalUnchecked => DoctorObservedState.Unavailable,
            SourceLinkTargetResolution.Unsupported => DoctorObservedState.Unsupported,
            SourceLinkTargetResolution.Malformed
                or SourceLinkTargetResolution.Absolute
                or SourceLinkTargetResolution.Query
                or SourceLinkTargetResolution.EncodingUnsupported => DoctorObservedState.Malformed,
            SourceLinkTargetResolution.OutsideWorkspace or SourceLinkTargetResolution.PhysicalEscape or SourceLinkTargetResolution.Ambiguous => DoctorObservedState.Blocked,
            _ => throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "The target resolution is not defined."),
        };
}
