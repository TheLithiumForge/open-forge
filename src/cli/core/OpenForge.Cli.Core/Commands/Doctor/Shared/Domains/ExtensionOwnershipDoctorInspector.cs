using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class ExtensionOwnershipDoctorInspector
{
    internal static void Inspect(
        LifecycleOwnershipReadResult ownership,
        ICollection<DoctorFinding> findings,
        ICollection<DoctorLimitation> limitations,
        ref DoctorCoverageState coverage)
    {
        foreach (var finding in ownership.Findings.Where(item =>
                     item.Code == LifecycleOwnershipFindingCode.CrossSectionCollision))
        {
            findings.Add(DoctorDomainSupport.Create(
                DoctorDomainSupport.Warning(
                    DoctorFindingKind.ExtensionOwnershipCollision,
                    finding.Cause,
                    DoctorResolutionLane.ManualDecision),
                DoctorDomainSupport.Subject(
                    DoctorSubjectKind.Extension,
                    finding.Path,
                    finding.Path),
                DoctorDomainSupport.Provenance(
                    DoctorDomainKind.ExtensionLifecycle,
                    DoctorProvenanceSource.LifecycleOwnership,
                    finding.Path),
                [new DoctorStateEvidence(DoctorObservedState.Invalid)]));
        }

        if (ownership.Framework.State == LifecycleOwnershipReadState.Trusted
            && ownership.Extensions.State == LifecycleOwnershipReadState.Trusted)
        {
            return;
        }

        coverage = DoctorDomainSupport.Combine(
            coverage,
            DoctorCoverageState.Incomplete);
        limitations.Add(DoctorDomainSupport.Limitation(
            DoctorCoverageState.Incomplete,
            "Lifecycle ownership sections did not establish complete trusted ownership facts."));
    }
}
