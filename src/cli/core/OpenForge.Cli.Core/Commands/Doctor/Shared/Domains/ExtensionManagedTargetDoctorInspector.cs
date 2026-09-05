using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class ExtensionManagedTargetDoctorInspector
{
    private const DoctorDomainKind Domain = DoctorDomainKind.ExtensionLifecycle;

    internal static void Inspect(
        ExtensionManagedTargetDoctorObservation observation,
        ICollection<DoctorFinding> findings,
        ICollection<DoctorLimitation> limitations,
        ref DoctorCoverageState coverage)
    {
        var target = observation.Target;
        var descriptor = target.State switch
        {
            OperationalTargetState.Current => null,
            OperationalTargetState.Changed => TargetFinding(
                DoctorFindingKind.ExtensionManagedChanged,
                "An Extension-managed target differs from its baseline."),
            OperationalTargetState.Missing => TargetFinding(
                DoctorFindingKind.ExtensionManagedMissing,
                "An Extension-managed target is missing."),
            OperationalTargetState.Unavailable or OperationalTargetState.Blocked => null,
            _ => throw new ArgumentOutOfRangeException(nameof(target), target.State, "The Extension target state is not defined."),
        };
        if (descriptor is not null)
        {
            findings.Add(DoctorDomainSupport.Create(
                descriptor,
                DoctorDomainSupport.Subject(DoctorSubjectKind.ManagedFile, target.Path),
                DoctorDomainSupport.Provenance(Domain, DoctorProvenanceSource.ExtensionLifecycle, target.Path),
                ReadEvidence(observation)));
        }

        if (target.State is OperationalTargetState.Unavailable or OperationalTargetState.Blocked)
        {
            var targetCoverage = target.State == OperationalTargetState.Blocked
                ? DoctorCoverageState.Blocked
                : DoctorCoverageState.Incomplete;
            coverage = DoctorDomainSupport.Combine(coverage, targetCoverage);
            limitations.Add(DoctorDomainSupport.Limitation(
                targetCoverage,
                $"Extension target evidence is unavailable for {target.Path}."));
        }
    }

    internal static void InspectSet(
        ExtensionManagedSetState state,
        ICollection<DoctorFinding> findings)
    {
        if (state == ExtensionManagedSetState.Mixed)
        {
            findings.Add(DoctorDomainSupport.Create(
                DoctorDomainSupport.Error(
                    DoctorFindingKind.ExtensionPartialLifecycle,
                    "The trusted Extension managed set contains both current and non-current targets."),
                DoctorDomainSupport.Subject(DoctorSubjectKind.Extension, path: null),
                DoctorDomainSupport.Provenance(
                    Domain,
                    DoctorProvenanceSource.ExtensionLifecycle,
                    path: null),
                [new DoctorStateEvidence(DoctorObservedState.Incomplete)]));
        }
    }

    private static DoctorFindingDescriptor TargetFinding(DoctorFindingKind kind, string message)
        => DoctorDomainSupport.Warning(
            kind,
            message,
            DoctorResolutionLane.TargetedOperation,
            new DoctorNextAction
            {
                Kind = DoctorNextActionKind.AcceptedOperation,
                Operation = DoctorNextOperation.ExtensionUpdate,
                Command = "open-forge extension update",
                Reason = "Extension update owns managed-target reconciliation.",
            });

    private static IReadOnlyList<DoctorEvidence> ReadEvidence(
        ExtensionManagedTargetDoctorObservation observation)
        => observation.Target.State switch
        {
            OperationalTargetState.Changed when observation.CurrentFingerprint is { } current =>
                [new DoctorComparisonEvidence(observation.Target.BaselineFingerprint, current)],
            OperationalTargetState.Missing =>
                [new DoctorStateEvidence(DoctorObservedState.Missing)],
            _ => throw new InvalidOperationException(
                "Only changed or missing Extension targets produce findings."),
        };
}
