using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class LocalReferenceDoctorCandidateInspector
{
    internal static IEnumerable<DoctorFinding> Project(
        LocalReferenceObservation observation,
        DoctorCandidateSet candidates)
    {
        foreach (var basis in candidates.Items.SelectMany(item => item.Evidence).Select(item => item.Kind).Distinct().Order())
        {
            var kind = basis switch
            {
                DoctorCandidateBasisKind.Filename => DoctorFindingKind.ReferenceCandidateFilename,
                DoctorCandidateBasisKind.Title => DoctorFindingKind.ReferenceCandidateTitle,
                DoctorCandidateBasisKind.LiteralContent => DoctorFindingKind.ReferenceCandidateLiteralContent,
                DoctorCandidateBasisKind.RouteNeighborhood => DoctorFindingKind.ReferenceCandidateRouteNeighborhood,
                _ => throw new ArgumentOutOfRangeException(nameof(basis), basis, "The local-reference candidate basis is not defined."),
            };
            yield return LocalReferenceDoctorFindingFactory.Create(
                observation,
                DoctorDomainSupport.Warning(kind, "A bounded local-reference candidate basis was retained.", DoctorResolutionLane.GuidedChoice),
                candidates);
        }

        var cardinalityKind = candidates.Cardinality switch
        {
            DoctorCandidateCardinality.None => DoctorFindingKind.ReferenceCandidatesNone,
            DoctorCandidateCardinality.One => DoctorFindingKind.ReferenceCandidatesOne,
            DoctorCandidateCardinality.Several => DoctorFindingKind.ReferenceCandidatesSeveral,
            _ => throw new ArgumentOutOfRangeException(nameof(candidates), candidates.Cardinality, "The candidate cardinality is not defined."),
        };
        yield return LocalReferenceDoctorFindingFactory.Create(
            observation,
            candidates.Cardinality == DoctorCandidateCardinality.None
                ? DoctorDomainSupport.Warning(cardinalityKind, "No bounded reference candidate is available.", DoctorResolutionLane.ManualDecision)
                : DoctorDomainSupport.Warning(cardinalityKind, "Bounded reference candidates were retained without selecting one.", DoctorResolutionLane.GuidedChoice),
            candidates);
    }
}
