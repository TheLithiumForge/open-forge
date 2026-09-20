using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;
using OpenForge.Cli.Core.Commands.Shared.Serialization;
using OpenForge.Cli.Core.Commands.Shared;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class RecoveryResidualDoctorInspector
{
    private const DoctorDomainKind Domain = DoctorDomainKind.RecoveryResiduals;

    internal static DoctorDomainReport Inspect(RecoveryResidualDoctorView view)
    {
        var findings = view.Candidates.Select(CreateFinding).ToArray();
        var coverage = DoctorDomainSupport.Coverage(view.State);
        var limitations = coverage == DoctorCoverageState.Complete
            ? []
            : new[]
            {
                DoctorDomainSupport.Limitation(
                    coverage,
                    view.Cause ?? "The exact recovery bucket could not be inspected completely."),
            };
        return new DoctorDomainReport
        {
            Domain = Domain,
            Boundary = new DoctorBoundary { Kind = DoctorBoundaryKind.RecoveryStore, Path = null },
            Coverage = coverage,
            Lifecycle = null,
            SourceAvailability = null,
            Limitations = limitations,
            Counts = DoctorFindingAggregation.Count(findings),
            Findings = DoctorFindingAggregation.Order(findings),
            Actions = DoctorFindingAggregation.Actions(findings),
        };
    }

    private static DoctorFinding CreateFinding(RecoveryDoctorCandidateObservation candidate)
    {
        var action = new DoctorNextAction
        {
            Kind = DoctorNextActionKind.AcceptedOperation,
            Operation = DoctorNextOperation.Cleanup,
            Command = CommandLines.Cleanup,
            Reason = "Cleanup owns exact recovery-item deletion after under-lease revalidation.",
        };
        var descriptor = (candidate.Kind, candidate.Integrity) switch
        {
            (RecoveryBundleCandidateKind.Final, RecoveryBundleIntegrity.Verified) => DoctorDomainSupport.Information(
                DoctorFindingKind.RecoveryBundleRecognized,
                "A semantically verified recovery final is present.",
                action),
            (RecoveryBundleCandidateKind.Draft, _) => DoctorDomainSupport.Warning(
                DoctorFindingKind.RecoveryDraftRecognized,
                "An incomplete recovery draft is present.",
                DoctorResolutionLane.Informational,
                action),
            (RecoveryBundleCandidateKind.Final, RecoveryBundleIntegrity.Malformed or RecoveryBundleIntegrity.Unsupported or RecoveryBundleIntegrity.Unavailable) => DoctorDomainSupport.Error(
                DoctorFindingKind.RecoveryBundleCollision,
                "The exact recovery final is not semantically valid."),
            (RecoveryBundleCandidateKind.Final, RecoveryBundleIntegrity.Incomplete) => DoctorDomainSupport.Error(
                DoctorFindingKind.RecoveryProvenanceUnavailable,
                "The exact recovery final does not provide complete semantic provenance."),
            _ => throw new ArgumentOutOfRangeException(nameof(candidate), candidate, "The recovery candidate condition is not defined."),
        };
        var evidence = new List<DoctorEvidence>
        {
            new DoctorIntegrityEvidence(ReadIntegrity(candidate.Integrity)),
        };
        if (candidate.Comparison is { } comparison)
        {
            evidence.AddRange(comparison.Targets.Select(target =>
                new DoctorComparisonEvidence(
                    $"{target.TargetPath}:prior-or-intended",
                    RecoveryWireVocabulary.TargetComparison(target.State))));
        }

        return DoctorDomainSupport.Create(
            descriptor,
            DoctorDomainSupport.Subject(DoctorSubjectKind.RecoveryItem, candidate.Path),
            DoctorDomainSupport.Provenance(Domain, DoctorProvenanceSource.RecoveryResiduals, candidate.Path),
            evidence);
    }

    private static DoctorIntegrityState ReadIntegrity(RecoveryBundleIntegrity integrity)
        => integrity switch
        {
            RecoveryBundleIntegrity.Verified => DoctorIntegrityState.Verified,
            RecoveryBundleIntegrity.Malformed => DoctorIntegrityState.Malformed,
            RecoveryBundleIntegrity.Unsupported => DoctorIntegrityState.Unsupported,
            RecoveryBundleIntegrity.Unavailable => DoctorIntegrityState.Unavailable,
            RecoveryBundleIntegrity.Incomplete => DoctorIntegrityState.Incomplete,
            _ => throw new ArgumentOutOfRangeException(nameof(integrity), integrity, "The recovery integrity is not defined."),
        };
}
