using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Lifecycle.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class FrameworkLifecycleDoctorInspector
{
    private const DoctorDomainKind Domain = DoctorDomainKind.FrameworkLifecycle;

    internal static DoctorDomainReport Inspect(
        FrameworkLifecycleDoctorView view,
        RecoveryResidualDoctorView recovery,
        LifecycleOwnershipReadResult ownership,
        bool absenceProven)
    {
        var findings = new List<DoctorFinding>();
        var limitations = new List<DoctorLimitation>();
        var coverage = absenceProven
            ? DoctorCoverageState.Complete
            : DoctorDomainSupport.Coverage(view.State);
        if (absenceProven)
        {
            findings.Add(Create(
                DoctorDomainSupport.Information(
                    DoctorFindingKind.FrameworkInstallAbsent,
                    "Framework lifecycle absence is established by the complete operational proof.",
                    InstallAction()),
                path: null,
                DoctorProvenanceSource.FrameworkLifecycle,
                DoctorObservedState.Absent));
        }
        else
        {
            FrameworkLifecycleStateDoctorInspector.Inspect(
                view,
                findings,
                limitations,
                ref coverage);
            foreach (var target in view.Targets)
            {
                FrameworkManagedTargetDoctorInspector.Inspect(target, findings, limitations, ref coverage);
            }

            AddManagedSet(view.ManagedSet, findings);
            FrameworkOwnershipDoctorInspector.Inspect(
                ownership,
                findings,
                limitations,
                ref coverage);
        }

        FrameworkRecoveryDoctorInspector.Inspect(recovery, findings);

        return new DoctorDomainReport
        {
            Domain = Domain,
            Boundary = new DoctorBoundary { Kind = DoctorBoundaryKind.FrameworkLifecycle, Path = null },
            Coverage = coverage,
            Lifecycle = absenceProven
                ? OperationalLifecycleState.Absent
                : view.LifecycleState,
            SourceAvailability = absenceProven
                ? OperationalSourceAvailability.NotApplicable
                : view.SourceAvailability,
            Limitations = limitations,
            Counts = DoctorFindingAggregation.Count(findings),
            Findings = DoctorFindingAggregation.Order(findings),
            Actions = DoctorFindingAggregation.Actions(findings),
        };
    }

    private static void AddManagedSet(
        FrameworkManagedSetState state,
        ICollection<DoctorFinding> findings)
    {
        if (state == FrameworkManagedSetState.Mixed)
        {
            findings.Add(Create(
                DoctorDomainSupport.Error(
                    DoctorFindingKind.FrameworkPartialLifecycle,
                    "The trusted Framework managed set contains both current and non-current targets."),
                path: null,
                DoctorProvenanceSource.FrameworkLifecycle,
                DoctorObservedState.Incomplete));
        }
    }

    private static DoctorFinding Create(
        DoctorFindingDescriptor descriptor,
        string? path,
        DoctorProvenanceSource source,
        DoctorObservedState state)
        => DoctorDomainSupport.Create(
            descriptor,
            DoctorDomainSupport.Subject(DoctorSubjectKind.ManagedFile, path),
            DoctorDomainSupport.Provenance(Domain, source, path),
            [new DoctorStateEvidence(state)]);

    private static DoctorNextAction InstallAction()
        => new()
        {
            Kind = DoctorNextActionKind.AcceptedOperation,
            Operation = DoctorNextOperation.Install,
            Command = "open-forge install",
            Reason = "Install owns Framework establishment.",
        };

}
