using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Extensions.Operational;
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
        if (ExtensionLifecycleOperationalContributor.ReadOwnershipObservation(view.Ownership) is { } cause)
        {
            findings.Add(Create(
                DoctorDomainSupport.Information(DoctorFindingKind.ExtensionOwnershipObservation, cause),
                view.Ownership.LogicalPath, DoctorProvenanceSource.ExtensionLifecycle,
                DoctorObservedState.Unavailable));
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
