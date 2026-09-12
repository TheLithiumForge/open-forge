using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class ExtensionLifecycleDoctorInspector
{
    private const DoctorDomainKind Domain = DoctorDomainKind.ExtensionLifecycle;

    internal static DoctorDomainReport Inspect(ExtensionLifecycleDoctorView view)
    {
        var findings = new List<DoctorFinding>();
        var limitations = new List<DoctorLimitation>();
        var coverage = DoctorDomainSupport.Coverage(view.State);
        AddLifecycle(view, findings, limitations, ref coverage);
        foreach (var source in view.Sources)
        {
            ExtensionSourceDoctorInspector.Inspect(source, findings, limitations, ref coverage);
        }

        ExtensionOwnershipDoctorInspector.Inspect(
            view.Ownership,
            findings,
            limitations,
            ref coverage);
        foreach (var target in view.Targets)
        {
            ExtensionManagedTargetDoctorInspector.Inspect(target, findings, limitations, ref coverage);
        }

        ExtensionPackageDoctorInspector.Inspect(view.Packages, findings);
        ExtensionManagedTargetDoctorInspector.InspectSet(
            view.ManagedSet,
            findings);
        ExtensionObservationHorizonDoctorInspector.AddBridgeRegistrationObservations(
            view.BridgeRegistrations,
            findings,
            limitations,
            ref coverage);

        return new DoctorDomainReport
        {
            Domain = Domain,
            Boundary = new DoctorBoundary { Kind = DoctorBoundaryKind.ExtensionLifecycle, Path = null },
            Coverage = coverage,
            Lifecycle = view.LifecycleState,
            SourceAvailability = view.SourceAvailability,
            Limitations = limitations,
            Counts = DoctorFindingAggregation.Count(findings),
            Findings = DoctorFindingAggregation.Order(findings),
            Actions = DoctorFindingAggregation.Actions(findings),
        };
    }

    private static void AddLifecycle(
        ExtensionLifecycleDoctorView view,
        ICollection<DoctorFinding> findings,
        ICollection<DoctorLimitation> limitations,
        ref DoctorCoverageState coverage)
    {
        var lifecycle = view.Lifecycle;
        var descriptor = view.Section switch
        {
            ExtensionLifecycleSectionState.Present
                when lifecycle.Trust is LifecycleExtensionTrust.Trusted or LifecycleExtensionTrust.Absent => null,
            ExtensionLifecycleSectionState.DocumentMissing => DoctorDomainSupport.Information(
                DoctorFindingKind.ExtensionLifecycleDocumentMissing,
                lifecycle.Cause ?? "The lifecycle document is missing."),
            ExtensionLifecycleSectionState.SectionMissing => DoctorDomainSupport.Error(
                DoctorFindingKind.ExtensionLifecycleSectionMissing,
                "The unified lifecycle document has no Extension section."),
            ExtensionLifecycleSectionState.Invalid => DoctorDomainSupport.Error(
                DoctorFindingKind.ExtensionLifecycleDocumentInvalid,
                lifecycle.Cause ?? "The lifecycle document is invalid."),
            ExtensionLifecycleSectionState.Present => DoctorDomainSupport.Error(
                DoctorFindingKind.ExtensionLifecycleUntrusted,
                lifecycle.Cause ?? "Extension lifecycle facts are not trusted."),
            ExtensionLifecycleSectionState.Unavailable
                or ExtensionLifecycleSectionState.Blocked
                or ExtensionLifecycleSectionState.Interrupted => null,
            _ => throw new ArgumentOutOfRangeException(
                nameof(view),
                view.Section,
                "The Extension lifecycle condition is not defined."),
        };
        if (descriptor is not null)
        {
            findings.Add(Create(
                descriptor,
                path: null,
                DoctorProvenanceSource.ExtensionLifecycle,
                view.Section switch
                {
                    ExtensionLifecycleSectionState.DocumentMissing => DoctorObservedState.Missing,
                    ExtensionLifecycleSectionState.SectionMissing => DoctorObservedState.Missing,
                    ExtensionLifecycleSectionState.Invalid => DoctorObservedState.Invalid,
                    ExtensionLifecycleSectionState.Present => DoctorObservedState.Untrusted,
                    _ => throw new InvalidOperationException(
                        "Only diagnosed Extension lifecycle states produce findings."),
                }));
        }

        if (view.Section is ExtensionLifecycleSectionState.Unavailable
            or ExtensionLifecycleSectionState.Blocked
            or ExtensionLifecycleSectionState.Interrupted)
        {
            var state = view.Section == ExtensionLifecycleSectionState.Blocked
                ? DoctorCoverageState.Blocked
                : DoctorCoverageState.Incomplete;
            coverage = DoctorDomainSupport.Combine(coverage, state);
            limitations.Add(DoctorDomainSupport.Limitation(
                state,
                lifecycle.Cause ?? "Extension lifecycle inspection was unavailable or interrupted."));
        }
    }

    private static DoctorFinding Create(
        DoctorFindingDescriptor descriptor,
        string? path,
        DoctorProvenanceSource source,
        DoctorObservedState state)
        => DoctorDomainSupport.Create(
            descriptor,
            DoctorDomainSupport.Subject(DoctorSubjectKind.Extension, path, path),
            DoctorDomainSupport.Provenance(Domain, source, path),
            [new DoctorStateEvidence(state)]);

}
