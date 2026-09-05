using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Operational.Models;
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
        var lifecycle = view.Lifecycle;
        DoctorFindingDescriptor? descriptor = lifecycle.State switch
        {
            LifecycleStoreReadState.Available
                when view.LifecycleState == OperationalLifecycleState.Untrusted =>
                DoctorDomainSupport.Error(
                    DoctorFindingKind.FrameworkLifecycleUntrusted,
                    "Framework lifecycle, payload, and managed-source facts do not establish trust."),
            LifecycleStoreReadState.Available => null,
            LifecycleStoreReadState.DocumentMissing => DoctorDomainSupport.Error(
                DoctorFindingKind.FrameworkInstallIncomplete,
                lifecycle.Cause ?? "Framework installation evidence is incomplete."),
            LifecycleStoreReadState.SectionMissing => DoctorDomainSupport.Error(
                DoctorFindingKind.FrameworkLifecycleSectionMissing,
                lifecycle.Cause ?? "The Framework lifecycle section is missing."),
            LifecycleStoreReadState.Invalid => DoctorDomainSupport.Error(
                DoctorFindingKind.FrameworkLifecycleEvidenceMalformed,
                lifecycle.Cause ?? "Framework lifecycle evidence is malformed."),
            LifecycleStoreReadState.Unavailable => DoctorDomainSupport.Error(
                DoctorFindingKind.FrameworkLifecycleEvidenceUnavailable,
                lifecycle.Cause ?? "Framework lifecycle evidence is unavailable."),
            LifecycleStoreReadState.Blocked or LifecycleStoreReadState.Cancelled => null,
            _ => throw new ArgumentOutOfRangeException(
                nameof(view),
                lifecycle.State,
                "The Framework lifecycle state is not defined."),
        };
        if (descriptor is not null)
        {
            findings.Add(Create(
                descriptor,
                lifecycle.File?.LogicalPath,
                DoctorProvenanceSource.FrameworkLifecycle,
                lifecycle.State switch
                {
                    LifecycleStoreReadState.Available => DoctorObservedState.Untrusted,
                    LifecycleStoreReadState.DocumentMissing => DoctorObservedState.Incomplete,
                    LifecycleStoreReadState.SectionMissing => DoctorObservedState.Missing,
                    LifecycleStoreReadState.Invalid => DoctorObservedState.Malformed,
                    LifecycleStoreReadState.Unavailable => DoctorObservedState.Unavailable,
                    _ => throw new InvalidOperationException(
                        "Only diagnosed Framework lifecycle states produce findings."),
                }));
        }

        if (lifecycle.State is LifecycleStoreReadState.Blocked or LifecycleStoreReadState.Cancelled)
        {
            var lifecycleCoverage = lifecycle.State == LifecycleStoreReadState.Blocked
                ? DoctorCoverageState.Blocked
                : DoctorCoverageState.Incomplete;
            coverage = DoctorDomainSupport.Combine(coverage, lifecycleCoverage);
            limitations.Add(DoctorDomainSupport.Limitation(
                lifecycleCoverage,
                lifecycle.Cause ?? "Framework lifecycle inspection was interrupted or blocked."));
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
            FrameworkPayloadReadState.Unavailable => DoctorDomainSupport.Error(
                DoctorFindingKind.FrameworkLifecycleEvidenceUnavailable,
                payload.Cause ?? "The embedded Framework payload is unavailable."),
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
