using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Maintenance;
using OpenForge.Cli.Core.Commands.Doctor.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class LocalReferenceDoctorInspector
{
    private const DoctorDomainKind Domain = DoctorDomainKind.LocalReferences;

    internal static DoctorDomainReport Inspect(LocalReferenceDoctorView view, DoctorMaintenanceObservation? maintenance = null)
    {
        var findings = view.References
            .SelectMany(observation => CreateFindings(
                observation,
                view.CandidateScan == LocalReferenceCandidateScanState.Complete,
                view.Candidates, maintenance))
            .ToArray();
        var coverage = DoctorDomainSupport.Coverage(view.State);
        if (view.References.Any(reference =>
                reference.Facts.Target.Resolution == SourceLinkTargetResolution.Unreadable
                || reference.Fragment.State == LocalReferenceFragmentState.Unverified))
        {
            coverage = DoctorDomainSupport.Combine(coverage, DoctorCoverageState.Incomplete);
        }

        if (view.CandidateScan != LocalReferenceCandidateScanState.Complete)
        {
            coverage = DoctorDomainSupport.Combine(
                coverage,
                view.CandidateScan == LocalReferenceCandidateScanState.Blocked
                    ? DoctorCoverageState.Blocked
                    : DoctorCoverageState.Incomplete);
        }

        var limitations = new List<DoctorLimitation>();
        if (coverage != DoctorCoverageState.Complete)
        {
            limitations.Add(DoctorDomainSupport.Limitation(
                coverage,
                "At least one local reference occurrence could not be inspected completely."));
        }

        if (view.CandidateScan != LocalReferenceCandidateScanState.Complete)
        {
            limitations.Add(DoctorDomainSupport.Limitation(
                view.CandidateScan == LocalReferenceCandidateScanState.Blocked
                    ? DoctorCoverageState.Blocked
                    : DoctorCoverageState.Incomplete,
                "The bounded local-reference candidate scan did not complete; no cardinality was inferred."));
        }
        if (maintenance?.Renames?.Facts.Git.Availability == RenameGitAvailability.TimedOut)
        {
            coverage = DoctorDomainSupport.Combine(coverage, DoctorCoverageState.Incomplete);
            limitations.Add(new()
            {
                Kind = DoctorLimitationKind.Incomplete,
                Message = "Git rename evidence timed out.",
                GitRenameEvidenceTimedOut = true,
            });
        }
        return new DoctorDomainReport
        {
            Domain = Domain,
            Boundary = new DoctorBoundary { Kind = DoctorBoundaryKind.LocalReferenceUniverse, Path = null },
            Coverage = coverage,
            Lifecycle = null,
            SourceAvailability = null,
            Limitations = limitations,
            Counts = DoctorFindingAggregation.Count(findings),
            Findings = DoctorFindingAggregation.Order(findings),
            Actions = DoctorFindingAggregation.Actions(findings),
        };
    }

    private static IEnumerable<DoctorFinding> CreateFindings(
        LocalReferenceObservation observation,
        bool candidateScanComplete,
        IReadOnlyList<LocalReferenceCandidateObservation> observations,
        DoctorMaintenanceObservation? maintenance)
    {
        var target = observation.Facts.Target;
        if (target.Resolution == SourceLinkTargetResolution.Missing
            && observation.Location is { } location
            && maintenance?.GeneratedEntries.TryGetValue(observation.SourcePath, out var entries) == true
            && location.ByteOffset >= entries.ByteOffset && location.ByteOffset + location.ByteLength <= entries.ByteOffset + entries.ByteLength)
            yield break;
        if (maintenance?.ReferenceProtectionPolicy is { } protection
            && DoctorReferenceProtectionFindingFactory.Read(observation, protection) is { } protectedFinding)
        {
            yield return protectedFinding;
            yield break;
        }
        if (observation.Facts.Finding?.Code == SourceLinkDestinationFindingCode.IdentityCollision)
        {
            yield return LocalReferenceDoctorFindingFactory.Create(
                observation,
                DoctorDomainSupport.Error(
                    DoctorFindingKind.ReferenceTargetAlias,
                    observation.Facts.Finding.Cause),
                observedState: DoctorObservedState.Blocked,
                additionalEvidence: observation.Facts.Finding.Candidates
                    .Select(candidate => (DoctorEvidence)new DoctorAuthoredValueEvidence(
                        candidate.Path,
                        Location: null))
                    .ToArray());
            yield break;
        }

        var renames = maintenance?.Renames;
        if (target.Resolution == SourceLinkTargetResolution.Missing && renames?.Facts.Git.Availability == RenameGitAvailability.TimedOut)
            yield break;
        var sourceMove = maintenance?.SourceMoves.FirstOrDefault(move => move.Reference == observation);
        if (renames is not null && sourceMove is not null && DoctorRenameFindingFactory.SourceMove(observation, sourceMove.Target, renames) is { } movedFinding)
        {
            yield return movedFinding;
            yield break;
        }
        if (target.Resolution == SourceLinkTargetResolution.Missing && target.Path is not null && renames is not null)
        {
            yield return DoctorRenameFindingFactory.Create(observation, renames, candidateScanComplete ? observations : []);
            yield break;
        }
        var candidates = candidateScanComplete
            && target.Resolution is SourceLinkTargetResolution.Missing or SourceLinkTargetResolution.FragmentMissing
            ? LocalReferenceDoctorCandidateProjector.Project(observation, observations)
            : null;
        var descriptor = LocalReferenceDoctorDescriptorReader.Read(observation);
        if (descriptor is not null)
        {
            yield return LocalReferenceDoctorFindingFactory.Create(observation, descriptor, candidates);
        }

        foreach (var canonicalization in observation.Canonicalizations)
        {
            yield return LocalReferenceDoctorCanonicalizationProjector.Project(
                observation,
                canonicalization);
        }
    }
}
