using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class FrameworkOwnershipDoctorInspector
{
    private const DoctorDomainKind Domain = DoctorDomainKind.FrameworkLifecycle;

    internal static void Inspect(
        LifecycleOwnershipReadResult ownership,
        ICollection<DoctorFinding> findings,
        ICollection<DoctorLimitation> limitations,
        ref DoctorCoverageState coverage)
    {
        foreach (var finding in ownership.Findings.Where(finding =>
                     finding.Code == LifecycleOwnershipFindingCode.CrossSectionCollision))
        {
            findings.Add(DoctorDomainSupport.Create(
                DoctorDomainSupport.Warning(
                    DoctorFindingKind.FrameworkOwnershipConflict,
                    finding.Cause,
                    DoctorResolutionLane.ManualDecision),
                DoctorDomainSupport.Subject(DoctorSubjectKind.ManagedFile, finding.Path),
                DoctorDomainSupport.Provenance(
                    Domain,
                    DoctorProvenanceSource.LifecycleOwnership,
                    finding.Path),
                [new DoctorStateEvidence(DoctorObservedState.Invalid)]));
        }

        if (ownership.Framework.State != LifecycleOwnershipReadState.Trusted)
        {
            var state = ownership.Findings.Any(finding =>
                    finding.Code == LifecycleOwnershipFindingCode.LifecycleMissing)
                ? DoctorCoverageState.Incomplete
                : ownership.Framework.State == LifecycleOwnershipReadState.Blocked
                ? DoctorCoverageState.Blocked
                : DoctorCoverageState.Incomplete;
            coverage = DoctorDomainSupport.Combine(coverage, state);
            limitations.Add(DoctorDomainSupport.Limitation(
                state,
                ownership.Framework.Cause
                    ?? "Framework ownership evidence is unavailable."));
        }
    }
}
