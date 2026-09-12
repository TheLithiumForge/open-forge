using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class LocalReferenceDoctorInspector
{
    private const DoctorDomainKind Domain = DoctorDomainKind.LocalReferences;

    internal static DoctorDomainReport Inspect(LocalReferenceDoctorView view)
    {
        var findings = view.References
            .SelectMany(observation => CreateFindings(
                observation,
                view.CandidateScan == LocalReferenceCandidateScanState.Complete,
                view.Candidates))
            .Concat(LocalReferenceDoctorGraphProjector.Project(view.GraphFindings))
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
        IReadOnlyList<LocalReferenceCandidateObservation> observations)
    {
        var target = observation.Facts.Target;
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

        if (observation.Kind == LocalReferenceKind.Image
            && target.Resolution == SourceLinkTargetResolution.Complete)
        {
            yield return LocalReferenceDoctorFindingFactory.Create(
                observation,
                DoctorDomainSupport.Information(DoctorFindingKind.ReferenceImage, "A bounded local image occurrence is valid."));
            yield break;
        }

        var candidates = candidateScanComplete
            && target.Resolution == SourceLinkTargetResolution.Missing
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
        if (candidates is { } value)
        {
            foreach (var finding in LocalReferenceDoctorCandidateInspector.Project(observation, value))
            {
                yield return finding;
            }
        }
    }
}
