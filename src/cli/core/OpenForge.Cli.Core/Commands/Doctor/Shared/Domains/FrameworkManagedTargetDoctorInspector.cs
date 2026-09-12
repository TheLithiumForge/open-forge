using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Identity;
using OpenForge.Cli.Core.Framework.Lifecycle.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class FrameworkManagedTargetDoctorInspector
{
    private const DoctorDomainKind Domain = DoctorDomainKind.FrameworkLifecycle;

    internal static void Inspect(
        FrameworkManagedTargetDoctorObservation observation,
        ICollection<DoctorFinding> findings,
        ICollection<DoctorLimitation> limitations,
        ref DoctorCoverageState coverage)
    {
        var target = observation.Target;
        if (target.Source.State == FrameworkLifecycleTargetSourceState.SourceMismatch)
        {
            findings.Add(Create(
                DoctorDomainSupport.Error(
                    DoctorFindingKind.FrameworkLifecycleUntrusted,
                    target.Source.Cause ?? "The managed target source does not match the current embedded payload."),
                target.Path,
                DoctorProvenanceSource.FrameworkLifecycle,
                DoctorObservedState.Untrusted));
            coverage = DoctorDomainSupport.Combine(
                coverage,
                DoctorCoverageState.Incomplete);
            limitations.Add(DoctorDomainSupport.Limitation(
                DoctorCoverageState.Incomplete,
                target.Source.Cause
                    ?? "The Framework target source identity is not trusted."));
            return;
        }

        if (target.Source.State == FrameworkLifecycleTargetSourceState.Blocked)
        {
            coverage = DoctorCoverageState.Blocked;
            limitations.Add(DoctorDomainSupport.Limitation(
                DoctorCoverageState.Blocked,
                target.Source.Cause ?? "The managed target source boundary is blocked."));
            return;
        }

        var descriptor = target.State switch
        {
            OperationalTargetState.Current => null,
            OperationalTargetState.Changed => TargetFinding(
                DoctorFindingKind.FrameworkManagedChanged,
                "A Framework-managed target differs from its semantic baseline."),
            OperationalTargetState.Missing => TargetFinding(
                DoctorFindingKind.FrameworkManagedMissing,
                "A Framework-managed target is missing."),
            OperationalTargetState.Unavailable or OperationalTargetState.Blocked => null,
            _ => throw new ArgumentOutOfRangeException(nameof(target), target.State, "The managed target state is not defined."),
        };
        if (descriptor is not null)
        {
            findings.Add(DoctorDomainSupport.Create(
                descriptor,
                DoctorDomainSupport.Subject(DoctorSubjectKind.ManagedFile, target.Path),
                DoctorDomainSupport.Provenance(
                    Domain,
                    DoctorProvenanceSource.FrameworkLifecycle,
                    target.Path),
                ReadEvidence(observation)));
        }

        if (observation.Boundary is { } boundary
            && target.State != OperationalTargetState.Current)
        {
            var kind = boundary switch
            {
                FrameworkManagedTargetBoundaryKind.ProviderBridge =>
                    DoctorFindingKind.FrameworkBridgeBoundary,
                FrameworkManagedTargetBoundaryKind.RootRegion =>
                    DoctorFindingKind.FrameworkRootRegionBoundary,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(observation),
                    boundary,
                    "The Framework managed boundary is not defined."),
            };
            findings.Add(Create(
                DoctorDomainSupport.Error(
                    kind,
                    $"The trusted Framework {ReadBoundaryName(boundary)} boundary is not current."),
                target.Path,
                DoctorProvenanceSource.FrameworkLifecycle,
                ReadState(target.State)));
        }

        if (target.State is OperationalTargetState.Unavailable or OperationalTargetState.Blocked)
        {
            var targetCoverage = target.State == OperationalTargetState.Blocked
                ? DoctorCoverageState.Blocked
                : DoctorCoverageState.Incomplete;
            coverage = DoctorDomainSupport.Combine(coverage, targetCoverage);
            limitations.Add(DoctorDomainSupport.Limitation(
                targetCoverage,
                $"Framework target evidence is unavailable for {target.Path}."));
        }
    }

    private static string ReadBoundaryName(FrameworkManagedTargetBoundaryKind boundary)
        => boundary switch
        {
            FrameworkManagedTargetBoundaryKind.ProviderBridge => "provider bridge",
            FrameworkManagedTargetBoundaryKind.RootRegion => "root region",
            _ => throw new ArgumentOutOfRangeException(
                nameof(boundary),
                boundary,
                "The Framework managed boundary is not defined."),
        };

    private static DoctorFindingDescriptor TargetFinding(DoctorFindingKind kind, string message)
        => DoctorDomainSupport.Warning(
            kind,
            message,
            DoctorResolutionLane.TargetedOperation,
            new DoctorNextAction
            {
                Kind = DoctorNextActionKind.AcceptedOperation,
                Operation = DoctorNextOperation.Update,
                Command = "open-forge update",
                Reason = "Update owns Framework managed-target reconciliation.",
            });

    private static DoctorFinding Create(
        DoctorFindingDescriptor descriptor,
        string path,
        DoctorProvenanceSource source,
        DoctorObservedState state)
        => DoctorDomainSupport.Create(
            descriptor,
            DoctorDomainSupport.Subject(DoctorSubjectKind.ManagedFile, path),
            DoctorDomainSupport.Provenance(Domain, source, path),
            [new DoctorStateEvidence(state)]);

    private static IReadOnlyList<DoctorEvidence> ReadEvidence(
        FrameworkManagedTargetDoctorObservation observation)
        => observation.Target.State switch
        {
            OperationalTargetState.Changed when observation.CurrentFingerprint is { } current =>
                [new DoctorComparisonEvidence(observation.Target.BaselineFingerprint, current)],
            OperationalTargetState.Missing =>
                [new DoctorStateEvidence(DoctorObservedState.Missing)],
            _ => throw new InvalidOperationException(
                "Only changed or missing Framework targets produce comparison findings."),
        };

    private static DoctorObservedState ReadState(OperationalTargetState state)
        => state switch
        {
            OperationalTargetState.Changed => DoctorObservedState.Changed,
            OperationalTargetState.Missing => DoctorObservedState.Missing,
            OperationalTargetState.Unavailable => DoctorObservedState.Unavailable,
            OperationalTargetState.Blocked => DoctorObservedState.Blocked,
            _ => throw new InvalidOperationException(
                "A current Framework target does not produce a boundary finding."),
        };
}
