using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Distribution.Operational;
using OpenForge.Cli.Core.Framework.Distribution.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class FrameworkLifecycleStateDoctorInspector
{
    internal static void Inspect(
        FrameworkLifecycleDoctorView view,
        ICollection<DoctorFinding> findings,
        ICollection<DoctorLimitation> limitations,
        ref DoctorCoverageState coverage)
    {
        AddLifecycle(view, findings, limitations, ref coverage);
        AddPayload(view.Payload, findings, limitations, ref coverage);
    }

    private static void AddLifecycle(
        FrameworkLifecycleDoctorView view,
        ICollection<DoctorFinding> findings,
        ICollection<DoctorLimitation> limitations,
        ref DoctorCoverageState coverage)
    {
        if (FrameworkLifecycleOperationalContributor.ReadOwnershipObservation(view.Ownership) is { } cause)
        {
            findings.Add(Create(
                DoctorDomainSupport.Information(DoctorFindingKind.FrameworkOwnershipObservation, cause),
                view.Ownership.LogicalPath,
                DoctorProvenanceSource.FrameworkLifecycle,
                DoctorObservedState.Unavailable));
        }
    }

    private static void AddPayload(
        FrameworkPayloadReadResult payload,
        ICollection<DoctorFinding> findings,
        ICollection<DoctorLimitation> limitations,
        ref DoctorCoverageState coverage)
    {
        if (payload.State == FrameworkPayloadReadState.Available)
        {
            return;
        }

        var descriptor = payload.State switch
        {
            FrameworkPayloadReadState.Unavailable => DoctorDomainSupport.Warning(
                DoctorFindingKind.FrameworkLifecycleEvidenceUnavailable,
                payload.Cause ?? "The embedded Framework payload is unavailable.",
                DoctorResolutionLane.BlockedRepair),
            FrameworkPayloadReadState.Invalid => DoctorDomainSupport.Warning(
                DoctorFindingKind.FrameworkDistributedPayloadDefect,
                payload.Cause ?? "The embedded Framework payload is invalid.",
                DoctorResolutionLane.ManualDecision),
            _ => throw new ArgumentOutOfRangeException(
                nameof(payload),
                payload.State,
                "The Framework payload state is not defined."),
        };
        findings.Add(Create(
            descriptor,
            path: null,
            DoctorProvenanceSource.FrameworkPayload,
            payload.State == FrameworkPayloadReadState.Invalid
                ? DoctorObservedState.Invalid
                : DoctorObservedState.Unavailable));
        coverage = DoctorDomainSupport.Combine(coverage, DoctorCoverageState.Incomplete);
        limitations.Add(DoctorDomainSupport.Limitation(
            DoctorCoverageState.Incomplete,
            "Framework target comparison cannot be complete without a valid embedded payload."));
    }

    private static DoctorFinding Create(
        DoctorFindingDescriptor descriptor,
        string? path,
        DoctorProvenanceSource source,
        DoctorObservedState state)
        => DoctorDomainSupport.Create(
            descriptor,
            DoctorDomainSupport.Subject(DoctorSubjectKind.ManagedFile, path),
            DoctorDomainSupport.Provenance(
                DoctorDomainKind.FrameworkLifecycle,
                source,
                path),
            [new DoctorStateEvidence(state)]);
}
