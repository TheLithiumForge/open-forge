using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class LocalReferenceDoctorGraphProjector
{
    internal static IEnumerable<DoctorFinding> Project(
        IReadOnlyList<LocalReferenceGraphFinding> findings)
    {
        foreach (var finding in findings)
        {
            var occurrence = finding.Occurrences[0];
            var kind = finding.Kind switch
            {
                LocalReferenceGraphFindingKind.Cycle => DoctorFindingKind.ReferenceCycle,
                LocalReferenceGraphFindingKind.Repeat => DoctorFindingKind.ReferenceRepeat,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(finding),
                    finding.Kind,
                    "The local-reference graph finding kind is not defined."),
            };
            yield return DoctorDomainSupport.Create(
                DoctorDomainSupport.Information(
                    kind,
                    "Bounded local-reference evidence contains a repeated occurrence or cycle."),
                new DoctorSubject
                {
                    Kind = DoctorSubjectKind.SourceOccurrence,
                    Path = occurrence.SourcePath,
                    Identifier = occurrence.Destination,
                    Location = occurrence.Location,
                },
                new DoctorProvenance
                {
                    Domain = DoctorDomainKind.LocalReferences,
                    Source = DoctorProvenanceSource.LocalReferences,
                    Path = occurrence.SourcePath,
                    Location = occurrence.Location,
                },
                finding.Occurrences
                    .Select(item => (DoctorEvidence)new DoctorAuthoredValueEvidence(
                        item.Destination,
                        item.Location))
                    .ToArray());
        }
    }
}
