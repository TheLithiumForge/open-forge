using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class FrameworkRecoveryDoctorInspector
{
    private const DoctorDomainKind Domain = DoctorDomainKind.FrameworkLifecycle;

    internal static void Inspect(
        RecoveryResidualDoctorView recovery,
        ICollection<DoctorFinding> findings)
    {
        foreach (var candidate in recovery.Candidates.Where(candidate =>
                     candidate.Comparison?.IsPartial == true))
        {
            var comparison = candidate.Comparison
                ?? throw new InvalidOperationException(
                    "A partial recovery candidate requires its independent comparison.");
            var evidence = comparison.Targets
                .Select(target => new DoctorComparisonEvidence(
                    $"{target.TargetPath}:prior-or-intended",
                    target.State switch
                    {
                        RecoveryBundleTargetComparisonState.Prior => "prior",
                        RecoveryBundleTargetComparisonState.Intended => "intended",
                        _ => throw new InvalidOperationException(
                            "A partial recovery comparison contains only prior and intended targets."),
                    }))
                .Cast<DoctorEvidence>()
                .ToArray();
            findings.Add(DoctorDomainSupport.Create(
                DoctorDomainSupport.Error(
                    DoctorFindingKind.FrameworkPartialRecovery,
                    "One verified Framework recovery final has an exact mixed prior/intended current target state."),
                DoctorDomainSupport.Subject(
                    DoctorSubjectKind.RecoveryItem,
                    candidate.Path,
                    comparison.Attribution.Subject.Identity),
                DoctorDomainSupport.Provenance(
                    Domain,
                    DoctorProvenanceSource.RecoveryResiduals,
                    candidate.Path),
                evidence));
        }
    }
}
